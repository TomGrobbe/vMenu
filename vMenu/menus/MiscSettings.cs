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
        private CommonFunctions cf = MainMenu.Cf;

        public bool ShowSpeedoKmh { get; private set; } = UserDefaults.MiscSpeedKmh;
        public bool ShowSpeedoMph { get; private set; } = UserDefaults.MiscSpeedMph;
        public bool ShowCoordinates { get; private set; } = false;
        public bool HideHud { get; private set; } = false;
        public bool HideRadar { get; private set; } = false;
        public bool ShowLocation { get; private set; } = UserDefaults.MiscShowLocation;
        public bool DeathNotifications { get; private set; } = UserDefaults.MiscDeathNotifications;
        public bool JoinQuitNotifications { get; private set; } = UserDefaults.MiscJoinQuitNotifications;
        public bool NightVision { get; private set; } = false;
        public bool ThermalVision { get; private set; } = false;

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
            UIMenuItem saveSettings = new UIMenuItem("Save Personal Settings", "Save your current settings. All saving is done on the client side, if you re-install windows you will lose your settings. Settings are shared across all servers using vMenu.");
            saveSettings.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
            UIMenuCheckboxItem joinQuitNotifs = new UIMenuCheckboxItem("Join / Quit Notifications", JoinQuitNotifications, "Receive notifications when someone joins or leaves the server.");
            UIMenuCheckboxItem deathNotifs = new UIMenuCheckboxItem("Death Notifications", DeathNotifications, "Receive notifications when someone dies or gets killed.");
            UIMenuCheckboxItem nightVision = new UIMenuCheckboxItem("Toggle Night Vision", NightVision, "Enable or disable night vision.");
            UIMenuCheckboxItem thermalVision = new UIMenuCheckboxItem("Toggle Thermal Vision", ThermalVision, "Enable or disable thermal vision.");

            UIMenuItem clearArea = new UIMenuItem("Clear Area", "Clears the area around your player (100 meters) of everything! Damage, dirt, peds, props, vehicles, etc. Everything gets cleaned up and reset.");

            // Add menu items to the menu.
            if (cf.IsAllowed(Permission.MSTeleportToWp))
            {
                menu.AddItem(tptowp);
            }

            // Always allowed
            menu.AddItem(speedKmh);
            menu.AddItem(speedMph);

            if (cf.IsAllowed(Permission.MSShowCoordinates))
            {
                menu.AddItem(coords);
            }
            if (cf.IsAllowed(Permission.MSShowLocation))
            {
                menu.AddItem(showLocation);
            }
            if (cf.IsAllowed(Permission.MSJoinQuitNotifs))
            {
                menu.AddItem(deathNotifs);
            }
            if (cf.IsAllowed(Permission.MSDeathNotifs))
            {
                menu.AddItem(joinQuitNotifs);
            }
            if (cf.IsAllowed(Permission.MSNightVision))
            {
                menu.AddItem(nightVision);
            }
            if (cf.IsAllowed(Permission.MSThermalVision))
            {
                menu.AddItem(thermalVision);
            }
            if (cf.IsAllowed(Permission.MSClearArea))
            {
                menu.AddItem(clearArea);
            }

            // Always allowed
            menu.AddItem(hideRadar);
            menu.AddItem(hideHud);
            menu.AddItem(saveSettings);

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
                else if (item == deathNotifs)
                {
                    DeathNotifications = _checked;
                }
                else if (item == joinQuitNotifs)
                {
                    JoinQuitNotifications = _checked;
                }
                else if (item == nightVision)
                {
                    //NightVision = _checked;
                    SetNightvision(_checked);
                }
                else if (item == thermalVision)
                {
                    //ThermalVision = _checked;
                    SetSeethrough(_checked);
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
                else if (item == saveSettings)
                {
                    UserDefaults.SaveSettingsAsync();
                }
                else if (item == clearArea)
                {
                    var pos = Game.PlayerPed.Position;
                    ClearAreaOfEverything(pos.X, pos.Y, pos.Z, 100f, false, false, false, false);
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
