using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient
{
    public class MiscSettings
    {
        // Variables
        private UIMenu menu;

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
        public bool ShowVehicleModelDimensions { get; private set; } = false;
        public bool RestorePlayerAppearance { get; private set; } = UserDefaults.MiscRestorePlayerAppearance;
        public bool RestorePlayerWeapons { get; private set; } = UserDefaults.MiscRestorePlayerWeapons;
        public bool DrawTimeOnScreen { get; internal set; } = UserDefaults.MiscShowTime;
        public bool MiscRightAlignMenu { get; private set; } = UserDefaults.MiscRightAlignMenu;


        private List<Vector3> tpLocations = new List<Vector3>();
        private List<float> tpLocationsHeading = new List<float>();

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(Game.Player.Name, "Misc Settings", RightAlignMenus());

            // teleport menu
            UIMenu teleportMenu = new UIMenu(Game.Player.Name, "Teleport Locations", RightAlignMenus());
            UIMenuItem teleportMenuBtn = new UIMenuItem("Teleport Locations", "Teleport to pre-configured locations, added by the server owner.");
            menu.BindMenuToItem(teleportMenu, teleportMenuBtn);
            MainMenu.Mp.Add(teleportMenu);

            // Create the menu items.
            UIMenuItem tptowp = new UIMenuItem("Teleport To Waypoint", "Teleport to the waypoint on your map.");
            UIMenuCheckboxItem rightAlignMenu = new UIMenuCheckboxItem("Right Align Menu", MiscRightAlignMenu, "If you want vMenu to appear on the left side of your screen, disable this option. It will be saved immediately, but it will only take affect after restarting your game.");
            UIMenuCheckboxItem speedKmh = new UIMenuCheckboxItem("Show Speed KM/H", ShowSpeedoKmh, "Show a speedometer on your screen indicating your speed in KM/h.");
            UIMenuCheckboxItem speedMph = new UIMenuCheckboxItem("Show Speed MPH", ShowSpeedoMph, "Show a speedometer on your screen indicating your speed in MPH.");
            UIMenuCheckboxItem coords = new UIMenuCheckboxItem("Show Coordinates", ShowCoordinates, "Show your current coordinates at the top of your screen.");
            UIMenuCheckboxItem hideRadar = new UIMenuCheckboxItem("Hide Radar", HideRadar, "Hide the radar/minimap.");
            UIMenuCheckboxItem hideHud = new UIMenuCheckboxItem("Hide Hud", HideHud, "Hide all hud elements.");
            UIMenuCheckboxItem showLocation = new UIMenuCheckboxItem("Location Display", ShowLocation, "Shows your current location and heading, as well as the nearest cross road. Just like PLD.");
            UIMenuCheckboxItem drawTime = new UIMenuCheckboxItem("Show Time On Screen", DrawTimeOnScreen, "Shows you the current time on screen.");
            UIMenuItem saveSettings = new UIMenuItem("Save Personal Settings", "Save your current settings. All saving is done on the client side, if you re-install windows you will lose your settings. Settings are shared across all servers using vMenu.");
            saveSettings.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
            UIMenuCheckboxItem joinQuitNotifs = new UIMenuCheckboxItem("Join / Quit Notifications", JoinQuitNotifications, "Receive notifications when someone joins or leaves the server.");
            UIMenuCheckboxItem deathNotifs = new UIMenuCheckboxItem("Death Notifications", DeathNotifications, "Receive notifications when someone dies or gets killed.");
            UIMenuCheckboxItem nightVision = new UIMenuCheckboxItem("Toggle Night Vision", false, "Enable or disable night vision.");
            UIMenuCheckboxItem thermalVision = new UIMenuCheckboxItem("Toggle Thermal Vision", false, "Enable or disable thermal vision.");
            UIMenuCheckboxItem modelDimensions = new UIMenuCheckboxItem("Show Vehicle Dimensions", ShowVehicleModelDimensions, "Draws lines for the model dimensions of your vehicle (debug function).");

            UIMenuItem clearArea = new UIMenuItem("Clear Area", "Clears the area around your player (100 meters) of everything! Damage, dirt, peds, props, vehicles, etc. Everything gets cleaned up and reset.");
            UIMenuCheckboxItem lockCamX = new UIMenuCheckboxItem("Lock Camera Horizontal Rotation", false, "Locks your camera horizontal rotation. Could be useful in helicopters I guess.");
            UIMenuCheckboxItem lockCamY = new UIMenuCheckboxItem("Lock Camera Vertical Rotation", false, "Locks your camera vertical rotation. Could be useful in helicopters I guess.");

            UIMenu connectionSubmenu = new UIMenu(GetPlayerName(Game.Player.Handle), "Connection Options", true);
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
            UIMenuCheckboxItem restorePlayerAppearance = new UIMenuCheckboxItem("Restore Player Appearance", RestorePlayerAppearance, "Restore your player's skin whenever you respawn after being dead. Re-joining a server will not restore your previous skin.");
            UIMenuCheckboxItem restorePlayerWeapons = new UIMenuCheckboxItem("Restore Player Weapons", RestorePlayerWeapons, "Restore your weapons whenever you respawn after being dead. Re-joining a server will not restore your previous weapons.");

            MainMenu.Mp.Add(connectionSubmenu);
            connectionSubmenu.RefreshIndex();
            connectionSubmenu.UpdateScaleform();
            menu.BindMenuToItem(connectionSubmenu, connectionSubmenuBtn);

            connectionSubmenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == quitGame)
                {
                    QuitGame();
                }
                else if (item == quitSession)
                {
                    QuitSession();
                }
                else if (item == disconnectFromServer)
                {
                    BaseScript.TriggerServerEvent("vMenu:DisconnectSelf");
                }
            };

            // Add menu items to the menu.
            if (IsAllowed(Permission.MSTeleportToWp))
            {
                menu.AddItem(tptowp);
            }

            // Always allowed
            menu.AddItem(rightAlignMenu);
            menu.AddItem(speedKmh);
            menu.AddItem(speedMph);
            menu.AddItem(modelDimensions);
            if (IsAllowed(Permission.MSConnectionMenu))
            {
                menu.AddItem(connectionSubmenuBtn);
                connectionSubmenuBtn.SetRightLabel("→→→");
            }
            if (IsAllowed(Permission.MSShowCoordinates))
            {
                menu.AddItem(coords);
            }
            if (IsAllowed(Permission.MSShowLocation))
            {
                menu.AddItem(showLocation);
            }
            menu.AddItem(drawTime); // always allowed
            if (IsAllowed(Permission.MSJoinQuitNotifs))
            {
                menu.AddItem(deathNotifs);
            }
            if (IsAllowed(Permission.MSDeathNotifs))
            {
                menu.AddItem(joinQuitNotifs);
            }
            if (IsAllowed(Permission.MSNightVision))
            {
                menu.AddItem(nightVision);
            }
            if (IsAllowed(Permission.MSThermalVision))
            {
                menu.AddItem(thermalVision);
            }
            if (IsAllowed(Permission.MSLocationBlips))
            {
                menu.AddItem(locationBlips);
                ToggleBlips(ShowLocationBlips);
            }
            if (IsAllowed(Permission.MSPlayerBlips))
            {
                menu.AddItem(playerBlips);
            }
            if (IsAllowed(Permission.MSTeleportLocations))
            {
                menu.AddItem(teleportMenuBtn);
                teleportMenuBtn.SetRightLabel("→→→");

                string json = LoadResourceFile(GetCurrentResourceName(), "config/locations.json");
                if (string.IsNullOrEmpty(json))
                {
                    Notify.Error("An error occurred while loading the locations file.");
                }
                else
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject data = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
                        foreach (Newtonsoft.Json.Linq.JToken teleport in data["teleports"])
                        {
                            string name = teleport["name"].ToString();
                            float heading = (float)teleport["heading"];
                            Vector3 coordinates = new Vector3((float)teleport["coordinates"]["x"], (float)teleport["coordinates"]["y"], (float)teleport["coordinates"]["z"]);
                            UIMenuItem tpBtn = new UIMenuItem(name, $"Teleport to X: {(int)coordinates.X} Y: {(int)coordinates.Y} Z: {(int)coordinates.Z} HEADING: {(int)heading}.");
                            teleportMenu.AddItem(tpBtn);
                            tpLocations.Add(coordinates);
                            tpLocationsHeading.Add(heading);
                        }
                        teleportMenu.OnItemSelect += async (sender, item, index) =>
                        {
                            await TeleportToCoords(tpLocations[index], true);
                            SetEntityHeading(Game.PlayerPed.Handle, tpLocationsHeading[index]);
                        };
                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.Write($"\n[vMenu] An error occurred whie loading the teleport locations!\nReport the following error details to the server owner:\n{ex.Message}.\n");
                    }
                }
            }
            if (IsAllowed(Permission.MSClearArea))
            {
                menu.AddItem(clearArea);
            }
            if (IsAllowed(Permission.MSRestoreAppearance))
            {
                menu.AddItem(restorePlayerAppearance);
            }
            if (IsAllowed(Permission.MSRestoreWeapons))
            {
                menu.AddItem(restorePlayerWeapons);
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
                if (item == rightAlignMenu)
                {
                    MiscRightAlignMenu = _checked;
                    UserDefaults.MiscRightAlignMenu = MiscRightAlignMenu;
                    Notify.Alert("You must restart your game before this option takes affect.");
                }
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
                    DisplayHud(!_checked);
                }
                else if (item == hideRadar)
                {
                    HideRadar = _checked;
                    if (!_checked)
                    {
                        DisplayRadar(true);
                    }
                }
                else if (item == showLocation)
                {
                    ShowLocation = _checked;
                }
                else if (item == drawTime)
                {
                    DrawTimeOnScreen = _checked;
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
                else if (item == restorePlayerAppearance)
                {
                    RestorePlayerAppearance = _checked;
                }
                else if (item == restorePlayerWeapons)
                {
                    RestorePlayerWeapons = _checked;
                }
                else if (item == modelDimensions)
                {
                    ShowVehicleModelDimensions = _checked;
                }
            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Teleport to waypoint.
                if (item == tptowp)
                {
                    TeleportToWp();
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
                    BaseScript.TriggerServerEvent("vMenu:ClearArea", pos.X, pos.Y, pos.Z);
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
            public readonly Vector3 Location;
            public readonly int Sprite;
            public readonly string Name;
            public readonly int Color;
            public readonly int blipID;

            public Blip(Vector3 Location, int Sprite, string Name, int Color, int blipID)
            {
                this.Location = Location;
                this.Sprite = Sprite;
                this.Name = Name;
                this.Color = Color;
                this.blipID = blipID;
            }
        }

        private List<Blip> blips = new List<Blip>();

        /// <summary>
        /// Toggles blips on/off.
        /// </summary>
        /// <param name="enable"></param>
        private void ToggleBlips(bool enable)
        {
            if (enable)
            {
                string json = LoadResourceFile(GetCurrentResourceName(), "config/locations.json");
                if (string.IsNullOrEmpty(json))
                {
                    Notify.Error("An error occurred while loading the locations file.");
                }
                else
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject data = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
                        if (data != null && data["blips"] != null && data["blips"].Count() > 0)
                        {
                            foreach (Newtonsoft.Json.Linq.JToken blip in data["blips"])
                            {
                                string name = blip["name"].ToString();
                                int color = (int)blip["color"];
                                int spriteID = (int)blip["spriteID"];
                                Vector3 coords = new Vector3((float)blip["coordinates"]["x"], (float)blip["coordinates"]["y"], (float)blip["coordinates"]["z"]);
                                int blipID = AddBlipForCoord(coords.X, coords.Y, coords.Z);
                                SetBlipSprite(blipID, spriteID);
                                BeginTextCommandSetBlipName("STRING");
                                AddTextComponentSubstringPlayerName(name);
                                EndTextCommandSetBlipName(blipID);
                                SetBlipColour(blipID, color);
                                SetBlipAsShortRange(blipID, true);


                                Blip b = new Blip(coords, spriteID, name, color, blipID);
                                blips.Add(b);
                            }
                        }

                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.Write($"\n\n[vMenu] An error occurred while loading the locations.json file. Please contact the server owner to resolve this.\nWhen contacting the owner, provide the following error details:\n{ex.Message}.\n\n\n");
                    }
                }
            }
            else
            {
                if (blips.Count > 0)
                {
                    foreach (Blip blip in blips)
                    {
                        int id = blip.blipID;
                        if (DoesBlipExist(id))
                        {
                            RemoveBlip(ref id);
                        }
                    }
                }
                blips.Clear();
            }
        }

    }
}
