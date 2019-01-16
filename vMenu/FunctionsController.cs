using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.ConfigManager;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    /// <summary>
    /// This class manages all things that need to be done every tick based on
    /// checkboxes/things changing in any of the (sub) menus.
    /// </summary>
    class FunctionsController : BaseScript
    {
        private int LastVehicle = 0;
        private bool SwitchedVehicle = false;
        private Dictionary<int, string> playerList = new Dictionary<int, string>();
        private List<int> deadPlayers = new List<int>();
        //private Menu lastOpenMenu = null;
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
        //private bool wasMenuJustOpen = false;
        private List<int> waypointPlayerIdsToRemove = new List<int>();
        private int voiceTimer = 0;
        private int voiceCycle = 1;
        private const float voiceIndicatorWidth = 0.02f;
        private const float voiceIndicatorHeight = 0.041f;
        private const float voiceIndicatorMutedWidth = voiceIndicatorWidth + 0.0021f;
        public const string clothingAnimationDecor = "clothing_animation_type";
        private bool clothingAnimationReverse = false;
        private float clothingOpacity = 1f;

        private const string snowball_anim_dict = "anim@mp_snowball";
        private const string snowball_anim_name = "pickup_snowball";
        private readonly uint snowball_hash = (uint)GetHashKey("weapon_snowball");
        private bool showSnowballInfo = false;

        /// I made these seperate bools that only get set once after initial load 
        /// to prevent the CommonFunctions.IsAllowed() function being called over and over again multiple times every tick.
        public static bool flaresAllowed = false;
        public static bool bombsAllowed = false;


        /// <summary>
        /// Constructor.
        /// </summary>
        public FunctionsController()
        {
            // Load the initial playerlist.
            foreach (Player p in Players)
            {
                playerList.Add(p.Handle, p.Name);
            }

            // Add all tick events.
            Tick += GeneralTasks;
            Tick += PlayerOptions;
            Tick += VehicleOptions;
            Tick += MoreVehicleOptions;
            Tick += VoiceChat;
            Tick += TimeOptions;
            Tick += WeatherOptions;
            Tick += WeaponOptions;
            Tick += OnlinePlayersTasks;
            Tick += MiscSettings;
            Tick += DeathNotifications;
            Tick += JoinQuitNotifications;
            Tick += UpdateLocation;
            Tick += ManagePlayerAppearanceCamera;
            Tick += PlayerBlipsControl;
            Tick += RestorePlayerAfterBeingDead;
            Tick += PlayerClothingAnimationsController;
            //Tick += FlaresAndBombsTick;
            Tick += AnimationsAndInteractions;
            Tick += HelpMessageController;
            Tick += ModelDrawDimensions;
            Tick += GcTick;
            Tick += InteriorHideExterior;
            Tick += InteriorChecker;
            Tick += InteriorTv;
            Tick += InteriorRadio;
            Tick += InteriorTpManager;
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
            // Check if the player has switched to a new vehicle.
            if (IsPedInAnyVehicle(Game.PlayerPed.Handle, true)) // added this for improved performance.
            {
                var tmpVehicle = GetVehicle();
                if (tmpVehicle != null && tmpVehicle.Exists() && tmpVehicle.Handle != LastVehicle)
                {
                    // Set the last vehicle to the new vehicle entity.
                    LastVehicle = tmpVehicle.Handle;
                    SwitchedVehicle = true;
                }
            }

            if (MenuController.IsAnyMenuOpen())
            {
                if (UpdateOnscreenKeyboard() == 0)
                {
                    await Delay(0);
                    MenuController.CloseAllMenus();
                }
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
            if (MainMenu.PlayerOptionsMenu != null && IsAllowed(Permission.POMenu))
            {
                // perms
                bool ignorePlayerAllowed = IsAllowed(Permission.POIgnored);
                bool godmodeAllowed = IsAllowed(Permission.POGod);
                bool noRagdollAllowed = IsAllowed(Permission.PONoRagdoll);
                bool vehicleGodModeAllowed = IsAllowed(Permission.VOGod);
                bool playerFrozenAllowed = IsAllowed(Permission.POFreeze);

                if (MainMenu.MpPedCustomizationMenu != null && MainMenu.MpPedCustomizationMenu.appearanceMenu != null && MainMenu.MpPedCustomizationMenu.faceShapeMenu != null && MainMenu.MpPedCustomizationMenu.createCharacterMenu != null && MainMenu.MpPedCustomizationMenu.inheritanceMenu != null && MainMenu.MpPedCustomizationMenu.propsMenu != null && MainMenu.MpPedCustomizationMenu.clothesMenu != null && MainMenu.MpPedCustomizationMenu.tattoosMenu != null)
                {
                    // Manage Player God Mode
                    bool IsMpPedCreatorOpen()
                    {
                        return
                            MainMenu.MpPedCustomizationMenu.appearanceMenu.Visible ||
                            MainMenu.MpPedCustomizationMenu.faceShapeMenu.Visible ||
                            MainMenu.MpPedCustomizationMenu.createCharacterMenu.Visible ||
                            MainMenu.MpPedCustomizationMenu.inheritanceMenu.Visible ||
                            MainMenu.MpPedCustomizationMenu.propsMenu.Visible ||
                            MainMenu.MpPedCustomizationMenu.clothesMenu.Visible ||
                            MainMenu.MpPedCustomizationMenu.tattoosMenu.Visible;
                    }
                    if (!IsMpPedCreatorOpen())
                    {
                        SetEntityInvincible(Game.PlayerPed.Handle, MainMenu.PlayerOptionsMenu.PlayerGodMode && godmodeAllowed);
                    }
                }


                // Manage Super jump.
                if (MainMenu.PlayerOptionsMenu.PlayerSuperJump && IsAllowed(Permission.POSuperjump))
                {
                    SetSuperJumpThisFrame(Game.Player.Handle);
                }

                // Manage PlayerNoRagdoll
                SetPedCanRagdoll(Game.PlayerPed.Handle, (!MainMenu.PlayerOptionsMenu.PlayerNoRagdoll && noRagdollAllowed) ||
                    (!noRagdollAllowed));


                // Fall off bike / dragged out of car.
                if (MainMenu.VehicleOptionsMenu != null)
                {
                    SetPedCanBeKnockedOffVehicle(Game.PlayerPed.Handle, (((MainMenu.PlayerOptionsMenu.PlayerNoRagdoll && noRagdollAllowed)
                        || (MainMenu.VehicleOptionsMenu.VehicleGodMode) && vehicleGodModeAllowed) ? 1 : 0));

                    SetPedCanBeDraggedOut(Game.PlayerPed.Handle, ((MainMenu.PlayerOptionsMenu.PlayerIsIgnored && ignorePlayerAllowed) ||
                        (MainMenu.VehicleOptionsMenu.VehicleGodMode && vehicleGodModeAllowed) ||
                        (MainMenu.PlayerOptionsMenu.PlayerGodMode && godmodeAllowed)));

                    SetPedCanBeShotInVehicle(Game.PlayerPed.Handle, !((MainMenu.PlayerOptionsMenu.PlayerGodMode && godmodeAllowed) ||
                        (MainMenu.VehicleOptionsMenu.VehicleGodMode && vehicleGodModeAllowed)));
                }
                else
                {
                    SetPedCanBeKnockedOffVehicle(Game.PlayerPed.Handle, ((MainMenu.PlayerOptionsMenu.PlayerNoRagdoll && noRagdollAllowed) ? 1 : 0));
                    SetPedCanBeDraggedOut(Game.PlayerPed.Handle, (MainMenu.PlayerOptionsMenu.PlayerIsIgnored && ignorePlayerAllowed));
                    SetPedCanBeShotInVehicle(Game.PlayerPed.Handle, !(MainMenu.PlayerOptionsMenu.PlayerGodMode && godmodeAllowed));
                }

                // Manage never wanted.
                if (MainMenu.PlayerOptionsMenu.PlayerNeverWanted && GetPlayerWantedLevel(Game.Player.Handle) > 0 && IsAllowed(Permission.PONeverWanted))
                {
                    ClearPlayerWantedLevel(Game.Player.Handle);
                }

                if (DriveToWpTaskActive && !Game.IsWaypointActive)
                {
                    ClearPedTasks(Game.PlayerPed.Handle);
                    Notify.Custom("Destination reached, the car will now stop driving!");
                    DriveToWpTaskActive = false;
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
            if (MainMenu.VehicleOptionsMenu != null && IsAllowed(Permission.VOMenu))
            {
                // When the player is in a valid vehicle:
                if (IsPedInAnyVehicle(Game.PlayerPed.Handle, true))
                {
                    Vehicle veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        // God mode
                        bool god = MainMenu.VehicleOptionsMenu.VehicleGodMode && IsAllowed(Permission.VOGod);
                        veh.CanBeVisiblyDamaged = !god;
                        veh.CanEngineDegrade = !god;
                        veh.CanWheelsBreak = !god;
                        veh.IsAxlesStrong = god;
                        veh.IsBulletProof = god;
                        veh.IsCollisionProof = god;
                        veh.IsExplosionProof = god;
                        veh.IsFireProof = god;
                        veh.IsInvincible = god;
                        veh.IsMeleeProof = god;
                        foreach (VehicleDoor vehicleDoor in veh.Doors.GetAll())
                        {
                            vehicleDoor.CanBeBroken = !god;
                        }
                        bool specialgod = MainMenu.VehicleOptionsMenu.VehicleSpecialGodMode && IsAllowed(Permission.VOSpecialGod);
                        if (specialgod && veh.EngineHealth < 1000)
                        {
                            veh.Repair(); // repair vehicle if special god mode is on and the vehicle is not full health.
                        }

                        // Freeze Vehicle Position (if enabled).
                        if (MainMenu.VehicleOptionsMenu.VehicleFrozen && IsAllowed(Permission.VOFreeze))
                        {
                            FreezeEntityPosition(veh.Handle, true);
                        }

                        if (MainMenu.VehicleOptionsMenu.VehicleNeverDirty && veh.DirtLevel > 0f && IsAllowed(Permission.VOKeepClean))
                        {
                            veh.Wash();
                        }

                        await Delay(0);
                        // If the torque multiplier is enabled and the player is allowed to use it.
                        if (MainMenu.VehicleOptionsMenu.VehicleTorqueMultiplier && IsAllowed(Permission.VOTorqueMultiplier))
                        {
                            // Set the torque multiplier to the selected value by the player.
                            // no need for an "else" to reset this value, because when it's not called every frame, nothing happens.
                            SetVehicleEngineTorqueMultiplier(veh.Handle, MainMenu.VehicleOptionsMenu.VehicleTorqueMultiplierAmount);
                        }
                        // If the player has switched to a new vehicle, and the vehicle engine power multiplier is turned on. Set the new value.
                        if (SwitchedVehicle)
                        {
                            // Only needs to be set once.
                            SwitchedVehicle = false;

                            // Vehicle engine power multiplier. Enable it once the player switched vehicles.
                            // Only do this if the option is enabled AND the player has permissions for it.
                            if (MainMenu.VehicleOptionsMenu.VehiclePowerMultiplier && IsAllowed(Permission.VOPowerMultiplier))
                            {
                                SetVehicleEnginePowerMultiplier(veh.Handle, MainMenu.VehicleOptionsMenu.VehiclePowerMultiplierAmount);
                            }
                            // If the player switched vehicles and the option is turned off or the player has no permissions for it
                            // Then reset the power multiplier ONCE.
                            else
                            {
                                SetVehicleEnginePowerMultiplier(veh.Handle, 1f);
                            }

                            // disable this if els compatibility is turned on.
                            if (!GetSettingsBool(Setting.vmenu_use_els_compatibility_mode))
                            {
                                // No Siren Toggle
                                veh.IsSirenSilent = MainMenu.VehicleOptionsMenu.VehicleNoSiren && IsAllowed(Permission.VONoSiren);
                            }

                            // Set the plane turbulence multiplier in case the vehicle was changed:
                            if (veh.Model.IsPlane)
                            {
                                if (MainMenu.VehicleOptionsMenu.DisablePlaneTurbulence && IsAllowed(Permission.VODisableTurbulence))
                                {
                                    SetPlaneTurbulenceMultiplier(veh.Handle, 0f);
                                }
                                else
                                {
                                    SetPlaneTurbulenceMultiplier(veh.Handle, 1.0f);
                                }
                            }

                            /// TODO: add help message on how to toggle flares/bombs.
                            /*
                            if (CanShootFlares())
                            {
                                BeginTextCommandDisplayHelp("STRING")
                            }
                            */
                        }

                        // Manage "no helmet"
                        var ped = new Ped(Game.PlayerPed.Handle);
                        // If the no helmet feature is turned on, disalbe "ped can wear helmet"
                        if (MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet && IsAllowed(Permission.VONoHelmet))
                        {
                            ped.CanWearHelmet = false;
                        }
                        // otherwise, allow helmets.
                        else if (!MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet || !IsAllowed(Permission.VONoHelmet))
                        {
                            ped.CanWearHelmet = true;
                        }
                        // If the player is still wearing a helmet, even if the option is set to: no helmet, then remove the helmet.
                        if (ped.IsWearingHelmet && MainMenu.VehicleOptionsMenu.VehicleNoBikeHelemet && IsAllowed(Permission.VONoHelmet))
                        {
                            ped.RemoveHelmet(true);
                        }

                        await Delay(0);
                    }
                }
                // When the player is not inside a vehicle:
                else
                {
                    Menu[] vehicleSubmenus = new Menu[6];
                    vehicleSubmenus[0] = MainMenu.VehicleOptionsMenu.VehicleModMenu;
                    vehicleSubmenus[1] = MainMenu.VehicleOptionsMenu.VehicleLiveriesMenu;
                    vehicleSubmenus[2] = MainMenu.VehicleOptionsMenu.VehicleColorsMenu;
                    vehicleSubmenus[3] = MainMenu.VehicleOptionsMenu.VehicleDoorsMenu;
                    vehicleSubmenus[4] = MainMenu.VehicleOptionsMenu.VehicleWindowsMenu;
                    vehicleSubmenus[5] = MainMenu.VehicleOptionsMenu.VehicleComponentsMenu;
                    foreach (Menu m in vehicleSubmenus)
                    {
                        if (m.Visible)
                        {
                            MainMenu.VehicleOptionsMenu.GetMenu().OpenMenu();
                            m.CloseMenu();
                            Notify.Error(CommonErrors.NoVehicle, placeholderValue: "to access this menu");
                        }
                    }
                }

                await Delay(1);

                // Manage vehicle engine always on.
                if (MainMenu.VehicleOptionsMenu.VehicleEngineAlwaysOn && GetVehicle(true) != null && GetVehicle(true).Exists() && !Game.PlayerPed.IsInVehicle() && IsAllowed(Permission.VOEngineAlwaysOn))
                {
                    await Delay(100);
                    if (GetVehicle(true) != null)
                        SetVehicleEngineOn(GetVehicle(true).Handle, true, true, true);
                }

            }
            else
            {
                await Delay(1);
            }
        }
        private async Task MoreVehicleOptions()
        {
            if (MainMenu.VehicleOptionsMenu != null && IsPedInAnyVehicle(Game.PlayerPed.Handle, true) && MainMenu.VehicleOptionsMenu.FlashHighbeamsOnHonk && IsAllowed(Permission.VOFlashHighbeamsOnHonk))
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    if (veh.Driver == Game.PlayerPed && veh.IsEngineRunning && !IsPauseMenuActive())
                    {
                        // turn on high beams when honking.
                        if (Game.IsControlPressed(0, Control.VehicleHorn))
                        {
                            veh.AreHighBeamsOn = true;
                        }
                        // turn high beams back off when just stopped honking.
                        if (Game.IsControlJustReleased(0, Control.VehicleHorn))
                        {
                            veh.AreHighBeamsOn = false;
                        }
                    }
                    else
                    {
                        await Delay(1);
                    }
                }
                else
                {
                    await Delay(1);
                }
            }
            else
            {
                await Delay(1);
            }

        }
        #endregion
        #region Weather Options
        private async Task WeatherOptions()
        {
            await Delay(1000);
            if (MainMenu.WeatherOptionsMenu != null && IsAllowed(Permission.WOMenu) && GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                if (MainMenu.WeatherOptionsMenu.GetMenu().Visible)
                {
                    MainMenu.WeatherOptionsMenu.GetMenu().GetMenuItems().ForEach(mi => { if (mi.GetType() != typeof(MenuCheckboxItem)) mi.RightIcon = MenuItem.Icon.NONE; });
                    var item = vMenuClient.WeatherOptions.weatherHashMenuIndex[GetNextWeatherTypeHashName().ToString()];
                    item.RightIcon = MenuItem.Icon.TICK;
                    if (IsAllowed(Permission.WODynamic))
                    {
                        MenuCheckboxItem dynWeatherTmp = (MenuCheckboxItem)MainMenu.WeatherOptionsMenu.GetMenu().GetMenuItems()[0];
                        dynWeatherTmp.Checked = EventManager.dynamicWeather;
                        if (IsAllowed(Permission.WOBlackout))
                        {
                            MenuCheckboxItem blackoutTmp = (MenuCheckboxItem)MainMenu.WeatherOptionsMenu.GetMenu().GetMenuItems()[1];
                            blackoutTmp.Checked = EventManager.blackoutMode;
                        }
                    }
                    else if (IsAllowed(Permission.WOBlackout))
                    {
                        MenuCheckboxItem blackoutTmp = (MenuCheckboxItem)MainMenu.WeatherOptionsMenu.GetMenu().GetMenuItems()[0];
                        blackoutTmp.Checked = EventManager.blackoutMode;
                    }


                }
            }
        }
        #endregion
        #region Misc Settings Menu Tasks
        private async void DrawMiscSettingsText()
        {
            if (MainMenu.MiscSettingsMenu != null)
            {

                // draw coordinates
                if (MainMenu.MiscSettingsMenu.ShowCoordinates && IsAllowed(Permission.MSShowCoordinates))
                {
                    var pos = Game.PlayerPed.Position;
                    double x = Math.Round(pos.X, 2), y = Math.Round(pos.Y, 2), z = Math.Round(pos.Z, 2), heading = Math.Round(Game.PlayerPed.Heading, 2);
                    SetScriptGfxAlign(0, 84);
                    SetScriptGfxAlignParams(0f, 0f, 0f, 0f);
                    DrawTextOnScreen($"~r~X~s~ \t\t{x}\n~r~Y~s~ \t\t{y}\n~r~Z~s~ \t\t{z}\n~r~Heading~s~ \t{heading}", 0.5f - (30f / Resolution.Width), 0f, 0.5f, Alignment.Left, 6, false);
                    ResetScriptGfxAlign();

                }

                // draw location
                if (MainMenu.MiscSettingsMenu.ShowLocation && IsAllowed(Permission.MSShowLocation))
                {
                    SetScriptGfxAlign(0, 84);
                    SetScriptGfxAlignParams(0f, 0f, 0f, 0f);
                    ShowLocation();
                    ResetScriptGfxAlign();
                }

                // draw time
                if (MainMenu.MiscSettingsMenu.DrawTimeOnScreen)
                {
                    int hour = World.CurrentDayTime.Hours;
                    int minute = World.CurrentDayTime.Minutes;
                    string timestring = $"{(hour < 10 ? "0" + hour.ToString() : hour.ToString())}:{(minute < 10 ? "0" + minute.ToString() : minute.ToString())}";
                    SetScriptGfxAlign(0, 84);
                    SetScriptGfxAlignParams(0f, 0f, 0f, 0f);
                    DrawTextOnScreen($"~c~{timestring}", 0.208f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(0.4f, 1), 0.40f, Alignment.Center);
                    ResetScriptGfxAlign();
                }

                if (MainMenu.MiscSettingsMenu.ShowSpeedoKmh && Game.PlayerPed.IsInVehicle())
                {
                    ShowSpeedKmh();
                }

                if (MainMenu.MiscSettingsMenu.ShowSpeedoMph && Game.PlayerPed.IsInVehicle())
                {
                    ShowSpeedMph();
                }
            }
            else
            {
                await Delay(0);
            }

        }

        #region Update Location for location display
        /// <summary>
        /// Updates the location for location display.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateLocation()
        {
            if (MainMenu.MiscSettingsMenu != null)
            {
                if (MainMenu.MiscSettingsMenu.ShowLocation && IsAllowed(Permission.MSShowLocation))
                {
                    // Get the current location.
                    currentPos = GetEntityCoords(Game.PlayerPed.Handle, true);

                    // Get the nearest vehicle node.
                    nodePos = currentPos;
                    node = GetNthClosestVehicleNode(currentPos.X, currentPos.Y, currentPos.Z, 0, ref nodePos, 0, 0, 0);
                    heading = Game.PlayerPed.Heading;

                    // Get the safezone size for x and y to be able to move with the minimap.
                    safeZoneSizeX = (1 / GetSafeZoneSize() / 3.0f) - 0.358f;
                    safeZoneSizeY = GetSafeZoneSize() - 0.27f;
                    //safeZoneSizeY = (1 / GetSafeZoneSize() / 3.6f) - 0.27f;

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
                    await Delay(1000);
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
            SetTextWrap(0f, 1f);
            DrawTextOnScreen(prefix + World.GetStreetName(currentPos) + suffix, 0.234f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(0.48f, 6) - GetTextScaleHeight(0.48f, 6)/*0.925f - safeZoneSizeY*/, 0.48f);

            // Draw the zone name.
            SetTextWrap(0f, 1f);
            DrawTextOnScreen(World.GetZoneLocalizedName(currentPos), 0.234f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(0.45f, 6) - GetTextScaleHeight(0.95f, 6)/*0.9485f - safeZoneSizeY*/, 0.45f);

            // Draw the left border for the heading character.
            SetTextWrap(0f, 1f);
            DrawTextOnScreen("~t~|", 0.188f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(1.2f, 6) - GetTextScaleHeight(0.4f, 6)/*0.915f - safeZoneSizeY*/, 1.2f, Alignment.Left);

            // Draw the heading character.
            SetTextWrap(0f, 1f);
            DrawTextOnScreen(headingCharacter, 0.208f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(1.2f, 6) - GetTextScaleHeight(0.4f, 6)/*0.915f - safeZoneSizeY*/, 1.2f, Alignment.Center);

            // Draw the right border for the heading character.
            SetTextWrap(0f, 1f);
            DrawTextOnScreen("~t~|", 0.228f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(1.2f, 6) - GetTextScaleHeight(0.4f, 6)/*0.915f - safeZoneSizeY*/, 1.2f, Alignment.Right);
        }
        #endregion

        #region Private ShowSpeed Functions
        /// <summary>
        /// Shows the current speed in km/h.
        /// Must be in a vehicle.
        /// </summary>
        private void ShowSpeedKmh()
        {
            int speed = int.Parse(Math.Round(GetEntitySpeed(GetVehicle().Handle) * 3.6f).ToString());
            DrawTextOnScreen($"{speed} KM/h", 0.995f, 0.955f, 0.7f, Alignment.Right, 4);
        }

        /// <summary>
        /// Shows the current speed in mph.
        /// Must be in a vehicle.
        /// </summary>
        private void ShowSpeedMph()
        {
            var speed = Math.Round(GetEntitySpeed(GetVehicle().Handle) * 2.23694f);

            if (MainMenu.MiscSettingsMenu.ShowSpeedoKmh)
            {
                DrawTextOnScreen($"{speed} MPH", 0.995f, 0.925f, 0.7f, Alignment.Right, 4);
                HideHudComponentThisFrame((int)HudComponent.StreetName);
            }
            else
            {
                DrawTextOnScreen($"{speed} MPH", 0.995f, 0.955f, 0.7f, Alignment.Right, 4);
            }
        }
        #endregion

        /// <summary>
        /// Run all tasks that need to be handeled for the Misc Settings Menu.
        /// </summary>
        /// <returns></returns>
        private async Task MiscSettings()
        {
            if (MainMenu.MiscSettingsMenu != null)
            {

                DrawMiscSettingsText();

                #region Misc Settings
                // Hide radar.
                if (MainMenu.MiscSettingsMenu.HideRadar)
                {
                    DisplayRadar(false);
                }
                // Show radar (or hide it if the user disabled it in pausemenu > settings > display > show radar.
                else if (!IsRadarHidden()) // this should allow other resources to still disable it
                {
                    DisplayRadar(IsRadarPreferenceSwitchedOn());
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

                if (MainMenu.MiscSettingsMenu.KbTpToWaypoint)
                {
                    if (IsAllowed(Permission.MSTeleportToWp))
                    {
                        if (Game.IsControlJustReleased(0, (Control)MainMenu.MiscSettingsMenu.KbTpToWaypointKey)
                            && Fading.IsFadedIn
                            && !IsPlayerSwitchInProgress()
                            && Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                        {
                            if (Game.IsWaypointActive)
                            {
                                TeleportToWp();
                                Notify.Success("Teleported to waypoint.");
                            }
                            else
                            {
                                Notify.Error("You need to set a waypoint first.");
                            }
                        }
                    }
                }
                if (MainMenu.MiscSettingsMenu.KbDriftMode)
                {
                    if (IsAllowed(Permission.MSDriftMode))
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            Vehicle veh = GetVehicle();
                            if (veh != null && veh.Exists() && !veh.IsDead)
                            {
                                if ((Game.IsControlPressed(0, Control.Sprint) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                                    (Game.IsControlPressed(0, Control.Jump) && Game.CurrentInputMode == InputMode.GamePad))
                                {
                                    SetVehicleReduceGrip(veh.Handle, true);
                                }
                                else
                                if ((Game.IsControlJustReleased(0, Control.Sprint) && Game.CurrentInputMode == InputMode.MouseAndKeyboard) ||
                                    (Game.IsControlJustReleased(0, Control.Jump) && Game.CurrentInputMode == InputMode.GamePad))
                                {
                                    SetVehicleReduceGrip(veh.Handle, false);
                                }
                            }
                        }
                    }
                }
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
                if (MainMenu.MiscSettingsMenu.JoinQuitNotifications && IsAllowed(Permission.MSJoinQuitNotifs))
                {
                    PlayerList plist = Players;
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
                                Notify.Custom($"~g~<C>{GetSafePlayerName(player.Value)}</C>~s~ joined the server.");
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
                                Notify.Custom($"~r~<C>{GetSafePlayerName(player.Value)}</C>~s~ left the server.");
                                await Delay(0);
                            }
                        }
                    }
                    playerList = pl;
                    await Delay(100);
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
                if (MainMenu.MiscSettingsMenu.DeathNotifications && IsAllowed(Permission.MSDeathNotifs))
                {
                    PlayerList pl = Players;
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
                                        if (killer.Model.IsPed)
                                        {
                                            bool found = false;
                                            foreach (Player playerKiller in pl)
                                            {
                                                if (playerKiller.Character.Handle == killer.Handle)
                                                {
                                                    Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~has been murdered by ~y~<C>{GetSafePlayerName(playerKiller.Name)}</C>~s~.");
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (!found)
                                            {
                                                Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~has been murdered.");
                                            }
                                        }
                                        else if (killer.Model.IsVehicle)
                                        {
                                            bool found = false;
                                            foreach (Player playerKiller in pl)
                                            {
                                                if (playerKiller.Character.IsInVehicle())
                                                {
                                                    if (playerKiller.Character.CurrentVehicle.Handle == killer.Handle)
                                                    {
                                                        Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~has been murdered by ~y~<C>{GetSafePlayerName(playerKiller.Name)}</C>~s~.");
                                                        found = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!found)
                                            {
                                                Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~has been murdered.");
                                            }
                                        }
                                        else
                                        {
                                            Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~has been murdered.");
                                        }
                                    }
                                    else
                                    {
                                        Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~has been murdered.");
                                    }
                                }
                                else
                                {
                                    Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~committed suicide.");
                                }
                            }
                            else
                            {
                                Notify.Custom($"~o~<C>{GetSafePlayerName(p.Name)}</C> ~s~died.");
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
                    await Delay(50);
                }
            }
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
            if (MainMenu.VoiceChatSettingsMenu != null && IsAllowed(Permission.VCMenu))
            {
                if (MainMenu.VoiceChatSettingsMenu.EnableVoicechat && IsAllowed(Permission.VCEnable))
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
                    if (MainMenu.VoiceChatSettingsMenu.ShowCurrentSpeaker && IsAllowed(Permission.VCShowSpeaker))
                    {
                        PlayerList pl = Players;
                        var i = 1;
                        var currentlyTalking = false;
                        foreach (Player p in pl)
                        {
                            if (NetworkIsPlayerTalking(p.Handle))
                            {
                                if (!currentlyTalking)
                                {
                                    DrawTextOnScreen("~s~Currently Talking", 0.5f, 0.00f, 0.5f, Alignment.Center, 6);
                                    currentlyTalking = true;
                                }
                                DrawTextOnScreen($"~b~{p.Name}", 0.5f, 0.00f + (i * 0.03f), 0.5f, Alignment.Center, 6);
                                i++;
                            }
                        }
                    }
                    if (MainMenu.VoiceChatSettingsMenu.ShowVoiceStatus)
                    {

                        if (GetGameTimer() - voiceTimer > 150)
                        {
                            voiceTimer = GetGameTimer();
                            voiceCycle++;
                            if (voiceCycle > 3)
                            {
                                voiceCycle = 1;
                            }
                        }
                        if (!HasStreamedTextureDictLoaded("mpleaderboard"))
                        {
                            RequestStreamedTextureDict("mpleaderboard", false);
                            while (!HasStreamedTextureDictLoaded("mpleaderboard"))
                            {
                                await Delay(0);
                            }
                        }
                        if (NetworkIsPlayerTalking(Game.Player.Handle))
                        {
                            DrawSprite("mpleaderboard", $"leaderboard_audio_{voiceCycle}", 0.008f, 0.985f, voiceIndicatorWidth, voiceIndicatorHeight, 0f, 255, 55, 0, 255);
                        }
                        else
                        {
                            DrawSprite("mpleaderboard", "leaderboard_audio_mute", 0.008f, 0.985f, voiceIndicatorMutedWidth, voiceIndicatorHeight, 0f, 255, 55, 0, 255);
                        }
                    }
                    else
                    {
                        if (HasStreamedTextureDictLoaded("mpleaderboard"))
                        {
                            SetStreamedTextureDictAsNoLongerNeeded("mpleaderboard");
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
            if (MainMenu.TimeOptionsMenu != null && IsAllowed(Permission.TOMenu) && GetSettingsBool(Setting.vmenu_enable_time_sync))
            {
                if ((MainMenu.TimeOptionsMenu.freezeTimeToggle != null && MainMenu.TimeOptionsMenu.GetMenu().Visible) && IsAllowed(Permission.TOFreezeTime))
                {
                    // Update the current time displayed in the Time Options menu (only when the menu is actually visible).
                    var hours = GetClockHours();
                    var minutes = GetClockMinutes();
                    var hoursString = hours < 10 ? "0" + hours.ToString() : hours.ToString();
                    var minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
                    MainMenu.TimeOptionsMenu.freezeTimeToggle.Label = $"(Current Time {hoursString}:{minutesString})";
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
            if (MainMenu.WeaponOptionsMenu != null && IsAllowed(Permission.WPMenu))
            {
                // If no reload is enabled.
                if (MainMenu.WeaponOptionsMenu.NoReload && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Minigun && IsAllowed(Permission.WPNoReload))
                {
                    // Disable reloading.
                    //PedSkipNextReloading(Game.PlayerPed.Handle);
                    SetAmmoInClip(Game.PlayerPed.Handle, (uint)Game.PlayerPed.Weapons.Current.Hash, 5);
                }

                // Enable/disable infinite ammo.
                //SetPedInfiniteAmmoClip(Game.PlayerPed.Handle, MainMenu.WeaponOptionsMenu.UnlimitedAmmo && CommonFunctions.IsAllowed(Permission.WPUnlimitedAmmo));
                if (Game.PlayerPed.Weapons.Current != null && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
                {
                    Game.PlayerPed.Weapons.Current.InfiniteAmmo = MainMenu.WeaponOptionsMenu.UnlimitedAmmo && IsAllowed(Permission.WPUnlimitedAmmo);
                }


                /// THIS SOLUTION IS BUGGED AND CAUSES CRASHES
                //// workaround for mk2 weapons (the infinite ammo doesn't seem to work all the time for mk2 weapons)
                //if (MainMenu.WeaponOptionsMenu.UnlimitedAmmo && CommonFunctions.IsAllowed(Permission.WPUnlimitedAmmo) && Game.PlayerPed.Weapons.Current.IsMk2 &&
                //    Game.PlayerPed.Weapons.Current.Ammo != Game.PlayerPed.Weapons.Current.MaxAmmo)
                //{
                //    Game.PlayerPed.Weapons.Current.Ammo = Game.PlayerPed.Weapons.Current.MaxAmmo;
                //}

                if (MainMenu.WeaponOptionsMenu.AutoEquipChute)
                {
                    if ((IsPedInAnyHeli(Game.PlayerPed.Handle) || IsPedInAnyPlane(Game.PlayerPed.Handle)) && !HasPedGotWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, false))
                    {
                        GiveWeaponToPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, 1, false, true);
                        SetPlayerHasReserveParachute(Game.Player.Handle);
                        SetPlayerCanLeaveParachuteSmokeTrail(Game.PlayerPed.Handle, true);
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
            if (MainMenu.OnlinePlayersMenu != null && IsAllowed(Permission.OPMenu) && IsAllowed(Permission.OPSpectate))
            {
                // When the player dies while spectating, cancel the spectating to prevent an infinite black loading screen.
                if (GetEntityHealth(Game.PlayerPed.Handle) < 1 && NetworkIsInSpectatorMode())
                {
                    DoScreenFadeOut(50);
                    await Delay(50);
                    NetworkSetInSpectatorMode(true, Game.PlayerPed.Handle);
                    NetworkSetInSpectatorMode(false, Game.PlayerPed.Handle);

                    await Delay(50);
                    DoScreenFadeIn(50);
                    while (GetEntityHealth(Game.PlayerPed.Handle) < 1)
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


        /// <summary>
        /// Manages the camera view for when the mp ped creator is open.
        /// </summary>
        /// <returns></returns>
        private async Task ManagePlayerAppearanceCamera()
        {
            if (MainMenu.MpPedCustomizationMenu != null)
            {
                var menu = MainMenu.MpPedCustomizationMenu.GetMenu();

                bool IsOpen()
                {
                    return
                        MainMenu.MpPedCustomizationMenu.appearanceMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.faceShapeMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.createCharacterMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.inheritanceMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.propsMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.clothesMenu.Visible ||
                        MainMenu.MpPedCustomizationMenu.tattoosMenu.Visible;
                }

                if (IsOpen())
                {
                    List<KeyValuePair<Vector3, Vector3>> camPositions = new List<KeyValuePair<Vector3, Vector3>>()
                    {
                        // normal variants
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 1.8f, 0.2f)), Game.PlayerPed.Position + new Vector3(0f, 0f, 0.0f)),     // default 0
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 0.5f, 0.65f)), Game.PlayerPed.Position + new Vector3(0f, 0f, 0.65f)),   // head 1
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 1.2f, 0.40f)), Game.PlayerPed.Position + new Vector3(0f, 0f, 0.35f)), // upper body 2
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 1.3f, -0.2f)), Game.PlayerPed.Position + new Vector3(0f, 0f, -0.25f)), // lower body 3
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 0.7f, -0.5f)), Game.PlayerPed.Position + new Vector3(0f, 0f, -0.8f)), // shoes 4
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(-0.4f, 0.7f, -0.1f)), Game.PlayerPed.Position + new Vector3(0f, -0.1f, -0.25f)), // left wrist 5
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0.4f, 0.7f, -0.1f)), Game.PlayerPed.Position + new Vector3(0f, -0.1f, -0.25f)), // right wrist 6

                        // tattoo turn left variants
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(-0.4f, 0.5f, 0.65f)), Game.PlayerPed.Position + new Vector3(0f, 0f, 0.65f)), // head 7
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(-0.7f, 1.2f, 0.40f)), Game.PlayerPed.Position + new Vector3(0f, 0f, 0.35f)), // head 8
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(-0.7f, 1.3f, -0.2f)), Game.PlayerPed.Position + new Vector3(0f, 0f, -0.25f)), // head 9

                        // tattoo turn right variants
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0.4f, 0.5f, 0.65f)), Game.PlayerPed.Position + new Vector3(0f, 0f, 0.65f)), // head 10
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0.7f, 1.2f, 0.40f)), Game.PlayerPed.Position + new Vector3(0f, 0f, 0.35f)), // head 11
                        new KeyValuePair<Vector3, Vector3>(Game.PlayerPed.GetOffsetPosition(new Vector3(0.7f, 1.3f, -0.2f)), Game.PlayerPed.Position + new Vector3(0f, 0f, -0.25f)), // head 12
                    };

                    int cam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
                    Camera camera = new Camera(cam);

                    Game.PlayerPed.Task.ClearAllImmediately();

                    /* 
                     * Camera positions and PointAt locations.
                    
                    // head close up
                    camera.Position = Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 0.5f, 0.65f));
                    camera.PointAt(Game.PlayerPed.Position + new Vector3(0f, 0f, 0.65f));
                    
                    // upper body close up
                    camera.Position = Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 1.2f, 0.40f));
                    camera.PointAt(Game.PlayerPed.Position + new Vector3(0f, 0f, 0.35f));
                    
                    // lower body close up
                    camera.Position = Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 1.3f, -0.2f));
                    camera.PointAt(Game.PlayerPed.Position + new Vector3(0f, 0f, -0.25f));
                    
                    // very low (feet level) very close up
                    camera.Position = Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 0.7f, -0.5f));
                    camera.PointAt(Game.PlayerPed.Position + new Vector3(0f, 0f, -0.8f));
                    
                    // default normal height full character visible.
                    camera.Position = Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 1.8f, 0.2f));
                    camera.PointAt(Game.PlayerPed.Position + new Vector3(0f, 0f, 0.0f));
                    
                    */

                    bool rearCamActive = false;

                    void SetCameraPosition()
                    {
                        if (MainMenu.MpPedCustomizationMenu.appearanceMenu.Visible)
                        {
                            int index = MainMenu.MpPedCustomizationMenu.appearanceMenu.CurrentIndex;
                            switch (index)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                case 15:
                                case 16:
                                case 17:
                                case 18:
                                case 19:
                                case 20:
                                case 21:
                                case 22:
                                case 23:
                                case 24:
                                case 25:
                                case 26:
                                case 27:
                                case 31:
                                    // close up head.
                                    camera.Position = camPositions[1].Key;
                                    camera.PointAt(camPositions[1].Value);
                                    break;
                                case 28:
                                case 29:
                                case 30:
                                    // torso
                                    camera.Position = camPositions[2].Key;
                                    camera.PointAt(camPositions[2].Value);
                                    break;
                                default:
                                    // normal position (full character visible)
                                    camera.Position = camPositions[0].Key;
                                    camera.PointAt(camPositions[0].Value);
                                    break;
                            }
                        }
                        else if (MainMenu.MpPedCustomizationMenu.clothesMenu.Visible)
                        {
                            int index = MainMenu.MpPedCustomizationMenu.clothesMenu.CurrentIndex;
                            switch (index)
                            {
                                case 0:
                                    // head level
                                    camera.Position = camPositions[1].Key;
                                    camera.PointAt(camPositions[1].Value);
                                    break;
                                case 1:
                                case 3:
                                case 5:
                                case 6:
                                case 7:
                                case 9:
                                    // upper body level
                                    camera.Position = camPositions[2].Key;
                                    camera.PointAt(camPositions[2].Value);
                                    break;
                                case 2:
                                    // lower body level
                                    camera.Position = camPositions[3].Key;
                                    camera.PointAt(camPositions[3].Value);
                                    break;
                                case 4:
                                    // feet (ground) level (close up)
                                    camera.Position = camPositions[4].Key;
                                    camera.PointAt(camPositions[4].Value);
                                    break;
                                case 8:
                                default:
                                    // normal position (full character visible)
                                    camera.Position = camPositions[0].Key;
                                    camera.PointAt(camPositions[0].Value);
                                    break;
                            }
                        }
                        else if (MainMenu.MpPedCustomizationMenu.inheritanceMenu.Visible)
                        {
                            // head level
                            camera.Position = camPositions[1].Key;
                            camera.PointAt(camPositions[1].Value);
                        }
                        else if (MainMenu.MpPedCustomizationMenu.propsMenu.Visible)
                        {
                            int index = MainMenu.MpPedCustomizationMenu.propsMenu.CurrentIndex;
                            switch (index)
                            {
                                case 0:
                                case 1:
                                case 2:
                                    // head level
                                    camera.Position = camPositions[1].Key;
                                    camera.PointAt(camPositions[1].Value);
                                    break;
                                case 3:
                                    // left wrist
                                    if (rearCamActive)
                                    {
                                        camera.Position = camPositions[6].Key;
                                        camera.PointAt(camPositions[6].Value);
                                    }
                                    else
                                    {
                                        camera.Position = camPositions[5].Key;
                                        camera.PointAt(camPositions[5].Value);
                                    }
                                    break;
                                case 4:
                                    // right wrist
                                    if (rearCamActive)
                                    {
                                        camera.Position = camPositions[5].Key;
                                        camera.PointAt(camPositions[5].Value);
                                    }
                                    else
                                    {
                                        camera.Position = camPositions[6].Key;
                                        camera.PointAt(camPositions[6].Value);
                                    }
                                    break;
                                default:
                                    // normal position (full character visible)
                                    camera.Position = camPositions[0].Key;
                                    camera.PointAt(camPositions[0].Value);
                                    break;
                            }
                        }
                        // face shape
                        else if (MainMenu.MpPedCustomizationMenu.faceShapeMenu.Visible)
                        {
                            camera.Position = camPositions[1].Key;
                            camera.PointAt(camPositions[1].Value);
                        }
                        // tattoos
                        else if (MainMenu.MpPedCustomizationMenu.tattoosMenu.Visible)
                        {
                            int index = MainMenu.MpPedCustomizationMenu.tattoosMenu.CurrentIndex;
                            switch (index)
                            {
                                case 0:
                                    // head level
                                    if (Game.IsControlPressed(0, Control.ParachuteBrakeRight)) // turn camera to the right
                                    {
                                        camera.Position = camPositions[7].Key;
                                        camera.PointAt(camPositions[7].Value);
                                    }
                                    else if (Game.IsControlPressed(0, Control.ParachuteBrakeLeft)) // turn camera to the left
                                    {
                                        camera.Position = camPositions[10].Key;
                                        camera.PointAt(camPositions[10].Value);
                                    }
                                    else // normal
                                    {
                                        camera.Position = camPositions[1].Key;
                                        camera.PointAt(camPositions[1].Value);
                                    }
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                    // upper body level
                                    if (Game.IsControlPressed(0, Control.ParachuteBrakeRight)) // turn camera to the right
                                    {
                                        camera.Position = camPositions[8].Key;
                                        camera.PointAt(camPositions[8].Value);
                                    }
                                    else if (Game.IsControlPressed(0, Control.ParachuteBrakeLeft)) // turn camera to the left
                                    {
                                        camera.Position = camPositions[11].Key;
                                        camera.PointAt(camPositions[11].Value);
                                    }
                                    else // normal
                                    {
                                        camera.Position = camPositions[2].Key;
                                        camera.PointAt(camPositions[2].Value);
                                    }
                                    break;
                                case 4:
                                case 5:
                                    // lower body level
                                    if (Game.IsControlPressed(0, Control.ParachuteBrakeRight)) // turn camera to the right
                                    {
                                        camera.Position = camPositions[9].Key;
                                        camera.PointAt(camPositions[9].Value);
                                    }
                                    else if (Game.IsControlPressed(0, Control.ParachuteBrakeLeft)) // turn camera to the left
                                    {
                                        camera.Position = camPositions[12].Key;
                                        camera.PointAt(camPositions[12].Value);
                                    }
                                    else // normal
                                    {
                                        camera.Position = camPositions[3].Key;
                                        camera.PointAt(camPositions[3].Value);
                                    }
                                    break;
                                default:
                                    // normal position (full character visible)
                                    camera.Position = camPositions[0].Key;
                                    camera.PointAt(camPositions[0].Value);
                                    break;
                            }
                        }
                        else
                        {
                            if (MainMenu.MpPedCustomizationMenu.createCharacterMenu.Visible && MainMenu.MpPedCustomizationMenu.createCharacterMenu.CurrentIndex == 6)
                            {
                                // head level
                                camera.Position = camPositions[1].Key;
                                camera.PointAt(camPositions[1].Value);
                            }
                            else
                            {
                                camera.Position = camPositions[0].Key;
                                camera.PointAt(camPositions[0].Value);
                            }

                        }
                    }

                    float heading = Game.PlayerPed.Heading;

                    while (IsOpen())
                    {
                        await Delay(0);
                        DisplayRadar(false);

                        SetEntityInvincible(Game.PlayerPed.Handle, true);
                        SetEntityCollision(Game.PlayerPed.Handle, false, false);

                        RenderScriptCams(true, false, 0, true, false);
                        if (Game.IsControlJustReleased(0, Control.PhoneExtraOption))
                        {
                            var Pos = Game.PlayerPed.Position;
                            if (rearCamActive)
                            {
                                SetEntityCollision(Game.PlayerPed.Handle, true, true);
                                FreezeEntityPosition(Game.PlayerPed.Handle, false);
                                TaskGoStraightToCoord(Game.PlayerPed.Handle, Pos.X, Pos.Y, Pos.Z, 8f, 1600, heading, 0.1f);
                                int timer = GetGameTimer();
                                while (true)
                                {
                                    await Delay(0);
                                    DisplayRadar(false);
                                    //CommonFunctions.DisableMovementControlsThisFrame(true, true);
                                    Game.DisableAllControlsThisFrame(0);
                                    if (GetGameTimer() - timer > 1600)
                                    {
                                        break;
                                    }
                                }
                                ClearPedTasks(Game.PlayerPed.Handle);
                                Game.PlayerPed.PositionNoOffset = Pos;
                                FreezeEntityPosition(Game.PlayerPed.Handle, true);
                                SetEntityCollision(Game.PlayerPed.Handle, false, false);
                            }
                            else
                            {
                                SetEntityCollision(Game.PlayerPed.Handle, true, true);
                                FreezeEntityPosition(Game.PlayerPed.Handle, false);
                                TaskGoStraightToCoord(Game.PlayerPed.Handle, Pos.X, Pos.Y, Pos.Z, 8f, 1600, heading - 180f, 0.1f);
                                int timer = GetGameTimer();
                                while (true)
                                {
                                    await Delay(0);
                                    DisplayRadar(false);
                                    Game.DisableAllControlsThisFrame(0);
                                    if (GetGameTimer() - timer > 1600)
                                    {
                                        break;
                                    }
                                }
                                ClearPedTasks(Game.PlayerPed.Handle);
                                Game.PlayerPed.PositionNoOffset = Pos;
                                FreezeEntityPosition(Game.PlayerPed.Handle, true);
                                SetEntityCollision(Game.PlayerPed.Handle, false, false);
                            }
                            rearCamActive = !rearCamActive;
                        }

                        FreezeEntityPosition(Game.PlayerPed.Handle, true);

                        DisableMovementControlsThisFrame(true, true);

                        SetCameraPosition();

                        Game.PlayerPed.Task.ClearAll();

                        var offsetRight = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, -2f, 0.05f, 0.7f);
                        var offsetLeft = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 2f, 0.05f, 0.7f);

                        if (Game.IsDisabledControlPressed(0, Control.MoveRight))
                        {
                            Game.PlayerPed.Task.LookAt(offsetRight, 100);
                        }
                        else if (Game.IsDisabledControlPressed(0, Control.MoveLeftOnly))
                        {
                            Game.PlayerPed.Task.LookAt(offsetLeft, 100);
                        }
                        else
                        {
                            float input = GetDisabledControlNormal(0, 1);

                            if (input > 0.5f)
                            {
                                Game.PlayerPed.Task.LookAt(offsetRight, 100);
                            }
                            else if (input < -0.5f)
                            {
                                Game.PlayerPed.Task.LookAt(offsetLeft, 100);
                            }
                            else
                            {
                                if (MainMenu.MpPedCustomizationMenu.tattoosMenu.Visible)
                                {
                                    Game.PlayerPed.Task.LookAt(Game.PlayerPed.GetOffsetPosition(new Vector3(0f, -3f, 0f)), 100);
                                }
                                else
                                {
                                    Game.PlayerPed.Task.LookAt(camera.Position, 100);
                                }

                            }
                        }
                    }
                    DisplayRadar(IsRadarPreferenceSwitchedOn());
                    RenderScriptCams(false, false, 0, false, false);
                    camera.Delete();
                    DestroyAllCams(true);
                    ClearPedTasksImmediately(Game.PlayerPed.Handle);
                    SetEntityCollision(Game.PlayerPed.Handle, true, true);
                    FreezeEntityPosition(Game.PlayerPed.Handle, false);
                    SetEntityInvincible(Game.PlayerPed.Handle, false);

                }
            }
        }
        #endregion
        #region Restore player skin & weapons after respawning.
        private async Task RestorePlayerAfterBeingDead()
        {
            if (Game.PlayerPed.IsDead)
            {
                if (MainMenu.MiscSettingsMenu != null)
                {
                    bool restoreDefault = false;
                    if (MainMenu.MiscSettingsMenu.MiscRespawnDefaultCharacter && !GetSettingsBool(Setting.vmenu_disable_spawning_as_default_character))
                    {
                        if (!string.IsNullOrEmpty(GetResourceKvpString("vmenu_default_character")))
                        {
                            restoreDefault = true;
                        }
                        else
                        {
                            Notify.Error("You did not set a saved character to restore to. Do so in the ~g~MP Ped Customization~s~ > ~g~Saved Characters~s~ menu.");
                        }
                    }
                    if (!restoreDefault)
                    {
                        if (MainMenu.MiscSettingsMenu.RestorePlayerAppearance && IsAllowed(Permission.MSRestoreAppearance))
                        {
                            SavePed("vMenu_tmp_saved_ped");
                        }
                    }

                    if (MainMenu.MiscSettingsMenu.RestorePlayerWeapons && IsAllowed(Permission.MSRestoreWeapons) || (MainMenu.WeaponLoadoutsMenu != null && MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn && IsAllowed(Permission.WLEquipOnRespawn)))
                    {
                        //await SaveWeaponLoadout();
                        if (SaveWeaponLoadout("vmenu_temp_weapons_loadout_before_respawn"))
                        {
                            Log($"weapons saved {GetResourceKvpString("vmenu_temp_weapons_loadout_before_respawn")}");
                        }
                        else
                        {
                            Log("save failed from restore weapons after death");
                        }
                    }

                    while (Game.PlayerPed.IsDead || IsScreenFadedOut() || IsScreenFadingOut() || IsScreenFadingIn())
                    {
                        await Delay(0);
                    }

                    if (restoreDefault)
                    {
                        await MainMenu.MpPedCustomizationMenu.SpawnThisCharacter(GetResourceKvpString("vmenu_default_character"), false);
                    }
                    else
                    {
                        if (IsTempPedSaved() && MainMenu.MiscSettingsMenu.RestorePlayerAppearance && IsAllowed(Permission.MSRestoreAppearance))
                        {
                            LoadSavedPed("vMenu_tmp_saved_ped", false);
                        }
                    }

                    if (MainMenu.MiscSettingsMenu != null && MainMenu.MiscSettingsMenu.RestorePlayerWeapons && IsAllowed(Permission.MSRestoreWeapons) || (MainMenu.WeaponLoadoutsMenu != null && MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn && IsAllowed(Permission.WLEquipOnRespawn)))
                    {
                        await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", true, false);
                        Log("weapons restored, deleting kvp");
                        DeleteResourceKvp("vmenu_temp_weapons_loadout_before_respawn");
                    }


                }

            }
        }
        #endregion
        #region Player clothing animations controller.
        private async Task PlayerClothingAnimationsController()
        {
            if (!DecorIsRegisteredAsType(clothingAnimationDecor, 3))
            {
                DecorRegister(clothingAnimationDecor, 3);
            }
            else
            {
                DecorSetInt(Game.PlayerPed.Handle, clothingAnimationDecor, PlayerAppearance.ClothingAnimationType);
                foreach (Player player in Players)
                {
                    Ped p = player.Character;
                    if (p != null && p.Exists() && !p.IsDead)
                    {
                        // CommonFunctions.Log($"Player {player.Name}, is {(DecorExistOn(p.Handle, clothingAnimationDecor) ? "registered" : "not registered")}");

                        if (DecorExistOn(p.Handle, clothingAnimationDecor))
                        {
                            int decorVal = DecorGetInt(p.Handle, clothingAnimationDecor);
                            if (decorVal == 0) // on solid/no animation.
                            {
                                SetPedIlluminatedClothingGlowIntensity(p.Handle, 1f);
                            }
                            else if (decorVal == 1) // off.
                            {
                                SetPedIlluminatedClothingGlowIntensity(p.Handle, 0f);
                            }
                            else if (decorVal == 2) // fade.
                            {
                                SetPedIlluminatedClothingGlowIntensity(p.Handle, clothingOpacity);
                            }
                            else if (decorVal == 3) // flash.
                            {
                                float result = 0f;
                                if (clothingAnimationReverse)
                                {
                                    if ((clothingOpacity >= 0f && clothingOpacity <= 0.5f)) // || (clothingOpacity >= 0.5f && clothingOpacity <= 0.75f))
                                    {
                                        result = 1f;
                                    }
                                }
                                else
                                {
                                    if ((clothingOpacity >= 0.5f && clothingOpacity <= 1.0f)) //|| (clothingOpacity >= 0.75f && clothingOpacity <= 1.0f))
                                    {
                                        result = 1f;
                                    }
                                }
                                SetPedIlluminatedClothingGlowIntensity(p.Handle, result);
                            }
                        }
                    }
                }
                if (clothingAnimationReverse)
                {
                    clothingOpacity -= 0.05f;
                    if (clothingOpacity < 0f)
                    {
                        clothingOpacity = 0f;
                        clothingAnimationReverse = false;
                    }
                }
                else
                {
                    clothingOpacity += 0.05f;
                    if (clothingOpacity > 1f)
                    {
                        clothingOpacity = 1f;
                        clothingAnimationReverse = true;
                    }
                }
                int timer = GetGameTimer();
                while (GetGameTimer() - timer < 25)
                {
                    await Delay(0);
                }
            }
            DecorSetInt(Game.PlayerPed.Handle, clothingAnimationDecor, PlayerAppearance.ClothingAnimationType);
        }

        #endregion
        #region player blips tasks
        private async Task PlayerBlipsControl()
        {
            if (DecorIsRegisteredAsType("vmenu_player_blip_sprite_id", 3))
            {

                int sprite = 1;
                if (IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
                {
                    Vehicle veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        sprite = BlipInfo.GetBlipSpriteForVehicle(veh.Handle);
                    }
                }

                DecorSetInt(Game.PlayerPed.Handle, "vmenu_player_blip_sprite_id", sprite);

                if (MainMenu.MiscSettingsMenu != null)
                {
                    bool enabled = MainMenu.MiscSettingsMenu.ShowPlayerBlips && IsAllowed(Permission.MSPlayerBlips);

                    foreach (Player p in MainMenu.PlayersList)
                    {
                        // continue only if this player is valid.
                        if (p != null && NetworkIsPlayerActive(p.Handle) && p.Character != null && p.Character.Exists())
                        {

                            //    
                            //else
                            //    SetBlipDisplay(blip, 3);

                            // if blips are enabled and the player has permisisons to use them.
                            if (enabled)
                            {
                                if (p != Game.Player)
                                {
                                    int ped = p.Character.Handle;
                                    int blip = GetBlipFromEntity(ped);

                                    // if blip id is invalid.
                                    if (blip < 1)
                                    {
                                        blip = AddBlipForEntity(ped);
                                    }
                                    // only manage the blip for this player if the player is nearby
                                    if (p.Character.Position.DistanceToSquared2D(Game.PlayerPed.Position) < 500000 || Game.IsPaused)
                                    {
                                        // (re)set the blip color in case something changed it.
                                        SetBlipColour(blip, 0);

                                        // if the decorator exists on this player, use the decorator value to determine what the blip sprite should be.
                                        if (DecorExistOn(p.Character.Handle, "vmenu_player_blip_sprite_id"))
                                        {
                                            int decorSprite = DecorGetInt(p.Character.Handle, "vmenu_player_blip_sprite_id");
                                            // set the sprite according to the decorator value.
                                            SetBlipSprite(blip, decorSprite);

                                            // show heading on blip only if the player is on foot (blip sprite 1)
                                            ShowHeadingIndicatorOnBlip(blip, decorSprite == 1);

                                            // set the blip rotation if the player is not in a helicopter (sprite 422).
                                            if (decorSprite != 422)
                                            {
                                                SetBlipRotation(blip, (int)GetEntityHeading(ped));
                                            }
                                        }
                                        else // backup method for when the decorator value is not found.
                                        {
                                            // set the blip sprite using the backup method in case decorators failed.
                                            SetCorrectBlipSprite(ped, blip);

                                            // only show the heading indicator if the player is NOT in a vehicle.
                                            if (!IsPedInAnyVehicle(ped, false))
                                            {
                                                ShowHeadingIndicatorOnBlip(blip, true);
                                            }
                                            else
                                            {
                                                ShowHeadingIndicatorOnBlip(blip, false);

                                                // If the player is not in a helicopter, set the blip rotation.
                                                if (!p.Character.IsInHeli)
                                                {
                                                    SetBlipRotation(blip, (int)GetEntityHeading(ped));
                                                }
                                            }
                                        }

                                        // set the player name.
                                        SetBlipNameToPlayerName(blip, p.Handle);

                                        // thanks lambda menu for hiding this great feature in their source code!
                                        // sets the blip category to 7, which makes the blips group under "Other Players:"
                                        SetBlipCategory(blip, 7);

                                        //N_0x75a16c3da34f1245(blip, false); // unknown

                                        // display on minimap and main map.
                                        SetBlipDisplay(blip, 6);
                                    }
                                    else
                                    {
                                        // hide it from the minimap.
                                        SetBlipDisplay(blip, 3);
                                    }
                                }
                            }
                            else // blips are not enabled.
                            {
                                if (!(p.Character.AttachedBlip == null || !p.Character.AttachedBlip.Exists()) && MainMenu.OnlinePlayersMenu != null && !MainMenu.OnlinePlayersMenu.PlayersWaypointList.Contains(p.Handle))
                                {
                                    p.Character.AttachedBlip.Delete(); // remove player blip if it exists.
                                }
                            }
                        }
                    }
                }
                else // misc settings is null
                {
                    await Delay(1000);
                }
            }
            else // decorator does not exist.
            {
                DecorRegister("vmenu_player_blip_sprite_id", 3);
                while (!DecorIsRegisteredAsType("vmenu_player_blip_sprite_id", 3))
                {
                    await Delay(0);
                }
            }
        }

        #endregion
        #region Online Player Options Tasks
        private async Task OnlinePlayersTasks()
        {
            await Delay(500);
            if (MainMenu.OnlinePlayersMenu != null && MainMenu.OnlinePlayersMenu.PlayersWaypointList.Count > 0)
            {
                foreach (int playerId in MainMenu.OnlinePlayersMenu.PlayersWaypointList)
                {
                    if (!NetworkIsPlayerActive(playerId))
                    {
                        waypointPlayerIdsToRemove.Add(playerId);
                    }
                    else
                    {
                        Vector3 pos1 = GetEntityCoords(GetPlayerPed(playerId), true);
                        Vector3 pos2 = Game.PlayerPed.Position;
                        if (Vdist2(pos1.X, pos1.Y, pos1.Z, pos2.X, pos2.Y, pos2.Z) < 20f)
                        {
                            int blip = GetBlipFromEntity(GetPlayerPed(playerId));
                            if (DoesBlipExist(blip))
                            {
                                SetBlipRoute(blip, false);
                                RemoveBlip(ref blip);
                                waypointPlayerIdsToRemove.Add(playerId);
                                Notify.Custom($"~g~You've reached ~s~<C>{GetPlayerName(playerId)}</C>'s~g~ location, disabling GPS route.");
                            }
                        }
                    }
                    await Delay(10);
                }
                if (waypointPlayerIdsToRemove.Count > 0)
                {
                    foreach (int id in waypointPlayerIdsToRemove)
                    {
                        MainMenu.OnlinePlayersMenu.PlayersWaypointList.Remove(id);
                    }
                    await Delay(10);
                }
                waypointPlayerIdsToRemove.Clear();
            }
        }
        #endregion
        #region Flares and plane bombs controler

        private readonly List<uint> flareVehicles = new List<uint>()
        {
            (uint)GetHashKey("mogul"),
            (uint)GetHashKey("rogue"),
            (uint)GetHashKey("starling"),
            (uint)GetHashKey("seabreeze"),
            (uint)GetHashKey("tula"),
            (uint)GetHashKey("bombushka"),
            (uint)GetHashKey("hunter"),
            (uint)GetHashKey("nokota"),
            (uint)GetHashKey("pyro"),
            (uint)GetHashKey("molotok"),
            (uint)GetHashKey("havok"),
            (uint)GetHashKey("alphaz1"),
            (uint)GetHashKey("microlight"),
            (uint)GetHashKey("howard"),
            (uint)GetHashKey("avenger"),
            (uint)GetHashKey("thruster"),
            (uint)GetHashKey("volatol")
        };

        private readonly List<uint> bombVehicles = new List<uint>()
        {
            (uint)GetHashKey("cuban800"),
            (uint)GetHashKey("mogul"),
            (uint)GetHashKey("rogue"),
            (uint)GetHashKey("starling"),
            (uint)GetHashKey("seabreeze"),
            (uint)GetHashKey("tula"),
            (uint)GetHashKey("bombushka"),
            (uint)GetHashKey("hunter"),
            (uint)GetHashKey("avenger"),
            (uint)GetHashKey("akula"),
            (uint)GetHashKey("volatol")
        };

        /// Returns true if the player can currently fire flares.
        bool CanShootFlares()
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle veh = GetVehicle();
                if (veh == null || !veh.Exists() || veh.IsDead)
                {
                    return false;
                }

                if (flareVehicles.Contains((uint)veh.Model.Hash) && GetVehicleMod(veh.Handle, 1) == 1 && GetAircraftCountermeasureCount(veh.Handle) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// Returns true if the player can currently drop bombs.
        bool CanDropBombs()
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle veh = GetVehicle();
                if (veh == null || !veh.Exists() || veh.IsDead)
                {
                    return false;
                }

                if (bombVehicles.Contains((uint)veh.Model.Hash) && GetVehicleMod(veh.Handle, 9) > -1 && GetAircraftBombCount(veh.Handle) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        void ShootFlares()
        {
            // TODO
        }

        void DropBombs()
        {
            // TODO
        }

        private async Task FlaresAndBombsTick()
        {
            if (!MenuController.IsAnyMenuOpen() && !MainMenu.DontOpenMenus && !Game.IsPaused && Fading.IsFadedIn && !IsPlayerSwitchInProgress())
            {
                if (flaresAllowed && CanShootFlares())
                {

                }

                if (bombsAllowed && CanDropBombs())
                {

                }
            }
            else
            {
                await Delay(1);
            }
        }
        #endregion
        #region Tick related to animations and interactions in-game
        /// <summary>
        /// Manages (triggers) all interactions and animations that happen in the world without direct use of the menu.
        /// </summary>
        /// <returns></returns>
        private async Task AnimationsAndInteractions()
        {

            if (!(MenuController.IsAnyMenuOpen() || MainMenu.DontOpenMenus || !Fading.IsFadedIn || Game.IsPaused || IsPlayerSwitchInProgress() || Game.PlayerPed.IsDead))
            {
                // snowballs
                if (Game.IsControlJustReleased(0, Control.Detonate))
                {
                    if (World.NextWeather == Weather.Christmas)
                    {
                        if (!(Game.PlayerPed.IsInVehicle() || Game.PlayerPed.IsDead || !Fading.IsFadedIn || IsPlayerSwitchInProgress() || Game.IsPaused
                            || GetInteriorFromEntity(Game.PlayerPed.Handle) != 0 || !Game.PlayerPed.IsOnFoot || Game.PlayerPed.IsInParachuteFreeFall ||
                            Game.PlayerPed.IsFalling || Game.PlayerPed.IsBeingStunned || Game.PlayerPed.IsWalking || Game.PlayerPed.IsRunning ||
                            Game.PlayerPed.IsSprinting || Game.PlayerPed.IsSwimming || Game.PlayerPed.IsSwimmingUnderWater || Game.PlayerPed.IsDiving && GetSelectedPedWeapon(Game.PlayerPed.Handle) == snowball_hash || GetSelectedPedWeapon(Game.PlayerPed.Handle) == GetHashKey("unarmed")))
                        {
                            await PickupSnowball();
                        }
                    }
                }
                // helmet visor
                if (Game.IsControlPressed(0, Control.SwitchVisor))
                {
                    int timer = GetGameTimer();
                    while (!(MenuController.IsAnyMenuOpen() || MainMenu.DontOpenMenus || !Fading.IsFadedIn || Game.IsPaused || IsPlayerSwitchInProgress() || Game.PlayerPed.IsDead) && Game.IsControlPressed(0, Control.SwitchVisor))
                    {
                        await Delay(0);
                        if (GetGameTimer() - timer > 400)
                        {
                            await SwitchHelmet();
                            break;
                        }
                    }
                    while (Game.IsControlPressed(0, Control.SwitchVisor))
                    {
                        await Delay(0);
                    }
                }
            }
        }
        #endregion
        #region help message controller
        private async Task HelpMessageController()
        {
            if (World.NextWeather == Weather.Christmas && IsAllowed(Permission.WPSnowball))
            {
                void ShowSnowballInfoMessage()
                {
                    int maxAmmo = 10;
                    GetMaxAmmo(Game.PlayerPed.Handle, snowball_hash, ref maxAmmo);
                    if (maxAmmo > GetAmmoInPedWeapon(Game.PlayerPed.Handle, snowball_hash))
                    {
                        BeginTextCommandDisplayHelp("HELP_SNOWP");
                        AddTextComponentInteger(2);
                        AddTextComponentInteger(maxAmmo);
                        EndTextCommandDisplayHelp(0, false, true, 6000);
                    }

                }

                // help text control for snowballs
                if (!IsPedInAnyVehicle(Game.PlayerPed.Handle, true))
                {
                    if (showSnowballInfo)
                    {
                        BeginTextCommandIsThisHelpMessageBeingDisplayed("HELP_SNOWP");
                        if (EndTextCommandIsThisHelpMessageBeingDisplayed(0))
                        {
                            showSnowballInfo = false;
                            return;
                        }
                        else if (IsHelpMessageBeingDisplayed())
                        {
                            ClearAllHelpMessages();
                        }
                        ShowSnowballInfoMessage();
                    }
                    showSnowballInfo = false;
                }
                else
                {
                    showSnowballInfo = true;
                }
            }
            await Delay(100);
        }
        #endregion
        #region draw model dimensions
        Text Text1 = new Text("1", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text2 = new Text("2", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text3 = new Text("3", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text4 = new Text("4", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text5 = new Text("5", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text6 = new Text("6", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text7 = new Text("7", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text8 = new Text("8", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text Text9 = new Text("9", new System.Drawing.PointF(0f, 0f), 0.5f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        Text entityIdText = new Text("Handle: ", new System.Drawing.PointF(0f, 0f), 0.3f, System.Drawing.Color.FromArgb(255, 255, 255), Font.Monospace, Alignment.Center, true, true);
        private async Task ModelDrawDimensions()
        {
            if (MainMenu.MiscSettingsMenu != null && MainMenu.MiscSettingsMenu.ShowVehicleModelDimensions)
            {
                var veh = GetVehicle(true);
                if (veh == null)
                {
                    veh = GetVehicle();
                }
                if (veh == null)
                {
                    veh = new Vehicle(GetVehiclePedIsUsing(Game.PlayerPed.Handle));
                }
                if (veh != null && veh.Exists())
                {
                    int ent = veh.Handle;
                    int model = GetEntityModel(veh.Handle);
                    Vector3 pos1 = new Vector3(), pos2 = new Vector3();
                    GetModelDimensions((uint)model, ref pos1, ref pos2);
                    Vector3 pos3 = new Vector3(pos1.X - (pos1.X - pos2.X), pos1.Y, pos1.Z);
                    Vector3 pos4 = new Vector3(pos1.X, pos1.Y - (pos1.Y - pos2.Y), pos1.Z);
                    Vector3 pos5 = new Vector3(pos1.X, pos1.Y, pos1.Z - (pos1.Z - pos2.Z));
                    Vector3 pos6 = new Vector3(pos2.X - (pos2.X - pos1.X), pos2.Y, pos2.Z);
                    Vector3 pos7 = new Vector3(pos2.X, pos2.Y - (pos2.Y - pos1.Y), pos2.Z);
                    Vector3 pos8 = new Vector3(pos2.X, pos2.Y, pos2.Z - (pos2.Z - pos1.Z));

                    var off1 = GetOffsetFromEntityInWorldCoords(ent, pos1.X, pos1.Y, pos1.Z);
                    var off2 = GetOffsetFromEntityInWorldCoords(ent, pos5.X, pos5.Y, pos5.Z);
                    var off3 = GetOffsetFromEntityInWorldCoords(ent, pos7.X, pos7.Y, pos7.Z);
                    var off4 = GetOffsetFromEntityInWorldCoords(ent, pos3.X, pos3.Y, pos3.Z);
                    var off5 = GetOffsetFromEntityInWorldCoords(ent, pos4.X, pos4.Y, pos4.Z);
                    var off6 = GetOffsetFromEntityInWorldCoords(ent, pos6.X, pos6.Y, pos6.Z);
                    var off7 = GetOffsetFromEntityInWorldCoords(ent, pos2.X, pos2.Y, pos2.Z);
                    var off8 = GetOffsetFromEntityInWorldCoords(ent, pos8.X, pos8.Y, pos8.Z);

                    var x = pos4.X - ((pos4.X - pos8.X) / 2f);
                    var y = pos4.Y - ((pos4.Y - pos8.Y) / 2f);
                    var z = pos8.Z - ((pos8.Z - pos7.Z) / 2f);
                    //var pos9 = new Vector3(x, y, z - 0.1f);
                    //var pos10 = new Vector3(x, y, z + 0.1f);
                    var pos9 = new Vector3(x, y, z);
                    var off9 = GetOffsetFromEntityInWorldCoords(ent, pos9.X, pos9.Y, pos9.Z - 0.1f);
                    var off9bottom = GetOffsetFromEntityInWorldCoords(ent, pos9.X, pos9.Y, pos9.Z - 0.1f);
                    var off9up = GetOffsetFromEntityInWorldCoords(ent, pos9.X, pos9.Y, pos9.Z + 0.1f);
                    var off9left = GetOffsetFromEntityInWorldCoords(ent, pos9.X + 0.1f, pos9.Y, pos9.Z);
                    var off9right = GetOffsetFromEntityInWorldCoords(ent, pos9.X - 0.1f, pos9.Y, pos9.Z);
                    var possomething = new Vector3(pos7.X - ((pos7.X - pos6.X) / 2f), 0f, pos7.Z - ((pos7.Z - pos2.Z) / 2f));
                    var centerMiddleTop = GetOffsetFromEntityInWorldCoords(ent, possomething.X, possomething.Y, possomething.Z);

                    SetDrawOrigin(off1.X, off1.Y, off1.Z, 0); Text1.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off2.X, off2.Y, off2.Z + 0.15f, 0); Text2.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off3.X, off3.Y, off3.Z + 0.15f, 0); Text3.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off4.X, off4.Y, off4.Z, 0); Text4.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off5.X, off5.Y, off5.Z, 0); Text5.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off6.X, off6.Y, off6.Z + 0.15f, 0); Text6.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off7.X, off7.Y, off7.Z + 0.15f, 0); Text7.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off8.X, off8.Y, off8.Z, 0); Text8.Draw(); ClearDrawOrigin();
                    SetDrawOrigin(off9bottom.X, off9bottom.Y, off9bottom.Z, 0); Text9.Draw(); ClearDrawOrigin();
                    entityIdText.Caption = $"Handle {ent}";
                    SetDrawOrigin(centerMiddleTop.X, centerMiddleTop.Y, centerMiddleTop.Z, 0); entityIdText.Draw(); ClearDrawOrigin();

                    DrawLine(off1.X, off1.Y, off1.Z, off7.X, off7.Y, off7.Z, 255, 255, 255, 255); // white min to max


                    DrawLine(off1.X, off1.Y, off1.Z, off4.X, off4.Y, off4.Z, 255, 0, 0, 255); // red   (x)
                    DrawLine(off1.X, off1.Y, off1.Z, off5.X, off5.Y, off5.Z, 0, 255, 0, 255); // green (y)
                    DrawLine(off1.X, off1.Y, off1.Z, off2.X, off2.Y, off2.Z, 0, 0, 255, 255); // blue  (z)

                    DrawLine(off8.X, off8.Y, off8.Z, off5.X, off5.Y, off5.Z, 255, 0, 0, 255); // red   (x)
                    DrawLine(off8.X, off8.Y, off8.Z, off4.X, off4.Y, off4.Z, 0, 255, 0, 255); // green (y)
                    DrawLine(off8.X, off8.Y, off8.Z, off7.X, off7.Y, off7.Z, 0, 0, 255, 255); // blue  (z)

                    DrawLine(off2.X, off2.Y, off2.Z, off3.X, off3.Y, off3.Z, 255, 0, 0, 255); // red   (x)
                    DrawLine(off2.X, off2.Y, off2.Z, off6.X, off6.Y, off6.Z, 0, 255, 0, 255); // green (y)
                    DrawLine(off3.X, off3.Y, off3.Z, off4.X, off4.Y, off4.Z, 0, 0, 255, 255); // blue  (z)

                    DrawLine(off6.X, off6.Y, off6.Z, off7.X, off7.Y, off7.Z, 255, 0, 0, 255); // red   (x)
                    DrawLine(off3.X, off3.Y, off3.Z, off7.X, off7.Y, off7.Z, 0, 255, 0, 255); // green (y)
                    DrawLine(off6.X, off6.Y, off6.Z, off5.X, off5.Y, off5.Z, 0, 0, 255, 255); // blue  (z)

                    DrawLine(off9bottom.X, off9bottom.Y, off9bottom.Z, off9up.X, off9up.Y, off9up.Z, 255, 0, 255, 255); // pink (center front up/down)
                    DrawLine(off9left.X, off9left.Y, off9left.Z, off9right.X, off9right.Y, off9right.Z, 255, 255, 0, 255); // yellow (center front x)
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion
        #region garbadge collection stuff
        int gcTimer = GetGameTimer();
        private async Task GcTick()
        {
            if (GetGameTimer() - gcTimer > 60000)
            {
                gcTimer = GetGameTimer();
                GC.Collect();
                Log($"[vMenu] GC at {GetGameTimer()} ({GetTimeAsString(GetGameTimer())}).");

            }
            await Delay(1000);
        }
        #endregion
        #region Interior management


        private IplManager.Interior currentInterior = null;
        private int tvObject = 0;
        private bool EnableTvs => MainMenu.IplManagementMenu != null && MainMenu.IplManagementMenu.EnableIplTvs;
        private int CurrentTvChannel => MainMenu.IplManagementMenu != null ? MainMenu.IplManagementMenu.SelectedTvChannel : 0;
        private readonly List<uint> tvModels = new List<uint>()
        {
            (uint)GetHashKey("prop_tv_flat_01"), // 1036195894
            unchecked((uint)(-1546399138)),
            330240957,
            unchecked((uint)(-1223496606)),
            unchecked((uint)(-1073182005)),
            unchecked((uint)(-1820646534)),
            170618079,
            1653710254,
            777010715,
            1020451759,
        };
        private readonly List<string> tvChannels = new List<string>() {
            "PL_STD_CNT",
            "PL_STD_WZL_FOS_EP2",
            //"PL_STD_WZL",
            //"PL_LO_WZL",
            "PL_SP_WORKOUT",
            //"PL_SP_PLSH1_INTRO",
            "PL_LES1_FAME_OR_SHAME",
            //"PL_STD_WZL_FOS_EP2",
            "PL_MP_WEAZEL",
            "PL_MP_CCTV",
        };

        private readonly List<string> stiltHouseIplsExteriors = new List<string>()
        {
            "apa_stilt_ch2_04_ext1", // 2044 North Conker Avenue
            "apa_stilt_ch2_04_ext2", // 2045 North Conker Avenue
            "apa_stilt_ch2_05c_ext1", // 3677 Whispymound Drive
            "apa_stilt_ch2_05e_ext1", // 3655 Wild Oats Drive
            "apa_stilt_ch2_09b_ext2", // 2874 Hillcrest Avenue
            "apa_stilt_ch2_09b_ext3", // 2868 Hillcrest Avenue
            "apa_stilt_ch2_09c_ext1", // 2866 Hilcrest Avenue
            "apa_stilt_ch2_09c_ext2", // 2862 Hillcrest Avenue
            "apa_stilt_ch2_09c_ext3", // 2117 Milton Road
            "apa_stilt_ch2_12b_ext1", // 2113 Mad Wayne Thunder
        };

        //private readonly Dictionary<string, string> stiltHouseIplsExteriors = new Dictionary<string, string>()
        //{
        //    ["House, 2044 North Conker Avenue"] = "apa_stilt_ch2_04_ext1", // 2044 North Conker Avenue
        //    ["House, 2045 North Conker Avenue"] = "apa_stilt_ch2_04_ext2", // 2045 North Conker Avenue
        //    ["House, 2677 Whispymound Drive"] = "apa_stilt_ch2_05c_ext1", // 3677 Whispymound Drive
        //    ["House, 3655 Wild Oats Drive"] = "apa_stilt_ch2_05e_ext1", // 3655 Wild Oats Drive
        //    ["House, 2874 Hillcrest Avenue"] = "apa_stilt_ch2_09b_ext2", // 2874 Hillcrest Avenue
        //    ["House, 2868 Hillcrest Avenue"] = "apa_stilt_ch2_09b_ext3", // 2868 Hillcrest Avenue
        //    //[""] = "apa_stilt_ch2_09c_ext1", // 2866 Hilcrest Avenue
        //    ["House, 2862 Hillcrest Avenue"] = "apa_stilt_ch2_09c_ext2", // 2862 Hillcrest Avenue
        //    //[""] = "apa_stilt_ch2_09c_ext3", // 2117 Milton Road
        //    ["House, 2113 Mad Wayne Thunder"] = "apa_stilt_ch2_12b_ext1", // 2113 Mad Wayne Thunder
        //};



        private async Task InteriorTpManager()
        {
            if (MainMenu.IplManagementMenu != null && MainMenu.IplManagementMenu.EnableIplTeleports)
            {
                if (currentInterior != null)
                {
                    foreach (var outsidePos in currentInterior.posTpExt)
                    {
                        var insidePos = currentInterior.posTpInt;
                        var heading = currentInterior.teleportHeading - 180;

                        DrawMarker(1, insidePos.X, insidePos.Y, insidePos.Z - 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0.8f, 0.8f, 0.2f, 25, 140, 255, 150, false, false, 0, false, null, null, false);

                        if (Game.PlayerPed.Position.DistanceToSquared(insidePos) < 0.5f)
                        {
                            HelpMessage.CustomLooped(HelpMessage.Label.EXIT_INTERIOR_HELP_MESSAGE);
                            if (Game.IsControlJustReleased(0, Control.Context) && Game.PlayerPed.Position.DistanceToSquared(insidePos) < 0.5f)
                            {
                                var posOut = outsidePos;
                                if (currentInterior is IplManager.Hanger han)
                                {
                                    if (han.outPos == Vector3.Zero)
                                    {
                                        han.outPos = outsidePos;
                                    }
                                    posOut = han.outPos;
                                }
                                DoScreenFadeOut(500);
                                while (!IsScreenFadedOut())
                                {
                                    await Delay(0);
                                }
                                RequestCollisionAtCoord(posOut.X, posOut.Y, posOut.Z);
                                SetEntityCoords(Game.PlayerPed.Handle, posOut.X, posOut.Y, posOut.Z, false, false, false, true);
                                DoScreenFadeIn(500);
                                SetEntityHeading(Game.PlayerPed.Handle, heading);
                            }
                        }
                    }
                    //var outsidePos = currentInterior.posTpExt;
                    //if (currentInterior is IplManager.Hanger han)
                    //{
                    //    outsidePos = han.outPos;
                    //    if (outsidePos.IsZero)
                    //    {
                    //        outsidePos = han.posTpExt;
                    //    }
                    //}

                }
                else
                {
                    foreach (IplManager.Interior interior in IplManager.interiors)
                    {
                        var pedPos = Game.PlayerPed.Position;
                        var locations = new List<Vector3>();
                        foreach (Vector3 pos in interior.posTpExt)
                        {
                            if (pedPos.DistanceToSquared(pos) < 500f)
                            {
                                if (!locations.Contains(pos))
                                {
                                    locations.Add(pos);
                                    DrawMarker(1, pos.X, pos.Y, pos.Z - 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0.8f, 0.8f, 0.3f, 25, 140, 255, 150, false, false, 0, false, null, null, false);
                                    if (pedPos.DistanceToSquared(pos) < 0.5f)
                                    {
                                        var menu = MainMenu.IplManagementMenu.EnterInteriorMenu;
                                        var oldMenu = MenuController.GetCurrentMenu();
                                        MenuController.CloseAllMenus();
                                        menu.ClearMenuItems();
                                        foreach (IplManager.Interior tmpInter in IplManager.interiors)
                                        {
                                            if (tmpInter.posTpExt.Contains(pos))
                                            {
                                                var menuItem = new MenuItem(tmpInter.name);
                                                menu.AddMenuItem(menuItem);
                                            }
                                        }

                                        var oldValue = MenuController.MenuAlignment;
                                        MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                                        menu.OpenMenu();
                                        while (Game.PlayerPed.Position.DistanceToSquared(pos) < 0.5f)
                                        {
                                            await Delay(0);
                                            DrawMarker(1, pos.X, pos.Y, pos.Z - 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0.8f, 0.8f, 0.3f, 25, 140, 255, 150, false, false, 0, false, null, null, false);
                                        }
                                        menu.CloseMenu();
                                        MenuController.MenuAlignment = oldValue;
                                        if (oldMenu != null)
                                        {
                                            oldMenu.OpenMenu();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //if (IplManager.interiors.Any(inter => (inter is IplManager.Hanger han && han.outTpLocations.Any(d => d.DistanceToSquared2D(Game.PlayerPed.Position) < 500f)) || (inter.posTpExt.DistanceToSquared2D(Game.PlayerPed.Position) < 500f)))
                    //{
                    //    var positions = new List<Vector3>();

                    //    foreach (var inter in IplManager.interiors)
                    //    {
                    //        var locations = new List<Vector3>
                    //        {
                    //            inter.posTpExt
                    //        };
                    //        if (inter is IplManager.Hanger han)
                    //        {
                    //            locations.AddRange(han.outTpLocations);
                    //        }
                    //        //if (locations.Any(d => d.DistanceToSquared2D(Game.PlayerPed.Position) < 500f))
                    //        {
                    //            //if (foreach!positions.Contains(inter.posTpExt))

                    //            foreach (Vector3 pos in locations)
                    //            {
                    //                if (!positions.Contains(pos))
                    //                {
                    //                    if (pos.DistanceToSquared2D(Game.PlayerPed.Position) < 500f)
                    //                    {
                    //                        DrawMarker(1, pos.X, pos.Y, pos.Z - 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0.8f, 0.8f, 0.3f, 25, 140, 255, 150, false, false, 0, false, null, null, false);
                    //                        positions.Add(pos);
                    //                    }
                    //                }
                    //            }

                    //{
                    //    positions.Add(inter.posTpExt);

                    //    DrawMarker(1, inter.posTpExt.X, inter.posTpExt.Y, inter.posTpExt.Z - 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0.8f, 0.8f, 0.3f, 25, 140, 255, 150, false, false, 0, false, null, null, false);
                    //}

                    //            foreach (Vector3 pos in positions)
                    //            {
                    //                if (Game.PlayerPed.Position.DistanceToSquared2D(pos) < 0.5f)
                    //                {
                    //                    var menu = MainMenu.IplManagementMenu.EnterInteriorMenu;
                    //                    menu.ClearMenuItems();
                    //                    MenuController.CloseAllMenus();

                    //                    //var possibleInteriors = new List<IplManager.Interior>();
                    //                    foreach (IplManager.Interior tmpInter in IplManager.interiors)
                    //                    {
                    //                        if (tmpInter.posTpExt == pos)
                    //                        {
                    //                            //possibleInteriors.Add(tmpInter);
                    //                            var menuItem = new MenuItem(tmpInter.name);
                    //                            menu.AddMenuItem(menuItem);
                    //                        }
                    //                    }
                    //                    var oldValue = MenuController.MenuAlignment;
                    //                    //menu.LeftAligned = true;
                    //                    MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
                    //                    menu.OpenMenu();

                    //                    while (Game.PlayerPed.Position.DistanceToSquared2D(pos) < 0.5f)
                    //                    {
                    //                        await Delay(0);
                    //                        DrawMarker(1, pos.X, pos.Y, pos.Z - 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0.8f, 0.8f, 0.3f, 25, 140, 255, 150, false, false, 0, false, null, null, false);
                    //                    }
                    //                    menu.CloseMenu();
                    //                    MenuController.MenuAlignment = oldValue;
                    //                }
                    //            }

                    //        }
                    //    }
                    //}
                    //await Delay(1);
                }
            }

            await Task.FromResult(0);
        }

        private async Task InteriorHideExterior()
        {
            if (currentInterior != null)
            {
                // make a copy just in case the currentInterior object becomes null while in this loop.
                var inter = currentInterior;

                if (inter is IplManager.Hanger interior)
                {
                    string interiorRadarHash = "sm_SmugDLC_Int_01";
                    int zoom = 0;
                    Vector3 pos = Game.PlayerPed.Position;
                    bool s = false;
                    if (pos.Z < -45.5f)
                    {
                        zoom = 0;
                        if (interior.ModArea) // condition 1 func_348
                        {
                            interiorRadarHash = "sm_SmugDLC_Int_01_level_0_ModArea";
                        }
                        else
                        {
                            interiorRadarHash = "sm_SmugDLC_Int_01_level_0_NoModArea";
                        }
                    }
                    else if (pos.Z < -39.25f)
                    {
                        zoom = 1;
                        if (interior.BedroomStyle > 0) // condition 1 func_351 || func_350
                        {
                            if (interior.ModArea) // condition 2 func_348
                            {
                                interiorRadarHash = "sm_SmugDLC_Int_01_level_1_Bed_ModArea";
                            }
                            else
                            {
                                interiorRadarHash = "sm_SmugDLC_Int_01_level_1_Bed_NoModArea";
                            }
                        }
                        else if (interior.ModArea) // condition 2 func_348
                        {
                            interiorRadarHash = "sm_SmugDLC_Int_01_level_1_NoBed_ModArea";
                        }
                        else
                        {
                            interiorRadarHash = "sm_SmugDLC_Int_01_level_1_NoBed_NoModArea";
                        }
                    }
                    else if (pos.Z < -34.75f)
                    {
                        zoom = 2;
                        interiorRadarHash = "sm_SmugDLC_Int_01_lvl_2";
                    }
                    else if (pos.Z < -23f)
                    {
                        zoom = 3;
                        interiorRadarHash = "sm_SmugDLC_Int_01_lvl_3";
                    }
                    SetRadarAsInteriorThisFrame((uint)GetHashKey(interiorRadarHash), -1266.802f, -3014.836f, 0, zoom);
                }

                if ((inter is IplManager.Apartment || inter is IplManager.Penthouse) && ((inter is IplManager.House) == false))
                {
                    //apartment buildings:
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ss1_11_flats"));
                    HideMapObjectThisFrame((uint)GetHashKey("ss1_11_ss1_emissive_a"));
                    HideMapObjectThisFrame((uint)GetHashKey("ss1_11_detail01b"));
                    HideMapObjectThisFrame((uint)GetHashKey("ss1_11_Flats_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("SS1_02_Building01_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("SS1_LOD_01_02_08_09_10_11"));
                    HideMapObjectThisFrame((uint)GetHashKey("SS1_02_SLOD1"));
                    HideMapObjectThisFrame((uint)GetHashKey("hei_dt1_20_build2"));
                    HideMapObjectThisFrame((uint)GetHashKey("dt1_20_dt1_emissive_dt1_20"));
                    HideMapObjectThisFrame((uint)GetHashKey("sm_14_emissive"));
                    HideMapObjectThisFrame((uint)GetHashKey("hei_sm_14_bld2"));
                    HideMapObjectThisFrame((uint)GetHashKey("hei_bh1_09_bld_01"));
                    HideMapObjectThisFrame((uint)GetHashKey("bh1_09_ema"));
                    HideMapObjectThisFrame((uint)GetHashKey("prop_wall_light_12a"));
                    HideMapObjectThisFrame((uint)GetHashKey("hei_dt1_03_build1x"));
                    HideMapObjectThisFrame((uint)GetHashKey("DT1_Emissive_DT1_03_b1"));
                    HideMapObjectThisFrame((uint)GetHashKey("dt1_03_dt1_Emissive_b1"));
                    HideMapObjectThisFrame((uint)GetHashKey("hei_bh1_08_bld2"));
                    HideMapObjectThisFrame((uint)GetHashKey("bh1_emissive_bh1_08"));
                    HideMapObjectThisFrame((uint)GetHashKey("bh1_08_bld2_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("hei_bh1_08_bld2"));
                    HideMapObjectThisFrame((uint)GetHashKey("bh1_08_em"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ss1_02_building01"));
                    HideMapObjectThisFrame((uint)GetHashKey("SS1_Emissive_SS1_02a_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ss1_02_ss1_emissive_ss1_02"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ss1_02_building01"));
                    HideMapObjectThisFrame((uint)GetHashKey("SS1_02_Building01_LOD"));
                }
                else if (inter is IplManager.House)
                {
                    //Debug.WriteLine("test");
                    // houses:
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_05e_res5"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_05e_res5_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_04_house02"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_04_house02_d"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_04_M_a_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_04_house02_railings"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_04_emissive_04"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_04_emissive_04_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_04_house02_details"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_hs01a_details"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_hs01"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_hs01_balcony"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_Emissive_11_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_Emissive_11"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_CH2_09b_House08_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09c_hs11"));
                    HideMapObjectThisFrame((uint)GetHashKey("CH2_09c_Emissive_11_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("CH2_09c_Emissive_11"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09c_hs11_details"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_05c_b4"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_05c_emissive_07"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_05c_decals_05"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_05c_B4_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09c_hs07"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_09c_build_11_07_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("CH2_09c_Emissive_07_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09c_build_11_07_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_09c_hs07_details"));
                    HideMapObjectThisFrame((uint)GetHashKey("CH2_09c_Emissive_07"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09c_hs13"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09c_hs13_details"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_CH2_09c_House11_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_09c_Emissive_13_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_09c_Emissive_13"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_hs02"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_hs02b_details"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_Emissive_09_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_09b_botpoolHouse02_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_Emissive_09"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_09b_hs02_balcony"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_12b_house03mc"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_12b_emissive_02"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_12b_house03_MC_a_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_12b_emissive_02_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_12b_railing_06"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_04_house01"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_04_house01_d"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_04_emissive_05_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("apa_ch2_04_M_b_LOD"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_04_emissive_05"));
                    HideMapObjectThisFrame((uint)GetHashKey("ch2_04_house01_details"));
                }
            }
            await Task.FromResult(0);
        }

        private async Task InteriorChecker()
        {
            if (MainMenu.IplManagementMenu != null)
            {
                Vector3 playerPos = Game.PlayerPed.Position;

                bool inInterior = false;

                foreach (IplManager.Interior interior in IplManager.interiors)
                {
                    if (interior.InteriorId == GetInteriorFromEntity(Game.PlayerPed.Handle))
                    {
                        currentInterior = interior;
                        inInterior = true;
                        break;
                    }
                }

                if (!inInterior)
                {
                    currentInterior = null;
                }

                if (currentInterior == null || !(currentInterior is IplManager.House))
                {
                    foreach (string ipl in stiltHouseIplsExteriors)
                    {
                        if (IsIplActive(ipl))
                        {
                            RemoveIpl(ipl);
                        }
                    }
                }
                else if (currentInterior != null && currentInterior is IplManager.House)
                {
                    foreach (string ipl in stiltHouseIplsExteriors)
                    {
                        if (!IsIplActive(ipl))
                        {
                            RequestIpl(ipl);
                        }
                    }
                }


                foreach (MenuItem item in MainMenu.IplManagementMenu.apartmentsMenu.GetMenuItems())
                {
                    if (currentInterior != null)
                    {
                        if (currentInterior.name == item.Text)
                        {
                            item.LeftIcon = MenuItem.Icon.TICK;
                        }
                        else if (item.LeftIcon == MenuItem.Icon.TICK)
                        {
                            item.LeftIcon = MenuItem.Icon.NONE;
                        }
                    }
                    else if (item.LeftIcon == MenuItem.Icon.TICK)
                    {
                        item.LeftIcon = MenuItem.Icon.NONE;
                    }
                }
            }
            await Delay(100);
        }

        private void SetRadioEmitters(IplManager.Interior interior, bool enabled)
        {
            var list = new List<string>();
            if (interior is IplManager.Hanger inter)
            {
                // bedroom
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Living_Quarters_01", enabled && inter.BedroomStyle > 0);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Living_Quarters_02", enabled && inter.BedroomStyle > 0);
                SetEmitterRadioStation("SE_DLC_SM_Hangar_Radio_Living_Quarters_01", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                SetEmitterRadioStation("SE_DLC_SM_Hangar_Radio_Living_Quarters_02", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                // office
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_01", enabled);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_02", enabled);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_03", enabled);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_04", enabled);
                SetEmitterRadioStation("SE_DLC_SM_Hangar_Radio_Office_01", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                SetEmitterRadioStation("SE_DLC_SM_Hangar_Radio_Office_02", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                SetEmitterRadioStation("SE_DLC_SM_Hangar_Radio_Office_03", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                SetEmitterRadioStation("SE_DLC_SM_Hangar_Radio_Office_04", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                // mod area
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Mechanic", enabled && inter.ModArea);
                SetEmitterRadioStation("SE_DLC_SM_Hangar_Radio_Mechanic", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
            }
            else
            {
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Living_Quarters_01", false);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Living_Quarters_02", false);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_01", false);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_02", false);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_03", false);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Office_04", false);
                SetStaticEmitterEnabled("SE_DLC_SM_Hangar_Radio_Mechanic", false);
            }

            if (interior is IplManager.Apartment)
            {
                for (var i = 0; i < 18; i++)
                {
                    for (var b = 1; b < 4; b++)
                    {
                        SetStaticEmitterEnabled($"SE_MP_APT_{i}_{b}", enabled);
                        SetEmitterRadioStation($"SE_MP_APT_{i}_{b}", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                    }
                }
                for (var i = 0; i < 5; i++)
                {
                    for (var b = 1; b < 4; b++)
                    {
                        SetStaticEmitterEnabled($"SE_MP_APT_NEW_{i}_{b}", enabled);
                        SetEmitterRadioStation($"SE_MP_APT_NEW_{i}_{b}", GetRadioStationName(MainMenu.IplManagementMenu.SelectedRadioStation));
                    }
                }
            }
            else
            {
                for (var i = 0; i < 40; i++)
                {
                    for (var b = 1; b < 4; b++)
                    {
                        SetStaticEmitterEnabled($"SE_MP_APT_{i}_{b}", false);
                    }
                }
                for (var i = 0; i < 5; i++)
                {
                    for (var b = 1; b < 4; b++)
                    {
                        SetStaticEmitterEnabled($"SE_MP_APT_NEW_{i}_{b}", false);
                    }
                }
            }
        }

        private async Task InteriorRadio()
        {
            if (MainMenu.IplManagementMenu != null && MainMenu.IplManagementMenu.EnableIplRadios)
            {
                var interior = currentInterior;
                if (interior != null)
                {
                    if (!EnableTvs)
                    {
                        SetRadioEmitters(interior, true);

                        int selection = MainMenu.IplManagementMenu.SelectedRadioStation;

                        while (!EnableTvs && MainMenu.IplManagementMenu.EnableIplRadios && currentInterior != null && currentInterior == interior)
                        {
                            if (selection != MainMenu.IplManagementMenu.SelectedRadioStation)
                            {
                                selection = MainMenu.IplManagementMenu.SelectedRadioStation;
                                if (!(selection > -1 && selection < MaxRadioStationIndex()))
                                {
                                    selection = 0;
                                }
                                SetRadioEmitters(interior, true);
                            }
                            await Delay(0);
                        }

                        SetRadioEmitters(interior, false);

                    }



                }
            }



            await Task.FromResult(0);
        }

        private async Task InteriorTv()
        {
            if (EnableTvs && currentInterior != null)
            {
                var interior = currentInterior;

                if (interior.TvPosition != Vector3.Zero)
                {
                    if (tvObject != 0 && DoesEntityExist(tvObject))
                    {
                        int newChannelIndex = MainMenu.IplManagementMenu.SelectedTvChannel;
                        int channel = 0;
                        LoadTvChannelSequence(channel, tvChannels[MainMenu.IplManagementMenu.SelectedTvChannel], false);

                        uint tvModel = (uint)GetEntityModel(tvObject);

                        if (!IsNamedRendertargetRegistered("tvscreen"))
                        {
                            RegisterNamedRendertarget("tvscreen", false);
                        }

                        if (!IsNamedRendertargetLinked(tvModel))
                        {
                            LinkNamedRendertarget(tvModel);
                        }

                        var renderHandle = GetNamedRendertargetRenderId("tvscreen");

                        SetTvChannel(channel);
                        SetTvVolume(100f);
                        SetTvAudioFrontend(false);

                        while (EnableTvs && currentInterior != null && DoesEntityExist(tvObject))
                        {
                            await Delay(0);

                            EnableMovieSubtitles(IsSubtitlePreferenceSwitchedOn());

                            if (newChannelIndex != MainMenu.IplManagementMenu.SelectedTvChannel)
                            {
                                newChannelIndex = MainMenu.IplManagementMenu.SelectedTvChannel;
                                if ((float)newChannelIndex % 2f != 0f)
                                {
                                    channel = 1;
                                }
                                else
                                {
                                    channel = 0;
                                }

                                LoadTvChannelSequence(channel, tvChannels[newChannelIndex], false);

                                SetTvChannel(channel);

                                SetTvVolume(100f);

                                SetTvAudioFrontend(false);
                            }

                            SetTextRenderId(renderHandle);
                            SetScriptGfxDrawOrder(4);
                            SetScriptGfxDrawBehindPausemenu(true);

                            DrawTvChannel(0.5f, 0.5f, 1f, 1f, 0f, 255, 255, 255, 255);

                            //CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0x35FB78DC42B7BD21, tvObject, 16);
                            AttachTvAudioToEntity(tvObject);

                            SetScriptGfxDrawBehindPausemenu(false);
                            SetTextRenderId(1);
                        }

                        // Disable movie subtitles.
                        EnableMovieSubtitles(false);
                        SetTvChannel(-1); // disable tv.
                    }
                    else
                    {
                        foreach (uint model in tvModels)
                        {
                            tvObject = GetClosestObjectOfType(interior.TvPosition.X, interior.TvPosition.Y, interior.TvPosition.Z, 2f, model, false, false, false);
                            if (DoesEntityExist(tvObject))
                            {
                                break;
                            }
                        }

                    }
                }
            }
        }
        #endregion

        #region animation functions
        /// <summary>
        /// This triggers a helmet visor/goggles toggle if available.
        /// </summary>
        /// <returns></returns>
        async Task SwitchHelmet()
        {
            int component = GetPedPropIndex(Game.PlayerPed.Handle, 0);      // helmet index
            await Delay(1);
            int texture = GetPedPropTextureIndex(Game.PlayerPed.Handle, 0); // texture
            await Delay(1);
            int compHash = GetHashNameForProp(Game.PlayerPed.Handle, 0, component, texture); // prop combination hash
            await Delay(1);
            if (N_0xd40aac51e8e4c663((uint)compHash) > 0) // helmet has visor.
            {
                int newHelmet = 0;
                string animName = "visor_up";
                string animDict = "anim@mp_helmets@on_foot";
                if ((uint)Game.PlayerPed.Model.Hash == (uint)PedHash.FreemodeFemale01)
                {
                    switch (component)
                    {
                        case 49:
                            newHelmet = 67;
                            animName = "visor_up";
                            break;
                        case 50:
                            newHelmet = 68;
                            animName = "visor_up";
                            break;
                        case 51:
                            newHelmet = 69;
                            animName = "visor_up";
                            break;
                        case 52:
                            newHelmet = 70;
                            animName = "visor_up";
                            break;
                        case 62:
                            newHelmet = 71;
                            animName = "visor_up";
                            break;
                        case 66:
                            newHelmet = 81;
                            animName = "visor_down";
                            break;
                        case 67:
                            newHelmet = 49;
                            animName = "visor_down";
                            break;
                        case 68:
                            newHelmet = 50;
                            animName = "visor_down";
                            break;
                        case 69:
                            newHelmet = 51;
                            animName = "visor_down";
                            break;
                        case 70:
                            newHelmet = 52;
                            animName = "visor_down";
                            break;
                        case 71:
                            newHelmet = 62;
                            animName = "visor_down";
                            break;
                        case 72:
                            newHelmet = 73;
                            animName = "visor_up";
                            break;
                        case 73:
                            newHelmet = 72;
                            animName = "visor_down";
                            break;
                        case 77:
                            newHelmet = 78;
                            animName = "visor_up";
                            break;
                        case 78:
                            newHelmet = 77;
                            animName = "visor_down";
                            break;
                        case 79:
                            newHelmet = 80;
                            animName = "visor_up";
                            break;
                        case 80:
                            newHelmet = 79;
                            animName = "visor_down";
                            break;
                        case 81:
                            newHelmet = 66;
                            animName = "visor_up";
                            break;
                        case 90:
                            newHelmet = 91;
                            animName = "visor_up";
                            break;
                        case 91:
                            newHelmet = 90;
                            animName = "visor_down";
                            break;
                        case 115:
                            newHelmet = 116;
                            animName = "goggles_up";
                            break;
                        case 116:
                            newHelmet = 115;
                            animName = "goggles_down";
                            break;
                        case 117:
                            newHelmet = 118;
                            animName = "goggles_up";
                            break;
                        case 118:
                            newHelmet = 117;
                            animName = "goggles_down";
                            break;
                        case 122:
                            newHelmet = 123;
                            animName = "visor_up";
                            break;
                        case 123:
                            newHelmet = 122;
                            animName = "visor_down";
                            break;
                        case 124:
                            newHelmet = 125;
                            animName = "visor_up";
                            break;
                        case 125:
                            newHelmet = 124;
                            animName = "visor_down";
                            break;
                    }
                }
                else if ((uint)Game.PlayerPed.Model.Hash == (uint)PedHash.FreemodeMale01)
                {
                    switch (component)
                    {
                        case 50:
                            newHelmet = 68;
                            animName = "visor_up";
                            break;
                        case 51:
                            newHelmet = 69;
                            animName = "visor_up";
                            break;
                        case 52:
                            newHelmet = 70;
                            animName = "visor_up";
                            break;
                        case 53:
                            newHelmet = 71;
                            animName = "visor_up";
                            break;
                        case 62:
                            newHelmet = 72;
                            animName = "visor_up";
                            break;
                        case 67:
                            newHelmet = 82;
                            animName = "visor_down";
                            break;
                        case 68:
                            newHelmet = 50;
                            animName = "visor_down";
                            break;
                        case 69:
                            newHelmet = 51;
                            animName = "visor_down";
                            break;
                        case 70:
                            newHelmet = 52;
                            animName = "visor_down";
                            break;
                        case 71:
                            newHelmet = 53;
                            animName = "visor_down";
                            break;
                        case 72:
                            newHelmet = 62;
                            animName = "visor_down";
                            break;
                        case 73:
                            newHelmet = 74;
                            animName = "visor_up";
                            break;
                        case 74:
                            newHelmet = 73;
                            animName = "visor_down";
                            break;
                        case 78:
                            newHelmet = 79;
                            animName = "visor_up";
                            break;
                        case 79:
                            newHelmet = 78;
                            animName = "visor_down";
                            break;
                        case 80:
                            newHelmet = 81;
                            animName = "visor_up";
                            break;
                        case 81:
                            newHelmet = 80;
                            animName = "visor_down";
                            break;
                        case 82:
                            newHelmet = 67;
                            animName = "visor_up";
                            break;
                        case 91:
                            newHelmet = 92;
                            animName = "visor_up";
                            break;
                        case 92:
                            newHelmet = 91;
                            animName = "visor_down";
                            break;
                        case 116:
                            newHelmet = 117;
                            animName = "goggles_up";
                            break;
                        case 117:
                            newHelmet = 116;
                            animName = "goggles_down";
                            break;
                        case 118:
                            newHelmet = 119;
                            animName = "goggles_up";
                            break;
                        case 119:
                            newHelmet = 118;
                            animName = "goggles_down";
                            break;
                        case 123:
                            newHelmet = 124;
                            animName = "visor_up";
                            break;
                        case 124:
                            newHelmet = 123;
                            animName = "visor_down";
                            break;
                        case 125:
                            newHelmet = 126;
                            animName = "visor_up";
                            break;
                        case 126:
                            newHelmet = 125;
                            animName = "visor_down";
                            break;
                    }
                }
                else
                {
                    return;
                }
                if (GetFollowPedCamViewMode() == 4)
                {
                    if (animName.Contains("goggles"))
                    {
                        animName = animName.Replace("goggles", "visor");
                    }
                    animName = "pov_" + animName;
                }
                if (Game.PlayerPed.IsInVehicle())
                {
                    if (animName.Contains("goggles"))
                    {
                        ClearAllHelpMessages();
                        BeginTextCommandDisplayHelp("string");
                        AddTextComponentSubstringPlayerName("You can not toggle your goggles while in a vehicle.");
                        EndTextCommandDisplayHelp(0, false, true, 6000);
                        return;
                    }
                    Vehicle veh = GetVehicle();
                    if (veh != null && veh.Exists() && !veh.IsDead && (veh.Model.IsBicycle || veh.Model.IsBike || veh.Model.IsQuadbike))
                    {
                        if (veh.Model.IsQuadbike)
                        {
                            animDict = "anim@mp_helmets@on_bike@quad";
                        }
                        else if (veh.Model.IsBike)
                        {
                            List<uint> sportBikes = new List<uint>()
                            {
                                (uint)GetHashKey("AKUMA"),
                                (uint)GetHashKey("BATI"),
                                (uint)GetHashKey("BATI2"),
                                (uint)GetHashKey("CARBONRS"),
                                (uint)GetHashKey("DEFILER"),
                                (uint)GetHashKey("DIABLOUS2"),
                                (uint)GetHashKey("DOUBLE"),
                                (uint)GetHashKey("FCR"),
                                (uint)GetHashKey("FCR2"),
                                (uint)GetHashKey("HAKUCHOU"),
                                (uint)GetHashKey("HAKUCHOU2"),
                                (uint)GetHashKey("LECTRO"),
                                (uint)GetHashKey("NEMESIS"),
                                (uint)GetHashKey("OPPRESSOR"),
                                (uint)GetHashKey("OPPRESSOR2"),
                                (uint)GetHashKey("PCJ"),
                                (uint)GetHashKey("RUFFIAN"),
                                (uint)GetHashKey("SHOTARO"),
                                (uint)GetHashKey("VADER"),
                                (uint)GetHashKey("VORTEX"),
                            };
                            List<uint> chopperBikes = new List<uint>()
                            {
                                (uint)GetHashKey("SANCTUS"),
                                (uint)GetHashKey("ZOMBIEA"),
                                (uint)GetHashKey("ZOMBIEB"),
                            };
                            List<uint> dirtBikes = new List<uint>()
                            {
                                (uint)GetHashKey("BF400"),
                                (uint)GetHashKey("ENDURO"),
                                (uint)GetHashKey("MANCHEZ"),
                                (uint)GetHashKey("SANCHEZ"),
                                (uint)GetHashKey("SANCHEZ2"),
                                (uint)GetHashKey("ESSKEY"),
                            };
                            List<uint> scooters = new List<uint>()
                            {
                                (uint)GetHashKey("FAGGIO"),
                                (uint)GetHashKey("FAGGIO2"),
                                (uint)GetHashKey("FAGGIO3"),
                                (uint)GetHashKey("CLIFFHANGER"),
                                (uint)GetHashKey("BAGGER"),
                            };
                            List<uint> policeb = new List<uint>()
                            {
                                (uint)GetHashKey("AVARUS"),
                                (uint)GetHashKey("CHIMERA"),
                                (uint)GetHashKey("POLICEB"),
                                (uint)GetHashKey("SOVEREIGN"),
                                (uint)GetHashKey("HEXER"),
                                (uint)GetHashKey("INNOVATION"),
                                (uint)GetHashKey("NIGHTBLADE"),
                                (uint)GetHashKey("RATBIKE"),
                                (uint)GetHashKey("DAEMON"),
                                (uint)GetHashKey("DAEMON2"),
                                (uint)GetHashKey("DIABLOUS"),
                                (uint)GetHashKey("GARGOYLE"),
                                (uint)GetHashKey("THRUST"),
                                (uint)GetHashKey("VINDICATOR"),
                                (uint)GetHashKey("WOLFSBANE"),
                            };

                            if (policeb.Contains((uint)veh.Model.Hash))
                            {
                                animDict = "anim@mp_helmets@on_bike@policeb";
                            }
                            else if (sportBikes.Contains((uint)veh.Model.Hash))
                            {
                                animDict = "anim@mp_helmets@on_bike@sports";
                            }
                            else if (chopperBikes.Contains((uint)veh.Model.Hash))
                            {
                                animDict = "anim@mp_helmets@on_bike@chopper";
                            }
                            else if (dirtBikes.Contains((uint)veh.Model.Hash))
                            {
                                animDict = "anim@mp_helmets@on_bike@dirt";
                            }
                            else if (scooters.Contains((uint)veh.Model.Hash))
                            {
                                animDict = "anim@mp_helmets@on_bike@scooter";
                            }
                            else
                            {
                                animDict = "anim@mp_helmets@on_bike@sports";
                            }
                        }
                        else if (veh.Model.IsBicycle)
                        {
                            animDict = "anim@mp_helmets@on_bike@scooter";
                        }
                    }
                }
                if (!HasAnimDictLoaded(animDict))
                {
                    RequestAnimDict(animDict);
                    while (!HasAnimDictLoaded(animDict))
                    {
                        await Delay(0);
                    }
                }
                ClearPedTasks(Game.PlayerPed.Handle);
                TaskPlayAnim(Game.PlayerPed.Handle, animDict, animName, 8.0f, 1.0f, -1, 48, 0.0f, false, false, false);
                int timeoutTimer = GetGameTimer();
                while (GetEntityAnimCurrentTime(Game.PlayerPed.Handle, animDict, animName) <= 0.0f)
                {
                    if (GetGameTimer() - timeoutTimer > 2000)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Debug.WriteLine("[vMenu] [WARNING] Waiting for animation to start took too long. Preventing hanging of function.");
                        return;
                    }
                    await Delay(0);
                }
                timeoutTimer = GetGameTimer();
                while (GetEntityAnimCurrentTime(Game.PlayerPed.Handle, animDict, animName) > 0.0f)
                {
                    await Delay(0);
                    if (GetGameTimer() - timeoutTimer > 3000)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Debug.WriteLine("[vMenu] [WARNING] Waiting for animation duration took too long. Preventing hanging of function.");
                        return;
                    }
                    if (GetEntityAnimCurrentTime(Game.PlayerPed.Handle, animDict, animName) > 0.39f)
                    {
                        SetPedPropIndex(Game.PlayerPed.Handle, 0, newHelmet, texture, true);
                    }
                }
                ClearPedTasks(Game.PlayerPed.Handle);
                RemoveAnimDict(animDict);
            }
        }

        /// <summary>
        /// Pickup a snowball.
        /// </summary>
        /// <returns></returns>
        async Task PickupSnowball()
        {
            ClearPedTasks(Game.PlayerPed.Handle);
            if (IsAllowed(Permission.WPSnowball)) // only if the player is allowed to spawn in snowballs.
            {
                int maxAmmo = 10;
                GetMaxAmmo(Game.PlayerPed.Handle, snowball_hash, ref maxAmmo);
                if (GetAmmoInPedWeapon(Game.PlayerPed.Handle, snowball_hash) < maxAmmo)
                {
                    SetPedCurrentWeaponVisible(Game.PlayerPed.Handle, false, true, false, false);
                    if (!HasAnimDictLoaded(snowball_anim_dict))
                    {
                        RequestAnimDict(snowball_anim_dict);
                        while (!HasAnimDictLoaded(snowball_anim_dict))
                        {
                            await Delay(0);
                        }
                    }
                    TaskPlayAnim(Game.PlayerPed.Handle, snowball_anim_dict, snowball_anim_name, 8f, 1f, -1, 0, 0f, false, false, false);
                    bool fired = false;

                    var dur = GetAnimDuration(snowball_anim_dict, snowball_anim_name);
                    int timer = GetGameTimer();
                    while (GetEntityAnimCurrentTime(Game.PlayerPed.Handle, snowball_anim_dict, snowball_anim_name) < 0.97f)
                    {
                        await Delay(0);
                        if (!fired)
                        {
                            if (HasAnimEventFired(Game.PlayerPed.Handle, (uint)GetHashKey("CreateObject")))
                            {
                                AddAmmoToPed(Game.PlayerPed.Handle, snowball_hash, 2);
                                GiveWeaponToPed(Game.PlayerPed.Handle, snowball_hash, 0, true, true);
                                if (GetAmmoInPedWeapon(Game.PlayerPed.Handle, snowball_hash) > maxAmmo)
                                {
                                    SetPedAmmo(Game.PlayerPed.Handle, snowball_hash, maxAmmo);
                                }
                                fired = true;
                            }
                            else if (HasAnimEventFired(Game.PlayerPed.Handle, (uint)GetHashKey("Interrupt")))
                            {
                                break;
                            }
                        }
                        else if (HasAnimEventFired(Game.PlayerPed.Handle, (uint)GetHashKey("Interrupt")))
                        {
                            break;
                        }
                        // fail safe just in case
                        if (GetGameTimer() - timer > (dur * 1000f))
                        {
                            break;
                        }
                    }

                }
                else
                {
                    ClearAllHelpMessages();
                    BeginTextCommandDisplayHelp("string");
                    AddTextComponentSubstringPlayerName($"You can not carry more than {maxAmmo} snowballs!");
                    EndTextCommandDisplayHelp(0, false, true, 6000);
                }
            }
        }
        #endregion
    }
}
