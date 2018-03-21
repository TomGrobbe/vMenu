﻿using System;
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
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.Cf;
        private Dictionary<string, Dictionary<string, string>> SavedVehiclesDict;


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
            UIMenu savedVehicles = new UIMenu(menuTitle, "Spawn Saved Vehicle", true)
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
                menu.BindMenuToItem(savedVehicles, savedVehiclesBtn);
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
                    savedVehicles.MenuItems.Clear();

                    // Get all saved vehicles.
                    SavedVehiclesDict = cf.GetSavedVehiclesDictionary();

                    // Loop through all saved vehicles and create a button for it, then add that button to the submenu.
                    foreach (KeyValuePair<string, Dictionary<string, string>> savedVehicle in SavedVehiclesDict)
                    {
                        UIMenuItem vehBtn = new UIMenuItem(savedVehicle.Key.Substring(4), "Click to spawn this saved vehicle.");
                        vehBtn.SetRightLabel($"({savedVehicle.Value["name"]})");
                        savedVehicles.AddItem(vehBtn);
                        if (!IsModelInCdimage((uint)Int64.Parse(savedVehicle.Value["model"])))
                        {
                            vehBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            vehBtn.Enabled = false;
                            vehBtn.Description = "This model is not available on this server, if this is an addon vehicle or DLC vehicle, please make sure " +
                            "that it's being streamed on this server.";
                        }
                    }

                    // Sort the menu items (case IN-sensitive) by name.
                    savedVehicles.MenuItems.Sort((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

                    // When a vehicle is selected...
                    savedVehicles.OnItemSelect += (sender2, item2, index2) =>
                    {
                        // Get the vehicle info.
                        var vehInfo = SavedVehiclesDict["veh_" + item2.Text];

                        // Get the model hash.
                        var model = vehInfo["model"];

                        // Spawn a vehicle using the hash, and pass on the vehicleInfo dictionary containing all saved vehicle mods.
                        cf.SpawnVehicle((uint)Int64.Parse(model), MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, vehicleInfo: vehInfo, saveName: item2.Text);
                    };

                    // Refresh the index of the page.
                    savedVehicles.RefreshIndex();
                    // Update the scaleform.
                    savedVehicles.UpdateScaleform();
                }
                // Delete saved vehicle.
                else if (item == deleteSavedVehiclesBtn)
                {
                    deleteSavedVehicles.MenuItems.Clear();

                    // Get the dictionary containing all saved vehicles.
                    SavedVehiclesDict = cf.GetSavedVehiclesDictionary();

                    // Loop through the list and add all saved vehicles to the menu. 
                    foreach (KeyValuePair<string, Dictionary<string, string>> savedVehicle in SavedVehiclesDict)
                    {
                        UIMenuItem vehBtn = new UIMenuItem(savedVehicle.Key.Substring(4), "Are you sure you want to delete this saved vehicle? This action cannot be undone!");
                        vehBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
                        vehBtn.SetRightLabel($"({savedVehicle.Value["name"]})");
                        deleteSavedVehicles.AddItem(vehBtn);
                    }

                    // Sort the menu items (case IN-sensitive) by name.
                    deleteSavedVehicles.MenuItems.Sort((pair1, pair2) => pair1.Text.ToString().ToLower().CompareTo(pair2.Text.ToString().ToLower()));

                    // Handle vehicle deletions
                    deleteSavedVehicles.OnItemSelect += (sender2, item2, index2) =>
                    {
                        var vehDictName = "veh_" + item2.Text;
                        new StorageManager().DeleteSavedDictionary(vehDictName);
                        deleteSavedVehicles.GoBack();
                    };
                }
            };
            #endregion

            // Add the submenus to the menu pool.
            MainMenu.Mp.Add(savedVehicles);
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
