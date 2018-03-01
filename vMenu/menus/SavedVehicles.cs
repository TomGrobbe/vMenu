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
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;
        private Dictionary<string, Dictionary<string, string>> SavedVehiclesDict;



        private void CreateMenu()
        {
            #region Create menus and submenus
            // Create the menu.
            menu = new UIMenu("Saved Vehicles", "Select an option", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            // Create submenus.
            UIMenu savedVehicles = new UIMenu("Saved Vehicles", "Spawn Saved Vehicle", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenu deleteSavedVehicles = new UIMenu("Saved Vehicles", "~r~Delete Saved Vehicle", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            #endregion

            #region Create, add and bind buttons to the menus.
            // Create menu buttons.
            UIMenuItem saveVeh = new UIMenuItem("~g~Save Vehicle", "Save your current vehicle.");
            UIMenuItem savedVehiclesBtn = new UIMenuItem("Spawn Vehicle", "Spawn a saved vehicle from the list.");
            savedVehiclesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Car);
            UIMenuItem deleteSavedVehiclesBtn = new UIMenuItem("~r~Delete Vehicle", "~r~Delete a saved vehicle.");
            deleteSavedVehiclesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);

            // Add buttons to the menu.
            menu.AddItem(saveVeh);
            menu.AddItem(savedVehiclesBtn);
            menu.AddItem(deleteSavedVehiclesBtn);

            // Bind submenus to menu items.
            menu.BindMenuToItem(savedVehicles, savedVehiclesBtn);
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
                    SavedVehiclesDict = cf.GetSavedVehicleList();

                    // Loop through all saved vehicles and create a button for it, then add that button to the submenu.
                    foreach (KeyValuePair<string, Dictionary<string, string>> savedVehicle in SavedVehiclesDict)
                    {
                        UIMenuItem vehBtn = new UIMenuItem(savedVehicle.Key.Substring(4), "Click to spawn this saved vehicle.");
                        vehBtn.SetRightLabel($"({savedVehicle.Value["name"]})");
                        savedVehicles.AddItem(vehBtn);
                    }

                    // When a vehicle is selected...
                    savedVehicles.OnItemSelect += (sender2, item2, index2) =>
                    {
                        // Get the vehicle info.
                        var vehInfo = SavedVehiclesDict["veh_" + item2.Text];

                        // Get the model hash.
                        var model = vehInfo["model"];

                        // Spawn a vehicle using the hash, and pass on the vehicleInfo dictionary containing all saved vehicle mods.
                        cf.SpawnVehicle((uint)Int64.Parse(model), MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle, vehicleInfo: vehInfo);
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

                    SavedVehiclesDict = cf.GetSavedVehicleList();

                    foreach (KeyValuePair<string, Dictionary<string, string>> savedVehicle in SavedVehiclesDict)
                    {
                        UIMenuItem vehBtn = new UIMenuItem(savedVehicle.Key.Substring(4), "Are you sure you want to delete this saved vehicle? This action cannot be undone!");
                        vehBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
                        vehBtn.SetRightLabel($"({savedVehicle.Value["name"]})");
                        deleteSavedVehicles.AddItem(vehBtn);
                    }
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
