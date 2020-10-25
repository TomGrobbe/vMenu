using System;
using System.Collections.Generic;
using System.Linq;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class MiscSettings
    {
        // Variables
        private Menu menu;
        private Menu teleportOptionsMenu;
        private Menu developerToolsMenu;

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
        public bool MiscShowOverheadNames { get; private set; } = UserDefaults.MiscShowOverheadNames;
        public bool ShowVehicleModelDimensions { get; private set; } = false;
        public bool ShowPedModelDimensions { get; private set; } = false;
        public bool ShowPropModelDimensions { get; private set; } = false;
        public bool ShowEntityHandles { get; private set; } = false;
        public bool ShowEntityModels { get; private set; } = false;
        public bool ShowEntityNetOwners { get; private set; } = false;
        public bool MiscRespawnDefaultCharacter { get; private set; } = UserDefaults.MiscRespawnDefaultCharacter;
        public bool RestorePlayerAppearance { get; private set; } = UserDefaults.MiscRestorePlayerAppearance;
        public bool RestorePlayerWeapons { get; private set; } = UserDefaults.MiscRestorePlayerWeapons;
        public bool DrawTimeOnScreen { get; internal set; } = UserDefaults.MiscShowTime;
        public bool MiscRightAlignMenu { get; private set; } = UserDefaults.MiscRightAlignMenu;
        public bool MiscDisablePrivateMessages { get; private set; } = UserDefaults.MiscDisablePrivateMessages;
        public bool MiscDisableControllerSupport { get; private set; } = UserDefaults.MiscDisableControllerSupport;

        internal bool TimecycleEnabled { get; private set; } = false;
        internal int LastTimeCycleModifierIndex { get; private set; } = UserDefaults.MiscLastTimeCycleModifierIndex;
        internal int LastTimeCycleModifierStrength { get; private set; } = UserDefaults.MiscLastTimeCycleModifierStrength;


        // keybind states
        public bool KbTpToWaypoint { get; private set; } = UserDefaults.KbTpToWaypoint;
        public int KbTpToWaypointKey { get; } = vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key) != -1
            ? vMenuShared.ConfigManager.GetSettingsInt(vMenuShared.ConfigManager.Setting.vmenu_teleport_to_wp_keybind_key)
            : 168; // 168 (F7 by default)
        public bool KbDriftMode { get; private set; } = UserDefaults.KbDriftMode;
        public bool KbRecordKeys { get; private set; } = UserDefaults.KbRecordKeys;
        public bool KbRadarKeys { get; private set; } = UserDefaults.KbRadarKeys;
        public bool KbPointKeys { get; private set; } = UserDefaults.KbPointKeys;

        internal static List<vMenuShared.ConfigManager.TeleportLocation> TpLocations = new List<vMenuShared.ConfigManager.TeleportLocation>();

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            MenuController.MenuAlignment = MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
            if (MenuController.MenuAlignment != (MiscRightAlignMenu ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
            {
                Notify.Error(CommonErrors.RightAlignedNotSupported);

                // (re)set the default to left just in case so they don't get this error again in the future.
                MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                MiscRightAlignMenu = false;
                UserDefaults.MiscRightAlignMenu = false;
            }

            // Create the menu.
            menu = new Menu(Game.Player.Name, "Misc Settings");
            teleportOptionsMenu = new Menu(Game.Player.Name, "Teleport Options");
            developerToolsMenu = new Menu(Game.Player.Name, "Development Tools");

            // teleport menu
            Menu teleportMenu = new Menu(Game.Player.Name, "Teleport Locations");
            MenuItem teleportMenuBtn = new MenuItem("Teleport Locations", "Teleport to pre-configured locations, added by the server owner.");
            MenuController.AddSubmenu(menu, teleportMenu);
            MenuController.BindMenuItem(menu, teleportMenu, teleportMenuBtn);

            // keybind settings menu
            Menu keybindMenu = new Menu(Game.Player.Name, "Keybind Settings");
            MenuItem keybindMenuBtn = new MenuItem("Keybind Settings", "Enable or disable keybinds for some options.");
            MenuController.AddSubmenu(menu, keybindMenu);
            MenuController.BindMenuItem(menu, keybindMenu, keybindMenuBtn);

            // keybind settings menu items
            MenuCheckboxItem kbTpToWaypoint = new MenuCheckboxItem("Teleport To Waypoint", "Teleport to your waypoint when pressing the keybind. By default, this keybind is set to ~r~F7~s~, server owners are able to change this however so ask them if you don't know what it is.", KbTpToWaypoint);
            MenuCheckboxItem kbDriftMode = new MenuCheckboxItem("Drift Mode", "Makes your vehicle have almost no traction while holding left shift on keyboard, or X on controller.", KbDriftMode);
            MenuCheckboxItem kbRecordKeys = new MenuCheckboxItem("Recording Controls", "Enables or disables the recording (gameplay recording for the Rockstar editor) hotkeys on both keyboard and controller.", KbRecordKeys);
            MenuCheckboxItem kbRadarKeys = new MenuCheckboxItem("Minimap Controls", "Press the Multiplayer Info (z on keyboard, down arrow on controller) key to switch between expanded radar and normal radar.", KbRadarKeys);
            MenuCheckboxItem kbPointKeysCheckbox = new MenuCheckboxItem("Finger Point Controls", "Enables the finger point toggle key. The default QWERTY keyboard mapping for this is 'B', or for controller quickly double tap the right analog stick.", KbPointKeys);
            MenuItem backBtn = new MenuItem("Back");

            // Create the menu items.
            MenuCheckboxItem rightAlignMenu = new MenuCheckboxItem("Right Align Menu", "If you want vMenu to appear on the left side of your screen, disable this option. This option will be saved immediately. You don't need to click save preferences.", MiscRightAlignMenu);
            MenuCheckboxItem disablePms = new MenuCheckboxItem("Disable Private Messages", "Prevent others from sending you a private message via the Online Players menu. This also prevents you from sending messages to other players.", MiscDisablePrivateMessages);
            MenuCheckboxItem disableControllerKey = new MenuCheckboxItem("Disable Controller Support", "This disables the controller menu toggle key. This does NOT disable the navigation buttons.", MiscDisableControllerSupport);
            MenuCheckboxItem speedKmh = new MenuCheckboxItem("Show Speed KM/H", "Show a speedometer on your screen indicating your speed in KM/h.", ShowSpeedoKmh);
            MenuCheckboxItem speedMph = new MenuCheckboxItem("Show Speed MPH", "Show a speedometer on your screen indicating your speed in MPH.", ShowSpeedoMph);
            MenuCheckboxItem coords = new MenuCheckboxItem("Show Coordinates", "Show your current coordinates at the top of your screen.", ShowCoordinates);
            MenuCheckboxItem hideRadar = new MenuCheckboxItem("Hide Radar", "Hide the radar/minimap.", HideRadar);
            MenuCheckboxItem hideHud = new MenuCheckboxItem("Hide Hud", "Hide all hud elements.", HideHud);
            MenuCheckboxItem showLocation = new MenuCheckboxItem("Location Display", "Shows your current location and heading, as well as the nearest cross road. Similar like PLD. ~r~Warning: This feature (can) take(s) up to -4.6 FPS when running at 60 Hz.", ShowLocation) { LeftIcon = MenuItem.Icon.WARNING };
            MenuCheckboxItem drawTime = new MenuCheckboxItem("Show Time On Screen", "Shows you the current time on screen.", DrawTimeOnScreen);
            MenuItem saveSettings = new MenuItem("Save Personal Settings", "Save your current settings. All saving is done on the client side, if you re-install windows you will lose your settings. Settings are shared across all servers using vMenu.")
            {
                RightIcon = MenuItem.Icon.TICK
            };
            MenuItem exportData = new MenuItem("Export/Import Data", "Coming soon (TM): the ability to import and export your saved data.");
            MenuCheckboxItem joinQuitNotifs = new MenuCheckboxItem("Join / Quit Notifications", "Receive notifications when someone joins or leaves the server.", JoinQuitNotifications);
            MenuCheckboxItem deathNotifs = new MenuCheckboxItem("Death Notifications", "Receive notifications when someone dies or gets killed.", DeathNotifications);
            MenuCheckboxItem nightVision = new MenuCheckboxItem("Toggle Night Vision", "Enable or disable night vision.", false);
            MenuCheckboxItem thermalVision = new MenuCheckboxItem("Toggle Thermal Vision", "Enable or disable thermal vision.", false);
            MenuCheckboxItem vehModelDimensions = new MenuCheckboxItem("Show Vehicle Dimensions", "Draws the model outlines for every vehicle that's currently close to you.", ShowVehicleModelDimensions);
            MenuCheckboxItem propModelDimensions = new MenuCheckboxItem("Show Prop Dimensions", "Draws the model outlines for every prop that's currently close to you.", ShowPropModelDimensions);
            MenuCheckboxItem pedModelDimensions = new MenuCheckboxItem("Show Ped Dimensions", "Draws the model outlines for every ped that's currently close to you.", ShowPedModelDimensions);
            MenuCheckboxItem showEntityHandles = new MenuCheckboxItem("Show Entity Handles", "Draws the the entity handles for all close entities (you must enable the outline functions above for this to work).", ShowEntityHandles);
            MenuCheckboxItem showEntityModels = new MenuCheckboxItem("Show Entity Models", "Draws the the entity models for all close entities (you must enable the outline functions above for this to work).", ShowEntityModels);
            MenuCheckboxItem showEntityNetOwners = new MenuCheckboxItem("Show Network Owners", "Draws the the entity net owner for all close entities (you must enable the outline functions above for this to work).", ShowEntityNetOwners);
            MenuSliderItem dimensionsDistanceSlider = new MenuSliderItem("Show Dimensions Radius", "Show entity model/handle/dimension draw range.", 0, 20, 20, false);

            MenuItem clearArea = new MenuItem("Clear Area", "Clears the area around your player (100 meters). Damage, dirt, peds, props, vehicles, etc. Everything gets cleaned up, fixed and reset to the default world state.");
            MenuCheckboxItem lockCamX = new MenuCheckboxItem("Lock Camera Horizontal Rotation", "Locks your camera horizontal rotation. Could be useful in helicopters I guess.", false);
            MenuCheckboxItem lockCamY = new MenuCheckboxItem("Lock Camera Vertical Rotation", "Locks your camera vertical rotation. Could be useful in helicopters I guess.", false);


            Menu connectionSubmenu = new Menu(Game.Player.Name, "Connection Options");
            MenuItem connectionSubmenuBtn = new MenuItem("Connection Options", "Server connection/game quit options.");

            MenuItem quitSession = new MenuItem("Quit Session", "Leaves you connected to the server, but quits the network session. ~r~Can not be used when you are the host.");
            MenuItem rejoinSession = new MenuItem("Re-join Session", "This may not work in all cases, but you can try to use this if you want to re-join the previous session after clicking 'Quit Session'.");
            MenuItem quitGame = new MenuItem("Quit Game", "Exits the game after 5 seconds.");
            MenuItem disconnectFromServer = new MenuItem("Disconnect From Server", "Disconnects you from the server and returns you to the serverlist. ~r~This feature is not recommended, quit the game completely instead and restart it for a better experience.");
            connectionSubmenu.AddMenuItem(quitSession);
            connectionSubmenu.AddMenuItem(rejoinSession);
            connectionSubmenu.AddMenuItem(quitGame);
            connectionSubmenu.AddMenuItem(disconnectFromServer);

            MenuCheckboxItem enableTimeCycle = new MenuCheckboxItem("Enable Timecycle Modifier", "Enable or disable the timecycle modifier from the list below.", TimecycleEnabled);
            List<string> timeCycleModifiersListData = TimeCycles.Timecycles.ToList();
            for (var i = 0; i < timeCycleModifiersListData.Count; i++)
            {
                timeCycleModifiersListData[i] += $" ({i + 1}/{timeCycleModifiersListData.Count})";
            }
            MenuListItem timeCycles = new MenuListItem("TM", timeCycleModifiersListData, MathUtil.Clamp(LastTimeCycleModifierIndex, 0, Math.Max(0, timeCycleModifiersListData.Count - 1)), "Select a timecycle modifier and enable the checkbox above.");
            MenuSliderItem timeCycleIntensity = new MenuSliderItem("Timecycle Modifier Intensity", "Set the timecycle modifier intensity.", 0, 20, LastTimeCycleModifierStrength, true);

            MenuCheckboxItem locationBlips = new MenuCheckboxItem("Location Blips", "Shows blips on the map for some common locations.", ShowLocationBlips);
            MenuCheckboxItem playerBlips = new MenuCheckboxItem("Show Player Blips", "Shows blips on the map for all players.", ShowPlayerBlips);
            MenuCheckboxItem playerNames = new MenuCheckboxItem("Show Player Names", "Enables or disables player overhead names.", MiscShowOverheadNames);
            MenuCheckboxItem respawnDefaultCharacter = new MenuCheckboxItem("Respawn As Default MP Character", "If you enable this, then you will (re)spawn as your default saved MP character. Note the server owner can globally disable this option. To set your default character, go to one of your saved MP Characters and click the 'Set As Default Character' button.", MiscRespawnDefaultCharacter);
            MenuCheckboxItem restorePlayerAppearance = new MenuCheckboxItem("Restore Player Appearance", "Restore your player's skin whenever you respawn after being dead. Re-joining a server will not restore your previous skin.", RestorePlayerAppearance);
            MenuCheckboxItem restorePlayerWeapons = new MenuCheckboxItem("Restore Player Weapons", "Restore your weapons whenever you respawn after being dead. Re-joining a server will not restore your previous weapons.", RestorePlayerWeapons);

            MenuController.AddSubmenu(menu, connectionSubmenu);
            MenuController.BindMenuItem(menu, connectionSubmenu, connectionSubmenuBtn);

            keybindMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == kbTpToWaypoint)
                {
                    KbTpToWaypoint = _checked;
                }
                else if (item == kbDriftMode)
                {
                    KbDriftMode = _checked;
                }
                else if (item == kbRecordKeys)
                {
                    KbRecordKeys = _checked;
                }
                else if (item == kbRadarKeys)
                {
                    KbRadarKeys = _checked;
                }
                else if (item == kbPointKeysCheckbox)
                {
                    KbPointKeys = _checked;
                }
            };
            keybindMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == backBtn)
                {
                    keybindMenu.GoBack();
                }
            };

            connectionSubmenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == quitGame)
                {
                    QuitGame();
                }
                else if (item == quitSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        if (NetworkIsHost())
                        {
                            Notify.Error("Sorry, you cannot leave the session when you are the host. This would prevent other players from joining/staying on the server.");
                        }
                        else
                        {
                            QuitSession();
                        }
                    }
                    else
                    {
                        Notify.Error("You are currently not in any session.");
                    }
                }
                else if (item == rejoinSession)
                {
                    if (NetworkIsSessionActive())
                    {
                        Notify.Error("You are already connected to a session.");
                    }
                    else
                    {
                        Notify.Info("Attempting to re-join the session.");
                        NetworkSessionHost(-1, 32, false);
                    }
                }
                else if (item == disconnectFromServer)
                {

                    RegisterCommand("disconnect", new Action<dynamic, dynamic, dynamic>((a, b, c) => { }), false);
                    ExecuteCommand("disconnect");
                }
            };

            // Teleportation options
            if (IsAllowed(Permission.MSTeleportToWp) || IsAllowed(Permission.MSTeleportLocations) || IsAllowed(Permission.MSTeleportToCoord))
            {
                MenuItem teleportOptionsMenuBtn = new MenuItem("Teleport Options", "Various teleport options.") { Label = "→→→" };
                menu.AddMenuItem(teleportOptionsMenuBtn);
                MenuController.BindMenuItem(menu, teleportOptionsMenu, teleportOptionsMenuBtn);

                MenuItem tptowp = new MenuItem("Teleport To Waypoint", "Teleport to the waypoint on your map.");
                MenuItem tpToCoord = new MenuItem("Teleport To Coords", "Enter x, y, z coordinates and you will be teleported to that location.");
                MenuItem saveLocationBtn = new MenuItem("Save Teleport Location", "Adds your current location to the teleport locations menu and saves it on the server.");
                teleportOptionsMenu.OnItemSelect += async (sender, item, index) =>
                {
                    // Teleport to waypoint.
                    if (item == tptowp)
                    {
                        TeleportToWp();
                    }
                    else if (item == tpToCoord)
                    {
                        string x = await GetUserInput("Enter X coordinate.");
                        if (string.IsNullOrEmpty(x))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        string y = await GetUserInput("Enter Y coordinate.");
                        if (string.IsNullOrEmpty(y))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }
                        string z = await GetUserInput("Enter Z coordinate.");
                        if (string.IsNullOrEmpty(z))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                            return;
                        }

                        float posX = 0f;
                        float posY = 0f;
                        float posZ = 0f;

                        if (!float.TryParse(x, out posX))
                        {
                            if (int.TryParse(x, out int intX))
                            {
                                posX = (float)intX;
                            }
                            else
                            {
                                Notify.Error("You did not enter a valid X coordinate.");
                                return;
                            }
                        }
                        if (!float.TryParse(y, out posY))
                        {
                            if (int.TryParse(y, out int intY))
                            {
                                posY = (float)intY;
                            }
                            else
                            {
                                Notify.Error("You did not enter a valid Y coordinate.");
                                return;
                            }
                        }
                        if (!float.TryParse(z, out posZ))
                        {
                            if (int.TryParse(z, out int intZ))
                            {
                                posZ = (float)intZ;
                            }
                            else
                            {
                                Notify.Error("You did not enter a valid Z coordinate.");
                                return;
                            }
                        }

                        await TeleportToCoords(new Vector3(posX, posY, posZ), true);
                    }
                    else if (item == saveLocationBtn)
                    {
                        SavePlayerLocationToLocationsFile();
                    }
                };

                if (IsAllowed(Permission.MSTeleportToWp))
                {
                    teleportOptionsMenu.AddMenuItem(tptowp);
                    keybindMenu.AddMenuItem(kbTpToWaypoint);
                }
                if (IsAllowed(Permission.MSTeleportToCoord))
                {
                    teleportOptionsMenu.AddMenuItem(tpToCoord);
                }
                if (IsAllowed(Permission.MSTeleportLocations))
                {
                    teleportOptionsMenu.AddMenuItem(teleportMenuBtn);

                    MenuController.AddSubmenu(teleportOptionsMenu, teleportMenu);
                    MenuController.BindMenuItem(teleportOptionsMenu, teleportMenu, teleportMenuBtn);
                    teleportMenuBtn.Label = "→→→";

                    teleportMenu.OnMenuOpen += (sender) =>
                    {
                        if (teleportMenu.Size != TpLocations.Count())
                        {
                            teleportMenu.ClearMenuItems();
                            foreach (var location in TpLocations)
                            {
                                var x = Math.Round(location.coordinates.X, 2);
                                var y = Math.Round(location.coordinates.Y, 2);
                                var z = Math.Round(location.coordinates.Z, 2);
                                var heading = Math.Round(location.heading, 2);
                                MenuItem tpBtn = new MenuItem(location.name, $"Teleport to ~y~{location.name}~n~~s~x: ~y~{x}~n~~s~y: ~y~{y}~n~~s~z: ~y~{z}~n~~s~heading: ~y~{heading}") { ItemData = location };
                                teleportMenu.AddMenuItem(tpBtn);
                            }
                        }
                    };

                    teleportMenu.OnItemSelect += async (sender, item, index) =>
                    {
                        if (item.ItemData is vMenuShared.ConfigManager.TeleportLocation tl)
                        {
                            await TeleportToCoords(tl.coordinates, true);
                            SetEntityHeading(Game.PlayerPed.Handle, tl.heading);
                            SetGameplayCamRelativeHeading(0f);
                        }
                    };

                    if (IsAllowed(Permission.MSTeleportSaveLocation))
                    {
                        teleportOptionsMenu.AddMenuItem(saveLocationBtn);
                    }
                }

            }

            #region dev tools menu

            MenuItem devToolsBtn = new MenuItem("Developer Tools", "Various development/debug tools.") { Label = "→→→" };
            menu.AddMenuItem(devToolsBtn);
            MenuController.AddSubmenu(menu, developerToolsMenu);
            MenuController.BindMenuItem(menu, developerToolsMenu, devToolsBtn);

            // clear area and coordinates
            if (IsAllowed(Permission.MSClearArea))
            {
                developerToolsMenu.AddMenuItem(clearArea);
            }
            if (IsAllowed(Permission.MSShowCoordinates))
            {
                developerToolsMenu.AddMenuItem(coords);
            }

            // model outlines
            if (!vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_disable_entity_outlines_tool))
            {
                developerToolsMenu.AddMenuItem(vehModelDimensions);
                developerToolsMenu.AddMenuItem(propModelDimensions);
                developerToolsMenu.AddMenuItem(pedModelDimensions);
                developerToolsMenu.AddMenuItem(showEntityHandles);
                developerToolsMenu.AddMenuItem(showEntityModels);
                developerToolsMenu.AddMenuItem(showEntityNetOwners);
                developerToolsMenu.AddMenuItem(dimensionsDistanceSlider);
            }


            // timecycle modifiers
            developerToolsMenu.AddMenuItem(timeCycles);
            developerToolsMenu.AddMenuItem(enableTimeCycle);
            developerToolsMenu.AddMenuItem(timeCycleIntensity);

            developerToolsMenu.OnSliderPositionChange += (sender, item, oldPos, newPos, itemIndex) =>
            {
                if (item == timeCycleIntensity)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        float intensity = ((float)newPos / 20f);
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
                else if (item == dimensionsDistanceSlider)
                {
                    FunctionsController.entityRange = ((float)newPos / 20f) * 2000f; // max radius = 2000f;
                }
            };

            developerToolsMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == timeCycles)
                {
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        float intensity = ((float)timeCycleIntensity.Position / 20f);
                        SetTimecycleModifierStrength(intensity);
                    }
                    UserDefaults.MiscLastTimeCycleModifierIndex = timeCycles.ListIndex;
                    UserDefaults.MiscLastTimeCycleModifierStrength = timeCycleIntensity.Position;
                }
            };

            developerToolsMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == clearArea)
                {
                    var pos = Game.PlayerPed.Position;
                    BaseScript.TriggerServerEvent("vMenu:ClearArea", pos.X, pos.Y, pos.Z);
                }
            };

            developerToolsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == vehModelDimensions)
                {
                    ShowVehicleModelDimensions = _checked;
                }
                else if (item == propModelDimensions)
                {
                    ShowPropModelDimensions = _checked;
                }
                else if (item == pedModelDimensions)
                {
                    ShowPedModelDimensions = _checked;
                }
                else if (item == showEntityHandles)
                {
                    ShowEntityHandles = _checked;
                }
                else if (item == showEntityModels)
                {
                    ShowEntityModels = _checked;
                }
                else if (item == showEntityNetOwners)
                {
                    ShowEntityNetOwners = _checked;
                }
                else if (item == enableTimeCycle)
                {
                    TimecycleEnabled = _checked;
                    ClearTimecycleModifier();
                    if (TimecycleEnabled)
                    {
                        SetTimecycleModifier(TimeCycles.Timecycles[timeCycles.ListIndex]);
                        float intensity = ((float)timeCycleIntensity.Position / 20f);
                        SetTimecycleModifierStrength(intensity);
                    }
                }
                else if (item == coords)
                {
                    ShowCoordinates = _checked;
                }
            };

            #endregion


            // Keybind options
            if (IsAllowed(Permission.MSDriftMode))
            {
                keybindMenu.AddMenuItem(kbDriftMode);
            }
            // always allowed keybind menu options
            keybindMenu.AddMenuItem(kbRecordKeys);
            keybindMenu.AddMenuItem(kbRadarKeys);
            keybindMenu.AddMenuItem(kbPointKeysCheckbox);
            keybindMenu.AddMenuItem(backBtn);

            // Always allowed
            menu.AddMenuItem(rightAlignMenu);
            menu.AddMenuItem(disablePms);
            menu.AddMenuItem(disableControllerKey);
            menu.AddMenuItem(speedKmh);
            menu.AddMenuItem(speedMph);
            menu.AddMenuItem(keybindMenuBtn);
            keybindMenuBtn.Label = "→→→";
            if (IsAllowed(Permission.MSConnectionMenu))
            {
                menu.AddMenuItem(connectionSubmenuBtn);
                connectionSubmenuBtn.Label = "→→→";
            }
            if (IsAllowed(Permission.MSShowLocation))
            {
                menu.AddMenuItem(showLocation);
            }
            menu.AddMenuItem(drawTime); // always allowed
            if (IsAllowed(Permission.MSJoinQuitNotifs))
            {
                menu.AddMenuItem(deathNotifs);
            }
            if (IsAllowed(Permission.MSDeathNotifs))
            {
                menu.AddMenuItem(joinQuitNotifs);
            }
            if (IsAllowed(Permission.MSNightVision))
            {
                menu.AddMenuItem(nightVision);
            }
            if (IsAllowed(Permission.MSThermalVision))
            {
                menu.AddMenuItem(thermalVision);
            }
            if (IsAllowed(Permission.MSLocationBlips))
            {
                menu.AddMenuItem(locationBlips);
                ToggleBlips(ShowLocationBlips);
            }
            if (IsAllowed(Permission.MSPlayerBlips))
            {
                menu.AddMenuItem(playerBlips);
            }
            if (IsAllowed(Permission.MSOverheadNames))
            {
                menu.AddMenuItem(playerNames);
            }
            // always allowed, it just won't do anything if the server owner disabled the feature, but players can still toggle it.
            menu.AddMenuItem(respawnDefaultCharacter);
            if (IsAllowed(Permission.MSRestoreAppearance))
            {
                menu.AddMenuItem(restorePlayerAppearance);
            }
            if (IsAllowed(Permission.MSRestoreWeapons))
            {
                menu.AddMenuItem(restorePlayerWeapons);
            }

            // Always allowed
            menu.AddMenuItem(hideRadar);
            menu.AddMenuItem(hideHud);
            menu.AddMenuItem(lockCamX);
            menu.AddMenuItem(lockCamY);
            if (MainMenu.EnableExperimentalFeatures)
            {
                menu.AddMenuItem(exportData);
            }
            menu.AddMenuItem(saveSettings);

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == rightAlignMenu)
                {

                    MenuController.MenuAlignment = _checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left;
                    MiscRightAlignMenu = _checked;
                    UserDefaults.MiscRightAlignMenu = MiscRightAlignMenu;

                    if (MenuController.MenuAlignment != (_checked ? MenuController.MenuAlignmentOption.Right : MenuController.MenuAlignmentOption.Left))
                    {
                        Notify.Error(CommonErrors.RightAlignedNotSupported);
                        // (re)set the default to left just in case so they don't get this error again in the future.
                        MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                        MiscRightAlignMenu = false;
                        UserDefaults.MiscRightAlignMenu = false;
                    }

                }
                else if (item == disablePms)
                {
                    MiscDisablePrivateMessages = _checked;
                }
                else if (item == disableControllerKey)
                {
                    MiscDisableControllerSupport = _checked;
                    MenuController.EnableMenuToggleKeyOnController = !_checked;
                }
                else if (item == speedKmh)
                {
                    ShowSpeedoKmh = _checked;
                }
                else if (item == speedMph)
                {
                    ShowSpeedoMph = _checked;
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
                else if (item == playerNames)
                {
                    MiscShowOverheadNames = _checked;
                }
                else if (item == respawnDefaultCharacter)
                {
                    MiscRespawnDefaultCharacter = _checked;
                }
                else if (item == restorePlayerAppearance)
                {
                    RestorePlayerAppearance = _checked;
                }
                else if (item == restorePlayerWeapons)
                {
                    RestorePlayerWeapons = _checked;
                }

            };

            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // export data
                if (item == exportData)
                {
                    MenuController.CloseAllMenus();
                    var vehicles = GetSavedVehicles();
                    var normalPeds = StorageManager.GetSavedPeds();
                    var mpPeds = StorageManager.GetSavedMpPeds();
                    var weaponLoadouts = WeaponLoadouts.GetSavedWeapons();
                    var data = JsonConvert.SerializeObject(new
                    {
                        saved_vehicles = vehicles,
                        normal_peds = normalPeds,
                        mp_characters = mpPeds,
                        weapon_loadouts = weaponLoadouts
                    });
                    SendNuiMessage(data);
                    SetNuiFocus(true, true);
                }
                // save settings
                else if (item == saveSettings)
                {
                    UserDefaults.SaveSettings();
                }
            };
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
                try
                {
                    foreach (var bl in vMenuShared.ConfigManager.GetLocationBlipsData())
                    {
                        int blipID = AddBlipForCoord(bl.coordinates.X, bl.coordinates.Y, bl.coordinates.Z);
                        SetBlipSprite(blipID, bl.spriteID);
                        BeginTextCommandSetBlipName("STRING");
                        AddTextComponentSubstringPlayerName(bl.name);
                        EndTextCommandSetBlipName(blipID);
                        SetBlipColour(blipID, bl.color);
                        SetBlipAsShortRange(blipID, true);

                        Blip b = new Blip(bl.coordinates, bl.spriteID, bl.name, bl.color, blipID);
                        blips.Add(b);
                    }
                }
                catch (JsonReaderException ex)
                {
                    Debug.Write($"\n\n[vMenu] An error occurred while loading the locations.json file. Please contact the server owner to resolve this.\nWhen contacting the owner, provide the following error details:\n{ex.Message}.\n\n\n");
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
