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
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Saved Vehicles")
            {
                MouseEdgeEnabled = false
            };

            UIMenuItem saveVeh = new UIMenuItem("Save Current Vehicle");
            menu.AddItem(saveVeh);
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveVeh)
                {
                    cf.SaveVehicle();
                }
            };

            UIMenu savedVehicles = new UIMenu("Saved Vehicles", "Select a vehicle to spawn");
            savedVehicles.ControlDisablingEnabled = false;
            savedVehicles.MouseControlsEnabled = false;
            savedVehicles.MouseEdgeEnabled = false;
            UIMenuItem savedVehiclesBtn = new UIMenuItem("Spawn Saved Vehicle", "Spawn a saved vehicle from the list.");
            menu.AddItem(savedVehiclesBtn);
            menu.BindMenuToItem(savedVehicles, savedVehiclesBtn);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == savedVehiclesBtn)
                {
                    savedVehicles.MenuItems.Clear();

                    SavedVehiclesDict = cf.GetSavedVehicleList();

                    foreach (KeyValuePair<string, Dictionary<string, string>> savedVehicle in SavedVehiclesDict)
                    {
                        UIMenuItem vehBtn = new UIMenuItem(savedVehicle.Key.Substring(4), "Click to spawn this saved vehicle.");
                        vehBtn.SetRightLabel($"({savedVehicle.Value["name"]})");
                        savedVehicles.AddItem(vehBtn);
                    }
                    savedVehicles.OnItemSelect += (sender2, item2, index2) =>
                    {
                        var vehInfo = SavedVehiclesDict["veh_" + item2.Text];
                        var model = vehInfo["model"];
                        cf.SpawnVehicle((uint)Int64.Parse(model), MainMenu.VehicleSpawnerMenu.SpawnInVehicle, MainMenu.VehicleSpawnerMenu.ReplaceVehicle);
                    };
                }
            };

            UIMenu deleteSavedVehicles = new UIMenu("Saved Vehicles", "~r~Delete saved vehicle");
            deleteSavedVehicles.ControlDisablingEnabled = false;
            deleteSavedVehicles.MouseControlsEnabled = false;
            deleteSavedVehicles.MouseEdgeEnabled = false;
            UIMenuItem deleteSavedVehiclesBtn = new UIMenuItem("~r~Delete Saved Vehicle", "~r~Delete a saved vehicle.");
            deleteSavedVehiclesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
            menu.AddItem(deleteSavedVehiclesBtn);
            menu.BindMenuToItem(deleteSavedVehicles, deleteSavedVehiclesBtn);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteSavedVehiclesBtn)
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



            MainMenu.Mp.Add(savedVehicles);
            MainMenu.Mp.Add(deleteSavedVehicles);

            menu.MouseControlsEnabled = false;

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
