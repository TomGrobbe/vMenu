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
        private CommonFunctions cf = MainMenu.Cf;

        public bool ShowSpeedoKmh { get; private set; } = UserDefaults.MiscSpeedKmh;
        public bool ShowSpeedoMph { get; private set; } = UserDefaults.MiscSpeedMph;
        public bool ShowCoordinates { get; private set; } = false;
        public bool HideHud { get; private set; } = false;
        public bool HideRadar { get; private set; } = false;
        public bool ShowLocation { get; private set; } = UserDefaults.MiscShowLocation;
        public bool DeathNotifications { get; private set; } = UserDefaults.MiscDeathNotifications;
        public bool JoinQuitNotifications { get; private set; } = UserDefaults.MiscJoinQuitNotifications;
        public bool LockCameraX { get; private set; } = false;
        public bool LockCameraY { get; private set; } = false;
        public bool ShowLocationBlips { get; private set; } = UserDefaults.MiscLocationBlips;
        public bool ShowPlayerBlips { get; private set; } = UserDefaults.MiscShowPlayerBlips;

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
            UIMenuCheckboxItem nightVision = new UIMenuCheckboxItem("Toggle Night Vision", false, "Enable or disable night vision.");
            UIMenuCheckboxItem thermalVision = new UIMenuCheckboxItem("Toggle Thermal Vision", false, "Enable or disable thermal vision.");

            UIMenuItem clearArea = new UIMenuItem("Clear Area", "Clears the area around your player (100 meters) of everything! Damage, dirt, peds, props, vehicles, etc. Everything gets cleaned up and reset.");
            UIMenuCheckboxItem lockCamX = new UIMenuCheckboxItem("Lock Camera Horizontal Rotation", false, "Locks your camera horizontal rotation. Could be useful in helicopters I guess.");
            UIMenuCheckboxItem lockCamY = new UIMenuCheckboxItem("Lock Camera Vertical Rotation", false, "Locks your camera vertical rotation. Could be useful in helicopters I guess.");

            UIMenu connectionSubmenu = new UIMenu(GetPlayerName(PlayerId()), "Connection Options", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenuItem connectionSubmenuBtn = new UIMenuItem("Connection Options", "Server connection/game quit options.");
            UIMenuItem quitSession = new UIMenuItem("Quit Session", "Leaves you connected to the server, but quits the network session. " +
                "Use this if you need to have addons streamed but want to use the rockstar editor.");
            UIMenuItem quitGame = new UIMenuItem("Quit Game", "Exits the game after 5 seconds.");
            UIMenuItem disconnectFromServer = new UIMenuItem("Disconnect From Server", "Disconnects you from the server and returns you to the serverlist. " +
                "~r~This feature is not recommended, quit the game instead for a better experience.");
            connectionSubmenu.AddItem(quitSession);
            connectionSubmenu.AddItem(quitGame);
            connectionSubmenu.AddItem(disconnectFromServer);

            UIMenuCheckboxItem locationBlips = new UIMenuCheckboxItem("Location Blips", ShowLocationBlips, "Shows blips on the map for some common locations.");
            UIMenuCheckboxItem playerBlips = new UIMenuCheckboxItem("Show Player Blips", ShowPlayerBlips, "Shows blips on the map for all players.");

            MainMenu.Mp.Add(connectionSubmenu);
            connectionSubmenu.RefreshIndex();
            connectionSubmenu.UpdateScaleform();
            menu.BindMenuToItem(connectionSubmenu, connectionSubmenuBtn);

            connectionSubmenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == quitGame)
                {
                    cf.QuitGame();
                }
                else if (item == quitSession)
                {
                    cf.QuitSession();
                }
                else if (item == disconnectFromServer)
                {
                    BaseScript.TriggerServerEvent("vMenu:DisconnectSelf");
                }
            };

            // Add menu items to the menu.
            if (cf.IsAllowed(Permission.MSTeleportToWp))
            {
                menu.AddItem(tptowp);
            }

            // Always allowed
            menu.AddItem(speedKmh);
            menu.AddItem(speedMph);
            if (cf.IsAllowed(Permission.MSConnectionMenu))
            {
                menu.AddItem(connectionSubmenuBtn);
            }
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
            //if (cf.IsAllowed(Permission.MSLocationBlips))
            //{
            //    menu.AddItem(locationBlips);
            //    if (!MainMenu.EnableExperimentalFeatures)
            //    {
            //        locationBlips.Enabled = false;
            //        locationBlips.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
            //        locationBlips.Description = "This experimental feature is not yet available, more details will be published on the forum thread soon.";
            //    }
            //}
            if (cf.IsAllowed(Permission.MSPlayerBlips))
            {
                menu.AddItem(playerBlips);
            }
            if (cf.IsAllowed(Permission.MSClearArea))
            {
                menu.AddItem(clearArea);
            }

            // Always allowed
            menu.AddItem(hideRadar);
            menu.AddItem(hideHud);
            menu.AddItem(lockCamX);
            menu.AddItem(lockCamY);
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
                    SetNightvision(_checked);
                }
                else if (item == thermalVision)
                {
                    SetSeethrough(_checked);
                }
                else if (item == lockCamX)
                {
                    LockCameraX = _checked;
                }
                else if (item == lockCamY)
                {
                    LockCameraY = _checked;
                }
                else if (item == locationBlips)
                {
                    ToggleBlips(_checked);
                    ShowLocationBlips = _checked;
                }
                else if (item == playerBlips)
                {
                    ShowPlayerBlips = _checked;
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
                // save settings
                else if (item == saveSettings)
                {
                    UserDefaults.SaveSettings();
                }
                // clear area
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

        private struct Blip
        {
            readonly Vector3 Location;
            readonly int Sprite;
            readonly string Name;
            readonly int Color;

            public Blip(Vector3 Location, int Sprite, string Name)
            {
                this.Location = Location;
                this.Sprite = Sprite;
                this.Name = Name;
                Color = 0;
            }

            public Blip(Vector3 Location, int Sprite, string Name, int Color)
            {
                this.Location = Location;
                this.Sprite = Sprite;
                this.Name = Name;
                this.Color = Color;
            }
        }

        //private readonly List<Blip> blips = new List<Blip>()
        //{
        //    // airports
        //    new Blip(new Vector3(-1089f, -2791f, 50f), 90, GetLabelText("BRS_MCL_0")), // LSIA
        //    new Blip(new Vector3(1728f, 3314f, 49f), 90, GetLabelText("VEX_LR_LOC1")), // sandy shores airfield
        //    new Blip(new Vector3(2132f, 4785f, 40f), 90, GetLabelText("SM_LOC_MCK")), // mckenzie field
        //    new Blip(new Vector3(-2198f, 2968f, 40f), 90, GetLabelText("PIM_MAGM206_1")), // fort zancudo

        //    // helicopter pad
        //    new Blip(new Vector3(-735f, -1455f, 4f), 370, GetLabelText("ACCNA_HELIPAD")), // Helipad
        //    // harbor / docks in La Puerta
        //    new Blip(new Vector3(-2198f, 2968f, 40f), 90, GetLabelText("PIM_MAGM206_1")), // fort zancudo


        //};

        private void ToggleBlips(bool enable)
        {
            if (enable)
            {

            }
        }

    }
}
