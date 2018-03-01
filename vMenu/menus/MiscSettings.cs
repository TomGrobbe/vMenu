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
    public class MiscSettings
    {
        // Variables
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;

        public bool ShowSpeedoKmh { get; private set; } = UserDefaults.SpeedKmh;
        public bool ShowSpeedoMph { get; private set; } = UserDefaults.SpeedMph;
        public bool ShowCoordinates { get; private set; } = false;
        public bool HideHud { get; private set; } = false;
        public bool HideRadar { get; private set; } = false;
        public bool ShowLocation { get; private set; } = UserDefaults.ShowLocation;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Misc Settings", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            // Create the menu items.
            UIMenuItem tptowp = new UIMenuItem("Teleport To Waypoint", "Teleport to the waypoint on your map.");
            UIMenuCheckboxItem speedKmh = new UIMenuCheckboxItem("Show Speed KM/H", ShowSpeedoKmh, "Show a speedometer on your screen indicating your speed in KM/h.");
            UIMenuCheckboxItem speedMph = new UIMenuCheckboxItem("Show Speed MPH", ShowSpeedoMph, "Show a speedometer on your screen indicating your speed in MPH.");
            UIMenuCheckboxItem coords = new UIMenuCheckboxItem("Show Coordinates", ShowCoordinates, "Show your current coordinates at the top of your screen.");
            UIMenuCheckboxItem hideRadar = new UIMenuCheckboxItem("Hide Radar", HideRadar, "Hide the radar/minimap.");
            UIMenuCheckboxItem hideHud = new UIMenuCheckboxItem("Hide Hud", HideHud, "Hide all hud elements.");
            UIMenuCheckboxItem showLocation = new UIMenuCheckboxItem("Location Display", ShowLocation, "Shows your current location and heading, as well as the nearest cross road. Just like PLD.");

            // Add menu items to the menu.
            menu.AddItem(tptowp);
            menu.AddItem(speedKmh);
            menu.AddItem(speedMph);
            menu.AddItem(coords);
            menu.AddItem(hideRadar);
            menu.AddItem(hideHud);
            menu.AddItem(showLocation);

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                if (item == speedKmh)
                {
                    ShowSpeedoKmh = _checked;
                }
                else if (item == speedMph)
                {
                    ShowSpeedoMph = _checked;
                }
                else if (item == coords)
                {
                    ShowCoordinates = _checked;
                }
                else if (item == hideHud)
                {
                    HideHud = _checked;
                }
                else if (item == hideRadar)
                {
                    HideRadar = _checked;
                }
                else if (item == showLocation)
                {
                    ShowLocation = _checked;
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Teleport to waypoint.
                if (item == tptowp)
                {
                    cf.TeleportToWp();
                }
            };
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
