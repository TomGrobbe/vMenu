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
        private readonly Menu vehicleCategoryMenu = new("Categories", "Manage Saved Vehicles");
        private readonly Menu savedVehiclesCategoryMenu = new("Category", "I get updated at runtime!");
        private readonly Menu selectedVehicleMenu = new("Manage Vehicle", "Manage this saved vehicle.");
        private readonly Menu unavailableVehiclesMenu = new("Missing Vehicles", "Unavailable Saved Vehicles");
        private Dictionary<string, VehicleInfo> savedVehicles = new();
        private readonly List<Menu> subMenus = new();
        private KeyValuePair<string, VehicleInfo> currentlySelectedVehicle = new();
        private int deleteButtonPressedCount = 0;
        private int replaceButtonPressedCount = 0;
        private SavedVehicleCategory currentCategory;

        // Need to be editable from other functions
        private readonly MenuListItem setCategoryBtn = new("Set Vehicle Category", new List<string> { }, 0, "Sets this Vehicle's category. Select to save.");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateClassMenu()
        {
            var menuTitle = "Saved Vehicles";
            #region Create menus and submenus
            // Create the menu.
            classMenu = new Menu(menuTitle, "Manage Saved Vehicles");

            for (var i = 0; i < 23; i++)
            {
                var categoryMenu = new Menu("Saved Vehicles", GetLabelText($"VEH_CLASS_{i}"));

                var vehClassButton = new MenuItem(GetLabelText($"VEH_CLASS_{i}"), $"All saved vehicles from the {GetLabelText($"VEH_CLASS_{i}")} category.");
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

            var unavailableModels = new MenuItem("Unavailable Saved Vehicles", "These vehicles are currently unavailable because the models are not present in the game. These vehicles are most likely not being streamed from the server.")
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
                if (item.ItemData is not SavedVehicleCategory)
                {
                    var name = await GetUserInput(windowTitle: "Enter a category name.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(name) || name.ToLower() == "uncategorized" || name.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "Enter a category description (optional).", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = name,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + name, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"Your category (~g~<C>{name}</C>~s~) has been saved.");
                            Log($"Saved Category {name}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                        }
                        else
                        {
                            Notify.Error($"Saving failed, most likely because this name (~y~<C>{name}</C>~s~) is already in use.");
                            return;
                        }
                    }
                }
                // Select an old category
                else
                {
                    currentCategory = item.ItemData;
                }

                bool isUncategorized = currentCategory.Name == "Uncategorized";

                savedVehiclesCategoryMenu.MenuTitle = currentCategory.Name;
                savedVehiclesCategoryMenu.MenuSubtitle = $"~s~Category: ~y~{currentCategory.Name}";
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

                var renameBtn = new MenuItem("Rename Category", "Rename this category.")
                {
                    Enabled = !isUncategorized
                };
                var descriptionBtn = new MenuItem("Change Category Description", "Change this category's description.")
                {
                    Enabled = !isUncategorized
                };
                var iconBtn = new MenuDynamicListItem("Change Category Icon", iconNames[(int)currentCategory.Icon], new MenuDynamicListItem.ChangeItemCallback(ChangeCallback), "Change this category's icon. Select to save.")
                {
                    Enabled = !isUncategorized,
                    RightIcon = currentCategory.Icon
                };
                var deleteBtn = new MenuItem("Delete Category", "Delete this category. This can not be undone!")
                {
                    RightIcon = MenuItem.Icon.WARNING,
                    Enabled = !isUncategorized
                };
                var deleteCharsBtn = new MenuCheckboxItem("Delete All Vehicles", "If checked, when \"Delete Category\" is pressed, all the saved vehicles in this category will be deleted as well. If not checked, saved vehicles will be moved to \"Uncategorized\".")
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
                        string buttonDescription = "Manage this saved vehicle.";

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
                        var name = await GetUserInput(windowTitle: "Enter a new category name", defaultText: currentCategory.Name, maxInputLength: 30);

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

                            Notify.Success($"Your category has been renamed to ~g~<C>{name}</C>~s~. {updatedCount}/{totalCount} vehicles updated.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("Something went wrong while renaming your category, your old category will NOT be deleted because of this.");
                        }
                        break;

                    // Change Category Description
                    case 1:
                        var description = await GetUserInput(windowTitle: "Enter a new category description", defaultText: currentCategory.Description, maxInputLength: 120);

                        currentCategory.Description = description;

                        if (StorageManager.SaveJsonData("saved_veh_category_" + currentCategory.Name, JsonConvert.SerializeObject(currentCategory), true))
                        {
                            Notify.Success($"Your category description has been changed.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            vehicleCategoryMenu.OpenMenu();
                        }
                        else
                        {
                            Notify.Error("Something went wrong while changing your category description.");
                        }
                        break;

                    // Delete Category
                    case 3:
                        if (item.Label == "Are you sure?")
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
                                        vehicle.Category = "Uncategorized";

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
                            item.Label = "Are you sure?";
                        }
                        break;

                    // Load saved vehicle menu
                    default:
                        List<string> categoryNames = GetAllCategoryNames();
                        List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
                        int nameIndex = categoryNames.IndexOf(currentCategory.Name);

                        setCategoryBtn.ItemData = categoryIcons;
                        setCategoryBtn.ListItems = categoryNames;
                        setCategoryBtn.ListIndex = nameIndex == 1 ? 0 : nameIndex;
                        setCategoryBtn.RightIcon = categoryIcons[setCategoryBtn.ListIndex];

                        var vehInfo = item.ItemData;
                        selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
                        currentlySelectedVehicle = vehInfo;
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
                    Notify.Success($"Your category icon been changed to ~g~<C>{iconNames[iconIndex]}</C>~s~.");
                    UpdateSavedVehicleCategoriesMenu();
                }
                else
                {
                    Notify.Error("Something went wrong while changing your category icon.");
                }
            };

            var spawnVehicle = new MenuItem("Spawn Vehicle");
            var renameVehicle = new MenuItem("Rename Vehicle", "Rename your saved vehicle.");
            var replaceVehicle = new MenuItem("~r~Replace Vehicle", "Your saved vehicle will be replaced with the vehicle you are currently sitting in. ~r~Warning: this can NOT be undone!");
            var deleteVehicle = new MenuItem("~r~Delete Vehicle", "~r~This will delete your saved vehicle. Warning: this can NOT be undone!");
            selectedVehicleMenu.AddMenuItem(spawnVehicle);
            selectedVehicleMenu.AddMenuItem(renameVehicle);
            selectedVehicleMenu.AddMenuItem(setCategoryBtn);
            selectedVehicleMenu.AddMenuItem(replaceVehicle);
            selectedVehicleMenu.AddMenuItem(deleteVehicle);

            selectedVehicleMenu.OnMenuOpen += (sender) =>
            {
                bool vehicleModelExists = IsModelInCdimage(currentlySelectedVehicle.Value.model);

                spawnVehicle.Enabled = vehicleModelExists;
                spawnVehicle.Description = vehicleModelExists ? "Spawn this saved vehicle." : "This model could not be found in the game files. Most likely because this is an addon vehicle and it's currently not streamed by the server.";

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
                    var newName = await GetUserInput(windowTitle: "Enter a new name for this vehicle.", maxInputLength: 30);
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
                            Notify.Success("Your vehicle has successfully been renamed.");
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>(); // clear the old info
                        }
                        else
                        {
                            Notify.Error("This name is already in use or something unknown failed. Contact the server owner if you believe something is wrong.");
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
                            item.Label = "Press again to confirm.";
                            Notify.Alert("Are you sure you want to replace this vehicle? Press the button again to confirm.");
                        }
                        else
                        {
                            replaceButtonPressedCount = 0;
                            item.Label = "";
                            SaveVehicle(currentlySelectedVehicle.Key.Substring(4), currentlySelectedVehicle.Value.Category);
                            selectedVehicleMenu.CloseMenu();
                            Notify.Success("Your saved vehicle has been replaced with your current vehicle.");
                        }
                    }
                    else
                    {
                        Notify.Error("You need to be in a vehicle before you can replace your old vehicle.");
                    }
                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.Label = "Press again to confirm.";
                        Notify.Alert("Are you sure you want to delete this vehicle? Press the button again to confirm.");
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.Label = "";
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success("Your saved vehicle has been deleted.");
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
            selectedVehicleMenu.OnListIndexChange += (_, listItem, _, newSelectionIndex, _) => listItem.RightIcon = listItem.ItemData[newSelectionIndex];

            // Update vehicle's category
            selectedVehicleMenu.OnListItemSelect += async (_, listItem, listIndex, _) =>
            {
                string name = listItem.ListItems[listIndex];

                if (name == "Create New")
                {
                    var newName = await GetUserInput(windowTitle: "Enter a category name.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName) || newName.ToLower() == "uncategorized" || newName.ToLower() == "create new")
                    {
                        Notify.Error(CommonErrors.InvalidInput);
                        return;
                    }
                    else
                    {
                        var description = await GetUserInput(windowTitle: "Enter a category description (optional).", maxInputLength: 120);
                        var newCategory = new SavedVehicleCategory
                        {
                            Name = newName,
                            Description = description
                        };

                        if (StorageManager.SaveJsonData("saved_veh_category_" + newName, JsonConvert.SerializeObject(newCategory), false))
                        {
                            Notify.Success($"Your category (~g~<C>{newName}</C>~s~) has been saved.");
                            Log($"Saved Category {newName}.");
                            MenuController.CloseAllMenus();
                            UpdateSavedVehicleCategoriesMenu();
                            savedVehiclesCategoryMenu.OpenMenu();

                            currentCategory = newCategory;
                            name = newName;
                        }
                        else
                        {
                            Notify.Error($"Saving failed, most likely because this name (~y~<C>{newName}</C>~s~) is already in use.");
                            return;
                        }
                    }
                }

                VehicleInfo vehicle = currentlySelectedVehicle.Value;

                vehicle.Category = name;

                if (StorageManager.SaveVehicleInfo(currentlySelectedVehicle.Key, vehicle, true))
                {
                    Notify.Success("Your vehicle was saved successfully.");
                }
                else
                {
                    Notify.Error("Your vehicle could not be saved. Reason unknown. :(");
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
                            if (item.Label == "~r~Are you sure?")
                            {
                                Log("Unavailable saved vehicle deleted, data: " + JsonConvert.SerializeObject(sd));
                                DeleteResourceKvp(sd.Key);
                                unavailableVehiclesMenu.GoBack();
                                UpdateMenuAvailableCategories();
                            }
                            else
                            {
                                item.Label = "~r~Are you sure?";
                            }
                        }
                        else
                        {
                            Notify.Error("Somehow this vehicle could not be found.");
                        }
                    }
                    else
                    {
                        Notify.Error("You somehow managed to trigger deletion of a menu item that doesn't exist, how...?");
                    }
                }
                else
                {
                    Notify.Error("There are currrently no unavailable vehicles to delete!");
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
            savedVehicleTypeMenu = new("Saved Vehicles", "Select from class or custom category");

            var saveVehicle = new MenuItem("Save Current Vehicle", "Save the vehicle you are currently sitting in.")
            {
                LeftIcon = MenuItem.Icon.CAR
            };
            var classButton = new MenuItem("Vehicle Class", "Selected a saved vehicle by its class.")
            {
                Label = "→→→"
            };
            var categoryButton = new MenuItem("Vehicle Category", "Selected a saved vehicle by its custom category.")
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
                        Notify.Error("You are currently not in any vehicle. Please enter a vehicle before trying to save it.");
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
            var vehInfo = selectedItem.ItemData;
            List<string> categoryNames = GetAllCategoryNames();
            List<MenuItem.Icon> categoryIcons = GetCategoryIcons(categoryNames);
            setCategoryBtn.ItemData = categoryIcons;
            setCategoryBtn.ListItems = categoryNames;
            setCategoryBtn.ListIndex = 0;
            setCategoryBtn.RightIcon = categoryIcons[0];
            selectedVehicleMenu.MenuSubtitle = $"{vehInfo.Key.Substring(4)} ({vehInfo.Value.name})";
            currentlySelectedVehicle = vehInfo;
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
                    GetClassMenu().GetMenuItems()[i].Description = $"All saved vehicles from the {GetClassMenu().GetMenuItems()[i].Text} category.";
                }
                else
                {
                    GetClassMenu().GetMenuItems()[i].Label = "";
                    GetClassMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.LOCK;
                    GetClassMenu().GetMenuItems()[i].Enabled = false;
                    GetClassMenu().GetMenuItems()[i].Description = $"You do not have any saved vehicles that belong to the {GetClassMenu().GetMenuItems()[i].Text} category.";
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

                    var savedVehicleBtn = new MenuItem(sv.Key.Substring(4), $"Manage this saved vehicle.")
                    {
                        Label = $"({sv.Value.name}) →→→",
                        ItemData = sv
                    };
                    menu.AddMenuItem(savedVehicleBtn);
                }
                else
                {
                    var missingVehItem = new MenuItem(sv.Key.Substring(4), "This model could not be found in the game files. Most likely because this is an addon vehicle and it's currently not streamed by the server.")
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

            var createCategoryBtn = new MenuItem("Create Category", "Create a new vehicle category.")
            {
                Label = "→→→"
            };
            vehicleCategoryMenu.AddMenuItem(createCategoryBtn);

            var spacer = GetSpacerMenuItem("↓ Vehicle Categories ↓");
            vehicleCategoryMenu.AddMenuItem(spacer);

            var uncategorized = new SavedVehicleCategory
            {
                Name = "Uncategorized",
                Description = "All saved vehicles that have not been assigned to a category."
            };
            var uncategorizedBtn = new MenuItem(uncategorized.Name, uncategorized.Description)
            {
                Label = "→→→",
                ItemData = uncategorized
            };
            vehicleCategoryMenu.AddMenuItem(uncategorizedBtn);
            MenuController.BindMenuItem(vehicleCategoryMenu, savedVehiclesCategoryMenu, uncategorizedBtn);

            // Remove "Create New" and "Uncategorized"
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

            categories.Insert(0, "Create New");
            categories.Insert(1, "Uncategorized");

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
