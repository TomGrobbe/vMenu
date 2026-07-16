using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient.menus
{
    public class SavedVehicles
    {
        // Variables
        private Menu classMenu;
        private Menu savedVehicleTypeMenu;
        private readonly Menu vehicleCategoryMenu = new("自定义分类", "管理已保存载具");
        private readonly Menu savedVehiclesCategoryMenu = new("自定义分类", "运行时更新!");
        private readonly Menu selectedVehicleMenu = new("载具管理", "管理保存的载具.");
        private readonly Menu unavailableVehiclesMenu = new("不可用载具", "无法使用的已保存载具");
        private Dictionary<string, VehicleInfo> savedVehicles = new();
        private readonly List<Menu> subMenus = new();
        private KeyValuePair<string, VehicleInfo> currentlySelectedVehicle = new();
        private int deleteButtonPressedCount = 0;
        private int replaceButtonPressedCount = 0;
        private SavedVehicleCategory currentCategory;

        // Need to be editable from other functions
        private readonly MenuListItem setCategoryBtn = new("设置自定义分类", new List<string> { }, 0, "设置该载具的自定义分类. 选择保存.");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateClassMenu()
        {
            var menuTitle = "已保存载具";
            #region Create menus and submenus
            // Create the menu.
            classMenu = new Menu(menuTitle, "管理已保存载具");

            for (var i = 0; i < 23; i++)
            {
                var categoryMenu = new Menu("已保存载具", GetLabelText($"VEH_CLASS_{i}"));

                var vehClassButton = new MenuItem(GetLabelText($"VEH_CLASS_{i}"), $"{GetLabelText($"VEH_CLASS_{i}")} 分类中的全部已保存载具。");
                subMenus.Add(categoryMenu);
                MenuController.AddSubmenu(classMenu, categoryMenu);
                classMenu.AddMenuItem(vehClassButton);
                vehClassButton.Label = "→→→";
                MenuController.BindMenuItem(classMenu, categoryMenu, vehClassButton);

                categoryMenu.OnMenuClose += (sender) =>
                {
                    UpdateMenuAvailableCategories();
                };

                categoryMenu.OnItemSelect += (sender, item, index) =>
                {
                    UpdateSelectedVehicleMenu(item, sender);
                };
            }

            var unavailableModels = new MenuItem("无法使用的已保存载具", "因为模型不存在于服务器内, 目前不可用.很可能没有从服务器正确的流式传输.")
            {
                Label = "→→→"
            };

            classMenu.AddMenuItem(unavailableModels);
            MenuController.BindMenuItem(classMenu, unavailableVehiclesMenu, unavailableModels);
            MenuController.AddSubmenu(classMenu, unavailableVehiclesMenu);


            MenuController.AddMenu(savedVehicleTypeMenu);
            MenuController.AddMenu(savedVehiclesCategoryMenu);
            MenuController.AddMenu(selectedVehicleMenu);

            // Load selected category
            vehicleCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                // Create new category
                if (item.ItemData is not SavedVehicleCategory savedVehicleCategory)
                {
                    var name = await GetUserInput(windowTitle: "输入自定义分类名称.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "输入自定义分类描述 (可选).", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = name,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + name, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"您的类别(~g~{name}~s~)已被保存.");
                            Log($"Saved Category {name}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                        }
                        else
                        {
                            Notify.Error($"保存失败! 很可能是因为该 (~y~{name}~s~) 名称已被使用.");
                            return;
                        }
                    }
                }
                // Select an old category
                else
                {
                    currentCategory = savedVehicleCategory;
                }

                bool isUncategorized = currentCategory.Name == "未分类";

                savedVehiclesCategoryMenu.MenuTitle = currentCategory.Name;
                savedVehiclesCategoryMenu.MenuSubtitle = $"~y~自定义分类: ~s~{currentCategory.Name}";
                savedVehiclesCategoryMenu.ClearMenuItems();

                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();

                string ChangeCallback(MenuDynamicListItem item, bool left)
                {
                    int currentIndex = iconNames.IndexOf(item.CurrentItem);
                    int newIndex = left ? currentIndex - 1 : currentIndex + 1;

                    // If going past the start or end of the list
                    if (iconNames.ElementAtOrDefault(newIndex) == default)
                    {
                        if (left)
                        {
                            newIndex = iconNames.Count - 1;
                        }
                        else
                        {
                            newIndex = 0;
                        }
                    }

                    item.RightIcon = (MenuItem.Icon)newIndex;

                    return iconNames[newIndex];
                }

                var renameBtn = new MenuItem("重命名分类", "重新命名此自定义分类.")
                {
                    Enabled = !isUncategorized
                };
                var descriptionBtn = new MenuItem("更改类别描述", "更改此自定义分类的描述.")
                {
                    Enabled = !isUncategorized
                };
                var iconBtn = new MenuDynamicListItem("更改分类图标", iconNames[(int)currentCategory.Icon], new MenuDynamicListItem.ChangeItemCallback(ChangeCallback), "更改此自定义分类的图标. 选择保存.")
                {
                    Enabled = !isUncategorized,
                    RightIcon = currentCategory.Icon
                };
                var deleteBtn = new MenuItem("移除分类", "删除此自定义分类. 此功能无法撤销!")
                {
                    RightIcon = MenuItem.Icon.WARNING,
                    Enabled = !isUncategorized
                };
                var deleteCharsBtn = new MenuCheckboxItem("删除全部载具", "如开启勾选, 当尝试 \"移除分类\" 时, 该分类中保存的所有载具也将被删除. 如关闭勾选, 所有载具将移至 \"未分类\" 中.")
                {
                    Enabled = !isUncategorized
                };

                savedVehiclesCategoryMenu.AddMenuItem(renameBtn);
                savedVehiclesCategoryMenu.AddMenuItem(descriptionBtn);
                savedVehiclesCategoryMenu.AddMenuItem(iconBtn);
                savedVehiclesCategoryMenu.AddMenuItem(deleteBtn);
                savedVehiclesCategoryMenu.AddMenuItem(deleteCharsBtn);

                var spacer = GetSpacerMenuItem("↓ Vehicles ↓");
                savedVehiclesCategoryMenu.AddMenuItem(spacer);

                if (savedVehicles.Count > 0)
                {
                    List<MenuItem> spawnableVehicles = [];   
                    List<MenuItem> unspawnableVehicles = [];   

                    foreach (var kvp in savedVehicles)
                    {
                        string name = kvp.Key;
                        VehicleInfo vehicle = kvp.Value;

                        if (string.IsNullOrEmpty(vehicle.Category))
                        {
                            if (!isUncategorized)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (vehicle.Category != currentCategory.Name)
                            {
                                continue;
                            }
                        }

                        string buttonName = name.Substring(4);
                        bool canUse = IsModelInCdimage(vehicle.model);
                        string buttonDescription = "管理保存的载具.";

                        if (!canUse)
                        {
                            buttonName = $"~italic~{buttonName}~italic~";
                            buttonDescription += "\n\n~r~NOTE~w~~s~: This model could not be found, and so cannot be spawned.";
                        }

                        var btn = new MenuItem(buttonName, buttonDescription)
                        {
                            Label = $"({vehicle.name}) →→→",
                            LeftIcon = canUse ? MenuItem.Icon.NONE : MenuItem.Icon.LOCK,
                            ItemData = kvp,
                        };

                        if (canUse)
                        {
                            spawnableVehicles.Add(btn);
                        }
                        else
                        {
                            unspawnableVehicles.Add(btn);
                        }
                    }

                    // Menu order: Category buttons -> Spawnable vehs -> Unspawnable vehs
                    foreach (MenuItem menuItem in spawnableVehicles.Concat(unspawnableVehicles))
                    {
                        savedVehiclesCategoryMenu.AddMenuItem(menuItem);
                    }
                }
            };

            savedVehiclesCategoryMenu.OnItemSelect += async (sender, item, index) =>
            {
                switch (index)
                {
                    // Rename Category
                    case 0:
                        var name = await GetUserInput(windowTitle: "输入新的分类名称", defaultText: currentCategory.Name, maxInputLength: 30);

                        if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        else if (GetAllCategoryNames().Contains(name) || !string.IsNullOrEmpty(GetResourceKvpString("saved_veh_category_" + name)))
                        {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists);
                            return;
                        }

                        string oldName = currentCategory.Name;

                        currentCategory.Name = name;

                        if (StorageManager.SaveJsonData("saved_veh_category_" + name, JsonConvert.SerializeObject(currentCategory), false))
                        {
                            StorageManager.DeleteSavedStorageItem("saved_veh_category_" + oldName);

                            int totalCount = 0;
                            int updatedCount = 0;

                            if (savedVehicles.Count > 0)
                            {
                                foreach (var kvp in savedVehicles)
                                {
                                    string saveName = kvp.Key;
                                    VehicleInfo vehicle = kvp.Value;

                                    if (string.IsNullOrEmpty(vehicle.Category))
                                    {
                                        continue;
                                    }

                                    if (vehicle.Category != oldName)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    vehicle.Category = name;

                                    if (StorageManager.SaveVehicleInfo(saveName, vehicle, true))
                                    {
                                        updatedCount++;
                                        Log($"Updated category for \"{saveName}\"");
                                    }
                                    else
                                    {
                                        Log($"Something went wrong when updating category for \"{saveName}\"");
                                    }
                                }
                            }

                            Notify.Success($"您的类别已更名为~g~{name}~s~. {updatedCount}/{totalCount} 载具更新.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("重新命名类别时出了问题, 您的旧类别不会因此被删除.");
                        }
                        break;

                    // Change Category Description
                    case 1:
                        var description = await GetUserInput(windowTitle: "输入新的分类描述", defaultText: currentCategory.Description, maxInputLength: 120);

                        currentCategory.Description = description;

                        if (StorageManager.SaveJsonData("saved_veh_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                        {
                            Notify.Success($"您的类别描述已更改.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("更改类别描述时出现问题.");
                        }
                        break;

                    // Delete Category
                    case 3:
                        if (item.Label == "再次确认?")
                        {
                            bool deleteVehicles = (sender.GetMenuItems().ElementAt(4) as MenuCheckboxItem).Checked;

                            item.Label = "";
                            DeleteResourceKvp("saved_veh_category_" + currentCategory.Name);

                            int totalCount = 0;
                            int updatedCount = 0;

                            if (savedVehicles.Count > 0)
                            {
                                foreach (var kvp in savedVehicles)
                                {
                                    string saveName = kvp.Key;
                                    VehicleInfo vehicle = kvp.Value;

                                    if (string.IsNullOrEmpty(vehicle.Category))
                                    {
                                        continue;
                                    }

                                    if (vehicle.Category != currentCategory.Name)
                                    {
                                        continue;
                                    }

                                    totalCount++;

                                    if (deleteVehicles)
                                    {
                                        updatedCount++;

                                        DeleteResourceKvp(saveName);
                                    }
                                    else
                                    {
                                        vehicle.Category = "未分类";

                                        if (StorageManager.SaveVehicleInfo(saveName, vehicle, true))
                                        {
                                            updatedCount++;
                                            Log($"Updated category for \"{saveName}\"");
                                        }
                                        else
                                        {
                                            Log($"Something went wrong when updating category for \"{saveName}\"");
                                        }
                                    }
                                }
                            }

                            Notify.Success($"Your saved category has been deleted. {updatedCount}/{totalCount} vehicles {(deleteVehicles ? "deleted" : "updated")}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            item.Label = "再次确认?";
                        }
                        break;

                    // Load saved vehicle menu
                    default:
                        if (item.ItemData is not KeyValuePair<string, VehicleInfo> vehicleInfo)
                        {
                            break;
                        }

                        List<string> categoryNames = GetAllCategoryNames();
                        List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
                        int nameIndex = categoryNames.IndexOf(currentCategory.Name);

                        setCategoryBtn.ItemData = categoryIcons;
                        setCategoryBtn.ListItems = categoryNames;
                        setCategoryBtn.ListIndex = nameIndex == 1 ? 0 : nameIndex;
                        setCategoryBtn.RightIcon = categoryIcons[setCategoryBtn.ListIndex];

                        selectedVehicleMenu.MenuSubtitle = $"{vehicleInfo.Key.Substring(4)} ({vehicleInfo.Value.name})";
                        currentlySelectedVehicle = vehicleInfo;
                        MenuController.CloseAllMenus();
                        selectedVehicleMenu.OpenMenu();
                        MenuController.AddSubmenu(savedVehiclesCategoryMenu, selectedVehicleMenu);
                        break;
                }
            };

            // Change Category Icon
            savedVehiclesCategoryMenu.OnDynamicListItemSelect += (_, _, currentItem) =>
            {
                var iconNames = Enum.GetNames(typeof(MenuItem.Icon)).ToList();
                int iconIndex = iconNames.IndexOf(currentItem);

                currentCategory.Icon = (MenuItem.Icon)iconIndex;

                if (StorageManager.SaveJsonData("saved_veh_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                {
                    Notify.Success($"您的类别图标已更改为 ~g~{iconNames[iconIndex]}~s~.");
                    UpdateSavedVehicleCategoriesMenu();
                }
                else
                {
                    Notify.Error("更改类别图标时发生问题.");
                }
            };

            var spawnVehicle = new MenuItem("载具生成");
            var renameVehicle = new MenuItem("重命名载具", "重新命名保存的载具.");
            var replaceVehicle = new MenuItem("~r~替换载具", "您保存的载具将替换为您当前驾驶的载具. ~r~警告: 此操作不可撤销!");
            var deleteVehicle = new MenuItem("~r~删除载具", "~r~这将删除您保存的载具. ~r~警告: 此操作不可撤销!");
            selectedVehicleMenu.AddMenuItem(spawnVehicle);
            selectedVehicleMenu.AddMenuItem(renameVehicle);
            selectedVehicleMenu.AddMenuItem(setCategoryBtn);
            selectedVehicleMenu.AddMenuItem(replaceVehicle);
            selectedVehicleMenu.AddMenuItem(deleteVehicle);

            selectedVehicleMenu.OnMenuOpen += (sender) =>
            {
                bool vehicleModelExists = IsModelInCdimage(currentlySelectedVehicle.Value.model);

                spawnVehicle.Enabled = vehicleModelExists;
                spawnVehicle.Description = vehicleModelExists ? "生成此已保存的载具." : "在游戏文件中找不到该模型. 最有可能的原因是这是添加载具, 服务器未对其正确进行流式传输.";

                spawnVehicle.Label = "(" + GetDisplayNameFromVehicleModel(currentlySelectedVehicle.Value.model).ToLower() + ")";
            };

            selectedVehicleMenu.OnMenuClose += (sender) =>
            {
                selectedVehicleMenu.RefreshIndex();
                deleteButtonPressedCount = 0;
                deleteVehicle.Label = "";
                replaceButtonPressedCount = 0;
                replaceVehicle.Label = "";
            };

            selectedVehicleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnVehicle)
                {
                    if (MainMenu.VehicleSpawnerMenu != null)
                    {
                        await SpawnVehicle(currentlySelectedVehicle.Value.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                    else
                    {
                        await SpawnVehicle(currentlySelectedVehicle.Value.model, true, true, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                }
                else if (item == renameVehicle)
                {
                    var newName = await GetUserInput(windowTitle: "输入此载具的新名称.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName))
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                    }
                    else
                    {
                        if (StorageManager.SaveVehicleInfo("veh_" + newName, currentlySelectedVehicle.Value, false))
                        {
                            DeleteResourceKvp(currentlySelectedVehicle.Key);
                            while (!selectedVehicleMenu.Visible)
                            {
                                await BaseScript.Delay(0);
                            }
                            Notify.Success("您的载具已成功重新命名.");
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>(); // clear the old info
                        }
                        else
                        {
                            Notify.Error("此名称已被使用或有未知故障. 如果您认为有问题, 请联系服务器所有者.");
                        }
                    }
                }
                else if (item == replaceVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        if (replaceButtonPressedCount == 0)
                        {
                            replaceButtonPressedCount = 1;
                            item.Label = "再次按键确认.";
                            Notify.Alert("您确定要更换此载具? 再次按键确认.");
                        }
                        else
                        {
                            replaceButtonPressedCount = 0;
                            item.Label = "";
                            SaveVehicle(currentlySelectedVehicle.Key.Substring(4), currentlySelectedVehicle.Value.Category);
                            selectedVehicleMenu.CloseMenu();
                            Notify.Success("您保存的载具已更换为当前载具.");
                        }
                    }
                    else
                    {
                        Notify.Error("在更换旧车之前, 您必须处于一辆有效载具内.");
                    }
                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.Label = "再次按键确认.";
                        Notify.Alert("您确定要删除此载具? 再次按键确认.");
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.Label = "";
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success("您已保存的载具已被删除.");
                    }
                }
                if (item != deleteVehicle) // if any other button is pressed, restore the delete vehicle button pressed count.
                {
                    deleteButtonPressedCount = 0;
                    deleteVehicle.Label = "";
                }
                if (item != replaceVehicle)
                {
                    replaceButtonPressedCount = 0;
                    replaceVehicle.Label = "";
                }
            };

            // Update category preview icon
            selectedVehicleMenu.OnListIndexChange += (_, listItem, _, newSelectionIndex, _) =>
            {
                if (listItem.ItemData is List<MenuItem.Icon> icons)
                {
                    listItem.RightIcon = icons[newSelectionIndex];
                }
            };

            // Update vehicle's category
            selectedVehicleMenu.OnListItemSelect += async (_, listItem, listIndex, _) =>
            {
                string name = listItem.ListItems[listIndex];

                if (name == "新建")
                {
                    var newName = await GetUserInput(windowTitle: "输入自定义分类名称.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName) || newName.ToLower() == "uncategorized" || newName.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "输入自定义分类描述 (可选).", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = newName,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + newName, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"您的类别 (~g~{newName}~s~) 已被保存.");
                            Log($"Saved Category {newName}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                            name = newName;
                        }
                        else
                        {
                            Notify.Error($"保存失败, 很可能是因为该 (~y~{newName}~s~) 名称已被使用.");
                            return;
                        }
                    }
                }

                VehicleInfo vehicle = currentlySelectedVehicle.Value;

                vehicle.Category = name;

                if (StorageManager.SaveVehicleInfo(currentlySelectedVehicle.Key, vehicle, true))
                {
                    Notify.Success("已成功保存您的载具.");
                }
                else
                {
                    Notify.Error("无法保存您的载具. 原因不明 :(");
                }

                MenuController.CloseAllMenus();
                UpdateSavedVehicleCategoriesMenu();
                vehicleCategoryMenu.OpenMenu();
            };

            unavailableVehiclesMenu.InstructionalButtons.Add(Control.FrontendDelete, "Delete Vehicle!");

            unavailableVehiclesMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.FrontendDelete, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>((m, c) =>
            {
                if (m.Size > 0)
                {
                    var index = m.CurrentIndex;
                    if (index < m.Size)
                    {
                        var item = m.GetMenuItems().Find(i => i.Index == index);
                        if (item != null && item.ItemData is KeyValuePair<string, VehicleInfo> sd)
                        {
                            if (item.Label == "~r~再次确认?")
                            {
                                Log("Unavailable saved vehicle deleted, data: " + JsonConvert.SerializeObject(sd));
                                DeleteResourceKvp(sd.Key);
                                unavailableVehiclesMenu.GoBack();
                                UpdateMenuAvailableCategories();
                            }
                            else
                            {
                                item.Label = "~r~再次确认?";
                            }
                        }
                        else
                        {
                            Notify.Error("不知何故找不到这辆载具.");
                        }
                    }
                    else
                    {
                        Notify.Error("您以某种方式删除了一个不存在的菜单项, 这是如何做到的...?");
                    }
                }
                else
                {
                    Notify.Error("目前没有可删除的载具!");
                }
            }), true));

            void ResetAreYouSure()
            {
                foreach (var i in unavailableVehiclesMenu.GetMenuItems())
                {
                    if (i.ItemData is KeyValuePair<string, VehicleInfo> vd)
                    {
                        i.Label = $"({vd.Value.name})";
                    }
                }
            }
            unavailableVehiclesMenu.OnMenuClose += (sender) => ResetAreYouSure();
            unavailableVehiclesMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) => ResetAreYouSure();

            #endregion
        }

        private void CreateTypeMenu()
        {
            savedVehicleTypeMenu = new("已保存载具", "从载具类型或自定义分类中选择管理载具");

            var saveVehicle = new MenuItem("保存当前载具", "保存您目前所乘坐的载具.")
            {
                LeftIcon = MenuItem.Icon.CAR
            };
            var classButton = new MenuItem("载具类型", "通过载具类型管理已保存的载具.")
            {
                Label = "→→→"
            };
            var categoryButton = new MenuItem("自定义分类", "通过自定义分类管理已保存的载具.")
            {
                Label = "→→→"
            };

            savedVehicleTypeMenu.AddMenuItem(saveVehicle);
            savedVehicleTypeMenu.AddMenuItem(classButton);
            savedVehicleTypeMenu.AddMenuItem(categoryButton);

            savedVehicleTypeMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle();
                    }
                    else
                    {
                        Notify.Error("您目前不在任何载具中. 请在尝试保存前进入载具.");
                    }
                }
                else if (item == classButton)
                {
                    UpdateMenuAvailableCategories();
                }
                else if (item == categoryButton)
                {
                    UpdateSavedVehicleCategoriesMenu();
                }
            };

            MenuController.BindMenuItem(savedVehicleTypeMenu, GetClassMenu(), classButton);
            MenuController.BindMenuItem(savedVehicleTypeMenu, vehicleCategoryMenu, categoryButton);
        }

        /// <summary>
        /// Updates the selected vehicle.
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns>A bool, true if successfull, false if unsuccessfull</returns>
        private bool UpdateSelectedVehicleMenu(MenuItem selectedItem, Menu parentMenu = null)
        {
            if (selectedItem.ItemData is not KeyValuePair<string, VehicleInfo> vehicleInfo)
            {
                return false;
            }

            List<string> categoryNames = GetAllCategoryNames();
            List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
            setCategoryBtn.ItemData = categoryIcons;
            setCategoryBtn.ListItems = categoryNames;
            setCategoryBtn.ListIndex = 0;
            setCategoryBtn.RightIcon = categoryIcons[0];
            selectedVehicleMenu.MenuSubtitle = $"{vehicleInfo.Key.Substring(4)} ({vehicleInfo.Value.name})";
            currentlySelectedVehicle = vehicleInfo;
            MenuController.CloseAllMenus();
            selectedVehicleMenu.OpenMenu();
            if (parentMenu != null)
            {
                MenuController.AddSubmenu(parentMenu, selectedVehicleMenu);
            }
            return true;
        }


        /// <summary>
        /// Updates the available vehicle category list.
        /// </summary>
        public void UpdateMenuAvailableCategories()
        {
            savedVehicles = GetSavedVehicles();

            for (var i = 0; i < GetClassMenu().Size - 1; i++)
            {
                if (savedVehicles.Any(a => GetVehicleClassFromName(a.Value.model) == i && IsModelInCdimage(a.Value.model)))
                {
                    GetClassMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.NONE;
                    GetClassMenu().GetMenuItems()[i].Label = "→→→";
                    GetClassMenu().GetMenuItems()[i].Enabled = true;
                    GetClassMenu().GetMenuItems()[i].Description = $"{GetClassMenu().GetMenuItems()[i].Text} 中全部的已保存载具.";
                }
                else
                {
                    GetClassMenu().GetMenuItems()[i].Label = "";
                    GetClassMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.LOCK;
                    GetClassMenu().GetMenuItems()[i].Enabled = false;
                    GetClassMenu().GetMenuItems()[i].Description = $"{GetClassMenu().GetMenuItems()[i].Text} 暂无任何已保存载具的数据.";
                }
            }

            // Check if the items count will be changed. If there are less cars than there were before, one probably got deleted
            // so in that case we need to refresh the index of that menu just to be safe. If not, keep the index where it is for improved
            // usability of the menu.
            foreach (var m in subMenus)
            {
                var size = m.Size;
                var vclass = subMenus.IndexOf(m);

                var count = savedVehicles.Count(a => GetVehicleClassFromName(a.Value.model) == vclass);
                if (count < size)
                {
                    m.RefreshIndex();
                }
            }

            foreach (var m in subMenus)
            {
                // Clear items but don't reset the index because we can guarantee that the index won't be out of bounds.
                // this is the case because of the loop above where we reset the index if the items count changes.
                m.ClearMenuItems(true);
            }

            // Always clear this index because it's useless anyway and it's safer.
            unavailableVehiclesMenu.ClearMenuItems();

            foreach (var sv in savedVehicles)
            {
                if (IsModelInCdimage(sv.Value.model))
                {
                    var vclass = GetVehicleClassFromName(sv.Value.model);
                    var menu = subMenus[vclass];

                    var savedVehicleBtn = new MenuItem(sv.Key.Substring(4), $"管理保存的载具.")
                    {
                        Label = $"({sv.Value.name}) →→→",
                        ItemData = sv
                    };
                    menu.AddMenuItem(savedVehicleBtn);
                }
                else
                {
                    var missingVehItem = new MenuItem(sv.Key.Substring(4), "在游戏文件中找不到该模型. 最有可能的原因是这是添加载具, 服务器未对其正确进行流式传输.")
                    {
                        Label = "(" + sv.Value.name + ")",
                        Enabled = false,
                        LeftIcon = MenuItem.Icon.LOCK,
                        ItemData = sv
                    };
                    //SetResourceKvp(sv.Key + "_tmp_dupe", JsonConvert.SerializeObject(sv.Value));
                    unavailableVehiclesMenu.AddMenuItem(missingVehItem);
                }
            }
            foreach (var m in subMenus)
            {
                m.SortMenuItems((A, B) =>
                {
                    return A.Text.ToLower().CompareTo(B.Text.ToLower());
                });
            }
        }

        /// <summary>
        /// Updates the saved vehicle categories menu.
        /// </summary>
        private void UpdateSavedVehicleCategoriesMenu()
        {
            savedVehicles = GetSavedVehicles();

            var categories = GetAllCategoryNames();

            vehicleCategoryMenu.ClearMenuItems();

            var createCategoryBtn = new MenuItem("新建自定义分类", "创建新的自定义载具分类.")
            {
                Label = "→→→"
            };
            vehicleCategoryMenu.AddMenuItem(createCategoryBtn);

            var spacer = GetSpacerMenuItem("↓ Vehicle Categories ↓");
            vehicleCategoryMenu.AddMenuItem(spacer);

            var uncategorized = new SavedVehicleCategory
            {
                Name = "未分类",
                Description = "所有已保存但尚未分配到自定义分类的载具."
            };
            var uncategorizedBtn = new MenuItem(uncategorized.Name, uncategorized.Description)
            {
                Label = "→→→",
                ItemData = uncategorized
            };
            vehicleCategoryMenu.AddMenuItem(uncategorizedBtn);
            MenuController.BindMenuItem(vehicleCategoryMenu, savedVehiclesCategoryMenu, uncategorizedBtn);

            // Remove "新建" and "未分类"
            categories.RemoveRange(0, 2);

            if (categories.Count > 0)
            {
                categories.Sort((a, b) => a.ToLower().CompareTo(b.ToLower()));
                foreach (var item in categories)
                {
                    SavedVehicleCategory category = StorageManager.GetSavedVehicleCategoryData("saved_veh_category_" + item);

                    var btn = new MenuItem(category.Name, category.Description)
                    {
                        Label = "→→→",
                        LeftIcon = category.Icon,
                        ItemData = category
                    };
                    vehicleCategoryMenu.AddMenuItem(btn);
                    MenuController.BindMenuItem(vehicleCategoryMenu, savedVehiclesCategoryMenu, btn);
                }
            }

            vehicleCategoryMenu.RefreshIndex();
        }

        private List<string> GetAllCategoryNames()
        {
            var categories = new List<string>();
            var handle = StartFindKvp("saved_veh_category_");
            while (true)
            {
                var foundCategory = FindKvp(handle);
                if (string.IsNullOrEmpty(foundCategory))
                {
                    break;
                }
                else
                {
                    categories.Add(foundCategory.Substring(19));
                }
            }
            EndFindKvp(handle);

            categories.Insert(0, "新建");
            categories.Insert(1, "未分类");

            return categories;
        }

        private List<MenuItem.Icon> GetCategoryIcons(List<string> categoryNames)
        {
            List<MenuItem.Icon> icons = new List<MenuItem.Icon> { };

            foreach (var name in categoryNames)
            {
                icons.Add(StorageManager.GetSavedVehicleCategoryData("saved_veh_category_" + name).Icon);
            }

            return icons;
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetClassMenu()
        {
            if (classMenu == null)
            {
                CreateClassMenu();
            }
            return classMenu;
        }

        public Menu GetTypeMenu()
        {
            if (savedVehicleTypeMenu == null)
            {
                CreateTypeMenu();
            }
            return savedVehicleTypeMenu;
        }

        public struct SavedVehicleCategory
        {
            public string Name;
            public string Description;
            public MenuItem.Icon Icon;
        }
    }
}
