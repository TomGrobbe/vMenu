using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class SavedVehicles
    {
        // Variables
        private UIMenu menu;
        private CommonFunctions cf = MainMenu.Cf;
        private Dictionary<string, CommonFunctions.VehicleInfo> SavedVehiclesDict;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            string menuTitle = "Saved Vehicles";
            #region Create menus and submenus
            // Create the menu.
            menu = new UIMenu(menuTitle, "Manage Saved Vehicles", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            // Create submenus.
            UIMenu spawnSavedVehicles = new UIMenu(menuTitle, "Spawn Saved Vehicle", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenu deleteSavedVehicles = new UIMenu(menuTitle, "~r~Delete Saved Vehicle", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            #endregion

            #region Create, add and bind buttons to the menus.
            // Create menu buttons.
            UIMenuItem saveVeh = new UIMenuItem("Save Current Vehicle", "Save the vehicle you are currently in.");
            saveVeh.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
            UIMenuItem savedVehiclesBtn = new UIMenuItem("Spawn Saved Vehicle", "Select a vehicle from your saved vehicles list.");
            savedVehiclesBtn.SetRightLabel("→→→");
            UIMenuItem deleteSavedVehiclesBtn = new UIMenuItem("~r~Delete Saved Vehicle", "~r~Delete ~s~a saved vehicle.");
            deleteSavedVehiclesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
            deleteSavedVehiclesBtn.SetRightLabel("→→→");

            // Add buttons to the menu.
            menu.AddItem(saveVeh);
            menu.AddItem(savedVehiclesBtn);
            menu.AddItem(deleteSavedVehiclesBtn);

            // Bind submenus to menu items.
            if (cf.IsAllowed(Permission.SVSpawn))
            {
                menu.BindMenuToItem(spawnSavedVehicles, savedVehiclesBtn);
            }
            else
            {
                savedVehiclesBtn.Enabled = false;
                savedVehiclesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                savedVehiclesBtn.Description = "This option has been disabled by the server owner.";
            }
            menu.BindMenuToItem(deleteSavedVehicles, deleteSavedVehiclesBtn);
            #endregion

            #region Button pressed events
            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Save the current vehicle.
                if (item == saveVeh)
                {
                    cf.SaveVehicle();
                }

                // Open and refresh the "spawn saved vehicle from list" submenu.
                else if (item == savedVehiclesBtn)
                {
                    // Remove all old items.
                    spawnSavedVehicles.MenuItems.Clear();

                    // Get all saved vehicles.
                    SavedVehiclesDict = cf.GetSavedVehicles();

                    // Loop through all saved vehicles and create a button for it, then add that button to the submenu.
                    foreach (KeyValuePair<string, CommonFunctions.VehicleInfo> savedVehicle in SavedVehiclesDict)
                    {
                        //MainMenu.Cf.Log(savedVehicle.ToString());
                        UIMenuItem vehBtn = new UIMenuItem(savedVehicle.Key.Substring(4), "Click to spawn this saved vehicle.");
                        vehBtn.SetRightLabel($"({savedVehicle.Value.name})");
                        spawnSavedVehicles.AddItem(vehBtn);
                        if (!IsModelInCdimage(savedVehicle.Value.model))
                        {
                            vehBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            vehBtn.Enabled = false;
                            vehBtn.Description = "This model is not available on this server, if this is an addon vehicle or DLC vehicle, please make sure " +
                            "that it's being streamed on this server.";
                        }
                        else
                        {
                            if (!VehicleSpawner.allowedCategories[GetVehicleClassFromName(savedVehicle.Value.model)])
                            {
                                vehBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                                vehBtn.Enabled = false;
                                vehBtn.Description = "This vehicle is not available on this server because you do not have permissions for this vehicle class.";
                            }
                        }
                    }

                    // Sort the menu items (case IN-sensitive) by name.
                    spawnSavedVehicles.MenuItems.Sort((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

                    // Refresh the index of the page.
                    spawnSavedVehicles.RefreshIndex();

                    // Update the scaleform.
                    spawnSavedVehicles.UpdateScaleform();
                }
                // Delete saved vehicle.
                else if (item == deleteSavedVehiclesBtn)
                {
                    deleteSavedVehicles.MenuItems.Clear();

                    // Get the dictionary containing all saved vehicles.
                    SavedVehiclesDict = cf.GetSavedVehicles();

                    // Loop through the list and add all saved vehicles to the menu. 
                    foreach (KeyValuePair<string, CommonFunctions.VehicleInfo> savedVehicle in SavedVehiclesDict)
                    {
                        //MainMenu.Cf.Log(savedVehicle.ToString());
                        UIMenuItem vehBtn = new UIMenuItem(savedVehicle.Key.Substring(4), "Are you sure you want to delete this saved vehicle? This action cannot be undone!");
                        vehBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
                        vehBtn.SetRightLabel($"({savedVehicle.Value.name})");
                        deleteSavedVehicles.AddItem(vehBtn);
                    }

                    // Sort the menu items (case IN-sensitive) by name.
                    deleteSavedVehicles.MenuItems.Sort((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));
                    deleteSavedVehicles.RefreshIndex();
                    deleteSavedVehicles.UpdateScaleform();
                }
            };
            #endregion

            #region Handle saved vehicles being pressed. (spawning)
            // When a vehicle is selected...
            spawnSavedVehicles.OnItemSelect += (sender2, item2, index2) =>
            {
                CommonFunctions.VehicleInfo vehInfo = SavedVehiclesDict["veh_" + item2.Text];

                // Spawn a vehicle using the hash, and pass on the vehicleInfo dictionary containing all saved vehicle mods.
                if (MainMenu.VehicleSpawnerMenu != null)
                {
                    cf.SpawnVehicle(vehInfo.model, MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, false, vehicleInfo: vehInfo, saveName: item2.Text);
                }
                else
                {
                    cf.SpawnVehicle(vehInfo.model, true, true, false, vehicleInfo: vehInfo, saveName: item2.Text);
                }

            };

            // Handle vehicle deletions
            deleteSavedVehicles.OnItemSelect += (sender2, item2, index2) =>
            {
                var vehDictName = "veh_" + item2.Text;
                new StorageManager().DeleteSavedStorageItem(vehDictName);
                deleteSavedVehicles.GoBack();
            };
            #endregion

            // Add the submenus to the menu pool.
            MainMenu.Mp.Add(spawnSavedVehicles);
            MainMenu.Mp.Add(deleteSavedVehicles);
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
