using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    /// <summary>
    /// This class manages all things that need to be done every tick based on
    /// checkboxes/things changing in any of the (sub) menus.
    /// </summary>
    class FunctionsController : BaseScript
    {
        // Variables
        private CommonFunctions cf = MainMenu.Cf;

        private int LastVehicle = 0;
        private bool SwitchedVehicle = false;
        private Dictionary<int, string> playerList = new Dictionary<int, string>();
        private List<int> deadPlayers = new List<int>();
        private UIMenu lastOpenMenu = null;
        private float cameraRotationHeading = 0f;

        // show location variables
        private Vector3 currentPos = Game.PlayerPed.Position;
        private Vector3 nodePos = Game.PlayerPed.Position;
        private bool node = false;
        private float heading = 0f;
        private float safeZoneSizeX = (1 / GetSafeZoneSize() / 3.0f) - 0.358f;
        private float safeZoneSizeY = (1 / GetSafeZoneSize() / 3.6f) - 0.27f;
        private uint crossing = 1;
        private string crossingName = "";
        private string suffix = "";
        private bool wasMenuJustOpen = false;
        private PlayerList blipsPlayerList = new PlayerList();

        /// <summary>
        /// Constructor.
        /// </summary>
        public FunctionsController()
        {
            // Load the initial playerlist.
            foreach (Player p in new PlayerList())
            {
                playerList.Add(p.Handle, p.Name);
            }

            // Add all tick events.
            Tick += GeneralTasks;
            Tick += PlayerOptions;
            Tick += VehicleOptions;
            Tick += VoiceChat;
            Tick += TimeOptions;
            Tick += _WeatherOptions;
            Tick += WeaponOptions;

            Tick += MiscSettings;
            Tick += DeathNotifications;
            Tick += JoinQuitNotifications;
            Tick += UpdateLocation;
            Tick += ManageCamera;
            Tick += PlayerBlipsControl;
        }

        /// Task related
        #region General Tasks
        /// <summary>
        /// All general tasks that run every game tick (and are not (sub)menu specific).
        /// </summary>
        /// <returns></returns>
        private async Task GeneralTasks()
        {
            // CommonFunctions is required, if it doesn't exist then we won't execute the checks.
            if (cf != null)
            {
                // Check if the player has switched to a new vehicle.
                if (IsPedInAnyVehicle(PlayerPedId(), true)) // added this for improved performance.
                {
                    var tmpVehicle = cf.GetVehicle();
                    if (DoesEntityExist(tmpVehicle) && tmpVehicle != LastVehicle)
                    {
                        // Set the last vehicle to the new vehicle entity.
                        LastVehicle = tmpVehicle;
                        SwitchedVehicle = true;
                    }
                }

                if (!MainMenu.DontOpenMenus && MainMenu.Mp.IsAnyMenuOpen())
                {
                    lastOpenMenu = cf.GetOpenMenu();
                }
                // If any on-screen keyboard is visible, close any open menus and disable any menu from opening.
                if (UpdateOnscreenKeyboard() == 0 && (MainMenu.Mp.IsAnyMenuOpen() || wasMenuJustOpen)) // still editing aka the input box is visible.
                {
                    MainMenu.DontOpenMenus = true;
                    MainMenu.DisableControls = true;
                    wasMenuJustOpen = true; // added for extra check to make sure only vMenu gets re-opened if vMenu was already open.
                }
                // Otherwise, check if the "DontOpenMenus" option is (still) true.
                else
                {
                    if (MainMenu.DontOpenMenus)
                    {
                        // Allow menus from being displayed.
                        MainMenu.DontOpenMenus = false;

                        // Check if the previous menu isn't null.
                        if (lastOpenMenu != null && wasMenuJustOpen)
                        {
                            // Re-open the last menu.
                            lastOpenMenu.Visible = true;
                            // Set the last menu to null.
                            lastOpenMenu = null;
                            wasMenuJustOpen = false; // reset the justOpen state.
                        }

                        // Wait 5 ticks before allowing the menu to be controlled, to prevent accidental interactions when the menu JUST re-appeared.
                        await Delay(5);
                        MainMenu.DisableControls = false;


                    }
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion
        #region Player Options Tasks
        /// <summary>
        /// Run all tasks for the Player Options menu.
        /// </summary>
        /// <returns></returns>
        private async Task PlayerOptions()
        {
            // Player options. Only run player options if the player options menu has actually been created.
            if (MainMenu.PlayerOptionsMenu != null && cf.IsAllowed(Permission.POMenu))
            {
                // Manage Player God Mode
                SetEntityInvincible(PlayerPedId(), MainMenu.PlayerOptionsMenu.PlayerGodMode && cf.IsAllowed(Permission.POGod));

                // Manage invisibility.
                SetEntityVisible(PlayerPedId(), (!MainMenu.PlayerOptionsMenu.PlayerInvisible && cf.IsAllowed(Permission.POInvisible)) ||
                    (!cf.IsAllowed(Permission.POInvisible)), false);

                // Manage Stamina
                if (MainMenu.PlayerOptionsMenu.PlayerStamina && cf.IsAllowed(Permission.POUnlimitedStamina))
                {
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), 100, true);
                }
                else
                {
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), 0, true);
                }
                // Manage other stats.
                StatSetInt((uint)GetHashKey("MP0_STRENGTH"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_LUNG_CAPACITY"), 80, true); // reduced because it was over powered
                StatSetInt((uint)GetHashKey("MP0_WHEELIE_ABILITY"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_FLYING_ABILITY"), 100, true);
                StatSetInt((uint)GetHashKey("MP0_SHOOTING_ABILITY"), 50, true); // reduced because it was over powered
                StatSetInt((uint)GetHashKey("MP0_STEALTH_ABILITY"), 100, true);

                // Manage Super jump.
                if (MainMenu.PlayerOptionsMenu.PlayerSuperJump && cf.IsAllowed(Permission.POSuperjump))
                {
                    SetSuperJumpThisFrame(PlayerId());
                }

                // Manage PlayerNoRagdoll
                SetPedCanRagdoll(PlayerPedId(), (!MainMenu.PlayerOptionsMenu.PlayerNoRagdoll && cf.IsAllowed(Permission.PONoRagdoll)) ||
                    (!cf.IsAllowed(Permission.PONoRagdoll)));


                // Fall off bike / dragged out of car.
                if (MainMenu.VehicleOptionsMenu != null)
                {
                    SetPedCanBeKnockedOffVehicle(PlayerPedId(), (((MainMenu.PlayerOptionsMenu.PlayerNoRagdoll && cf.IsAllowed(Permission.PONoRagdoll))
                        || (MainMenu.VehicleOptionsMenu.VehicleGodMode) && cf.IsAllowed(Permission.VOGod)) ? 1 : 0));

                    SetPedCanBeDraggedOut(PlayerPedId(), ((MainMenu.PlayerOptionsMenu.PlayerIsIgnored && cf.IsAllowed(Permission.POIgnored)) ||
                        (MainMenu.VehicleOptionsMenu.VehicleGodMode && cf.IsAllowed(Permission.VOGod)) ||
                        (MainMenu.PlayerOptionsMenu.PlayerGodMode && cf.IsAllowed(Permission.POGod))));

                    SetPedCanBeShotInVehicle(PlayerPedId(), !((MainMenu.PlayerOptionsMenu.PlayerGodMode && cf.IsAllowed(Permission.POGod)) ||
                        (MainMenu.VehicleOptionsMenu.VehicleGodMode && cf.IsAllowed(Permission.VOGod))));
                }
                else
                {
                    SetPedCanBeKnockedOffVehicle(PlayerPedId(), ((MainMenu.PlayerOptionsMenu.PlayerNoRagdoll && cf.IsAllowed(Permission.PONoRagdoll)) ? 1 : 0));
                    SetPedCanBeDraggedOut(PlayerPedId(), (MainMenu.PlayerOptionsMenu.PlayerIsIgnored && cf.IsAllowed(Permission.POIgnored)));
                    SetPedCanBeShotInVehicle(PlayerPedId(), !(MainMenu.PlayerOptionsMenu.PlayerGodMode && cf.IsAllowed(Permission.POGod)));
                }

                // Manage never wanted.
                if (MainMenu.PlayerOptionsMenu.PlayerNeverWanted && GetPlayerWantedLevel(PlayerId()) > 0 && cf.IsAllowed(Permission.PONeverWanted))
                {
                    ClearPlayerWantedLevel(PlayerId());
                }

                // Manage player is ignored by everyone.
                SetEveryoneIgnorePlayer(PlayerId(), MainMenu.PlayerOptionsMenu.PlayerIsIgnored && cf.IsAllowed(Permission.POIgnored));

                SetPoliceIgnorePlayer(PlayerId(), MainMenu.PlayerOptionsMenu.PlayerIsIgnored && cf.IsAllowed(Permission.POIgnored));

                SetPlayerCanBeHassledByGangs(PlayerId(), !((MainMenu.PlayerOptionsMenu.PlayerIsIgnored && cf.IsAllowed(Permission.POIgnored)) ||
                    (MainMenu.PlayerOptionsMenu.PlayerGodMode && cf.IsAllowed(Permission.POGod))));

                if (MainMenu.NoClipMenu != null)
                {
                    if (!MainMenu.NoClipEnabled)
                    {
                        // Manage player frozen.
                        if (MainMenu.PlayerOptionsMenu.PlayerFrozen && cf.IsAllowed(Permission.POFreeze))
                            FreezeEntityPosition(PlayerPedId(), true);
                    }
                }
                else
                {
                    // Manage player frozen.
                    if (MainMenu.PlayerOptionsMenu.PlayerFrozen && cf.IsAllowed(Permission.POFreeze))
                        FreezeEntityPosition(PlayerPedId(), true);
                }


                if (MainMenu.Cf.driveToWpTaskActive && !Game.IsWaypointActive)
                {
                    ClearPedTasks(PlayerPedId());
                    Notify.Custom("Destination reached, the car will now stop driving!");
                    MainMenu.Cf.driveToWpTaskActive = false;
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion
        #region Vehicle Options Tasks
        /// <summary>
        /// Manage all vehicle related tasks.
        /// </summary>
        /// <returns></returns>
        private async Task VehicleOptions()
        {
            // Vehicle options. Only run vehicle options if the vehicle options menu has actually been created.
            if (MainMenu.VehicleOptionsMenu != null && cf.IsAllowed(Permission.VOMenu))
            {
                // When the player is in a valid vehicle:
                if (DoesEntityExist(cf.GetVehicle()))
                {
                    // Create a new vehicle object.
                    Vehicle vehicle = new Vehicle(cf.GetVehicle());

                    // God mode
                    bool god = MainMenu.VehicleOptionsMenu.VehicleGodMode && cf.IsAllowed(Permission.VOGod);
                    vehicle.CanBeVisiblyDamaged = !god;
                    vehicle.CanEngineDegrade = !god;
                    vehicle.CanTiresBurst = !god;
                    vehicle.CanWheelsBreak = !god;
                    vehicle.IsAxlesStrong = god;
                    vehicle.IsBulletProof = god;
                    vehicle.IsCollisionProof = god;
                    vehicle.IsExplosionProof = god;
                    vehicle.IsFireProof = god;
                    vehicle.IsInvincible = god;
                    vehicle.IsMeleeProof = god;
                    foreach (VehicleDoor vd in vehicle.Doors.GetAll())
                    {
                        vd.CanBeBroken = !god;
                    }
                    bool specialgod = MainMenu.VehicleOptionsMenu.VehicleSpecialGodMode && cf.IsAllowed(Permission.VOSpecialGod);
                    if (specialgod && vehicle.EngineHealth < 1000)
                    {
                        vehicle.Repair(); // repair vehicle if special god mode is on and the vehicle is not full health.
                    }

                    // Freeze Vehicle Position (if enabled).
                    if (MainMenu.VehicleOptionsMenu.VehicleFrozen && cf.IsAllowed(Permission.VOFreeze))
                    {
                        FreezeEntityPosition(vehicle.Handle, true);
                    }


                    // If the torque multiplier is enabled and the player is allowed to use it.
                    if (MainMenu.VehicleOptionsMenu.VehicleTorqueMultiplier && cf.IsAllowed(Permission.VOTorqueMultiplier))
                    {
                        // Set the torque multiplier to the selected value by the player.
                        // no need for an "else" to reset this value, because when it's not called every frame, nothing happens.
                        SetVehicleEngineTorqueMultiplier(vehicle.Handle, MainMenu.VehicleOptionsMenu.VehicleTorqueMultiplierAmount);
                    }
                    // If the player has switched to a new vehicle, and the vehicle engine power multiplier is turned on. Set the new value.
                    if (SwitchedVehicle)
                    {
                        // Only needs to be set once.
                        SwitchedVehicle = false;

                        // Vehicle engine power multiplier. Enable it once the player switched vehicles.
                        // Only do this if the option is enabled AND the player has permissions for it.
                        if (MainMenu.VehicleOptionsMenu.VehiclePowerMultiplier && cf.IsAllowed(Permission.VOPowerMultiplier))
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, MainMenu.VehicleOptionsMenu.VehiclePowerMultiplierAmount);
                        }
                        // If the player switched vehicles and the option is turned off or the player has no permissions for it
                        // Then reset the power multiplier ONCE.
                        else
                        {
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, 1f);
                        }

                        // No Siren Toggle
                        vehicle.IsSirenSilent = MainMenu.VehicleOptionsMenu.VehicleNoSiren && cf.IsAllowed(Permission.VONoSiren);
                    }

                    // Manage "no helmet"
                    var ped = new Ped(PlayerPedId());
                    // If the no helmet feature is turned on, disalbe "ped can wear helmet"
                    if (MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet && cf.IsAllowed(Permission.VONoHelmet))
                    {
                        ped.CanWearHelmet = false;
                    }
                    // otherwise, allow helmets.
                    else if (!MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet || !cf.IsAllowed(Permission.VONoHelmet))
                    {
                        ped.CanWearHelmet = true;
                    }
                    // If the player is still wearing a helmet, even if the option is set to: no helmet, then remove the helmet.
                    if (ped.IsWearingHelmet && MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet && cf.IsAllowed(Permission.VONoHelmet))
                    {
                        ped.RemoveHelmet(true);
                    }

                    if (MainMenu.VehicleOptionsMenu.FlashHighbeamsOnHonk && vehicle.Driver == Game.PlayerPed && !IsPauseMenuActive())
                    {
                        // turn on high beams when honking.
                        if (Game.IsControlPressed(0, Control.VehicleHorn))
                        {
                            vehicle.AreHighBeamsOn = true;
                        }
                        // turn high beams back off when just stopped honking.
                        if (Game.IsControlJustReleased(0, Control.VehicleHorn))
                        {
                            vehicle.AreHighBeamsOn = false;
                        }
                    }
                }
                // When the player is not inside a vehicle:
                else
                {
                    UIMenu[] vehicleSubmenus = new UIMenu[6];
                    vehicleSubmenus[0] = MainMenu.VehicleOptionsMenu.VehicleModMenu;
                    vehicleSubmenus[1] = MainMenu.VehicleOptionsMenu.VehicleLiveriesMenu;
                    vehicleSubmenus[2] = MainMenu.VehicleOptionsMenu.VehicleColorsMenu;
                    vehicleSubmenus[3] = MainMenu.VehicleOptionsMenu.VehicleDoorsMenu;
                    vehicleSubmenus[4] = MainMenu.VehicleOptionsMenu.VehicleWindowsMenu;
                    vehicleSubmenus[5] = MainMenu.VehicleOptionsMenu.VehicleComponentsMenu;
                    foreach (UIMenu m in vehicleSubmenus)
                    {
                        if (m.Visible)
                        {
                            MainMenu.VehicleOptionsMenu.GetMenu().Visible = true;
                            m.Visible = false;
                            Notify.Error(CommonErrors.NoVehicle, placeholderValue: "to access this menu");
                        }
                    }
                }

                // Manage vehicle engine always on.
                if ((MainMenu.VehicleOptionsMenu.VehicleEngineAlwaysOn && DoesEntityExist(cf.GetVehicle(lastVehicle: true)) &&
                    !IsPedInAnyVehicle(PlayerPedId(), false)) && (cf.IsAllowed(Permission.VOEngineAlwaysOn)))
                {
                    await Delay(100);
                    SetVehicleEngineOn(cf.GetVehicle(lastVehicle: true), true, true, true);
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion
        #region Weather Options
        private async Task _WeatherOptions()
        {
            await Delay(1000);
            if (MainMenu.WeatherOptionsMenu != null && cf.IsAllowed(Permission.WOMenu))
            {
                if (MainMenu.WeatherOptionsMenu.GetMenu().Visible)
                {
                    MainMenu.WeatherOptionsMenu.GetMenu().MenuItems.ForEach(mi => { mi.SetRightBadge(UIMenuItem.BadgeStyle.None); });
                    var item = WeatherOptions.weatherHashMenuIndex[GetNextWeatherTypeHashName().ToString()];
                    item.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
                }
            }
        }
        #endregion
        #region Misc Settings Menu Tasks
        /// <summary>
        /// Run all tasks that need to be handeled for the Misc Settings Menu.
        /// </summary>
        /// <returns></returns>
        private async Task MiscSettings()
        {
            if (MainMenu.MiscSettingsMenu != null /*&& cf.IsAllowed(Permission.MSMenu)*/)
            {
                #region Misc Settings
                // Show speedometer km/h
                if (MainMenu.MiscSettingsMenu.ShowSpeedoKmh)
                {
                    ShowSpeedKmh();
                }

                // Show speedometer mph
                if (MainMenu.MiscSettingsMenu.ShowSpeedoMph)
                {
                    ShowSpeedMph();
                }

                // Show coordinates.
                if (MainMenu.MiscSettingsMenu.ShowCoordinates && cf.IsAllowed(Permission.MSShowCoordinates))
                {
                    var pos = GetEntityCoords(PlayerPedId(), true);
                    cf.DrawTextOnScreen($"~r~X~t~: {Math.Round(pos.X, 2)}" +
                        $"~n~~r~Y~t~: {Math.Round(pos.Y, 2)}" +
                        $"~n~~r~Z~t~: {Math.Round(pos.Z, 2)}" +
                        $"~n~~r~Heading~t~: {Math.Round(GetEntityHeading(PlayerPedId()), 1)}", 0.45f, 0f, 0.38f, Alignment.Left, (int)Font.ChaletLondon);
                }

                // Hide hud.
                if (MainMenu.MiscSettingsMenu.HideHud)
                {
                    HideHudAndRadarThisFrame();
                }

                // Hide radar.
                if (MainMenu.MiscSettingsMenu.HideRadar)
                {
                    DisplayRadar(false);
                }
                // Show radar (or hide it if the user disabled it in pausemenu > settings > display > show radar.
                else
                {
                    DisplayRadar(IsRadarPreferenceSwitchedOn());
                }
                #endregion

                #region Show Location
                // Show location & time.
                if (MainMenu.MiscSettingsMenu.ShowLocation && cf.IsAllowed(Permission.MSShowLocation))
                {
                    ShowLocation();
                }
                #endregion

                #region camera angle locking
                if (MainMenu.MiscSettingsMenu.LockCameraY)
                {
                    SetGameplayCamRelativePitch(0f, 0f);
                }
                if (MainMenu.MiscSettingsMenu.LockCameraX)
                {
                    if (Game.IsControlPressed(0, Control.LookLeftOnly))
                    {
                        cameraRotationHeading++;
                    }
                    else if (Game.IsControlPressed(0, Control.LookRightOnly))
                    {
                        cameraRotationHeading--;
                    }
                    SetGameplayCamRelativeHeading(cameraRotationHeading);
                }
                #endregion
            }
            else
            {
                await Delay(0);
            }
        }

        #region Join / Quit notifications
        /// <summary>
        /// Runs join/quit notification checks.
        /// </summary>
        /// <returns></returns>
        private async Task JoinQuitNotifications()
        {
            if (MainMenu.MiscSettingsMenu != null)
            {
                // Join/Quit notifications
                if (MainMenu.MiscSettingsMenu.JoinQuitNotifications && cf.IsAllowed(Permission.MSJoinQuitNotifs))
                {
                    PlayerList plist = new PlayerList();
                    Dictionary<int, string> pl = new Dictionary<int, string>();
                    foreach (Player p in plist)
                    {
                        pl.Add(p.Handle, p.Name);
                    }
                    await Delay(0);
                    // new player joined.
                    if (pl.Count > playerList.Count)
                    {
                        foreach (KeyValuePair<int, string> player in pl)
                        {
                            if (!playerList.Contains(player))
                            {
                                Notify.Custom($"~g~<C>{player.Value}</C>~s~ joined the server.");
                                await Delay(0);
                            }
                        }
                    }
                    // player left.
                    else if (pl.Count < playerList.Count)
                    {
                        foreach (KeyValuePair<int, string> player in playerList)
                        {
                            if (!pl.Contains(player))
                            {
                                Notify.Custom($"~r~<C>{player.Value}</C>~s~ left the server.");
                                await Delay(0);
                            }
                        }
                    }
                    playerList = pl;
                }
            }
        }
        #endregion

        #region Death Notifications
        /// <summary>
        /// Runs death notification checks.
        /// </summary>
        /// <returns></returns>
        private async Task DeathNotifications()
        {
            if (MainMenu.MiscSettingsMenu != null)
            {
                // Death notifications
                if (MainMenu.MiscSettingsMenu.DeathNotifications && cf.IsAllowed(Permission.MSDeathNotifs))
                {
                    PlayerList pl = new PlayerList();
                    var tmpiterator = 0;
                    foreach (Player p in pl)
                    {
                        tmpiterator++;
                        //if (tmpiterator % 5 == 0)
                        //{
                        await Delay(0);
                        //}
                        if (p.IsDead)
                        {
                            if (deadPlayers.Contains(p.Handle)) { return; }
                            var killer = p.Character.GetKiller();
                            if (killer != null)
                            {
                                if (killer.Handle != p.Character.Handle)
                                {
                                    if (killer.Exists())
                                    {
                                        foreach (Player playerKiller in pl)
                                        {
                                            if (playerKiller.Character.Handle == killer.Handle)
                                            {
                                                Notify.Custom($"~o~<C>{p.Name}</C> ~s~has been murdered by ~y~<C>{playerKiller.Name}</C>~s~.");
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Notify.Custom($"~o~<C>{p.Name}</C> ~s~has been murdered.");
                                    }
                                }
                                else
                                {
                                    Notify.Custom($"~o~<C>{p.Name}</C> ~s~committed suicide.");
                                }
                            }
                            else
                            {
                                Notify.Custom($"~o~<C>{p.Name}</C> ~s~died.");
                            }
                            deadPlayers.Add(p.Handle);
                        }
                        else
                        {
                            if (deadPlayers.Contains(p.Handle))
                            {
                                deadPlayers.Remove(p.Handle);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Update Location for location display
        /// <summary>
        /// Updates the location for location display.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateLocation()
        {
            if (MainMenu.MiscSettingsMenu != null)
            {
                if (MainMenu.MiscSettingsMenu.ShowLocation && cf.IsAllowed(Permission.MSShowLocation))
                {
                    // Get the current location.
                    currentPos = GetEntityCoords(PlayerPedId(), true);

                    // Get the nearest vehicle node.
                    nodePos = currentPos;
                    node = GetNthClosestVehicleNode(currentPos.X, currentPos.Y, currentPos.Z, 0, ref nodePos, 0, 0, 0);
                    heading = Game.PlayerPed.Heading;

                    // Get the safezone size for x and y to be able to move with the minimap.
                    safeZoneSizeX = (1 / GetSafeZoneSize() / 3.0f) - 0.358f;
                    safeZoneSizeY = (1 / GetSafeZoneSize() / 3.6f) - 0.27f;

                    // Get the cross road.
                    var p1 = (uint)1; // unused
                    crossing = (uint)1;
                    GetStreetNameAtCoord(currentPos.X, currentPos.Y, currentPos.Z, ref p1, ref crossing);
                    crossingName = GetStreetNameFromHashKey(crossing);

                    // Set the suffix for the road name to the corssing name, or to an empty string if there's no crossing.
                    suffix = (crossingName != "" && crossingName != "NULL" && crossingName != null) ? "~t~ / " + crossingName : "";

                    await Delay(200);
                }
                else
                {
                    await Delay(0);
                }
            }
        }
        #endregion

        #region ShowLocation
        /// <summary>
        /// Show location function to show the player's location.
        /// </summary>
        private void ShowLocation()
        {
            // Create the default prefix.
            var prefix = "~s~";

            // If the vehicle node is further away than 1400f, then the player is not near a valid road.
            // So we set the prefix to "Near " (<streetname>).
            if (Vdist2(currentPos.X, currentPos.Y, currentPos.Z, nodePos.X, nodePos.Y, nodePos.Z) > 1400f)
            {
                prefix = "~m~Near ~s~";
            }

            string headingCharacter = "";

            // Heading Facing North
            if (heading > 320 || heading < 45)
            {
                headingCharacter = "N";
            }
            // Heading Facing West
            else if (heading >= 45 && heading <= 135)
            {
                headingCharacter = "W";
            }
            // Heading Facing South
            else if (heading > 135 && heading < 225)
            {
                headingCharacter = "S";
            }
            // Heading Facing East
            else
            {
                headingCharacter = "E";
            }

            // Draw the street name + crossing.
            cf.DrawTextOnScreen(prefix + World.GetStreetName(currentPos) + suffix, 0.234f + safeZoneSizeX, 0.925f - safeZoneSizeY, 0.48f);
            // Draw the zone name.
            cf.DrawTextOnScreen(World.GetZoneLocalizedName(currentPos), 0.234f + safeZoneSizeX, 0.9485f - safeZoneSizeY, 0.45f);

            // Draw the left border for the heading character.
            cf.DrawTextOnScreen("~t~|", 0.188f + safeZoneSizeX, 0.915f - safeZoneSizeY, 1.2f, Alignment.Left);
            // Draw the heading character.
            cf.DrawTextOnScreen(headingCharacter, 0.208f + safeZoneSizeX, 0.915f - safeZoneSizeY, 1.2f, Alignment.Center);
            // Draw the right border for the heading character.
            cf.DrawTextOnScreen("~t~|", 0.228f + safeZoneSizeX, 0.915f - safeZoneSizeY, 1.2f, Alignment.Right);

            // Get and draw the time.
            var tth = GetClockHours();
            var ttm = GetClockMinutes();
            var th = (tth < 10) ? $"0{tth.ToString()}" : tth.ToString();
            var tm = (ttm < 10) ? $"0{ttm.ToString()}" : ttm.ToString();
            cf.DrawTextOnScreen($"~c~{th}:{tm}", 0.208f + safeZoneSizeX, 0.9748f - safeZoneSizeY, 0.40f, Alignment.Center);
        }
        #endregion
        #endregion
        #region Voice Chat Tasks
        /// <summary>
        /// Run all voice chat options tasks
        /// </summary>
        /// <returns></returns>
        private async Task VoiceChat()
        {
            if (MainMenu.VoiceChatSettingsMenu != null && cf.IsAllowed(Permission.VCMenu))
            {
                if (MainMenu.VoiceChatSettingsMenu.EnableVoicechat && cf.IsAllowed(Permission.VCEnable))
                {
                    NetworkSetVoiceActive(true);
                    NetworkSetTalkerProximity(MainMenu.VoiceChatSettingsMenu.currentProximity);
                    int channel = MainMenu.VoiceChatSettingsMenu.channels.IndexOf(MainMenu.VoiceChatSettingsMenu.currentChannel);
                    if (channel < 1)
                    {
                        NetworkClearVoiceChannel();
                    }
                    else
                    {
                        NetworkSetVoiceChannel(channel);
                    }
                    if (MainMenu.VoiceChatSettingsMenu.ShowCurrentSpeaker && cf.IsAllowed(Permission.VCShowSpeaker))
                    {
                        PlayerList pl = new PlayerList();
                        var i = 1;
                        var currentlyTalking = false;
                        //cf.DrawTextOnScreen($"~b~Debugging", 0.5f, 0.00f + (i * 0.03f), 0.5f, Alignment.Center, 6);
                        //i++;
                        foreach (Player p in pl)
                        {
                            if (NetworkIsPlayerTalking(p.Handle))
                            {
                                if (!currentlyTalking)
                                {
                                    cf.DrawTextOnScreen("~s~Currently Talking", 0.5f, 0.00f, 0.5f, Alignment.Center, 6);
                                    currentlyTalking = true;
                                }
                                cf.DrawTextOnScreen($"~b~{p.Name}", 0.5f, 0.00f + (i * 0.03f), 0.5f, Alignment.Center, 6);
                                i++;
                            }
                        }
                    }
                }
                else
                {
                    NetworkSetVoiceActive(false);
                    NetworkClearVoiceChannel();
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion
        #region Update Time Options Menu (current time display)
        /// <summary>
        /// Update the current time display in the time options menu.
        /// </summary>
        /// <returns></returns>
        private async Task TimeOptions()
        {
            if (MainMenu.TimeOptionsMenu != null && cf.IsAllowed(Permission.TOMenu))
            {
                if ((MainMenu.TimeOptionsMenu.freezeTimeToggle != null && MainMenu.TimeOptionsMenu.GetMenu().Visible) && cf.IsAllowed(Permission.TOFreezeTime))
                {
                    // Update the current time displayed in the Time Options menu (only when the menu is actually visible).
                    var hours = GetClockHours();
                    var minutes = GetClockMinutes();
                    var hoursString = hours < 10 ? "0" + hours.ToString() : hours.ToString();
                    var minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
                    MainMenu.TimeOptionsMenu.freezeTimeToggle.SetRightLabel($"(Current Time {hoursString}:{minutesString})");
                }
            }
            // This only needs to be updated once every 2 seconds so we can delay it.
            await Delay(2000);
        }
        #endregion
        #region Weapon Options Tasks
        /// <summary>
        /// Manage all weapon options that need to be handeled every tick.
        /// </summary>
        /// <returns></returns>
        private async Task WeaponOptions()
        {
            if (MainMenu.WeaponOptionsMenu != null && cf.IsAllowed(Permission.WPMenu))
            {
                // If no reload is enabled.
                if (MainMenu.WeaponOptionsMenu.NoReload && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Minigun && cf.IsAllowed(Permission.WPNoReload))
                {
                    // Disable reloading.
                    PedSkipNextReloading(PlayerPedId());
                }

                // Enable/disable infinite ammo.
                //SetPedInfiniteAmmoClip(PlayerPedId(), MainMenu.WeaponOptionsMenu.UnlimitedAmmo && cf.IsAllowed(Permission.WPUnlimitedAmmo));
                if (Game.PlayerPed.Weapons.Current != null && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
                {
                    Game.PlayerPed.Weapons.Current.InfiniteAmmo = MainMenu.WeaponOptionsMenu.UnlimitedAmmo && cf.IsAllowed(Permission.WPUnlimitedAmmo);
                }


                /// THIS SOLUTION IS BUGGED AND CAUSES CRASHES
                //// workaround for mk2 weapons (the infinite ammo doesn't seem to work all the time for mk2 weapons)
                //if (MainMenu.WeaponOptionsMenu.UnlimitedAmmo && cf.IsAllowed(Permission.WPUnlimitedAmmo) && Game.PlayerPed.Weapons.Current.IsMk2 &&
                //    Game.PlayerPed.Weapons.Current.Ammo != Game.PlayerPed.Weapons.Current.MaxAmmo)
                //{
                //    Game.PlayerPed.Weapons.Current.Ammo = Game.PlayerPed.Weapons.Current.MaxAmmo;
                //}

                if (MainMenu.WeaponOptionsMenu.AutoEquipChute)
                {
                    if ((IsPedInAnyHeli(PlayerPedId()) || IsPedInAnyPlane(PlayerPedId())) && !HasPedGotWeapon(PlayerPedId(), (uint)WeaponHash.Parachute, false))
                    {
                        GiveWeaponToPed(PlayerPedId(), (uint)WeaponHash.Parachute, 1, false, true);
                        SetPlayerHasReserveParachute(PlayerId());
                        SetPlayerCanLeaveParachuteSmokeTrail(PlayerPedId(), true);
                    }
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion
        #region Spectate Handling Tasks
        /// <summary>
        /// OnTick runs every game tick.
        /// Used here for the spectating feature.
        /// </summary>
        /// <returns></returns>
        private async Task SpectateHandling()
        {
            if (MainMenu.OnlinePlayersMenu != null && cf.IsAllowed(Permission.OPMenu) && cf.IsAllowed(Permission.OPSpectate))
            {
                // When the player dies while spectating, cancel the spectating to prevent an infinite black loading screen.
                if (GetEntityHealth(PlayerPedId()) < 1 && NetworkIsInSpectatorMode())
                {
                    DoScreenFadeOut(50);
                    await Delay(50);
                    NetworkSetInSpectatorMode(true, PlayerPedId());
                    NetworkSetInSpectatorMode(false, PlayerPedId());

                    await Delay(50);
                    DoScreenFadeIn(50);
                    while (GetEntityHealth(PlayerPedId()) < 1)
                    {
                        await Delay(0);
                    }
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion
        #region Player Appearance
        private async Task ManageCamera()
        {
            if (MainMenu.PlayerAppearanceMenu != null && MainMenu.PlayerAppearanceMenu.mpCharMenu != null)
            {
                //foreach (UIMenu m in )
                bool open = MainMenu.PlayerAppearanceMenu.mpCharMenus.Any(m => (m.Visible));
                if (open)
                {
                    int cam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
                    Camera camera = new Camera(cam);

                    Game.PlayerPed.Task.ClearAllImmediately();
                    while (open)
                    {
                        await Delay(0);
                        open = MainMenu.PlayerAppearanceMenu.mpCharMenus.Any(m => (m.Visible));

                        //SetFacialIdleAnimOverride(Game.PlayerPed.Handle, "mood_Happy_1", null);
                        SetFacialIdleAnimOverride(Game.PlayerPed.Handle, "mood_normal_1", null);

                        RenderScriptCams(true, false, 0, true, false);

                        FreezeEntityPosition(PlayerPedId(), true);

                        cf.DisableMovementControlsThisFrame(true, true);

                        camera.PointAt(Game.PlayerPed.Position + new Vector3(0f, 0f, 0.7f));
                        camera.Position = Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 0.8f, 0.7f));
                        Game.PlayerPed.Task.ClearAll();
                        float input = Game.GetDisabledControlNormal(0, Control.LookLeftRight);

                        if (input > 0.5f)
                        {
                            Game.PlayerPed.Task.LookAt(Game.PlayerPed.GetOffsetPosition(new Vector3(-2f, 0.05f, 0.7f)));
                        }
                        else if (input < -0.5f)
                        {
                            Game.PlayerPed.Task.LookAt(Game.PlayerPed.GetOffsetPosition(new Vector3(2f, 0.05f, 0.7f)));
                        }
                        else
                        {
                            Game.PlayerPed.Task.LookAt(camera.Position);
                        }
                    }
                    RenderScriptCams(false, false, 0, false, false);
                    camera.Delete();
                    DestroyAllCams(true);
                    Game.PlayerPed.Task.ClearLookAt();
                    FreezeEntityPosition(PlayerPedId(), false);
                }
            }
        }
        #endregion

        private async Task PlayerBlipsControl()
        {
            if (MainMenu.MiscSettingsMenu != null)
            {
                bool enabled = MainMenu.MiscSettingsMenu.ShowPlayerBlips && cf.IsAllowed(Permission.MSPlayerBlips);

                blipsPlayerList = new PlayerList();
                foreach (Player p in blipsPlayerList)
                {
                    if (enabled)
                    {
                        if (p.Character.AttachedBlip == null || !p.Character.AttachedBlip.Exists())
                        {
                            Debug.WriteLine("New blip added.");
                            p.Character.AttachBlip();
                        }
                        p.Character.AttachedBlip.Color = BlipColor.White;
                        //Debug.Write(p.Character.AttachedBlip.Sprite.ToString());
                        ShowHeadingIndicatorOnBlip(p.Character.AttachedBlip.Handle, true);
                        p.Character.AttachedBlip.IsShortRange = true;
                        p.Character.AttachedBlip.Name = p.Name;


                        if (IsPedInAnyVehicle(p.Character.Handle, false))
                        {
                            Vehicle veh = new Vehicle(cf.GetVehicle(p.Handle, false));
                            if (veh.Model.IsBoat)
                            {
                                p.Character.AttachedBlip.Sprite = BlipSprite.Speedboat; // 427 = speed boat
                            }
                            else if (veh.Model.IsBicycle)
                            {
                                p.Character.AttachedBlip.Sprite = BlipSprite
                            }
                            else if (veh.Model.IsBike)
                            {
                                p.Character.AttachedBlip.Sprite = BlipSprite
                            }
                            else if (veh.Model.IsCar)
                            {
                                switch ((VehicleHash)veh.Model.Hash)
                                {
                                    case VehicleHash.Apc:
                                        break;
                                    default:
                                        break;
                                }
                                //if (veh.Model.Hash == VehicleHash.Apc)
                                //p.Character.AttachedBlip.Sprite = BlipSprite
                            }
                            else if (veh.Model.IsHelicopter)
                            {
                                p.Character.AttachedBlip.Sprite = BlipSprite.HelicopterAnimated;
                            }
                            else if (veh.Model.IsPlane)
                            {
                                p.Character.AttachedBlip.Sprite = BlipSprite
                            }
                            else if (veh.Model.IsQuadbike)
                            {
                                p.Character.AttachedBlip.Sprite = BlipSprite.
                            }
                            else
                            {
                                p.Character.AttachedBlip.Sprite = BlipSprite.Standard;
                            }
                            //if (p.Character.IsInBoat)
                            //{
                            //    p.Character.AttachedBlip.Sprite = BlipSprite.Speedboat; // 427 = Speedboat
                            //}
                            //else if (p.Character.IsInPlane)
                            //{

                            //}
                            //else if (p.Character.IsInHeli)
                            //{

                            //}
                            //else if (p.Character.IsOnBike)
                            //{

                            //}

                            veh = null;
                        }
                        else
                        {
                            p.Character.AttachedBlip.Sprite = BlipSprite.Standard;
                        }
                    }
                    else
                    {
                        if (!(p.Character.AttachedBlip == null || !p.Character.AttachedBlip.Exists()))
                        {
                            p.Character.AttachedBlip.Delete();
                        }
                    }


                    await Delay(60); // wait 60 ticks before doing the next player.
                }
                await Delay(1000); // wait 1000 ticks before doing the next loop.
            }
            else
            {
                await Delay(1000);
            }
        }


        /// Not task related
        #region Private ShowSpeed Functions
        /// <summary>
        /// Shows the current speed in km/h.
        /// Must be in a vehicle.
        /// </summary>
        private void ShowSpeedKmh()
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                int speed = int.Parse(Math.Round(GetEntitySpeed(cf.GetVehicle()) * 3.6f).ToString());
                cf.DrawTextOnScreen($"{speed} KM/h", 0.995f, 0.955f, 0.7f, Alignment.Right, 4);
            }
        }

        /// <summary>
        /// Shows the current speed in mph.
        /// Must be in a vehicle.
        /// </summary>
        private void ShowSpeedMph()
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                int speed = int.Parse(Math.Round(GetEntitySpeed(cf.GetVehicle()) * 2.23694f).ToString());

                if (MainMenu.MiscSettingsMenu.ShowSpeedoKmh)
                {
                    cf.DrawTextOnScreen($"{speed} MPH", 0.995f, 0.925f, 0.7f, Alignment.Right, 4);
                    HideHudComponentThisFrame((int)HudComponent.StreetName);
                }
                else
                {
                    cf.DrawTextOnScreen($"{speed} MPH", 0.995f, 0.955f, 0.7f, Alignment.Right, 4);
                }
            }
        }
        #endregion
    }
}
