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
    public class VehicleOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private UIMenu menu;
        private static Notification Notify = new Notification();
        private static Subtitles Subtitle = new Subtitles();

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = false;
        public bool VehicleEngineAlwaysOn { get; private set; } = false;
        public bool VehicleNoSiren { get; private set; } = false;
        public bool VehicleNoBikeHelemet { get; private set; } = false;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Vehicle Options", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseEdgeEnabled = false
            };

            // Create Checkboxes.
            UIMenuCheckboxItem vehicleGod = new UIMenuCheckboxItem("God Mode", VehicleGodMode, "Disables any type of visual or physical damage to your vehicle.");
            UIMenuCheckboxItem vehicleEngineAO = new UIMenuCheckboxItem("Engine Always On", VehicleEngineAlwaysOn, "Keeps your vehicle engine on when you exit your vehicle.");
            UIMenuCheckboxItem vehicleNoSiren = new UIMenuCheckboxItem("Disable Siren", VehicleNoSiren, "Disables your vehicle's siren. Only works if your vehicle actually has a siren.");
            UIMenuCheckboxItem vehicleNoBikeHelmet = new UIMenuCheckboxItem("No Bike Helmet", VehicleNoBikeHelemet, "No longer auto-equip a helmet when getting on a bike or quad.");

            // Create buttons.
            UIMenuItem fixVehicle = new UIMenuItem("Repair Vehicle", "Repair any visual and physical damage present on your vehicle.");
            UIMenuItem cleanVehicle = new UIMenuItem("Repair Vehicle", "Repair any visual and physical damage present on your vehicle.");
            UIMenuItem setLicensePlateText = new UIMenuItem("Set License Plate Text", "Enter a custom license plate for your vehicle.");

            // Create lists.
            var dirtlevel = new List<dynamic> {"Clean", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            UIMenuListItem setDirtLevel = new UIMenuListItem("Set Dirt Level", dirtlevel, 0, "Select how much dirt should be visible on your vehicle. This won't freeze the dirt level, it will only set it once.");
            var licensePlates = new List<dynamic> { GetLabelText("CMOD_PLA_1"), GetLabelText("CMOD_PLA_2"), GetLabelText("CMOD_PLA_3"), GetLabelText("CMOD_PLA_4"), "North Yankton" };
            UIMenuListItem setLicensePlateType = new UIMenuListItem("License Plate Type", licensePlates, 0, "Select a license plate type and press enter to apply it to your vehicle.");

            menu.AddItem(setLicensePlateType);

        }

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
