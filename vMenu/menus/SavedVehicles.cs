﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class SavedVehicles
    {
        // Variables
        private Menu menu;
        private static LanguageManager LM = new LanguageManager();
        private Menu selectedVehicleMenu = new Menu(LM.Get("Manage Vehicle"), LM.Get("Manage this saved vehicle."));
        private Menu unavailableVehiclesMenu = new Menu(LM.Get("Missing Vehicles"), LM.Get("Unavailable Saved Vehicles"));
        private Dictionary<string, VehicleInfo> savedVehicles = new Dictionary<string, VehicleInfo>();
        private List<Menu> subMenus = new List<Menu>();
        private Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>> svMenuItems = new Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>>();
        private KeyValuePair<string, VehicleInfo> currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>();
        private int deleteButtonPressedCount = 0;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            string menuTitle = LM.Get("Saved Vehicles");
            #region Create menus and submenus
            // Create the menu.
            menu = new Menu(menuTitle, LM.Get("Manage Saved Vehicles"));

            MenuItem saveVehicle = new MenuItem(LM.Get("Save Current Vehicle"), LM.Get("Save the vehicle you are currently sitting in."));
            menu.AddMenuItem(saveVehicle);
            saveVehicle.LeftIcon = MenuItem.Icon.CAR;

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle();
                    }
                    else
                    {
                        Notify.Error(LM.Get("You are currently not in any vehicle. Please enter a vehicle before trying to save it."));
                    }
                }
            };

            for (int i = 0; i < 22; i++)
            {
                Menu categoryMenu = new Menu(LM.Get("Saved Vehicles"), GetLabelText($"VEH_CLASS_{i}"));

                MenuItem categoryButton = new MenuItem(GetLabelText($"VEH_CLASS_{i}"), $"All saved vehicles from the {(GetLabelText($"VEH_CLASS_{i}"))} category.");
                subMenus.Add(categoryMenu);
                MenuController.AddSubmenu(menu, categoryMenu);
                menu.AddMenuItem(categoryButton);
                categoryButton.Label = "→→→";
                MenuController.BindMenuItem(menu, categoryMenu, categoryButton);

                categoryMenu.OnMenuClose += (sender) =>
                {
                    UpdateMenuAvailableCategories();
                };

                categoryMenu.OnItemSelect += (sender, item, index) =>
                {
                    UpdateSelectedVehicleMenu(item, sender);
                };
            }

            MenuItem unavailableModels = new MenuItem(LM.Get("Unavailable Saved Vehicles"), LM.Get("These vehicles are currently unavailable because the models are not present in the game. These vehicles are most likely not being streamed from the server."))
            {
                Label = "→→→"
            };

            menu.AddMenuItem(unavailableModels);
            MenuController.BindMenuItem(menu, unavailableVehiclesMenu, unavailableModels);
            MenuController.AddSubmenu(menu, unavailableVehiclesMenu);


            MenuController.AddMenu(selectedVehicleMenu);
            MenuItem spawnVehicle = new MenuItem(LM.Get("Spawn Vehicle"), LM.Get("Spawn this saved vehicle."));
            MenuItem renameVehicle = new MenuItem(LM.Get("Rename Vehicle"), LM.Get("Rename your saved vehicle."));
            MenuItem replaceVehicle = new MenuItem(LM.Get("~r~Replace Vehicle"), LM.Get("Your saved vehicle will be replaced with the vehicle you are currently sitting in. ~r~Warning: this can NOT be undone!"));
            MenuItem deleteVehicle = new MenuItem(LM.Get("~r~Delete Vehicle"), LM.Get("~r~This will delete your saved vehicle. Warning: this can NOT be undone!"));
            selectedVehicleMenu.AddMenuItem(spawnVehicle);
            selectedVehicleMenu.AddMenuItem(renameVehicle);
            selectedVehicleMenu.AddMenuItem(replaceVehicle);
            selectedVehicleMenu.AddMenuItem(deleteVehicle);

            selectedVehicleMenu.OnMenuOpen += (sender) =>
            {
                spawnVehicle.Label = "(" + GetDisplayNameFromVehicleModel(currentlySelectedVehicle.Value.model).ToLower() + ")";
            };

            selectedVehicleMenu.OnMenuClose += (sender) =>
            {
                selectedVehicleMenu.RefreshIndex();
                deleteButtonPressedCount = 0;
                deleteVehicle.Label = "";
            };

            selectedVehicleMenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnVehicle)
                {
                    if (MainMenu.VehicleSpawnerMenu != null)
                    {
                        SpawnVehicle(currentlySelectedVehicle.Value.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                    else
                    {
                        SpawnVehicle(currentlySelectedVehicle.Value.model, true, true, false, vehicleInfo: currentlySelectedVehicle.Value, saveName: currentlySelectedVehicle.Key.Substring(4));
                    }
                }
                else if (item == renameVehicle)
                {
                    string newName = await GetUserInput(windowTitle: LM.Get("Enter a new name for this vehicle."), maxInputLength: 30);
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
                            Notify.Success(LM.Get("Your vehicle has successfully been renamed."));
                            UpdateMenuAvailableCategories();
                            selectedVehicleMenu.GoBack();
                            currentlySelectedVehicle = new KeyValuePair<string, VehicleInfo>(); // clear the old info
                        }
                        else
                        {
                            Notify.Error(LM.Get("This name is already in use or something unknown failed. Contact the server owner if you believe something is wrong."));
                        }
                    }
                }
                else if (item == replaceVehicle)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        SaveVehicle(currentlySelectedVehicle.Key.Substring(4));
                        selectedVehicleMenu.GoBack();
                        Notify.Success(LM.Get("Your saved vehicle has been replaced with your current vehicle."));
                    }
                    else
                    {
                        Notify.Error(LM.Get("You need to be in a vehicle before you can relplace your old vehicle."));
                    }
                }
                else if (item == deleteVehicle)
                {
                    if (deleteButtonPressedCount == 0)
                    {
                        deleteButtonPressedCount = 1;
                        item.Label = LM.Get("Press again to confirm.");
                        Notify.Alert(LM.Get("Are you sure you want to delete this vehicle? Press the button again to confirm."));
                    }
                    else
                    {
                        deleteButtonPressedCount = 0;
                        item.Label = "";
                        DeleteResourceKvp(currentlySelectedVehicle.Key);
                        UpdateMenuAvailableCategories();
                        selectedVehicleMenu.GoBack();
                        Notify.Success(LM.Get("Your saved vehicle has been deleted."));
                    }
                }
                if (item != deleteVehicle) // if any other button is pressed, restore the delete vehicle button pressed count.
                {
                    deleteButtonPressedCount = 0;
                    deleteVehicle.Label = "";
                }
            };
            unavailableVehiclesMenu.InstructionalButtons.Add(Control.FrontendDelete, LM.Get("Delete Vehicle!"));

            unavailableVehiclesMenu.ButtonPressHandlers.Add(new Menu.ButtonPressHandler(Control.FrontendDelete, Menu.ControlPressCheckType.JUST_RELEASED, new Action<Menu, Control>((m, c) =>
            {
                if (m.Size > 0)
                {
                    int index = m.CurrentIndex;
                    if (index < m.Size)
                    {
                        MenuItem item = m.GetMenuItems().Find(i => i.Index == index);
                        if (item != null && (item.ItemData is KeyValuePair<string, VehicleInfo> sd))
                        {
                            if (item.Label == LM.Get("~r~Are you sure?"))
                            {
                                Log("Unavailable saved vehicle deleted, data: " + JsonConvert.SerializeObject(sd));
                                DeleteResourceKvp(sd.Key);
                                unavailableVehiclesMenu.GoBack();
                                UpdateMenuAvailableCategories();
                            }
                            else
                            {
                                item.Label = LM.Get("~r~Are you sure?");
                            }
                        }
                        else
                        {
                            Notify.Error(LM.Get("Somehow this vehicle could not be found."));
                        }
                    }
                    else
                    {
                        Notify.Error(LM.Get("You somehow managed to trigger deletion of a menu item that doesn't exist, how...?"));
                    }
                }
                else
                {
                    Notify.Error(LM.Get("There are currrently no unavailable vehicles to delete!"));
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


        /// <summary>
        /// Updates the selected vehicle.
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns>A bool, true if successfull, false if unsuccessfull</returns>
        private bool UpdateSelectedVehicleMenu(MenuItem selectedItem, Menu parentMenu = null)
        {
            if (!svMenuItems.ContainsKey(selectedItem))
            {
                Notify.Error(LM.Get("In some very strange way, you've managed to select a button, that does not exist according to this list. So your vehicle could not be loaded. :( Maybe your save files are broken?"));
                return false;
            }
            var vehInfo = svMenuItems[selectedItem];
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
            svMenuItems = new Dictionary<MenuItem, KeyValuePair<string, VehicleInfo>>();

            for (int i = 1; i < GetMenu().Size - 1; i++)
            {
                if (savedVehicles.Any(a => GetVehicleClassFromName(a.Value.model) == i - 1 && IsModelInCdimage(a.Value.model)))
                {
                    GetMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.NONE;
                    GetMenu().GetMenuItems()[i].Label = "→→→";
                    GetMenu().GetMenuItems()[i].Enabled = true;
                    GetMenu().GetMenuItems()[i].Description = $"All saved vehicles from the {GetMenu().GetMenuItems()[i].Text} category.";
                }
                else
                {
                    GetMenu().GetMenuItems()[i].Label = "";
                    GetMenu().GetMenuItems()[i].RightIcon = MenuItem.Icon.LOCK;
                    GetMenu().GetMenuItems()[i].Enabled = false;
                    GetMenu().GetMenuItems()[i].Description = $"You do not have any saved vehicles that belong to the {GetMenu().GetMenuItems()[i].Text} category.";
                }
            }

            // Check if the items count will be changed. If there are less cars than there were before, one probably got deleted
            // so in that case we need to refresh the index of that menu just to be safe. If not, keep the index where it is for improved
            // usability of the menu.
            foreach (Menu m in subMenus)
            {
                int size = m.Size;
                int vclass = subMenus.IndexOf(m);

                int count = savedVehicles.Count(a => GetVehicleClassFromName(a.Value.model) == vclass);
                if (count < size)
                {
                    m.RefreshIndex();
                }
            }

            foreach (Menu m in subMenus)
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
                    int vclass = GetVehicleClassFromName(sv.Value.model);
                    Menu menu = subMenus[vclass];

                    MenuItem savedVehicleBtn = new MenuItem(sv.Key.Substring(4), $"Manage this saved vehicle.")
                    {
                        Label = $"({sv.Value.name}) →→→"
                    };
                    menu.AddMenuItem(savedVehicleBtn);

                    svMenuItems.Add(savedVehicleBtn, sv);
                }
                else
                {
                    MenuItem missingVehItem = new MenuItem(sv.Key.Substring(4), LM.Get("This model could not be found in the game files. Most likely because this is an addon vehicle and it's currently not streamed by the server."))
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
            foreach (Menu m in subMenus)
            {
                m.SortMenuItems((MenuItem A, MenuItem B) =>
                {
                    return A.Text.ToLower().CompareTo(B.Text.ToLower());
                });
            }
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
