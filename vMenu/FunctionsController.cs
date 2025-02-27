using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.UI;

using MenuAPI;

using Newtonsoft.Json;

using vMenuClient.data;
using vMenuClient.menus;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;
using static vMenuClient.CommonFunctions;
using static vMenuClient.data.PedModels;
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
        private readonly List<int> deadPlayers = new();
        private float cameraRotationHeading = 0f;

        // show location variables
        private float safeZoneSizeX = (1 / GetSafeZoneSize() / 3.0f) - 0.358f;
        private string zoneDisplay = "";
        private string streetDisplay = "";
        private string headingDisplay = "";

        private readonly List<int> waypointPlayerIdsToRemove = new();
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

        private bool stopPropsLoop = false;
        private bool stopVehiclesLoop = false;
        private bool stopPedsLoop = false;
        private List<Prop> props = new();
        private List<Vehicle> vehicles = new();
        private List<Ped> peds = new();

        public FunctionsController() { }

        /// <summary>
        /// Setup the required tick functions
        /// </summary>
        [EventHandler("vMenu:SetupTickFunctions")]
        public void SetupTickFunctions()
        {
            // Always needed
            Tick += AnimationsAndInteractions;
            Tick += PlayerClothingAnimationsController;
            Tick += MiscRecordingKeybinds;
            Tick += MiscSettings;
            Tick += GeneralTasks;
            Tick += GcTick;

            if (GetSettingsBool(Setting.keep_player_head_props))
            {
                if (Game.PlayerPed is not null && Game.PlayerPed.Exists())
                {
                    SetPedCanLosePropsOnDamage(Game.PlayerPed.Handle, false, 0);
                }

                Tick += PlayerHeadPropsTick;
            }

            // Configuration and permissions based
            if (IsAllowed(Permission.WOMenu) && GetSettingsBool(Setting.vmenu_enable_weather_sync))
            {
                Tick += WeatherOptions;
            }
            if (IsAllowed(Permission.TOMenu) && GetSettingsBool(Setting.vmenu_enable_time_sync))
            {
                Tick += TimeOptions;
            }

            // Configuration based
            if (!GetSettingsBool(Setting.vmenu_disable_spawning_as_default_character))
            {
                Tick += RestorePlayerAfterBeingDead;
            }
            if (!GetSettingsBool(Setting.vmenu_disable_entity_outlines_tool))
            {
                Tick += SlowMiscTick;
                Tick += ModelDrawDimensions;
            }

            // Permissions based
            if (IsAllowed(Permission.POMenu) || IsAllowed(Permission.VOMenu))
            {
                Tick += DoPlayerAndVehicleChecks;
            }
            if (IsAllowed(Permission.VOMenu))
            {
                Tick += VehicleOptions;
                Tick += VehicleShowHealthOnScreenTick;
                if (IsAllowed(Permission.VOFlashHighbeamsOnHonk))
                {
                    Tick += VehicleHighbeamFlashTick;
                }
            }
            if (IsAllowed(Permission.VCMenu))
            {
                Tick += VoiceChat;
            }
            if (IsAllowed(Permission.WPMenu))
            {
                Tick += WeaponOptions;
            }
            if (IsAllowed(Permission.OPMenu))
            {
                Tick += OnlinePlayersTasks;
            }
            if (IsAllowed(Permission.MSDeathNotifs))
            {
                Tick += DeathNotifications;
            }
            if (IsAllowed(Permission.MSShowLocation))
            {
                Tick += UpdateLocation;
            }
            if (IsAllowed(Permission.PAMenu))
            {
                Tick += ManageCamera;
                Tick += DisableMovement;
            }
            if (IsAllowed(Permission.MSPlayerBlips))
            {
                Tick += PlayerBlipsControl;
            }
            if (IsAllowed(Permission.MSOverheadNames))
            {
                Tick += PlayerOverheadNamesControl;
            }
            if (IsAllowed(Permission.POMenu))
            {
                Tick += PlayerOptions;
            }
            if (IsAllowed(Permission.WPSnowball))
            {
                Tick += SnowballPickupHelpMessageTask;
            }
            if (IsAllowed(Permission.PVLockDoors))
            {
                Tick += PersonalVehicleOptions;
            }
            if (GetSettingsBool(Setting.vmenu_enable_animals_spawn_menu))
            {
                Tick += AnimalPedCameraChangeBlocker;
            }
            if (IsAllowed(Permission.OPSpectate))
            {
                Tick += SpectateHandling;
            }
        }

        private async Task PlayerHeadPropsTick()
        {
            if (Game.PlayerPed is not null && Game.PlayerPed.Exists())
            {
                var ped = Game.PlayerPed.Handle;
                await Delay(100);
                if (Game.PlayerPed.Handle != ped)
                {
                    SetPedCanLosePropsOnDamage(Game.PlayerPed.Handle, false, 0);
                }
            }
            else
            {
                await Delay(1000);
            }
        }

        /// Task related
        #region gc thread
        int gcTimer = GetGameTimer();
        /// <summary>
        /// Task for clearing unused memory periodically.
        /// </summary>
        /// <returns></returns>
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

        #region General Tasks
        /// <summary>
        /// All general tasks that run every 1 game ticks (and are not (sub)menu specific).
        /// </summary>
        /// <returns></returns>
        private async Task GeneralTasks()
        {
            // Check if the player has switched to a new vehicle.
            if (Game.PlayerPed.IsInVehicle()) // added this for improved performance.
            {
                var tmpVehicle = GetVehicle();
                if (tmpVehicle != null && tmpVehicle.Exists() && tmpVehicle.Handle != LastVehicle)
                {
                    // Set the last vehicle to the new vehicle entity.
                    LastVehicle = tmpVehicle.Handle;
                    SwitchedVehicle = true;
                }
            }
            // this can wait 1 ms
            await Delay(1);
        }
        #endregion

        #region Player Options Tasks
        /// <summary>
        /// Run all tasks for the Player Options menu.
        /// </summary>
        /// <returns></returns>
        private async Task PlayerOptions()
        {
            // perms
            var godmodeAllowed = IsAllowed(Permission.POGod);
            var noRagdollAllowed = IsAllowed(Permission.PONoRagdoll);

            if (MainMenu.MpPedCustomizationMenu != null && MainMenu.MpPedCustomizationMenu.appearanceMenu != null && MainMenu.MpPedCustomizationMenu.faceShapeMenu != null && MainMenu.MpPedCustomizationMenu.createCharacterMenu != null && MainMenu.MpPedCustomizationMenu.inheritanceMenu != null && MainMenu.MpPedCustomizationMenu.propsMenu != null && MainMenu.MpPedCustomizationMenu.clothesMenu != null && MainMenu.MpPedCustomizationMenu.tattoosMenu != null)
            {
                // Manage Player God Mode
                static bool IsMpPedCreatorOpen()
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

            // Manage PlayerInvisible
            if (GetSettingsBool(Setting.vmenu_handle_invisibility) && MainMenu.PlayerOptionsMenu.PlayerInvisible && IsAllowed(Permission.POInvisible))
            {
                SetEntityVisible(Game.PlayerPed.Handle, false, false);
            }

            // Manage Super jump.
            if (MainMenu.PlayerOptionsMenu.PlayerSuperJump && IsAllowed(Permission.POSuperjump))
            {
                SetSuperJumpThisFrame(Game.Player.Handle);
            }

            // Manage PlayerNoRagdoll
            SetPedCanRagdoll(Game.PlayerPed.Handle, (!MainMenu.PlayerOptionsMenu.PlayerNoRagdoll && noRagdollAllowed) ||
                (!noRagdollAllowed));


            // Manage never wanted.
            if (MainMenu.PlayerOptionsMenu.PlayerNeverWanted && GetPlayerWantedLevel(Game.Player.Handle) > 0 && IsAllowed(Permission.PONeverWanted))
            {
                ClearPlayerWantedLevel(Game.Player.Handle);
                if (GetMaxWantedLevel() > 0)
                {
                    SetMaxWantedLevel(0);
                }
            }

            if (DriveToWpTaskActive && !Game.IsWaypointActive)
            {
                ClearPedTasks(Game.PlayerPed.Handle);
                Notify.Custom("Destination reached, the car will now stop driving!");
                DriveToWpTaskActive = false;
            }
            await Task.FromResult(0);
        }
        #endregion

        #region shared player options and vehicle options
        /// <summary>
        /// Slow tick that does some basic checks for shared vehicle/player options.
        /// </summary>
        /// <returns></returns>
        private async Task DoPlayerAndVehicleChecks()
        {
            var god = IsAllowed(Permission.POGod) && MainMenu.PlayerOptionsMenu != null && MainMenu.PlayerOptionsMenu.PlayerGodMode;
            await Delay(100);

            var vehGod = IsAllowed(Permission.VOGod) && MainMenu.VehicleOptionsMenu != null && MainMenu.VehicleOptionsMenu.VehicleGodMode;
            await Delay(100);

            var ignored = IsAllowed(Permission.POIgnored) && MainMenu.PlayerOptionsMenu != null && MainMenu.PlayerOptionsMenu.PlayerIsIgnored;
            await Delay(100);

            var stayInVeh = IsAllowed(Permission.POStayInVehicle) && MainMenu.PlayerOptionsMenu != null && MainMenu.PlayerOptionsMenu.PlayerStayInVehicle;
            await Delay(100);

            var bikeSeatbelt = IsAllowed(Permission.VOBikeSeatbelt) && MainMenu.VehicleOptionsMenu != null && MainMenu.VehicleOptionsMenu.VehicleBikeSeatbelt;
            await Delay(100);

            var noRagdoll = IsAllowed(Permission.PONoRagdoll) && MainMenu.PlayerOptionsMenu != null && MainMenu.PlayerOptionsMenu.PlayerNoRagdoll;
            await Delay(100);

            var cantBeKnockedOff = god || vehGod || bikeSeatbelt || noRagdoll;
            var cantBeDraggedOut = god || vehGod || ignored || stayInVeh;
            var cantBeShotInVehicle = god || vehGod;

            Game.PlayerPed.CanBeDraggedOutOfVehicle = !cantBeDraggedOut;
            Game.PlayerPed.CanBeShotInVehicle = !cantBeShotInVehicle;
            Game.PlayerPed.CanBeKnockedOffBike = !cantBeKnockedOff;
            await Delay(1000);
        }
        #endregion

        #region Vehicle Options Tasks
        /// <summary>
        /// Manage all vehicle related tasks.
        /// </summary>
        /// <returns></returns>
        private async Task VehicleOptions()
        {
            // When the player is in a valid vehicle:
            if (IsPedInAnyVehicle(Game.PlayerPed.Handle, true))
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    // God mode
                    var god = MainMenu.VehicleOptionsMenu.VehicleGodMode && IsAllowed(Permission.VOGod);
                    var invincibleGod = MainMenu.VehicleOptionsMenu.VehicleGodInvincible && god;
                    var visualGod = MainMenu.VehicleOptionsMenu.VehicleGodVisual && god;
                    var engineGod = MainMenu.VehicleOptionsMenu.VehicleGodEngine && god;
                    var strongWheelsGod = MainMenu.VehicleOptionsMenu.VehicleGodStrongWheels && god;
                    var autoRepairGod = MainMenu.VehicleOptionsMenu.VehicleGodAutoRepair && god;
                    var rampGod = MainMenu.VehicleOptionsMenu.VehicleGodRamp && god;

                    SetRampVehicleReceivesRampDamage(veh.Handle, !rampGod);

                    if (visualGod && IsVehicleDamaged(veh.Handle))
                    {
                        RemoveDecalsFromVehicle(veh.Handle);
                    }

                    if (autoRepairGod && IsVehicleDamaged(veh.Handle))
                    {
                        veh.Repair();
                    }

                    veh.CanBeVisiblyDamaged = !visualGod;

                    veh.CanEngineDegrade = !engineGod;
                    if (engineGod && veh.EngineHealth < 1000f)
                    {
                        veh.EngineHealth = 1000f;
                    }

                    veh.CanWheelsBreak = !strongWheelsGod;
                    veh.IsAxlesStrong = strongWheelsGod;

                    veh.IsBulletProof = invincibleGod;
                    veh.IsCollisionProof = invincibleGod;
                    veh.IsExplosionProof = invincibleGod;
                    veh.IsFireProof = invincibleGod;
                    veh.IsInvincible = invincibleGod;
                    veh.IsMeleeProof = invincibleGod;

                    foreach (var vehicleDoor in veh.Doors.GetAll())
                    {
                        vehicleDoor.CanBeBroken = !invincibleGod;
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

                        // Get the license plate type index.

                        // Set the license plate index list item to the correct index.
                        if (IsAllowed(Permission.VOChangePlate) && MainMenu.VehicleOptionsMenu.GetMenu().GetMenuItems().Find(mi => mi is MenuListItem li && li.ListItems.Any(liText => liText == GetLabelText("CMOD_PLA_0"))) is MenuListItem listItem)
                        {
                            // Set the license plate style.
                            switch (veh.Mods.LicensePlateStyle)
                            {
                                case LicensePlateStyle.BlueOnWhite1:
                                    listItem.ListIndex = 0;
                                    break;
                                case LicensePlateStyle.YellowOnBlack:
                                    listItem.ListIndex = 1;
                                    break;
                                case LicensePlateStyle.YellowOnBlue:
                                    listItem.ListIndex = 2;
                                    break;
                                case LicensePlateStyle.BlueOnWhite2:
                                    listItem.ListIndex = 3;
                                    break;
                                case LicensePlateStyle.BlueOnWhite3:
                                    listItem.ListIndex = 4;
                                    break;
                                case LicensePlateStyle.NorthYankton:
                                    listItem.ListIndex = 5;
                                    break;
                                case LicensePlateStyle.ECola:
                                    listItem.ListIndex = 6;
                                    break;
                                case LicensePlateStyle.LasVenturas:
                                    listItem.ListIndex = 7;
                                    break;
                                case LicensePlateStyle.LibertyCity:
                                    listItem.ListIndex = 8;
                                    break;
                                case LicensePlateStyle.LSCarMeet:
                                    listItem.ListIndex = 9;
                                    break;
                                case LicensePlateStyle.LSPanic:
                                    listItem.ListIndex = 10;
                                    break;
                                case LicensePlateStyle.LSPounders:
                                    listItem.ListIndex = 11;
                                    break;
                                case LicensePlateStyle.Sprunk:
                                    listItem.ListIndex = 12;
                                    break;
                                default:
                                    break;
                            }
                        }

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

                        // Set the helicopter turbulence multiplier in case the vehicle was changed:
                        if (veh.Model.IsHelicopter)
                        {
                            if (MainMenu.VehicleOptionsMenu.DisableHelicopterTurbulence && IsAllowed(Permission.VODisableTurbulence))
                            {
                                SetHeliTurbulenceScalar(veh.Handle, 0f);
                            }
                            else
                            {
                                SetHeliTurbulenceScalar(veh.Handle, 1.0f);
                            }
                        }

                        if (veh.Model.IsBoat)
                        {
                            if (MainMenu.VehicleOptionsMenu.AnchorBoat && IsAllowed(Permission.VOAnchorBoat) && CanAnchorBoatHere(veh.Handle))
                            {
                                SetBoatAnchor(veh.Handle, true);
                                SetBoatFrozenWhenAnchored(veh.Handle, true);
                                SetForcedBoatLocationWhenAnchored(veh.Handle, true);
                            }
                            else
                            {
                                SetBoatAnchor(veh.Handle, false);
                                SetBoatFrozenWhenAnchored(veh.Handle, false);
                                SetForcedBoatLocationWhenAnchored(veh.Handle, false);
                            }
                        }
                    }

                    // Manage "no helmet"
                    var ped = Game.PlayerPed;
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

                    if (MainMenu.VehicleOptionsMenu.VehicleInfiniteFuel && DecorIsRegisteredAsType("_Fuel_Level", 1) && IsAllowed(Permission.VOInfiniteFuel))
                    {
                        var maxFuelLevel = GetVehicleHandlingFloat(veh.Handle, "CHandlingData", "fPetrolTankVolume");
                        var currentFuelLevel = GetVehicleFuelLevel(veh.Handle);
                        if (maxFuelLevel > 5f && currentFuelLevel < (maxFuelLevel * 0.95f))
                        {
                            try
                            {
                                DecorSetFloat(veh.Handle, "_Fuel_Level", maxFuelLevel);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(@"[CRITICAL] A critical bug in one of your scripts was detected. vMenu is unable to set or register a decorator's value because another resource has already registered 1.5k or more decorators. vMenu will NOT work as long as this bug in your other scripts is unsolved. Please fix your other scripts. This is *NOT* caused by or fixable by vMenu!!!");
                                Debug.WriteLine($"Error Location: {e.StackTrace}\nError info: {e.Message}");
                                await Delay(1000);
                            }
                        }
                    }
                    await Delay(0);
                }
            }
            // When the player is not inside a vehicle:
            else
            {
                var lastVehicle = GetVehicle(true);
                if (lastVehicle != null && lastVehicle.Exists())
                {
                    if (!lastVehicle.IsVisible)
                    {
                        lastVehicle.IsVisible = true;
                    }
                }

                var subMenus = new List<Menu>()
                    {
                        MainMenu.VehicleOptionsMenu.DeleteConfirmMenu,
                        MainMenu.VehicleOptionsMenu.VehicleColorsMenu,
                        MainMenu.VehicleOptionsMenu.VehicleComponentsMenu,
                        MainMenu.VehicleOptionsMenu.VehicleDoorsMenu,
                        MainMenu.VehicleOptionsMenu.VehicleLiveriesMenu,
                        MainMenu.VehicleOptionsMenu.VehicleModMenu,
                        MainMenu.VehicleOptionsMenu.VehicleUnderglowMenu,
                        MainMenu.VehicleOptionsMenu.VehicleWindowsMenu,
                    };
                foreach (var m in subMenus)
                {
                    if (m.Visible)
                    {
                        m.GoBack();
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
                {
                    SetVehicleEngineOn(GetVehicle(true).Handle, true, true, true);
                }
            }
            await Task.FromResult(0);
        }

        /// <summary>
        /// Vehicle control options for flashing highbeams.
        /// </summary>
        /// <returns></returns>
        private async Task VehicleHighbeamFlashTick()
        {
            if (MainMenu.VehicleOptionsMenu.FlashHighbeamsOnHonk && IsPedInAnyVehicle(Game.PlayerPed.Handle, true))
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead)
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
                }
            }
            await Task.FromResult(0);
        }

        /// <summary>
        /// Shows the vehicle health on screen.
        /// </summary>
        /// <returns></returns>
        private async Task VehicleShowHealthOnScreenTick()
        {
            if (MainMenu.VehicleOptionsMenu.VehicleShowHealth)
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    static string GetHealthString(double health)
                    {
                        string color;
                        if (health <= 0)
                        {
                            color = "~r~";
                        }
                        else
                        {
                            color = (double)Math.Floor(Map(health, 0, 1000, 0, 4)) switch
                            {
                                0 => "~r~",
                                1 => "~o~",
                                2 => "~y~",
                                _ => "~g~",
                            };
                        }
                        return $"{color}{health}";
                    }
                    DrawTextOnScreen($"~n~Engine health: {GetHealthString(Math.Round(veh.EngineHealth, 2))}", 0.5f, 0.0f);
                    DrawTextOnScreen($"~n~~n~Body health: {GetHealthString(Math.Round(veh.BodyHealth, 2))}", 0.5f, 0.0f);
                    DrawTextOnScreen($"~n~~n~~n~Tank health: {GetHealthString(Math.Round(veh.PetrolTankHealth, 2))}", 0.5f, 0.0f);
                }
            }

            await Task.FromResult(0);
        }
        #endregion

        #region Weather Options
        private async Task WeatherOptions()
        {
            await Delay(100);
            var weatherMenu = MainMenu.WeatherOptionsMenu.GetMenu();
            if (weatherMenu != null && weatherMenu.Visible)
            {
                if (IsAllowed(Permission.WODynamic))
                {
                    MainMenu.WeatherOptionsMenu.dynamicWeatherEnabled.Checked = EventManager.DynamicWeatherEnabled;
                }
                if (IsAllowed(Permission.WOBlackout))
                {
                    MainMenu.WeatherOptionsMenu.blackout.Checked = EventManager.IsBlackoutEnabled;
                }
                if (IsAllowed(Permission.WOSetWeather))
                {
                    MainMenu.WeatherOptionsMenu.snowEnabled.Checked = EventManager.IsSnowEnabled;
                    weatherMenu.GetMenuItems().ForEach(it =>
                    {
                        if (it.ItemData is string weatherType)
                        {
                            if (weatherType == EventManager.GetServerWeather)
                            {
                                it.RightIcon = MenuItem.Icon.TICK;
                            }
                            else
                            {
                                it.RightIcon = MenuItem.Icon.NONE;
                            }
                        }
                    });
                }
            }
        }
        #endregion

        #region Misc Settings Menu Tasks

        #region misc settings draw text function
        /// <summary>
        /// Draws various misc settings menu text items, like coordinates, location time and speed.
        /// </summary>
        private async void DrawMiscSettingsText()
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
                var hour = World.CurrentDayTime.Hours;
                var minute = World.CurrentDayTime.Minutes;
                var timestring = $"{(hour < 10 ? "0" + hour.ToString() : hour.ToString())}:{(minute < 10 ? "0" + minute.ToString() : minute.ToString())}";
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
            await Task.FromResult(0);
        }
        #endregion

        #region Update Location for location display
        /// <summary>
        /// Updates the location for location display.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateLocation()
        {
            if (MainMenu.MiscSettingsMenu.ShowLocation)
            {
                // Get the current location.
                var currentPos = GetEntityCoords(Game.PlayerPed.Handle, true);
                var heading = Game.PlayerPed.Heading;
                zoneDisplay = World.GetZoneLocalizedName(currentPos);

                // Get the nearest vehicle node.
                var nodePos = Vector3.Zero;
                GetNthClosestVehicleNode(currentPos.X, currentPos.Y, currentPos.Z, 0, ref nodePos, 0, 0, 0);

                // Get the safezone size for x and y to be able to move with the minimap.
                safeZoneSizeX = (1 / GetSafeZoneSize() / 3.0f) - 0.358f;

                // Get the cross road.
                uint mainSt = 0, crossSt = 0;
                GetStreetNameAtCoord(currentPos.X, currentPos.Y, currentPos.Z, ref mainSt, ref crossSt);
                var mainName = GetStreetNameFromHashKey(mainSt);
                var crossName = GetStreetNameFromHashKey(crossSt);

                // Set the suffix for the road name to the corssing name, or to an empty string if there's no crossing.
                var prefix = currentPos.DistanceToSquared(nodePos) > 1400f ? "~m~Near ~s~" : "~s~";
                var suffix = crossSt != 0 ? "~t~ / " + crossName : "";
                streetDisplay = prefix + mainName + suffix;

                if (heading is > 320 or < 45) // North
                {
                    headingDisplay = "N";
                }
                else if (heading is >= 45 and <= 135) // West
                {
                    headingDisplay = "W";
                }
                else if (heading is > 135 and < 225) // South
                {
                    headingDisplay = "S";
                }
                else // East
                {
                    headingDisplay = "E";
                }

                await Delay(200);
            }
            else
            {
                await Delay(1000);
            }
        }
        #endregion

        #region ShowLocation
        /// <summary>
        /// Show location function to show the player's location.
        /// </summary>
        private void ShowLocation()
        {
            // Draw the street name + crossing.
            SetTextWrap(0f, 1f);
            DrawTextOnScreen(streetDisplay, 0.234f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(0.48f, 6) - GetTextScaleHeight(0.48f, 6)/*0.925f - safeZoneSizeY*/, 0.48f);

            // Draw the zone name.
            SetTextWrap(0f, 1f);
            DrawTextOnScreen(zoneDisplay, 0.234f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(0.45f, 6) - GetTextScaleHeight(0.95f, 6)/*0.9485f - safeZoneSizeY*/, 0.45f);

            // Draw the left border for the heading character.
            DrawTextOnScreen("~t~|", 0.188f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(1.2f, 6) - GetTextScaleHeight(0.4f, 6)/*0.915f - safeZoneSizeY*/, 1.2f, Alignment.Left);

            // Draw the heading character.
            SetTextWrap(0f, 1f);
            DrawTextOnScreen(headingDisplay, 0.208f + safeZoneSizeX, GetSafeZoneSize() - GetTextScaleHeight(1.2f, 6) - GetTextScaleHeight(0.4f, 6)/*0.915f - safeZoneSizeY*/, 1.2f, Alignment.Center);

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
            var speed = int.Parse(Math.Round(GetEntitySpeed(GetVehicle().Handle) * 3.6f).ToString());
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

        #region Main misc settings function
        int radarSwitchTimer = 0;
        int lastPressedPoint = 0;
        /// <summary>
        /// Run all tasks that need to be handeled for the Misc Settings Menu.
        /// </summary>
        /// <returns></returns>
        private async Task MiscSettings()
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
                        var veh = GetVehicle();
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
            if (MainMenu.MiscSettingsMenu.KbPointKeys)
            {
                static async Task TogglePointing()
                {
                    if (IsPedPointing(Game.PlayerPed.Handle))
                    {
                        ClearPedSecondaryTask(Game.PlayerPed.Handle);
                    }
                    else
                    {
                        if (!HasAnimDictLoaded("anim@mp_point"))
                        {
                            RequestAnimDict("anim@mp_point");
                        }
                        while (!HasAnimDictLoaded("anim@mp_point"))
                        {
                            await Delay(0);
                        }
                        TaskMoveNetwork(Game.PlayerPed.Handle, "task_mp_pointing", 0.5f, false, "anim@mp_point", 24);
                        RemoveAnimDict("anim@mp_point");
                    }
                }
                // Double press the right analog stick for controllers.
                if (Game.CurrentInputMode == InputMode.GamePad)
                {
                    if (Game.IsControlJustReleased(0, Control.SpecialAbilitySecondary) && !Game.PlayerPed.IsInVehicle())
                    {
                        if (GetGameTimer() - lastPressedPoint < 300)
                        {
                            lastPressedPoint = GetGameTimer();
                            await TogglePointing();
                        }
                        else
                        {
                            lastPressedPoint = GetGameTimer();
                        }
                    }
                }
                // Press the B button on keyboard once to toggle.
                else
                {
                    if (Game.IsControlJustReleased(0, Control.SpecialAbilitySecondary) && UpdateOnscreenKeyboard() != 0 && !Game.PlayerPed.IsInVehicle())
                    {
                        await TogglePointing();
                    }
                }

                // Set pitch, heading, blocking, first person and speed properties on animation.
                if (IsPedPointing(Game.PlayerPed.Handle))
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        ClearPedSecondaryTask(Game.PlayerPed.Handle);
                    }
                    else
                    {
                        SetTaskMoveNetworkSignalFloat(Game.PlayerPed.Handle, "Pitch", GetPointingPitch());
                        SetTaskMoveNetworkSignalFloat(Game.PlayerPed.Handle, "Heading", GetPointingHeading());
                        SetTaskMoveNetworkSignalBool(Game.PlayerPed.Handle, "isBlocked", GetPointingIsBlocked());
                        if (GetFollowPedCamViewMode() == 4)
                        {
                            SetTaskMoveNetworkSignalBool(Game.PlayerPed.Handle, "isFirstPerson", true);
                        }
                        else
                        {
                            SetTaskMoveNetworkSignalBool(Game.PlayerPed.Handle, "isFirstPerson", false);
                        }
                        SetTaskMoveNetworkSignalFloat(Game.PlayerPed.Handle, "Speed", 0.25f);
                    }
                }
            }

            if (GetProfileSetting(221) == 1) // 221 = settings > display > expanded radar
            {
                SetBigmapActive(true, false);
            }
            else
            {
                if (IsBigmapActive() && GetGameTimer() - radarSwitchTimer > 8000)
                {
                    SetBigmapActive(false, false);
                }
                if (Game.IsControlJustReleased(0, Control.MultiplayerInfo) && Game.IsControlEnabled(0, Control.MultiplayerInfo) && MainMenu.MiscSettingsMenu.KbRadarKeys && !MenuController.IsAnyMenuOpen() && !IsPauseMenuActive())
                {
                    var radarExpanded = IsBigmapActive();

                    if (radarExpanded)
                    {
                        SetBigmapActive(false, false);
                    }
                    else
                    {
                        SetBigmapActive(true, false);
                        radarSwitchTimer = GetGameTimer();
                    }
                }
            }
        }

        #endregion

        #region Misc settings recording keybinds
        /// <summary>
        /// Function that manages the recording keybinds.
        /// </summary>
        /// <returns></returns>
        private async Task MiscRecordingKeybinds()
        {
            if (MainMenu.MiscSettingsMenu.KbRecordKeys)
            {
                if (!IsPauseMenuActive() && IsScreenFadedIn() && !IsPlayerSwitchInProgress() && !MenuController.IsAnyMenuOpen())
                {
                    if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                    {
                        var recordKey = 0 == Control.ReplayStartStopRecording ? Control.SaveReplayClip : Control.ReplayStartStopRecording;
                        if (!IsRecording())
                        {
                            if (Game.IsControlJustReleased(0, recordKey))
                            {
                                StartRecording(1);
                                if (recordKey == Control.ReplayStartStopRecording)
                                {
                                    HelpMessage.Custom("Press ~INPUT_REPLAY_START_STOP_RECORDING~ to save the recording, press ~INPUT_REPLAY_CLIP_DELETE~ to discard the recording.");
                                }
                                else
                                {
                                    HelpMessage.Custom("Press ~INPUT_SAVE_REPLAY_CLIP~ to save the recording, press ~INPUT_REPLAY_CLIP_DELETE~ to discard the recording.");
                                }

                            }
                        }
                        else
                        {
                            if (Game.IsControlJustReleased(0, recordKey))
                            {
                                StopRecording();
                            }
                            if (Game.IsControlJustPressed(0, Control.ReplayClipDelete)) // delete key on keyboard
                            {
                                StopRecordingAndDiscardClip();
                            }
                        }
                    }
                    else
                    {
                        if (Game.IsControlPressed(0, Control.MultiplayerInfo))
                        {
                            var timer = GetGameTimer();
                            var longEnough = false;
                            var notifOne = -1;
                            var notifTwo = -1;
                            while (Game.IsControlPressed(0, Control.MultiplayerInfo))
                            {
                                if (GetGameTimer() - timer > 400 && !longEnough)
                                {
                                    longEnough = true;

                                    if (IsRecording())
                                    {
                                        SetNotificationTextEntry("STRING");
                                        notifOne = DrawNotificationWithButton(1, "~INPUT_REPLAY_START_STOP_RECORDING~", "Stop recording and save clip.");
                                        SetNotificationTextEntry("STRING");
                                        notifTwo = DrawNotificationWithButton(1, "~INPUT_SAVE_REPLAY_CLIP~", "Stop recording and delete clip.");
                                    }
                                    else
                                    {
                                        SetNotificationTextEntry("STRING");
                                        notifOne = DrawNotificationWithButton(1, "~INPUT_REPLAY_START_STOP_RECORDING~", "Start recording.");
                                    }
                                }

                                if (longEnough)
                                {
                                    Game.DisableControlThisFrame(0, Control.VehicleCinCam);

                                    if (IsRecording())
                                    {
                                        if (Game.IsControlJustReleased(0, Control.SaveReplayClip))
                                        {
                                            StopRecordingAndDiscardClip();
                                            break;
                                        }
                                        if (Game.IsControlJustReleased(0, Control.ReplayStartStopRecording))
                                        {
                                            StopRecording();
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (Game.IsControlJustReleased(0, Control.ReplayStartStopRecording))
                                        {
                                            StartRecording(1);
                                            HelpMessage.Custom("Hold down ~INPUT_MULTIPLAYER_INFO~ and press ~INPUT_REPLAY_START_STOP_RECORDING~ to save the recording, press ~INPUT_SAVE_REPLAY_CLIP~ to discard the recording.");
                                            break;
                                        }
                                    }
                                }
                                await Delay(0);
                            }

                            if (notifOne != -1)
                            {
                                RemoveNotification(notifOne);
                            }
                            if (notifTwo != -1)
                            {
                                RemoveNotification(notifTwo);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Join / Quit notifications (via events)
        /// <summary>
        /// Runs join/quit notification checks.
        /// </summary>
        /// <returns></returns>
        [EventHandler("vMenu:PlayerJoinQuit")]
        internal void OnJoinQuitNotification(string playerName, string dropReason)
        {
            if (MainMenu.PermissionsSetupComplete && MainMenu.MiscSettingsMenu != null)
            {
                // Join/Quit notifications
                if (MainMenu.MiscSettingsMenu.JoinQuitNotifications && IsAllowed(Permission.MSJoinQuitNotifs))
                {
                    if (dropReason == null)
                    {
                        Notify.Custom($"~g~<C>{GetSafePlayerName(playerName)}</C>~s~ joined the server.");
                    }
                    else
                    {
                        Notify.Custom($"~r~<C>{GetSafePlayerName(playerName)}</C>~s~ left the server. ~c~({GetSafePlayerName(dropReason)})");
                    }
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
            // Death notifications
            if (MainMenu.MiscSettingsMenu.DeathNotifications)
            {
                var pl = Players;
                var tmpiterator = 0;
                foreach (var p in pl)
                {
                    tmpiterator++;
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
                                        var found = false;
                                        foreach (var playerKiller in pl)
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
                                        var found = false;
                                        foreach (var playerKiller in pl)
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
            }
            await Task.FromResult(0);
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
            if (MainMenu.VoiceChatSettingsMenu.EnableVoicechat && IsAllowed(Permission.VCEnable))
            {
                NetworkSetVoiceActive(true);
                NetworkSetTalkerProximity(MainMenu.VoiceChatSettingsMenu.currentProximity);
                var channel = MainMenu.VoiceChatSettingsMenu.channels.IndexOf(MainMenu.VoiceChatSettingsMenu.currentChannel);
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
                    var pl = Players;
                    var i = 1;
                    var currentlyTalking = false;
                    foreach (var p in pl)
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
        #endregion

        #region Update Time Options Menu (current time display)
        /// <summary>
        /// Update the current time display in the time options menu.
        /// </summary>
        /// <returns></returns>
        private async Task TimeOptions()
        {
            if (MainMenu.TimeOptionsMenu.freezeTimeToggle != null && MainMenu.TimeOptionsMenu.GetMenu().Visible && IsAllowed(Permission.TOFreezeTime))
            {
                // Update the current time displayed in the Time Options menu (only when the menu is actually visible).
                var hours = GetClockHours();
                var minutes = GetClockMinutes();
                var hoursString = hours < 10 ? "0" + hours.ToString() : hours.ToString();
                var minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
                MainMenu.TimeOptionsMenu.freezeTimeToggle.Label = $"(Current Time {hoursString}:{minutesString})";
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
            // If no reload is enabled.
            if (MainMenu.WeaponOptionsMenu.NoReload && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Minigun && IsAllowed(Permission.WPNoReload))
            {
                // Disable reloading.
                SetAmmoInClip(Game.PlayerPed.Handle, (uint)Game.PlayerPed.Weapons.Current.Hash, 5);
            }

            // Enable/disable infinite ammo.
            if (IsAllowed(Permission.WPUnlimitedAmmo) && Game.PlayerPed.Weapons.Current != null && Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
            {
                Game.PlayerPed.Weapons.Current.InfiniteAmmo = MainMenu.WeaponOptionsMenu.UnlimitedAmmo;
            }

            if (MainMenu.WeaponOptionsMenu.AutoEquipChute)
            {
                if ((IsPedInAnyHeli(Game.PlayerPed.Handle) || IsPedInAnyPlane(Game.PlayerPed.Handle)) && !HasPedGotWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, false))
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)WeaponHash.Parachute, 1, false, true);
                    SetPlayerHasReserveParachute(Game.Player.Handle);
                    SetPlayerCanLeaveParachuteSmokeTrail(Game.PlayerPed.Handle, true);
                }
            }

            if (MainMenu.WeaponOptionsMenu.UnlimitedParachutes)
            {
                if (!HasPedGotWeapon(Game.PlayerPed.Handle, (uint)GetHashKey("gadget_parachute"), false))
                {
                    GiveWeaponToPed(Game.PlayerPed.Handle, (uint)GetHashKey("gadget_parachute"), 0, false, false);
                }

                if (!GetPlayerHasReserveParachute(Game.Player.Handle))
                {
                    SetPlayerHasReserveParachute(Game.Player.Handle);
                }
            }
            await Task.FromResult(0);
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
            if (MainMenu.PermissionsSetupComplete && MainMenu.OnlinePlayersMenu != null && IsAllowed(Permission.OPMenu) && IsAllowed(Permission.OPSpectate))
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

        internal static bool reverseCamera = false;
        private static Camera camera;
        internal static float CameraFov { get; set; } = 45;
        internal static int CurrentCam { get; set; }
        internal static List<KeyValuePair<Vector3, Vector3>> CameraOffsets { get; } = new List<KeyValuePair<Vector3, Vector3>>()
        {
            // Full body
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 2.8f, 0.3f), new Vector3(0f, 0f, 0f)),

            // Head level
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 0.9f, 0.65f), new Vector3(0f, 0f, 0.6f)),

            // Upper Body
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 1.4f, 0.5f), new Vector3(0f, 0f, 0.3f)),

            // Lower Body
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 1.6f, -0.3f), new Vector3(0f, 0f, -0.45f)),

            // Shoes
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 0.98f, -0.7f), new Vector3(0f, 0f, -0.90f)),

            // Lower Arms
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 0.98f, 0.1f), new Vector3(0f, 0f, 0f)),

            // Full arms
            new KeyValuePair<Vector3, Vector3>(new Vector3(0f, 1.3f, 0.35f), new Vector3(0f, 0f, 0.15f)),
        };

        private async Task UpdateCamera(Camera oldCamera, Vector3 pos, Vector3 pointAt)
        {
            var newCam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
            var newCamera = new Camera(newCam)
            {
                Position = pos,
                FieldOfView = CameraFov
            };
            newCamera.PointAt(pointAt);
            oldCamera.InterpTo(newCamera, 1000, true, true);
            while (oldCamera.IsInterpolating || !newCamera.IsActive)
            {
                SetEntityCollision(Game.PlayerPed.Handle, false, false);
                //Game.PlayerPed.IsInvincible = true;
                Game.PlayerPed.IsPositionFrozen = true;
                await Delay(0);
            }
            await Delay(50);
            oldCamera.Delete();
            CurrentCam = newCam;
            camera = newCamera;
        }

        private bool IsMpCharEditorOpen()
        {
            if (MainMenu.MpPedCustomizationMenu != null)
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
            return false;
        }

        /// <summary>
        /// Manages the camera for the mp character customization menu
        /// </summary>
        /// <returns></returns>
        private async Task ManageCamera()
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                if (MainMenu.MpPedCustomizationMenu.editPedBtn != null && MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled = false;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.LeftIcon = MenuItem.Icon.LOCK;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Description += " ~r~You need to get out of your vehicle before you can use this.";
                }
                if (MainMenu.MpPedCustomizationMenu.createMaleBtn != null && MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled = false;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.LeftIcon = MenuItem.Icon.LOCK;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Description += " ~r~You need to get out of your vehicle before you can use this.";
                }
                if (MainMenu.MpPedCustomizationMenu.createFemaleBtn != null && MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled = false;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.LeftIcon = MenuItem.Icon.LOCK;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Description += " ~r~You need to get out of your vehicle before you can use this.";
                }
            }
            else
            {
                if (MainMenu.MpPedCustomizationMenu.editPedBtn != null && !MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Enabled = true;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.LeftIcon = MenuItem.Icon.NONE;
                    MainMenu.MpPedCustomizationMenu.editPedBtn.Description = MainMenu.MpPedCustomizationMenu.editPedBtn.Description.Replace(" ~r~You need to get out of your vehicle before you can use this.", "");
                }
                if (MainMenu.MpPedCustomizationMenu.createMaleBtn != null && !MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Enabled = true;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.LeftIcon = MenuItem.Icon.NONE;
                    MainMenu.MpPedCustomizationMenu.createMaleBtn.Description = MainMenu.MpPedCustomizationMenu.createMaleBtn.Description.Replace(" ~r~You need to get out of your vehicle before you can use this.", "");
                }
                if (MainMenu.MpPedCustomizationMenu.createFemaleBtn != null && !MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled)
                {
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Enabled = true;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.LeftIcon = MenuItem.Icon.NONE;
                    MainMenu.MpPedCustomizationMenu.createFemaleBtn.Description = MainMenu.MpPedCustomizationMenu.createFemaleBtn.Description.Replace(" ~r~You need to get out of your vehicle before you can use this.", "");
                }
            }

            if (IsMpCharEditorOpen())
            {
                if (!HasAnimDictLoaded("anim@random@shop_clothes@watches"))
                {
                    RequestAnimDict("anim@random@shop_clothes@watches");
                }
                while (!HasAnimDictLoaded("anim@random@shop_clothes@watches"))
                {
                    await Delay(0);
                }

                while (IsMpCharEditorOpen())
                {
                    await Delay(0);

                    var index = GetCameraIndex(MenuController.GetCurrentMenu());
                    if (MenuController.GetCurrentMenu() == MainMenu.MpPedCustomizationMenu.propsMenu && MenuController.GetCurrentMenu().CurrentIndex == 3 && !reverseCamera)
                    {
                        TaskPlayAnim(Game.PlayerPed.Handle, "anim@random@shop_clothes@watches", "BASE", 8f, -8f, -1, 1, 0, false, false, false);
                    }
                    else
                    {
                        Game.PlayerPed.Task.ClearAll();
                    }

                    var xOffset = 0f;
                    var yOffset = 0f;

                    if ((Game.IsControlPressed(0, Control.ParachuteBrakeLeft) || Game.IsControlPressed(0, Control.ParachuteBrakeRight)) && !(Game.IsControlPressed(0, Control.ParachuteBrakeLeft) && Game.IsControlPressed(0, Control.ParachuteBrakeRight)))
                    {
                        switch (index)
                        {
                            case 0:
                                xOffset = 2.2f;
                                yOffset = -1f;
                                break;
                            case 1:
                                xOffset = 0.7f;
                                yOffset = -0.45f;
                                break;
                            case 2:
                                xOffset = 1.35f;
                                yOffset = -0.4f;
                                break;
                            case 3:
                                xOffset = 1.0f;
                                yOffset = -0.4f;
                                break;
                            case 4:
                                xOffset = 0.9f;
                                yOffset = -0.4f;
                                break;
                            case 5:
                                xOffset = 0.8f;
                                yOffset = -0.7f;
                                break;
                            case 6:
                                xOffset = 1.5f;
                                yOffset = -1.0f;
                                break;
                            default:
                                xOffset = 0f;
                                yOffset = 0.2f;
                                break;
                        }
                        if (Game.IsControlPressed(0, Control.ParachuteBrakeRight))
                        {
                            xOffset *= -1f;
                        }

                    }

                    Vector3 pos;
                    if (reverseCamera)
                    {
                        pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, (CameraOffsets[index].Key.X + xOffset) * -1f, (CameraOffsets[index].Key.Y + yOffset) * -1f, CameraOffsets[index].Key.Z);
                    }
                    else
                    {
                        pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, CameraOffsets[index].Key.X + xOffset, CameraOffsets[index].Key.Y + yOffset, CameraOffsets[index].Key.Z);
                    }

                    var pointAt = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, CameraOffsets[index].Value.X, CameraOffsets[index].Value.Y, CameraOffsets[index].Value.Z);

                    if (Game.IsControlPressed(0, Control.MoveLeftOnly))
                    {
                        Game.PlayerPed.Task.LookAt(GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 1.2f, .5f, .7f), 1100);
                    }
                    else if (Game.IsControlPressed(0, Control.MoveRightOnly))
                    {
                        Game.PlayerPed.Task.LookAt(GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, -1.2f, .5f, .7f), 1100);
                    }
                    else
                    {
                        Game.PlayerPed.Task.LookAt(GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, .5f, .7f), 1100);
                    }

                    if (Game.IsControlJustReleased(0, Control.Jump))
                    {
                        var Pos = Game.PlayerPed.Position;
                        SetEntityCollision(Game.PlayerPed.Handle, true, true);
                        FreezeEntityPosition(Game.PlayerPed.Handle, false);
                        TaskGoStraightToCoord(Game.PlayerPed.Handle, Pos.X, Pos.Y, Pos.Z, 8f, 1600, Game.PlayerPed.Heading + 180f, 0.1f);
                        var timer = GetGameTimer();
                        while (true)
                        {
                            await Delay(0);
                            //DisplayRadar(false);
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
                        reverseCamera = !reverseCamera;
                    }

                    SetEntityCollision(Game.PlayerPed.Handle, false, false);
                    //Game.PlayerPed.IsInvincible = true;
                    Game.PlayerPed.IsPositionFrozen = true;

                    if (!DoesCamExist(CurrentCam))
                    {
                        CurrentCam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
                        camera = new Camera(CurrentCam)
                        {
                            Position = pos,
                            FieldOfView = CameraFov
                        };
                        camera.PointAt(pointAt);
                        RenderScriptCams(true, false, 0, false, false);
                        camera.IsActive = true;
                    }
                    else
                    {
                        if (camera.Position != pos)
                        {
                            await UpdateCamera(camera, pos, pointAt);
                        }
                    }
                }

                SetEntityCollision(Game.PlayerPed.Handle, true, true);

                Game.PlayerPed.IsPositionFrozen = false;

                DisplayHud(true);
                DisplayRadar(true);

                if (HasAnimDictLoaded("anim@random@shop_clothes@watches"))
                {
                    RemoveAnimDict("anim@random@shop_clothes@watches");
                }

                reverseCamera = false;
            }
            else
            {
                if (camera != null)
                {
                    ClearCamera();
                    camera = null;
                }
            }
        }

        private int GetCameraIndex(Menu menu)
        {
            if (menu != null)
            {
                if (menu == MainMenu.MpPedCustomizationMenu.inheritanceMenu)
                {
                    return 1;
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.clothesMenu)
                {
                    return menu.CurrentIndex switch
                    {
                        // masks
                        0 => 1,
                        // upper body
                        1 => 2,
                        // lower body
                        2 => 3,
                        // bags & parachutes
                        3 => 2,
                        // shoes
                        4 => 4,
                        // scarfs & chains
                        5 => 2,
                        // shirt & accessory
                        6 => 2,
                        // body armor & accessory
                        7 => 2,
                        // badges & logos
                        8 => 0,
                        // shirt overlay & jackets
                        9 => 2,
                        _ => 0,
                    };
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.propsMenu)
                {
                    return menu.CurrentIndex switch
                    {
                        // 0 = hats & helmets
                        // 1 = glasses
                        // 2 = misc props
                        0 or 1 or 2 => 1,

                        // 3 = watches
                        3 => reverseCamera ? 5 : 6,

                        // 4 = bracelets
                        4 => 5,

                        _ => 0,
                    };
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.appearanceMenu)
                {
                    return menu.CurrentIndex switch
                    {
                        // 0 = hair style
                        // 1 = hair color
                        // 2 = hair highlight color
                        // 3 = blemishes
                        // 4 = blemishes opacity
                        // 5 = beard style
                        // 6 = beard opacity
                        // 7 = beard color
                        // 8 = eyebrows style
                        // 9 = eyebrows opacity
                        // 10 = eyebrows color
                        // 11 = ageing style
                        // 12 = ageing opacity
                        // 13 = makeup style
                        // 14 = makeup opacity
                        // 15 = makeup color
                        // 16 = blush style
                        // 17 = blush opacity
                        // 18 = blush color
                        // 19 = complexion style
                        // 20 = complexion opacity
                        // 21 = sun damage style
                        // 22 = sun damage opacity
                        // 23 = lipstick style
                        // 24 = lipstick opacity
                        // 25 = lipstick color
                        // 26 = moles and freckles style
                        // 27 = moles and freckles opacity
                        0 or 1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 27 => 1,

                        // 28 = chest hair style
                        // 29 = chest hair opacity
                        // 30 = chest hair color
                        // 31 = body blemishes style
                        // 32 = body blemishes opacity
                        28 or 29 or 30 or 31 or 32 => 2,

                        // 33 = eye colors
                        33 => 1,

                        _ => 0,
                    };
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.tattoosMenu)
                {
                    return menu.CurrentIndex switch
                    {
                        // 0 = head
                        0 => 1,

                        // 1 = torso
                        1 => 2,

                        // 2 = left arm
                        // 3 = right arm
                        2 or 3 => 6,

                        // 4 = left leg 
                        // 5 = right leg
                        4 or 5 => 3,

                        // 6 = badges
                        6 => 2,

                        _ => 0,
                    };
                }
                else if (menu == MainMenu.MpPedCustomizationMenu.faceShapeMenu)
                {
                    var item = menu.GetCurrentMenuItem();
                    if (item != null)
                    {
                        if (item.GetType() == typeof(MenuSliderItem))
                        {
                            return 1;
                        }
                    }
                    return 0;
                }
            }
            return 0;
        }

        internal static void ClearCamera()
        {
            camera.IsActive = false;
            RenderScriptCams(false, false, 0, false, false);
            DestroyCam(CurrentCam, false);
            CurrentCam = -1;
            camera.Delete();
        }

        /// <summary>
        /// Disables movement while the mp character creator is open.
        /// </summary>
        /// <returns></returns>
        private async Task DisableMovement()
        {
            if (IsMpCharEditorOpen())
            {
                Game.DisableControlThisFrame(0, Control.MoveDown);
                Game.DisableControlThisFrame(0, Control.MoveDownOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeft);
                Game.DisableControlThisFrame(0, Control.MoveLeftOnly);
                Game.DisableControlThisFrame(0, Control.MoveLeftRight);
                Game.DisableControlThisFrame(0, Control.MoveRight);
                Game.DisableControlThisFrame(0, Control.MoveRightOnly);
                Game.DisableControlThisFrame(0, Control.MoveUp);
                Game.DisableControlThisFrame(0, Control.MoveUpDown);
                Game.DisableControlThisFrame(0, Control.MoveUpOnly);
                Game.DisableControlThisFrame(0, Control.NextCamera);
                Game.DisableControlThisFrame(0, Control.LookBehind);
                Game.DisableControlThisFrame(0, Control.LookDown);
                Game.DisableControlThisFrame(0, Control.LookDownOnly);
                Game.DisableControlThisFrame(0, Control.LookLeft);
                Game.DisableControlThisFrame(0, Control.LookLeftOnly);
                Game.DisableControlThisFrame(0, Control.LookLeftRight);
                Game.DisableControlThisFrame(0, Control.LookRight);
                Game.DisableControlThisFrame(0, Control.LookRightOnly);
                Game.DisableControlThisFrame(0, Control.LookUp);
                Game.DisableControlThisFrame(0, Control.LookUpDown);
                Game.DisableControlThisFrame(0, Control.LookUpOnly);
                Game.DisableControlThisFrame(0, Control.Aim);
                Game.DisableControlThisFrame(0, Control.AccurateAim);
                Game.DisableControlThisFrame(0, Control.Cover);
                Game.DisableControlThisFrame(0, Control.Duck);
                Game.DisableControlThisFrame(0, Control.Jump);
                Game.DisableControlThisFrame(0, Control.SelectNextWeapon);
                Game.DisableControlThisFrame(0, Control.PrevWeapon);
                Game.DisableControlThisFrame(0, Control.WeaponSpecial);
                Game.DisableControlThisFrame(0, Control.WeaponSpecial2);
                Game.DisableControlThisFrame(0, Control.WeaponWheelLeftRight);
                Game.DisableControlThisFrame(0, Control.WeaponWheelNext);
                Game.DisableControlThisFrame(0, Control.WeaponWheelPrev);
                Game.DisableControlThisFrame(0, Control.WeaponWheelUpDown);
                Game.DisableControlThisFrame(0, Control.VehicleExit);
                Game.DisableControlThisFrame(0, Control.Enter);
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion

        #region Restore player skin & weapons after respawning.
        /// <summary>
        /// Restores player appearance after dying.
        /// </summary>
        /// <returns></returns>
        private async Task RestorePlayerAfterBeingDead()
        {
            if (MainMenu.MiscSettingsMenu != null && Game.PlayerPed.IsDead)
            {
                var restoreDefault = false;
                if (MainMenu.MiscSettingsMenu.MiscRespawnDefaultCharacter)
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
                        await SavePed("vMenu_tmp_saved_ped");
                    }
                }

                if ((MainMenu.MiscSettingsMenu.RestorePlayerWeapons && IsAllowed(Permission.MSRestoreWeapons)) || (MainMenu.WeaponLoadoutsMenu != null && MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn && IsAllowed(Permission.WLEquipOnRespawn)))
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

                while (Game.PlayerPed.IsDead || IsScreenFadedOut() || IsScreenFadingOut())
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

                if ((MainMenu.MiscSettingsMenu != null && MainMenu.MiscSettingsMenu.RestorePlayerWeapons && IsAllowed(Permission.MSRestoreWeapons)) || (MainMenu.WeaponLoadoutsMenu != null && MainMenu.WeaponLoadoutsMenu.WeaponLoadoutsSetLoadoutOnRespawn && IsAllowed(Permission.WLEquipOnRespawn)))
                {
                    await SpawnWeaponLoadoutAsync("vmenu_temp_weapons_loadout_before_respawn", true, false, false);
                    Log("weapons restored, deleting kvp");
                    DeleteResourceKvp("vmenu_temp_weapons_loadout_before_respawn");
                }
            }
        }
        #endregion

        #region Player clothing animations controller.
        private async Task PlayerClothingAnimationsController()
        {
            if (!DecorIsRegisteredAsType(clothingAnimationDecor, 3))
            {
                try
                {
                    DecorRegister(clothingAnimationDecor, 3);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(@"[CRITICAL] A critical bug in one of your scripts was detected. vMenu is unable to set or register a decorator's value because another resource has already registered 1.5k or more decorators. vMenu will NOT work as long as this bug in your other scripts is unsolved. Please fix your other scripts. This is *NOT* caused by or fixable by vMenu!!!");
                    Debug.WriteLine($"Error Location: {e.StackTrace}\nError info: {e.Message}");
                    await Delay(1000);
                }
                while (!DecorIsRegisteredAsType(clothingAnimationDecor, 3))
                {
                    await Delay(0);
                }
            }
            else
            {
                try
                {
                    DecorSetInt(Game.PlayerPed.Handle, clothingAnimationDecor, PlayerAppearance.ClothingAnimationType);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(@"[CRITICAL] A critical bug in one of your scripts was detected. vMenu is unable to set or register a decorator's value because another resource has already registered 1.5k or more decorators. vMenu will NOT work as long as this bug in your other scripts is unsolved. Please fix your other scripts. This is *NOT* caused by or fixable by vMenu!!!");
                    Debug.WriteLine($"Error Location: {e.StackTrace}\nError info: {e.Message}");
                    await Delay(1000);
                }
                foreach (var player in Players)
                {
                    var p = player.Character;
                    if (p != null && p.Exists() && !p.IsDead)
                    {
                        if (DecorExistOn(p.Handle, clothingAnimationDecor))
                        {
                            var decorVal = DecorGetInt(p.Handle, clothingAnimationDecor);
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
                                var result = 0f;
                                if (clothingAnimationReverse)
                                {
                                    if (clothingOpacity is >= 0f and <= 0.5f) // || (clothingOpacity >= 0.5f && clothingOpacity <= 0.75f))
                                    {
                                        result = 1f;
                                    }
                                }
                                else
                                {
                                    if (clothingOpacity is >= 0.5f and <= 1.0f) //|| (clothingOpacity >= 0.75f && clothingOpacity <= 1.0f))
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
                var timer = GetGameTimer();
                while (GetGameTimer() - timer < 25)
                {
                    await Delay(0);
                }
            }
            try
            {
                DecorSetInt(Game.PlayerPed.Handle, clothingAnimationDecor, PlayerAppearance.ClothingAnimationType);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"[CRITICAL] A critical bug in one of your scripts was detected. vMenu is unable to set or register a decorator's value because another resource has already registered 1.5k or more decorators. vMenu will NOT work as long as this bug in your other scripts is unsolved. Please fix your other scripts. This is *NOT* caused by or fixable by vMenu!!!");
                Debug.WriteLine($"Error Location: {e.StackTrace}\nError info: {e.Message}");
                await Delay(1000);
            }
        }
        #endregion

        #region player blips tasks
        private async Task PlayerBlipsControl()
        {
            if (DecorIsRegisteredAsType("vmenu_player_blip_sprite_id", 3))
            {
                var sprite = 1;
                if (IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
                {
                    var veh = GetVehicle();
                    if (veh != null && veh.Exists())
                    {
                        sprite = BlipInfo.GetBlipSpriteForVehicle(veh.Handle);
                    }
                }
                try
                {
                    DecorSetInt(Game.PlayerPed.Handle, "vmenu_player_blip_sprite_id", sprite);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(@"[CRITICAL] A critical bug in one of your scripts was detected. vMenu is unable to set or register a decorator's value because another resource has already registered 1.5k or more decorators. vMenu will NOT work as long as this bug in your other scripts is unsolved. Please fix your other scripts. This is *NOT* caused by or fixable by vMenu!!!");
                    Debug.WriteLine($"Error Location: {e.StackTrace}\nError info: {e.Message}");
                    await Delay(1000);
                }

                if (MainMenu.MiscSettingsMenu != null)
                {
                    var enabled = MainMenu.MiscSettingsMenu.ShowPlayerBlips;

                    foreach (var p in MainMenu.PlayersList)
                    {
                        // continue only if this player is valid.
                        if (p != null && NetworkIsPlayerActive(p.Handle) && p.Character != null && p.Character.Exists())
                        {
                            // if blips are enabled and the player has permisisons to use them.
                            if (enabled)
                            {
                                if (!p.IsLocal)
                                {
                                    var ped = p.Character.Handle;
                                    var blip = GetBlipFromEntity(ped);

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
                                            var decorSprite = DecorGetInt(p.Character.Handle, "vmenu_player_blip_sprite_id");
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

                                        // ShowHeightOnBlip(blip, false); // See if we need this or not

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
                                if (!(p.Character.AttachedBlip == null || !p.Character.AttachedBlip.Exists()) && MainMenu.OnlinePlayersMenu != null && !MainMenu.OnlinePlayersMenu.PlayersWaypointList.Contains(p.ServerId))
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
                try
                {
                    DecorRegister("vmenu_player_blip_sprite_id", 3);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(@"[CRITICAL] A critical bug in one of your scripts was detected. vMenu is unable to set or register a decorator's value because another resource has already registered 1.5k or more decorators. vMenu will NOT work as long as this bug in your other scripts is unsolved. Please fix your other scripts. This is *NOT* caused by or fixable by vMenu!!!");
                    Debug.WriteLine($"Error Location: {e.StackTrace}\nError info: {e.Message}");
                    await Delay(1000);
                }
                while (!DecorIsRegisteredAsType("vmenu_player_blip_sprite_id", 3))
                {
                    await Delay(0);
                }
            }
        }

        #endregion

        #region player overhead names
        private readonly Dictionary<Player, int> gamerTags = new();

        private readonly float playerNamesDistance = GetSettingsFloat(Setting.vmenu_player_names_distance) > 10f ? GetSettingsFloat(Setting.vmenu_player_names_distance) : 500f;


        /// <summary>
        /// Manages overhead player names.
        /// </summary>
        /// <returns></returns>
        private async Task PlayerOverheadNamesControl()
        {
            await Delay(500);

            if (MainMenu.MiscSettingsMenu != null)
            {
                var enabled = MainMenu.MiscSettingsMenu.MiscShowOverheadNames;
                if (!enabled)
                {
                    foreach (var gamerTag in gamerTags)
                    {
                        RemoveMpGamerTag(gamerTag.Value);
                    }
                    gamerTags.Clear();
                }
                else
                {
                    foreach (var p in Players)
                    {
                        if (p != Game.Player)
                        {
                            var dist = p.Character.Position.DistanceToSquared(Game.PlayerPed.Position);
                            var closeEnough = dist < playerNamesDistance;
                            if (gamerTags.ContainsKey(p))
                            {
                                if (!closeEnough)
                                {
                                    RemoveMpGamerTag(gamerTags[p]);
                                    gamerTags.Remove(p);
                                }
                                else
                                {
                                    gamerTags[p] = CreateMpGamerTag(p.Character.Handle, p.Name + $" [{p.ServerId}]", false, false, "", 0);
                                }
                            }
                            else if (closeEnough)
                            {
                                gamerTags.Add(p, CreateMpGamerTag(p.Character.Handle, p.Name + $" [{p.ServerId}]", false, false, "", 0));
                            }
                            if (closeEnough && gamerTags.ContainsKey(p))
                            {
                                SetMpGamerTagVisibility(gamerTags[p], 2, true); // healthArmor
                                if (p.WantedLevel > 0)
                                {
                                    SetMpGamerTagVisibility(gamerTags[p], 7, true); // wantedStars
                                    SetMpGamerTagWantedLevel(gamerTags[p], GetPlayerWantedLevel(p.Handle));
                                }
                                else
                                {
                                    SetMpGamerTagVisibility(gamerTags[p], 7, false); // wantedStars
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Online Player Options Tasks
        /// <summary>
        /// Manages online players tasks.
        /// </summary>
        /// <returns></returns>
        private async Task OnlinePlayersTasks()
        {
            await Delay(500);
            if (MainMenu.OnlinePlayersMenu.PlayersWaypointList.Count > 0)
            {
                foreach (var serverId in MainMenu.OnlinePlayersMenu.PlayersWaypointList)
                {
                    var player = MainMenu.PlayersList.FirstOrDefault(a => a.ServerId == serverId);

                    if (player == null)
                    {
                        waypointPlayerIdsToRemove.Add(serverId);
                    }
                    else if (player.Character != null)
                    {
                        var playerId = player.Handle;
                        var pos1 = GetEntityCoords(GetPlayerPed(playerId), true);
                        var pos2 = Game.PlayerPed.Position;
                        if (Vdist2(pos1.X, pos1.Y, pos1.Z, pos2.X, pos2.Y, pos2.Z) < 20f)
                        {
                            var blip = GetBlipFromEntity(GetPlayerPed(playerId));
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
                    foreach (var id in waypointPlayerIdsToRemove)
                    {
                        if (MainMenu.OnlinePlayersMenu.PlayerCoordWaypoints.TryGetValue(id, out var blip))
                        {
                            if (DoesBlipExist(blip))
                            {
                                SetBlipRoute(blip, false);
                                RemoveBlip(ref blip);
                            }
                        }

                        MainMenu.OnlinePlayersMenu.PlayersWaypointList.Remove(id);
                    }
                    await Delay(10);
                }
                waypointPlayerIdsToRemove.Clear();
            }
        }
        #endregion

        #region Flares and plane bombs controler (UNUSED)
        /*
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
        */
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
                if (EventManager.IsSnowEnabled && IsAllowed(Permission.WPSnowball))
                {
                    if (Game.IsControlJustReleased(0, Control.Detonate))
                    {
                        if (!(Game.PlayerPed.IsInVehicle() || Game.PlayerPed.IsDead || !Fading.IsFadedIn || IsPlayerSwitchInProgress() || Game.IsPaused
                            || GetInteriorFromEntity(Game.PlayerPed.Handle) != 0 || !Game.PlayerPed.IsOnFoot || Game.PlayerPed.IsInParachuteFreeFall ||
                            Game.PlayerPed.IsFalling || Game.PlayerPed.IsBeingStunned || Game.PlayerPed.IsWalking || Game.PlayerPed.IsRunning ||
                            Game.PlayerPed.IsSprinting || Game.PlayerPed.IsSwimming || Game.PlayerPed.IsSwimmingUnderWater || (Game.PlayerPed.IsDiving && GetSelectedPedWeapon(Game.PlayerPed.Handle) == snowball_hash) || GetSelectedPedWeapon(Game.PlayerPed.Handle) == GetHashKey("unarmed")))
                        {
                            await PickupSnowballOnce();
                        }
                    }
                }

                // helmet visor
                if (Game.IsControlPressed(0, Control.SwitchVisor))
                {
                    var timer = GetGameTimer();
                    while (!(MenuController.IsAnyMenuOpen() || MainMenu.DontOpenMenus || !Fading.IsFadedIn || Game.IsPaused || IsPlayerSwitchInProgress() || Game.PlayerPed.IsDead) && Game.IsControlPressed(0, Control.SwitchVisor))
                    {
                        await Delay(0);
                        var veh = GetVehicle();
                        var inVeh = veh != null && (veh.Model.IsBike || veh.Model.IsBicycle || veh.Model.IsQuadbike);
                        if (GetGameTimer() - timer > 380 && inVeh)
                        {
                            Game.DisableControlThisFrame(2, Control.VehicleHeadlight);
                        }
                        if (GetGameTimer() - timer > 400)
                        {
                            var t = SwitchHelmetOnce();
                            while (!t.IsCompleted && !t.IsCanceled && !t.IsFaulted)
                            {
                                if (inVeh)
                                {
                                    Game.DisableControlThisFrame(2, Control.VehicleHeadlight);
                                }
                                await Delay(0);
                            }
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
        /// <summary>
        /// Help message timer and stuff.
        /// </summary>
        /// <returns></returns>
        private async Task SnowballPickupHelpMessageTask()
        {
            if (EventManager.IsSnowEnabled)
            {
                void ShowSnowballInfoMessage()
                {
                    var maxAmmo = 10;
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
        /// <summary>
        /// Draws entity outlines if enabled (per entity type).
        /// </summary>
        /// <returns></returns>
        private async Task ModelDrawDimensions()
        {
            if (MainMenu.PermissionsSetupComplete && MainMenu.MiscSettingsMenu != null)
            {
                // Vehicles
                if (MainMenu.MiscSettingsMenu.ShowVehicleModelDimensions)
                {
                    foreach (var v in vehicles)
                    {
                        if (stopVehiclesLoop)
                        {
                            break;
                        }

                        DrawEntityBoundingBox(v, 250, 150, 0, 100);

                        if (MainMenu.MiscSettingsMenu.ShowEntityHandles && v.IsOnScreen)
                        {
                            SetDrawOrigin(v.Position.X, v.Position.Y, v.Position.Z, 0);
                            DrawTextOnScreen($"Veh {v.Handle}", 0f, 0f, 0.3f, Alignment.Center, 0);
                            ClearDrawOrigin();
                        }
                        if (MainMenu.MiscSettingsMenu.ShowEntityModels && v.IsOnScreen)
                        {
                            SetDrawOrigin(v.Position.X, v.Position.Y, v.Position.Z - 0.3f, 0);
                            var model = GetEntityModel(v.Handle);

                            var hashes = $"{model} / {(uint)model} / 0x{model:X8}";

                            DrawTextOnScreen($"Hash {hashes}", 0f, 0f, 0.3f, Alignment.Center, 0);
                            ClearDrawOrigin();
                        }
                        if (MainMenu.MiscSettingsMenu.ShowEntityNetOwners && v.IsOnScreen)
                        {
                            var netOwnerLocalId = NetworkGetEntityOwner(v.Handle);

                            if (netOwnerLocalId != 0)
                            {
                                var playerServerId = GetPlayerServerId(netOwnerLocalId);
                                var playerName = GetPlayerName(netOwnerLocalId);
                                SetDrawOrigin(v.Position.X, v.Position.Y, v.Position.Z + 0.3f, 0);
                                DrawTextOnScreen($"Owner ID {playerServerId} ({playerName})", 0f, 0f, 0.3f, Alignment.Center, 0);
                                ClearDrawOrigin();
                            }
                        }
                    }
                }

                // Props
                if (MainMenu.MiscSettingsMenu.ShowPropModelDimensions)
                {
                    foreach (var p in props)
                    {
                        if (stopPropsLoop)
                        {
                            break;
                        }

                        DrawEntityBoundingBox(p, 255, 0, 0, 100);

                        if (MainMenu.MiscSettingsMenu.ShowEntityHandles && p.IsOnScreen)
                        {
                            SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z, 0);
                            DrawTextOnScreen($"Prop {p.Handle}", 0f, 0f, 0.3f, Alignment.Center, 0);
                            ClearDrawOrigin();
                        }

                        if (MainMenu.MiscSettingsMenu.ShowEntityModels && p.IsOnScreen)
                        {
                            SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z - 0.3f, 0);
                            var model = GetEntityModel(p.Handle);

                            var hashes = $"{model} / {(uint)model} / 0x{model:X8}";

                            DrawTextOnScreen($"Hash {hashes}", 0f, 0f, 0.3f, Alignment.Center, 0);
                            ClearDrawOrigin();
                        }

                        if (MainMenu.MiscSettingsMenu.ShowEntityNetOwners && p.IsOnScreen)
                        {
                            var netOwnerLocalId = NetworkGetEntityOwner(p.Handle);

                            if (netOwnerLocalId != 0)
                            {
                                var playerServerId = GetPlayerServerId(netOwnerLocalId);
                                var playerName = GetPlayerName(netOwnerLocalId);
                                SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z + 0.3f, 0);
                                DrawTextOnScreen($"Owner ID {playerServerId} ({playerName})", 0f, 0f, 0.3f, Alignment.Center, 0);
                                ClearDrawOrigin();
                            }
                        }
                    }
                }

                // Peds
                if (MainMenu.MiscSettingsMenu.ShowPedModelDimensions)
                {
                    foreach (var p in peds)
                    {
                        if (stopPedsLoop)
                        {
                            break;
                        }

                        DrawEntityBoundingBox(p, 50, 255, 50, 100);

                        if (MainMenu.MiscSettingsMenu.ShowEntityHandles && p.IsOnScreen)
                        {
                            SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z, 0);
                            DrawTextOnScreen($"Ped {p.Handle}", 0f, 0f, 0.3f, Alignment.Center, 0);
                            ClearDrawOrigin();
                        }

                        if (MainMenu.MiscSettingsMenu.ShowEntityModels && p.IsOnScreen)
                        {
                            SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z - 0.3f, 0);
                            var model = GetEntityModel(p.Handle);

                            var hashes = $"{model} / {(uint)model} / 0x{model:X8}";

                            DrawTextOnScreen($"Hash {hashes}", 0f, 0f, 0.3f, Alignment.Center, 0);
                            ClearDrawOrigin();
                        }

                        if (MainMenu.MiscSettingsMenu.ShowEntityNetOwners && p.IsOnScreen)
                        {
                            var netOwnerLocalId = NetworkGetEntityOwner(p.Handle);

                            if (netOwnerLocalId != 0)
                            {
                                var playerServerId = GetPlayerServerId(netOwnerLocalId);
                                var playerName = GetPlayerName(netOwnerLocalId);
                                SetDrawOrigin(p.Position.X, p.Position.Y, p.Position.Z + 0.3f, 0);
                                DrawTextOnScreen($"Owner ID {playerServerId} ({playerName})", 0f, 0f, 0.3f, Alignment.Center, 0);
                                ClearDrawOrigin();
                            }
                        }
                    }
                }
            }
            else
            {
                await Task.FromResult(0);
            }
        }
        #endregion

        #region animal ped camera change blocker
        /// <summary>
        /// Prevents players from going into first person when they're currently using an animal as their player ped.
        /// This is to prevent them crashing their game or falling out of the sky as ~~birds~~ bricks.
        /// </summary>
        /// <returns></returns>
        private async Task AnimalPedCameraChangeBlocker()
        {
            var model = (uint)GetEntityModel(Game.PlayerPed.Handle);
            if (AnimalHashes.Contains(model))
            {
                while (model == (uint)GetEntityModel(Game.PlayerPed.Handle))
                {
                    DisableFirstPersonCamThisFrame();
                    await Delay(0);
                }
            }
        }
        #endregion

        #region Slow misc tick
        internal static float entityRange = 2000f;
        /// <summary>
        /// Slow functions for the model dimensions outline entities lists.
        /// </summary>
        /// <returns></returns>
        private async Task SlowMiscTick()
        {
            const int delay = 50;
            if (MainMenu.PermissionsSetupComplete && MainMenu.MiscSettingsMenu != null)
            {
                var pp = Game.PlayerPed.Position;
                if (MainMenu.MiscSettingsMenu.ShowPropModelDimensions)
                {
                    stopPropsLoop = true;
                    props = World.GetAllProps().Where(e => e.IsOnScreen && e.Position.DistanceToSquared(pp) < entityRange).ToList();
                    stopPropsLoop = false;

                    await Delay(delay);
                }

                if (MainMenu.MiscSettingsMenu.ShowPedModelDimensions)
                {
                    stopPedsLoop = true;
                    peds = World.GetAllPeds().Where(e => e.IsOnScreen && e.Position.DistanceToSquared(pp) < entityRange).ToList();
                    stopPedsLoop = false;

                    await Delay(delay);
                }

                if (MainMenu.MiscSettingsMenu.ShowVehicleModelDimensions)
                {
                    stopVehiclesLoop = true;
                    vehicles = World.GetAllVehicles().Where(e => e.IsOnScreen && e.Position.DistanceToSquared(pp) < entityRange).ToList();
                    stopVehiclesLoop = false;

                    await Delay(delay);
                }

            }
        }
        #endregion

        #region Personal Vehicle options
        private bool didShowPvHelpMessage = false;
        private int time = 0;
        /// <summary>
        /// Manages personal vehicle options like locking doors while close.
        /// </summary>
        /// <returns></returns>
        private async Task PersonalVehicleOptions()
        {
            if (MainMenu.PermissionsSetupComplete && MainMenu.PersonalVehicleMenu != null && MainMenu.PersonalVehicleMenu.CurrentPersonalVehicle != null)
            {
                if (!Game.PlayerPed.IsInVehicle(MainMenu.PersonalVehicleMenu.CurrentPersonalVehicle) && !Game.PlayerPed.IsGettingIntoAVehicle)
                {
                    if (Game.PlayerPed.Position.DistanceToSquared(MainMenu.PersonalVehicleMenu.CurrentPersonalVehicle.Position) < 650.0f)
                    {
                        if (Game.IsControlJustReleased(0, Control.VehicleHorn))
                        {
                            // check if it was recently pressed (within the last 500 ms).
                            if (GetGameTimer() - time < 500)
                            {
                                // lock or unlock the vehicle
                                PressKeyFob(MainMenu.PersonalVehicleMenu.CurrentPersonalVehicle);
                                await Delay(100);
                                var lockDoors = !GetVehicleDoorsLockedForPlayer(MainMenu.PersonalVehicleMenu.CurrentPersonalVehicle.Handle, Game.PlayerPed.Handle);
                                LockOrUnlockDoors(MainMenu.PersonalVehicleMenu.CurrentPersonalVehicle, lockDoors);

                                // reset the timer.
                                time = 0;
                            }
                            // otherwise count this as the first one.
                            else
                            {
                                time = GetGameTimer();
                            }
                        }
                        if (!didShowPvHelpMessage)
                        {
                            didShowPvHelpMessage = true;
                            HelpMessage.Custom("When you are close to your personal vehicle, you can double tap ~INPUT_VEH_HORN~ to lock or unlock it.", 10000, true);
                        }
                    }
                    else
                    {
                        await Delay(100);
                    }
                }
                else
                {
                    await Delay(100);
                }
            }
            else
            {
                await Delay(100);
            }
            await Task.FromResult(0);
        }
        #endregion

        #region animation functions
        /// <summary>
        /// This triggers a helmet visor/goggles toggle if available.
        /// THIS IS NOT A TICK FUNCTION
        /// </summary>
        /// <returns></returns>
        private async Task SwitchHelmetOnce()
        {
            if (MainMenu.PermissionsSetupComplete)
            {
                var component = GetPedPropIndex(Game.PlayerPed.Handle, 0);      // helmet index
                var texture = GetPedPropTextureIndex(Game.PlayerPed.Handle, 0); // texture
                var compHash = GetHashNameForProp(Game.PlayerPed.Handle, 0, component, texture); // prop combination hash
                if (GetShopPedApparelVariantPropCount((uint)compHash) > 0) // helmet has visor.
                {
                    var newHelmet = component;
                    var newHelmetTexture = texture;

                    var newHelmetData = Game.GetAltPropVariationData(Game.PlayerPed.Handle, 0);

                    Log(JsonConvert.SerializeObject(newHelmetData, Formatting.Indented));

                    if (newHelmetData != null && newHelmetData.Length > 0)
                    {
                        newHelmet = newHelmetData[0].altPropVariationIndex;
                        newHelmetTexture = newHelmetData[0].altPropVariationTexture;
                    }

                    var animName = component < newHelmet ? "visor_up" : "visor_down";
                    if (Game.PlayerPed.Model == PedHash.FreemodeFemale01)
                    {
                        if (component is 66 or 81)
                        {
                            animName = component > newHelmet ? "visor_up" : "visor_down";
                        }
                        if (component is >= 115 and <= 118)
                        {
                            animName = component < newHelmet ? "goggles_up" : "goggles_down";
                        }
                    }
                    else
                    {
                        if (component is 67 or 82)
                        {
                            animName = component > newHelmet ? "visor_up" : "visor_down";
                        }
                        if (component is >= 116 and <= 119)
                        {
                            animName = component < newHelmet ? "goggles_up" : "goggles_down";
                        }
                    }

                    var animDict = "anim@mp_helmets@on_foot";

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
                        var veh = GetVehicle();
                        if (veh != null && veh.Exists() && !veh.IsDead && (veh.Model.IsBicycle || veh.Model.IsBike || veh.Model.IsQuadbike))
                        {
                            if (veh.Model.IsQuadbike)
                            {
                                animDict = "anim@mp_helmets@on_bike@quad";
                            }
                            else if (veh.Model.IsBike)
                            {
                                var sportBikes = new List<uint>()
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
                                var chopperBikes = new List<uint>()
                                {
                                    (uint)GetHashKey("SANCTUS"),
                                    (uint)GetHashKey("ZOMBIEA"),
                                    (uint)GetHashKey("ZOMBIEB"),
                                };
                                var dirtBikes = new List<uint>()
                                {
                                    (uint)GetHashKey("BF400"),
                                    (uint)GetHashKey("ENDURO"),
                                    (uint)GetHashKey("MANCHEZ"),
                                    (uint)GetHashKey("SANCHEZ"),
                                    (uint)GetHashKey("SANCHEZ2"),
                                    (uint)GetHashKey("ESSKEY"),
                                };
                                var scooters = new List<uint>()
                                {
                                    (uint)GetHashKey("FAGGIO"),
                                    (uint)GetHashKey("FAGGIO2"),
                                    (uint)GetHashKey("FAGGIO3"),
                                    (uint)GetHashKey("CLIFFHANGER"),
                                    (uint)GetHashKey("BAGGER"),
                                };
                                var policeb = new List<uint>()
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
                    if (animName.StartsWith("pov_") && animDict != "anim@mp_helmets@on_foot")
                    {
                        animName = animName.Substring(4);
                    }
                    ClearPedTasks(Game.PlayerPed.Handle);
                    TaskPlayAnim(Game.PlayerPed.Handle, animDict, animName, 8.0f, 1.0f, -1, 48, 0.0f, false, false, false);
                    var timeoutTimer = GetGameTimer();
                    while (GetEntityAnimCurrentTime(Game.PlayerPed.Handle, animDict, animName) <= 0.0f)
                    {
                        if (GetGameTimer() - timeoutTimer > 1000)
                        {
                            ClearPedTasks(Game.PlayerPed.Handle);
                            Debug.WriteLine("[vMenu] [WARNING] Waiting for animation to start took too long. Preventing hanging of function. Dbg: fault in location 1.");
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
                            Debug.WriteLine("[vMenu] [WARNING] Waiting for animation duration took too long. Preventing hanging of function. Dbg: fault in location 2.");
                            return;
                        }
                        if (GetEntityAnimCurrentTime(Game.PlayerPed.Handle, animDict, animName) > 0.39f)
                        {
                            SetPedPropIndex(Game.PlayerPed.Handle, 0, newHelmet, newHelmetTexture, true);
                        }
                    }
                    ClearPedTasks(Game.PlayerPed.Handle);
                    RemoveAnimDict(animDict);
                }
            }
        }

        /// <summary>
        /// Pickup a snowball.
        /// THIS IS NOT A TICK FUNCTION
        /// </summary>
        /// <returns></returns>
        private async Task PickupSnowballOnce()
        {
            if (MainMenu.PermissionsSetupComplete)
            {
                ClearPedTasks(Game.PlayerPed.Handle);
                var maxAmmo = 10;
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
                    var fired = false;

                    var dur = GetAnimDuration(snowball_anim_dict, snowball_anim_name);
                    var timer = GetGameTimer();
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
