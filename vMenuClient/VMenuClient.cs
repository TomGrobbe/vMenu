using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Dynamic;

namespace vMenuClient
{
    public class VMenuClient : BaseScript
    {
        #region variables
        private static VehiclesList vehiclesList = new VehiclesList();
        private bool firstTick = true;
        private MenuPool _menuPool;
        private UIMenu mainMenu;
        private UIMenu noclipMenu;
        private Dictionary<string, bool> permissions = new Dictionary<string, bool>();
        //private object permissions;
        //private bool [] permissions = new bool[5] {false, false, false, false, false};
        private bool permsSetup = false;
        private int spectatingId = -1;

        private string lastVehicleSpawned = "Adder";
        private bool vehicleGodMode = false;
        private bool playerGodMode = false;
        private bool neverWanted = false;
        private bool superJump = false;
        private bool unlimitedStamina = true;
        private bool joinnotif = true;
        private bool deathnotif = true;

        private int hour = 7;
        private int minute = 0;
        private string weather = "CLEAR";
        private string previousWeather = "CLEAR";

        private string playerInvincibleText = GetLabelText("BLIP_6") + " " + GetLabelText("CHEAT_INVINCIBILITY_OFF"); // Player Invincibility
        private string playerOptionsText = GetLabelText("BLIP_6") + " " + GetLabelText("PM_MP_OPTIONS"); // Player Options
        private string vehicleOptionsText = GetLabelText("collision_w0oam1") + " " + GetLabelText("PM_MP_OPTIONS"); // Vehicle Options
        private string weatherMenuTitle = GetLabelText("collision_7ydzakm") + " " + GetLabelText("PM_MP_OPTIONS"); // Weather Options
        private string timeMenuTitle = GetLabelText("FM_HTUT_TME") + " " + GetLabelText("PM_MP_OPTIONS"); // Time Options

        private string extraSunnyWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_EXTRA_SUNNY"); // Extra sunny weather.
        private string clearWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_CLEAR"); // Clear weather.
        private string cloudsWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_CLOUDY"); // Cloudy weather.
        private string overcastWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_OVERCAST"); // Overcast weather.
        private string clearingWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_CLEARING"); // Clearing weather.
        private string rainWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_RAIN"); // Rainy weather.
        private string thunderWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_THUNDER"); // Thundery weather.
        private string smogWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_SMOGGY"); // Smoggy weather.
        private string foggyWeather = "Foggy " + GetLabelText("collision_7ydzakm").ToLower() + "."; // Foggy weather.
        private string snowWeather = GetLabelText("CHEAT_ADVANCE_WEATHER_SNOW"); // Snowy weather.
        private string snowLightWeather = "Light snowy " + GetLabelText("collision_7ydzakm").ToLower() + "."; // Light snowy weather.
        private string blizzardWeather = "Blizzard " + GetLabelText("collision_7ydzakm").ToLower() + "."; // Blizzard weather.
        private string xmasWeather = "Xmas " + GetLabelText("collision_7ydzakm").ToLower() + "."; // Xmas weather.
        private string halloweenWeather = "Halloween " + GetLabelText("collision_7ydzakm").ToLower() + "."; // Halloween weather.
        private string neutralWeather = "Neutral " + GetLabelText("collision_7ydzakm").ToLower() + "."; // Neutral weather.
        // Create a new banner with a dark red background.
        private UIResRectangle banner = new UIResRectangle
        {
            Color = System.Drawing.Color.FromArgb(255, 183, 28, 28)
        };
        #endregion

        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public VMenuClient()
        {
            EventHandlers.Add("vMenu:NotifyPlayer", new Action<string, bool>(Notify));
            EventHandlers.Add("vMenu:WeatherAndTimeSync", new Action<string, int, int>(SetWeatherAndTime));
            EventHandlers.Add("vMenu:SetPermissions", new Action<dynamic>(SetPermissions));
            Tick += OnTick;
            Tick += JoinQuitTick;
            Tick += SetTime;
            Tick += SetWeather;
        }
        #endregion

        #region Set Permissions
        /// <summary>
        /// Set the permissions when they are coming in from the "vMenu:SetPermissions" event.
        /// </summary>
        /// <param name="permissions"></param>
        private void SetPermissions(dynamic permissions)
        {
            foreach (dynamic dict in permissions)
            {
                this.permissions.Add(dict.Key.ToString(), (bool)dict.Value);
            }
            permsSetup = true;
        }
        #endregion

        #region Handle weather and time loops and updates
        /// <summary>
        /// Set the new weather and time.
        /// </summary>
        /// <param name="weather"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        private void SetWeatherAndTime(string weather, int hour, int minute)
        {
            this.weather = weather;
            this.hour = hour;
            this.minute = minute;
        }

        /// <summary>
        /// Loop every 100ms and set the weather to the correct type.
        /// </summary>
        /// <returns></returns>
        private async Task SetWeather()
        {
            await Delay(100);
            if (previousWeather != weather)
            {
                ClearOverrideWeather();
                ClearWeatherTypePersist();
                SetWeatherTypeOverTime(weather, 15);
                previousWeather = weather;
                if (weather == "XMAS")
                {
                    // Load snow/ice audio
                    RequestScriptAudioBank("ICE_FOOTSTEPS", false);
                    RequestScriptAudioBank("SNOW_FOOTSTEPS", false);
                    // Load snow particle effects
                    RequestNamedPtfxAsset("core_snow");
                    // Enable footsteps and vehicle tire tracks.
                    SetForcePedFootstepsTracks(true);
                    SetForceVehicleTrails(true);
                }
                else
                {
                    // Unload snow/ice audio
                    ReleaseNamedScriptAudioBank("ICE_FOOTSTEPS");
                    ReleaseNamedScriptAudioBank("SNOW_FOOTSTEPS");
                    // Unload snow particle effects
                    RemoveNamedPtfxAsset("core_snow");
                    // Disable footsteps and vehicle tire tracks.
                    SetForcePedFootstepsTracks(false);
                    SetForceVehicleTrails(false);
                }
                await Delay(15000);
            }
            if (weather == "EXTRASUNNY")
            {
                // Remove all clouds, it's EXTRA sunny after all ;^)
                ClearCloudHat();
            }
            else if (weather == "XMAS")
            {
                // Enable the snow particles.
                UseParticleFxAssetNextCall("core_snow");
            }
            ClearOverrideWeather();
            ClearWeatherTypePersist();
            SetWeatherTypeNow(previousWeather);
            SetWeatherTypeNowPersist(previousWeather);
            SetWeatherTypePersist(previousWeather);
        }

        private async Task SetTime()
        {
            await Delay(2000);
            minute++;
            if (minute > 59)
            {
                minute = 0;
                hour++;
            }
            if (hour > 23)
            {
                hour = 0;
            }
            NetworkOverrideClockTime(hour, minute, 0);
        }
        #endregion
        #region OnTick
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <returns></returns>
        public async Task OnTick()
        {
            #region firstTick
            // If the script has just started, run the code inside this block once.
            if (firstTick)
            {
                firstTick = false;
                TriggerServerEvent("vMenu:RequestPermissions", PlayerId());
                await CreateMainMenu();
                await CreateNoclipMenu();
                ClearBrief();
                ShowHelp();
                EventHandlers.Add("playerSpawned", new Action(ShowHelp));
                await Delay(0);
            }
            #endregion

            // Process all menus.
            _menuPool.ProcessMenus();
            _menuPool.ProcessMouse();
            //_menuPool.ProcessControl();

            #region opening/closing of menu's
            // Handle opening/closing of the menu for controller.
            if (!_menuPool.IsAnyMenuOpen())
            {
                int openDelay = 1;
                while (IsControlPressed(0, (int)Control.InteractionMenu) && !IsInputDisabled(2))
                {
                    await Delay(0);
                    openDelay++;
                    if (openDelay > 45)
                    {
                        mainMenu.Visible = true;
                        SetCursorLocation(0.5f, 0.5f);
                        PlaySoundFrontend(-1, mainMenu.AUDIO_SELECT, mainMenu.AUDIO_LIBRARY, false);
                        break;
                    }
                }
            }
            // Handle opening/closing of the menu for keyboard/mouse.
            if (IsControlJustPressed(0, (int)Control.InteractionMenu) && IsInputDisabled(2))
            {
                mainMenu.Visible = !_menuPool.IsAnyMenuOpen();
                SetCursorLocation(0.5f, 0.5f);
                PlaySoundFrontend(-1, mainMenu.AUDIO_SELECT, mainMenu.AUDIO_LIBRARY, false);
            }

            if (permsSetup)
            {
                if (permissions["noclip"])
                {
                    if (IsControlJustPressed(0, (int)Control.ReplayStartStopRecordingSecondary) && IsInputDisabled(2))
                    {
                        noclipMenu.Visible = !noclipMenu.Visible;
                        SetCursorLocation(0.5f, 0.5f);
                    }
                }
            }

            if (spectatingId != -1 && IsPlayerDead(PlayerId()))
            {
                spectatingId = -1;
                NetworkSetInSpectatorMode(false, PlayerPedId());
            }

            #endregion

            #region disable buttons when menu is open, also close all menu's if the game is paused.
            // If any menu is open, run the following.
            if (_menuPool.IsAnyMenuOpen() && !noclipMenu.Visible)
            {
                // If keyboard/mouse imput > disable looking left/right because we don't want the camera to freak out.
                if (IsInputDisabled(2))
                {
                    //DisableControlAction(0, (int)Control.LookLeftRight, true);
                    //DisableControlAction(0, (int)Control.LookUpDown, true);
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        DisableControlAction(0, (int)Control.VehicleSelectNextWeapon, true);
                    }
                }
                // Disable mouse-steering input to prevent screen rotation.
                if (IsPedInAnyVehicle(PlayerPedId(), false))
                {
                    DisableControlAction(0, (int)Control.VehicleMouseControlOverride, true);
                    DisableControlAction(0, (int)Control.VehicleRadioWheel, true);
                }
                DisableControlAction(0, (int)Control.Attack, true);
                DisableControlAction(0, (int)Control.Attack2, true);
                DisableControlAction(0, (int)Control.VehicleCinCam, true);
                DisableControlAction(0, (int)Control.MeleeAttack1, true);
                DisableControlAction(0, (int)Control.MeleeAttack2, true);
                DisableControlAction(0, (int)Control.MeleeAttackAlternate, true);
                DisableControlAction(0, (int)Control.MeleeAttackHeavy, true);
                DisableControlAction(0, (int)Control.MeleeAttackLight, true);
                DisableControlAction(0, (int)Control.Phone, true);

                // Close all menu's if the pause menu opens.
                if (IsPauseMenuActive())
                {
                    _menuPool.CloseAllMenus();
                    _menuPool.RefreshIndex();
                }
            }
            #endregion
            #region handle player options each tick
            // Handle player invincibility.
            SetPlayerInvincible(PlayerId(), playerGodMode);
            // Handle player wanted level based on neverWanted preference.
            if (neverWanted && GetPlayerWantedLevel(PlayerId()) > 0)
            {
                SetPlayerWantedLevel(PlayerId(), 0, false);
            }
            // Handle player stamina.
            if (unlimitedStamina)
            {
                ResetPlayerStamina(PlayerId());
            }
            // Handle super jump.
            if (superJump)
            {
                SetSuperJumpThisFrame(PlayerId());
            }
            #endregion
            #region handle vehicle options each tick
            // Handle Vehicle stuff.
            var veh = GetVehiclePedIsIn(PlayerPedId(), false);
            if (IsPedInAnyVehicle(PlayerPedId(), false) && DoesEntityExist(veh) && !IsEntityDead(veh))
            {
                // Handle vehicle invincibility.
                SetEntityInvincible(veh, vehicleGodMode);
                SetVehicleCanBeVisiblyDamaged(veh, !vehicleGodMode);
                SetVehicleCanBreak(veh, !vehicleGodMode);
                SetVehicleEngineCanDegrade(veh, !vehicleGodMode);
                SetVehicleStrong(veh, vehicleGodMode);
                SetVehicleTyresCanBurst(veh, !vehicleGodMode);
                SetVehicleWheelsCanBreak(veh, !vehicleGodMode);
                SetDisableVehiclePetrolTankDamage(veh, vehicleGodMode);
                SetDisableVehiclePetrolTankFires(veh, vehicleGodMode);

            }

            #endregion
            #region hanlde noclip menu
            if (noclipMenu.Visible)
            {
                DisableControlAction(0, (int)Control.Cover, true);
                DisableControlAction(0, (int)Control.MultiplayerInfo, true);
                DisableControlAction(0, (int)Control.MoveUpDown, true);
                DisableControlAction(0, (int)Control.MoveDown, true);
                DisableControlAction(0, (int)Control.MoveRightOnly, true);
                DisableControlAction(0, (int)Control.MoveLeftRight, true);
                DisableControlAction(0, (int)Control.MoveRight, true);
                DisableControlAction(0, (int)Control.MoveLeft, true);
                DisableControlAction(0, (int)Control.MoveLeftOnly, true);
                DisableControlAction(0, (int)Control.MoveDownOnly, true);
                DisableControlAction(0, (int)Control.VehicleRadioWheel, true);
                float noclipSpeedModifier = 3.5f;

                if (IsDisabledControlPressed(0, (int)Control.Sprint) || IsControlPressed(0, (int)Control.Sprint))
                {
                    noclipSpeedModifier = 15.5f;
                }
                else if (IsDisabledControlPressed(0, (int)Control.Duck) || IsControlPressed(0, (int)Control.Duck))
                {
                    noclipSpeedModifier = 1.5f;
                }

                if (!IsPedInAnyVehicle(PlayerPedId(), false))
                {

                    FreezeEntityPosition(PlayerPedId(), true);
                    FreezeEntityPosition(GetVehiclePedIsIn(PlayerPedId(), true), false);
                    if (IsDisabledControlPressed(0, (int)Control.MoveUpOnly))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0.0f, 0.2f * noclipSpeedModifier, 0.0f);
                        SetEntityCoordsNoOffset(PlayerPedId(), newPos.X, newPos.Y, newPos.Z, true, false, false);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MoveDownOnly))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0.0f, -0.2f * noclipSpeedModifier, 0.0f);
                        SetEntityCoordsNoOffset(PlayerPedId(), newPos.X, newPos.Y, newPos.Z, true, false, false);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MoveLeftOnly))
                    {
                        var heading = GetEntityHeading(PlayerPedId());
                        SetEntityHeading(PlayerPedId(), heading + 4f);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MoveRightOnly))
                    {
                        var heading = GetEntityHeading(PlayerPedId());
                        SetEntityHeading(PlayerPedId(), heading - 4f);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MultiplayerInfo))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0.0f, 0.0f, -noclipSpeedModifier / 5f);
                        SetEntityCoordsNoOffset(PlayerPedId(), newPos.X, newPos.Y, newPos.Z, true, false, false);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.Cover))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0.0f, 0.0f, noclipSpeedModifier / 5f);
                        SetEntityCoordsNoOffset(PlayerPedId(), newPos.X, newPos.Y, newPos.Z, true, false, false);
                    }
                }
                else
                {
                    var vehicle = GetVehiclePedIsIn(PlayerPedId(), false);
                    FreezeEntityPosition(PlayerPedId(), false);
                    FreezeEntityPosition(vehicle, true);
                    if (IsDisabledControlPressed(0, (int)Control.MoveUpOnly))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(vehicle, 0.0f, 0.2f * noclipSpeedModifier, 0.0f);
                        SetEntityCoordsNoOffset(vehicle, newPos.X, newPos.Y, newPos.Z, false, false, false);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MoveDownOnly))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(vehicle, 0.0f, -0.2f * noclipSpeedModifier, 0.0f);
                        SetEntityCoordsNoOffset(vehicle, newPos.X, newPos.Y, newPos.Z, false, false, false);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MoveLeftOnly))
                    {
                        var heading = GetEntityHeading(vehicle);
                        SetEntityHeading(PlayerPedId(), heading + 4f);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MoveRightOnly))
                    {
                        var heading = GetEntityHeading(vehicle);
                        SetEntityHeading(PlayerPedId(), heading - 4f);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.MultiplayerInfo))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(vehicle, 0.0f, 0.0f, -noclipSpeedModifier / 5f);
                        SetEntityCoordsNoOffset(vehicle, newPos.X, newPos.Y, newPos.Z, false, false, false);
                    }
                    if (IsDisabledControlPressed(0, (int)Control.Cover))
                    {
                        var newPos = GetOffsetFromEntityInWorldCoords(vehicle, 0.0f, 0.0f, noclipSpeedModifier / 5f);
                        SetEntityCoordsNoOffset(vehicle, newPos.X, newPos.Y, newPos.Z, false, false, false);
                    }
                }


                //if (IsInputDisabled(2))
                //{

                //}
            }
            else
            {
                FreezeEntityPosition(PlayerPedId(), false);
                FreezeEntityPosition(GetVehiclePedIsIn(PlayerPedId(), true), false);
            }
            #endregion
        }
        #endregion
        #region Join/Quit messages OnTick timed checks
        /// <summary>
        /// Runs once every second and checks for players who've joined/left.
        /// </summary>
        /// <returns></returns>
        private async Task JoinQuitTick()
        {
            await Delay(1000);
        }
        #endregion

        #region NoClip Menu
        /// <summary>
        /// No clip menu
        /// </summary>
        /// <returns></returns>
        private async Task CreateNoclipMenu()
        {
            //// Wait until the permissions are loaded.
            while (permsSetup == false)
            {
                await Delay(0);
            }
            await Delay(0);
            {
                noclipMenu = new UIMenu("Noclip", "NOCLIP");
                noclipMenu.AddInstructionalButton(new InstructionalButton(Control.MoveLeftRight, "Turn Left/Right"));
                noclipMenu.AddInstructionalButton(new InstructionalButton(Control.MoveUpDown, "Go Forwards/Backwards"));
                noclipMenu.AddInstructionalButton(new InstructionalButton(Control.Cover, "Go Up"));
                noclipMenu.AddInstructionalButton(new InstructionalButton(Control.MultiplayerInfo, "Go Down"));
                noclipMenu.AddInstructionalButton(new InstructionalButton(Control.ReplayStartStopRecordingSecondary, "Toggle Noclip"));
                noclipMenu.ControlDisablingEnabled = false;
                _menuPool.Add(noclipMenu);
            }
        }
        #endregion

        #region Show instructions on spawn/resource start on how to open the menu
        /// <summary>
        /// Show a help message to the user explaining how to open the menu.
        /// </summary>
        private void ShowHelp()
        {
            if (IsInputDisabled(2))
            {
                Help("Press ~INPUT_INTERACTION_MENU~ to open the interaction menu.");
            }
            else
            {
                Help("Press and hold down ~INPUT_INTERACTION_MENU~ to open the interaction menu.");
            }
        }
        #endregion

        #region Main menu
        /// <summary>
        /// Create the main menu.
        /// </summary>
        /// <returns></returns>
        public async Task CreateMainMenu()
        {
            // Create new menu pool.
            _menuPool = new MenuPool();

            // Create the main menu using the player's name as the title.
            string playerName = GetPlayerName(PlayerId());
            // If the script hasn't loaded properly yet or if the username is invalid for some other reason,
            // keep getting the user name until it's no longer "** Invalid **".
            while (playerName == "** Invalid **" || playerName == "**Invalid**")
            {
                playerName = GetPlayerName(PlayerId());
                await Delay(0);
            }
            // Wait until the permissions are loaded.
            while (permsSetup == false)
            {
                await Delay(0);
            }
            // When the username is valid, create the new menu.
            mainMenu = new UIMenu(playerName, "Main Menu");

            // Add the main menu to the _menuPool.
            _menuPool.Add(mainMenu);

            // Create and add each menu only if the player has permission for it.
            if (permissions["onlinePlayers"])
            {
                //Add Online Players Menu.
                CreateOnlinePlayers();
            }
            if (permissions["playerOptions"])
            {
                // Add Player Options.
                CreatePlayerOptions(mainMenu);
            }
            if (permissions["vehicleOptions"])
            {
                // Add vehicle options.
                CreateVehicleOptions(mainMenu);
            }
            if (permissions["spawnVehicle"])
            {
                // Add car spawn menu.
                CreateVehicleSpawn(mainMenu);
            }
            if (permissions["voiceChat"])
            {
                // Add car spawn menu.
                CreateVoiceChat();
            }
            if (permissions["timeOptions"])
            {
                // Add Time Options Menu.
                CreateTimeMenu();
            }
            if (permissions["weatherOptions"])
            {
                // Add Weather Options Menu.
                CreateWeatherMenu();
            }

            // Apply the banner to the main menu.
            mainMenu.SetBannerType(banner);

            // Allow all buttons to function when the user has the menu open.
            _menuPool.ControlDisablingEnabled = false;


            // Refresh the index so the menu opens at the top.
            _menuPool.RefreshIndex();
            
            // Broken setting?
            _menuPool.ResetCursorOnOpen = true;
            _menuPool.MouseEdgeEnabled = false;
            
            mainMenu.ResetCursorOnOpen = true;
            mainMenu.ScaleWithSafezone = true;
        }
        #endregion

        #region Vehilce Spawn Menu
        /// <summary>
        /// Create vehicle spawn menu
        /// </summary>
        /// <param name="mainmenu"></param>
        private void CreateVehicleSpawn(UIMenu mainmenu)
        {
            //var submenu = new UIMenu(GetPlayerName(PlayerId()), "Spawn Vehicle");

            // collision_a85oy1 = General Options
            var submenu = new UIMenu(GetPlayerName(PlayerId()), GetLabelText("BLIP_125") + " Menu"); // Localized Title: Spawn vehicle

            // Create mainMenu buttons
            //var carSpawnMenuBtn = new UIMenuItem("Spawn Vehicle", "Vehicle Spawn Menu.");
            var carSpawnMenuBtn = new UIMenuItem("Vehicle Spawner", "Select a vehicle from a specific category, or spawn one directly by entering it's name."); // Localized Title: Spawn vehicle
            //var carSpawnMenuBtn = new UIMenuItem(GetLabelText("BLIP_125") + " Menu", "Vehicle Spawn."); // Localized Title: Spawn vehicle

            // Add items/buttons to the menu
            mainmenu.AddItem(carSpawnMenuBtn);
            mainmenu.BindMenuToItem(submenu, carSpawnMenuBtn);

            #region catgory buttons
            // Create submenu buttons
            var spawnByNameBtn = new UIMenuItem("Spawn By Name", "Spawn a vehicle by entering it's name.");
            var Boats = new UIMenuItem("Boats", "Spawn a boat.");
            var Commercial = new UIMenuItem("Commercial", "Spawn a commercial vehicle.");
            var Compacts = new UIMenuItem("Compacts", "Spawn a compact car.");
            var Coupes = new UIMenuItem("Coupes", "Spawn a coupe.");
            var Cycles = new UIMenuItem("Cycles", "Spawn a bicycle.");
            var Emergency = new UIMenuItem("Emergency", "Spawn an emergency vehicle.");
            var Helicopters = new UIMenuItem("Helicopters", "Spawn a helicopter.");
            var Industrial = new UIMenuItem("Industrial", "Spawn an industrial vehicle.");
            var Military = new UIMenuItem("Military", "Spawn a military vehicle.");
            var Motorcycles = new UIMenuItem("Motorcycles", "Spawn a motorcycle.");
            var Muscle = new UIMenuItem("Muscle", "Spawn a musscle car.");
            var OffRoad = new UIMenuItem("Off-Road", "Spawn an offroad vehicle.");
            var Planes = new UIMenuItem("Planes", "Spawn a plane.");
            var Sedans = new UIMenuItem("Sedans", "Spawn a sedan.");
            var Service = new UIMenuItem("Service", "Spawn a service vehicle.");
            var Sports = new UIMenuItem("Sports", "Spawn a sports car.");
            var SportsClassics = new UIMenuItem("Sports Classics", "Spawn a sports classic car.");
            var Super = new UIMenuItem("Super", "Spawn a supercar.");
            var SUVs = new UIMenuItem("SUV's", "Spawn a SUV.");
            var Trains = new UIMenuItem("Trains", "Spawn a train.");
            var Utility = new UIMenuItem("Utility", "Spawn a utility vehicle.");
            var Vans = new UIMenuItem("Vans", "Spawn a van.");
            #endregion

            #region add category buttons to the menu
            // Add the buttons to the menu.
            submenu.AddItem(spawnByNameBtn);
            submenu.AddItem(Boats);
            submenu.AddItem(Commercial);
            submenu.AddItem(Compacts);
            submenu.AddItem(Coupes);
            submenu.AddItem(Cycles);
            submenu.AddItem(Emergency);
            submenu.AddItem(Helicopters);
            submenu.AddItem(Industrial);
            submenu.AddItem(Military);
            submenu.AddItem(Motorcycles);
            submenu.AddItem(Muscle);
            submenu.AddItem(OffRoad);
            submenu.AddItem(Planes);
            submenu.AddItem(Sedans);
            submenu.AddItem(Service);
            submenu.AddItem(Sports);
            submenu.AddItem(SportsClassics);
            submenu.AddItem(Super);
            submenu.AddItem(SUVs);
            submenu.AddItem(Trains);
            submenu.AddItem(Utility);
            submenu.AddItem(Vans);
            #endregion

            #region create the category submenu's
            var BoatsM = CreateBoats();
            BoatsM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var CommercialM = CreateCommercial();
            CommercialM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var CompactsM = CreateCompacts();
            CompactsM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var CoupesM = CreateCoupes();
            CoupesM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var CyclesM = CreateCycles();
            CyclesM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var EmergencyM = CreateEmergency();
            EmergencyM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var HelicoptersM = CreateHelicopters();
            HelicoptersM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var IndustrialM = CreateIndustrial();
            IndustrialM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var MilitaryM = CreateMilitary();
            MilitaryM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var MotorcyclesM = CreateMotorcycles();
            MotorcyclesM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var MuscleM = CreateMuscle();
            MuscleM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var OffRoadM = CreateOffRoad();
            OffRoadM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var PlanesM = CreatePlanes();
            PlanesM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var SedansM = CreateSedans();
            SedansM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var ServiceM = CreateService();
            ServiceM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var SportsM = CreateSports();
            SportsM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var SportsClassicsM = CreateSportsClassics();
            SportsClassicsM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var SuperM = CreateSuper();
            SuperM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var SUVsM = CreateSUVs();
            SUVsM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var TrainsM = CreateTrains();
            TrainsM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var UtilityM = CreateUtility();
            UtilityM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            var VansM = CreateVans();
            VansM.OnMenuClose += (sender) =>
            {
                submenu.Visible = true;
            };
            #endregion

            #region Set the banners for each menu.
            // Apply the banner to the main menu.
            submenu.SetBannerType(banner);
            BoatsM.SetBannerType(banner);
            CommercialM.SetBannerType(banner);
            CompactsM.SetBannerType(banner);
            CoupesM.SetBannerType(banner);
            CyclesM.SetBannerType(banner);
            EmergencyM.SetBannerType(banner);
            HelicoptersM.SetBannerType(banner);
            IndustrialM.SetBannerType(banner);
            MilitaryM.SetBannerType(banner);
            MotorcyclesM.SetBannerType(banner);
            MuscleM.SetBannerType(banner);
            OffRoadM.SetBannerType(banner);
            PlanesM.SetBannerType(banner);
            SedansM.SetBannerType(banner);
            ServiceM.SetBannerType(banner);
            SportsM.SetBannerType(banner);
            SportsClassicsM.SetBannerType(banner);
            SuperM.SetBannerType(banner);
            SUVsM.SetBannerType(banner);
            TrainsM.SetBannerType(banner);
            UtilityM.SetBannerType(banner);
            VansM.SetBannerType(banner);
            #endregion

            #region bind the category buttons to the category submenu's
            submenu.BindMenuToItem(BoatsM, Boats);
            submenu.BindMenuToItem(CommercialM, Commercial);
            submenu.BindMenuToItem(CompactsM, Compacts);
            submenu.BindMenuToItem(CoupesM, Coupes);
            submenu.BindMenuToItem(CyclesM, Cycles);
            submenu.BindMenuToItem(EmergencyM, Emergency);
            submenu.BindMenuToItem(HelicoptersM, Helicopters);
            submenu.BindMenuToItem(IndustrialM, Industrial);
            submenu.BindMenuToItem(MilitaryM, Military);
            submenu.BindMenuToItem(MotorcyclesM, Motorcycles);
            submenu.BindMenuToItem(MuscleM, Muscle);
            submenu.BindMenuToItem(OffRoadM, OffRoad);
            submenu.BindMenuToItem(PlanesM, Planes);
            submenu.BindMenuToItem(SedansM, Sedans);
            submenu.BindMenuToItem(ServiceM, Service);
            submenu.BindMenuToItem(SportsM, Sports);
            submenu.BindMenuToItem(SportsClassicsM, SportsClassics);
            submenu.BindMenuToItem(SuperM, Super);
            submenu.BindMenuToItem(SUVsM, SUVs);
            submenu.BindMenuToItem(TrainsM, Trains);
            submenu.BindMenuToItem(UtilityM, Utility);
            submenu.BindMenuToItem(VansM, Vans);
            #endregion

            #region handle spawnByName button clicked
            // Handle event when buttons are clicked.
            submenu.OnItemSelect += async (sender, item, index) =>
            {
                if (item == spawnByNameBtn)
                {

                    submenu.Visible = false;
                    //await Game.GetUserInput(WindowTitle.FMMC_KEY_TIP8, "Adder", 60); //Title: Enter message:
                    DisplayOnscreenKeyboard(6, "collision_9dkiwut", "", lastVehicleSpawned, "", "", "", 60); //Title: Enter a vehicle

                    var status = UpdateOnscreenKeyboard();
                    while (status == 0)
                    {
                        await Delay(0);
                        status = UpdateOnscreenKeyboard();
                    }
                    //status = UpdateOnscreenKeyboard();
                    var result = GetOnscreenKeyboardResult();
                    if (status == 1 || status == 2 && result != "" && result != null)
                    {
                        lastVehicleSpawned = result;
                        SpawnVehicleAsync(result);
                    }
                    else
                    {
                        Notify("~r~Error: ~w~You did not provide a vehicle name.", true);
                    }
                    submenu.Visible = true;
                }
            };
            #endregion

            submenu.OnMenuClose += (sender) =>
            {
                mainMenu.Visible = true;
            };

            _menuPool.Add(submenu);
            _menuPool.RefreshIndex();
        }
        #endregion
        #region Vehicle Categories
        #region Boats menu
        private UIMenu CreateBoats()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Boats");
            foreach (var veh in vehiclesList.Boats())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Commercial vehicle menu
        private UIMenu CreateCommercial()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Commercial");
            foreach (var veh in vehiclesList.Commercial())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Compacts vehicle menu
        private UIMenu CreateCompacts()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Compacts");
            foreach (var veh in vehiclesList.Compacts())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Coupes vehicle menu
        private UIMenu CreateCoupes()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Coupes");
            foreach (var veh in vehiclesList.Coupes())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Cycles vehicle menu
        private UIMenu CreateCycles()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Cycles");
            foreach (var veh in vehiclesList.Cycles())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Emergency vehicle menu
        private UIMenu CreateEmergency()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Emergency");
            foreach (var veh in vehiclesList.Emergency())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Helicopters vehicle menu
        private UIMenu CreateHelicopters()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Helicopters");
            foreach (var veh in vehiclesList.Helicopters())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Industrial vehicle menu
        private UIMenu CreateIndustrial()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Industrial");
            foreach (var veh in vehiclesList.Industrial())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Military vehicle menu
        private UIMenu CreateMilitary()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Military");
            foreach (var veh in vehiclesList.Military())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Motorcycles vehicle menu
        private UIMenu CreateMotorcycles()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Motorcycles");
            foreach (var veh in vehiclesList.Motorcycles())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Muscle vehicle menu
        private UIMenu CreateMuscle()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Muscle");
            foreach (var veh in vehiclesList.Muscle())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region OffRoad vehicle menu
        private UIMenu CreateOffRoad()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Off-Road");
            foreach (var veh in vehiclesList.OffRoad())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Planes vehicle menu
        private UIMenu CreatePlanes()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Planes");
            foreach (var veh in vehiclesList.Planes())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Sedans vehicle menu
        private UIMenu CreateSedans()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Sedans");
            foreach (var veh in vehiclesList.Sedans())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Service vehicle menu
        private UIMenu CreateService()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Service");
            foreach (var veh in vehiclesList.Service())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Sports vehicle menu
        private UIMenu CreateSports()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Sports");
            foreach (var veh in vehiclesList.Sports())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Sports Classics vehicle menu
        private UIMenu CreateSportsClassics()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Sports Classics");
            foreach (var veh in vehiclesList.SportsClassics())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Super vehicle menu
        private UIMenu CreateSuper()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Super");
            foreach (var veh in vehiclesList.Super())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region SUVs vehicle menu
        private UIMenu CreateSUVs()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "SUVs");
            foreach (var veh in vehiclesList.SUVs())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Trains vehicle menu
        private UIMenu CreateTrains()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Trains");
            foreach (var veh in vehiclesList.Trains())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Utility vehicle menu
        private UIMenu CreateUtility()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Utility");
            foreach (var veh in vehiclesList.Utility())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #region Vans vehicle menu
        private UIMenu CreateVans()
        {
            UIMenu menu = new UIMenu("Vehicle Spawner", "Vans");
            foreach (var veh in vehiclesList.Vans())
            {
                if (veh.Contains(","))
                {
                    var vehname = veh.Split(',')[0];
                    var vehDisplayName = veh.Split(',')[1];
                    if (IsModelInCdimage((uint)GetHashKey(vehname)))
                    {
                        menu.AddItem(new UIMenuItem(vehDisplayName, vehname));
                    }
                }
                else
                {
                    if (IsModelInCdimage((uint)GetHashKey(veh)))
                    {
                        var name = GetLabelText(GetDisplayNameFromVehicleModel((uint)(int)GetHashKey(veh)));
                        if (name == "NULL")
                        {
                            name = veh.ToLower();
                        }
                        menu.AddItem(new UIMenuItem(name, veh));
                    }
                }
            }
            menu.OnItemSelect += (sender, item, index) =>
            {
                SpawnVehicleAsync(item.Description);
            };
            menu.RefreshIndex();
            _menuPool.Add(menu);
            return menu;
        }
        #endregion
        #endregion

        #region Player Options Menu
        private void CreatePlayerOptions(UIMenu menu)
        {
            // Create submenu.
            var submenu = new UIMenu(GetPlayerName(PlayerId()), playerOptionsText);
            submenu.SetBannerType(banner);

            // Create buttons for main player options.
            UIMenuItem heal = new UIMenuItem("Heal Player", "Heal the player to max health.");
            UIMenuItem armor = new UIMenuItem("Max Armor", "Give the player max armor.");
            UIMenuItem clean = new UIMenuItem("Clean Player", "Clean the player's clothes.");

            // Add the buttons to the submenu.
            submenu.AddItem(heal);
            submenu.AddItem(armor);
            submenu.AddItem(clean);

            submenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == heal)
                {
                    SetEntityHealth(PlayerPedId(), GetEntityMaxHealth(PlayerPedId()));
                    Subtitle("Your player has been healed.");
                }
                else if (item == armor)
                {
                    SetPedArmour(PlayerPedId(), GetPlayerMaxArmour(PlayerId()));
                    Subtitle("Your player's armor has been restored to maximum capacity.");
                }
                else if (item == clean)
                {
                    ClearPedBloodDamage(PlayerPedId());
                    Subtitle("Your player's clothes have been cleaned.");
                }
            };

            // Create checkboxes for the main player toggle options.
            var godmodebtn = new UIMenuCheckboxItem(playerInvincibleText, false, "Toggle Player Invincibility.");
            var neverwantedbtn = new UIMenuCheckboxItem("Never Wanted", false, "Toggle Never Wanted.");
            var fastrunbtn = new UIMenuCheckboxItem("Fast Run", false, "Enable super walk/run speeds for your player.");
            var fastswimbtn = new UIMenuCheckboxItem("Fast Swimming", false, "Enable super swimming speeds for your player.");
            var superjumpbtn = new UIMenuCheckboxItem("Super Jump", false, "Enable super jump for your player.");
            var unlimstaminabtn = new UIMenuCheckboxItem("Unlimited Stamina", true, "Enable/disable unlimited stamina for your player. It's recommended to leave this enabled.");
            var deathnotifications = new UIMenuCheckboxItem("Death Notifications", true, "Receive notifications when your or other players die.");
            var joinleavenotifications = new UIMenuCheckboxItem("Join / Leave Notifications", true, "Receive notifications when other players join or leave the server.");

            // Add checkboxes to the submenu.
            submenu.AddItem(godmodebtn);
            submenu.AddItem(neverwantedbtn);
            submenu.AddItem(fastrunbtn);
            submenu.AddItem(fastswimbtn);
            submenu.AddItem(superjumpbtn);
            submenu.AddItem(unlimstaminabtn);
            submenu.AddItem(deathnotifications);
            submenu.AddItem(joinleavenotifications);

            // Handle checkbox changes.
            submenu.OnCheckboxChange += (sender, checkbox, _checked) =>
            {
                // God Mode
                if (checkbox == godmodebtn)
                {
                    playerGodMode = _checked;
                }
                // Never Wanted
                else if (checkbox == neverwantedbtn)
                {
                    neverWanted = _checked;
                }
                // Fast Run
                else if (checkbox == fastrunbtn)
                {
                    // Enabled
                    if (_checked)
                    {
                        SetRunSprintMultiplierForPlayer(PlayerId(), 1.49f);
                    }
                    // Disabled
                    else
                    {
                        SetRunSprintMultiplierForPlayer(PlayerId(), 1f);
                    }
                }
                // Fast Swimming
                else if (checkbox == fastswimbtn)
                {
                    // Enabled
                    if (_checked)
                    {
                        SetSwimMultiplierForPlayer(PlayerId(), 1.49f);
                    }
                    // Disabled
                    else
                    {
                        SetSwimMultiplierForPlayer(PlayerId(), 1f);
                    }
                }
                // Super Jump
                else if (checkbox == superjumpbtn)
                {
                    superJump = _checked;
                }
                // Unlimited Stamina
                else if (checkbox == unlimstaminabtn)
                {
                    unlimitedStamina = _checked;
                }
                // Join / Quit Notifications
                else if (checkbox == joinleavenotifications)
                {
                    joinnotif = _checked;
                }
                // Death Notifications
                else if (checkbox == deathnotifications)
                {
                    deathnotif = _checked;
                }
            };

            // If the menu closes, go back and re-open the main menu.
            submenu.OnMenuClose += (sender) =>
            {
                mainMenu.Visible = true;
            };

            // Add the submenu to the menu pool.
            _menuPool.Add(submenu);

            // Create new button for the "Player Options" menu.
            var playerOptions = new UIMenuItem(playerOptionsText, GetLabelText("PIM_HSTYL"));
            // Add the new button to the main menu.
            menu.AddItem(playerOptions);
            // Bind this submenu to the playerOptions button in the main menu.
            menu.BindMenuToItem(submenu, playerOptions);

            // Update the submenu selected item index so it starts at the top.
            submenu.RefreshIndex();
        }
        #endregion
        #region Online Players Menu
        private void CreateOnlinePlayers()
        {
            // Create the submenu.
            UIMenu submenu = new UIMenu("Online Players", "Currently connected players");
            // Add the submenu to the menu pool.
            _menuPool.Add(submenu);

            // Set the sub menu banner type.
            submenu.SetBannerType(banner);

            #region If the submenu is opened, refresh the playerlist
            mainMenu.OnMenuChange += (oldMenu, newMenu, forward) =>
            {
                if (newMenu == submenu)
                {
                    //Notify(submenu.MenuItems.Count().ToString());
                    if (submenu.MenuItems.Count() > 0)
                    {
                        for (var i = submenu.MenuItems.Count(); i > 0; i--)
                        {
                            submenu.RemoveItemAt(i - 1);
                        }
                    }
                    for (var i = 0; i < 32; i++)
                    {
                        if (NetworkIsPlayerActive(i))
                        {
                            var userMenu = new UIMenu(GetPlayerName(i), i + " - " + GetPlayerName(i));

                            _menuPool.Add(userMenu);
                            ///var userMenu = _menuPool.AddSubMenu(submenu, GetPlayerName(i));
                            userMenu.SetBannerType(banner);
                            var spectate = new UIMenuItem("Spectate Player", "Spectate this player.");
                            var teleport = new UIMenuItem("Teleport To Player", "Teleport to this player.");
                            var waypoint = new UIMenuItem("Set Waypoint", "Set a waypoint to this player on your map.");
                            var kick = new UIMenuItem("~r~Kick Player", "Kick this player from the server.");
                            kick.SetRightBadge(UIMenuItem.BadgeStyle.Alert);

                            if (!permissions["onlinePlayers_spectate"])
                            {
                                spectate.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            }
                            if (!permissions["onlinePlayers_teleport"])
                            {
                                teleport.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            }
                            if (!permissions["onlinePlayers_waypoint"])
                            {
                                waypoint.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            }
                            if (!permissions["onlinePlayers_kick"])
                            {
                                kick.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            }
                            // Add the menu items
                            userMenu.AddItem(spectate);
                            userMenu.AddItem(teleport);
                            userMenu.AddItem(waypoint);
                            userMenu.AddItem(kick);

                            // When an item is clicked...
                            userMenu.OnItemSelect += async (sender, item, index) =>
                            {
                                //foreach (KeyValuePair<string, bool> test in permissions)
                                //{
                                //    Notify(test.Key);
                                //}
                                int playerId = int.Parse(userMenu.Subtitle.Caption.ToString().Substring(0, 1));
                                if (item == spectate)
                                {
                                    if (this.permissions["onlinePlayers_spectate"])
                                    {
                                        if (playerId == PlayerId())
                                        {
                                            Notify("~r~That's a no, sorry. ~w~You can't spectate yourself!");
                                        }
                                        else
                                        {
                                            if (NetworkIsInSpectatorMode())
                                            {
                                                DoScreenFadeOut(200);
                                                await Delay(200);
                                                NetworkSetInSpectatorMode(false, GetPlayerPed(playerId));
                                                DoScreenFadeIn(200);
                                                await Delay(200);
                                                spectatingId = -1;
                                            }
                                            else
                                            {
                                                DoScreenFadeOut(200);
                                                await Delay(200);
                                                NetworkSetInSpectatorMode(true, GetPlayerPed(playerId));
                                                spectatingId = playerId;
                                                DoScreenFadeIn(200);
                                                await Delay(200);
                                                Notify("Currently Spectating: ~b~" + GetPlayerName(spectatingId) + " [" + spectatingId + "]~w~.");
                                            }
                                        }

                                    }
                                    else
                                    {
                                        Notify("~r~Sorry! ~w~You are not allowed to spectate this player.");
                                    }
                                }
                                if (item == teleport)
                                {
                                    if (this.permissions["onlinePlayers_teleport"])
                                    {
                                        if (playerId != PlayerId())
                                        {
                                            var pos = GetEntityCoords(GetPlayerPed(playerId), true);
                                            SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, pos.Z + 1f);
                                        }
                                        else
                                        {
                                            Notify("~r~Error: ~w~You can't teleport to yourself!");
                                        }
                                    }
                                    else
                                    {
                                        Notify("~r~Sorry! ~w~You are not allowed to teleport to this player.");
                                    }
                                }
                                if (item == waypoint)
                                {

                                    if (this.permissions["onlinePlayers_waypoint"])
                                    {
                                        if (playerId != PlayerId())
                                        {
                                            var pos = GetEntityCoords(GetPlayerPed(playerId), true);
                                            SetNewWaypoint(pos.X, pos.Y);
                                        }
                                        else
                                        {
                                            Notify("~r~Error: ~w~You can't set a waypoint to yourself!");
                                        }
                                    }
                                    else
                                    {
                                        Notify("~r~Sorry! ~w~You are not allowed to set a waypoint to this player.");
                                    }
                                }
                                if (item == kick)
                                {
                                    if (this.permissions["onlinePlayers_kick"])
                                    {
                                        userMenu.Visible = false;
                                        var kickMessage = await Game.GetUserInput("You have been kicked from this server.", 200);
                                        if (kickMessage == "" || kickMessage == null)
                                        {
                                            kickMessage = "You have been kicked from this server.";
                                        }
                                        TriggerServerEvent("vMenu:KickPlayer", GetPlayerServerId(playerId), kickMessage);
                                        userMenu.Visible = true;
                                        userMenu.GoBack();
                                    }
                                    else
                                    {
                                        Notify("~r~Sorry! ~w~You are not allowed to kick this player.");
                                    }
                                }
                            };

                            userMenu.OnMenuClose += (sender) =>
                            {
                                submenu.Visible = true;
                            };

                            var button = new UIMenuItem(GetPlayerName(i), "");
                            submenu.AddItem(button);
                            submenu.BindMenuToItem(userMenu, button);
                        }
                    }
                }
            };
            #endregion

            // If the submenu is closed, open the main menu.
            submenu.OnMenuClose += (sender) =>
            {
                mainMenu.Visible = true;
            };

            // Create + Add the PlayerOptions button to the main menu.
            var playerOptions = new UIMenuItem("Online Players", "All players currently playing on this server.");
            mainMenu.AddItem(playerOptions);
            // Bind the submenu to the playerOptions button.
            mainMenu.BindMenuToItem(submenu, playerOptions);

        }
        #endregion
        #region Vehicle Options Menu
        private void CreateVehicleOptions(UIMenu mainmenu)
        {
            // Make a new submenu for the vehicle options.
            var submenu = new UIMenu("", vehicleOptionsText); // Localized Titles: Vehicle, Options

            // Set banner to the Los Santos Customs image.
            submenu.SetBannerType(new Sprite("shopui_title_carmod", "shopui_title_carmod", new System.Drawing.PointF(0f, 0f), new System.Drawing.SizeF(0f, 0f)));

            // Create vehicle options checkboxes.
            UIMenuCheckboxItem vehgod = new UIMenuCheckboxItem("Vehicle Godmode", false, "If you enable vehicle god mode, your vehicle can't take any visual or physical damage.");

            // Create new buttons.
            UIMenuItem fixCar = new UIMenuItem("Fix vehicle", "Fix and clean your vehicle.");
            UIMenuItem cleanCar = new UIMenuItem("Clean Vehicle", "Wash your vehicle.");
            UIMenuItem deleteCar = new UIMenuItem("Delete Vehicle", "Delete your vehicle.");
            deleteCar.SetRightBadge(UIMenuItem.BadgeStyle.Alert);

            // Add the buttons/checkboxes to the menu.
            submenu.AddItem(vehgod);
            submenu.AddItem(fixCar);
            submenu.AddItem(cleanCar);
            submenu.AddItem(deleteCar);

            submenu.OnCheckboxChange += (sender, checkbox, _checked) =>
            {
                if (checkbox == vehgod)
                {
                    vehicleGodMode = _checked;
                }
            };

            // Event handler for when the buttons are pressed.
            submenu.OnItemSelect += (sender, item, index) =>
            {
                // Fix car
                if (item == fixCar)
                {
                    FixCar();
                }
                // Clean car
                else if (item == cleanCar)
                {
                    CleanCar();
                }
                // Delete car
                else if (item == deleteCar)
                {
                    DeleteCar();
                }
            };

            submenu.OnMenuClose += (sender) =>
            {
                mainMenu.Visible = true;
            };

            _menuPool.Add(submenu);
            var vehOptionsBtn = new UIMenuItem(vehicleOptionsText, "Repair, clean, upgrade and pimp your vehicles here.");
            mainMenu.AddItem(vehOptionsBtn);
            mainmenu.BindMenuToItem(submenu, vehOptionsBtn);
        }
        #endregion
        #region Voice Chat Menu
        private void CreateVoiceChat()
        {
            UIMenu submenu = new UIMenu("Voice Chat", "Voice Chat Options");
            submenu.OnMenuClose += (sender) =>
            {
                mainMenu.Visible = true;
            };

            UIMenuItem voicechat = new UIMenuItem("Voice Chat Options", "Here you can configure Voice Chat options, such as the channel you're in or the Voice Chat Proximity.");
            mainMenu.AddItem(voicechat);
            mainMenu.BindMenuToItem(submenu, voicechat);
            _menuPool.Add(submenu);
        }
        #endregion

        #region Weather Menu
        private void CreateWeatherMenu()
        {
            UIMenu submenu = new UIMenu(weatherMenuTitle, "Weather Options");
            submenu.SetBannerType(banner);
            var extraSunny = new UIMenuItem(extraSunnyWeather, "Set the weather to extra sunny.");
            UIMenuItem clear = new UIMenuItem(clearWeather, "Set the weather to clear.");
            UIMenuItem clouds = new UIMenuItem(cloudsWeather, "Set the weather to clouds.");
            UIMenuItem overcast = new UIMenuItem(overcastWeather, "Set the weather to overcast.");
            UIMenuItem clearing = new UIMenuItem(clearingWeather, "Set the weather to clearing.");
            UIMenuItem rain = new UIMenuItem(rainWeather, "Set the weather to rain.");
            UIMenuItem thunder = new UIMenuItem(thunderWeather, "Set the weather to thunder.");
            UIMenuItem smog = new UIMenuItem(smogWeather, "Set the weather to smog.");
            UIMenuItem foggy = new UIMenuItem(foggyWeather, "Set the weather to foggy.");
            UIMenuItem snow = new UIMenuItem(snowWeather, "Set the weather to snow.");
            UIMenuItem snowlight = new UIMenuItem(snowLightWeather, "Set the weather to snowlight.");
            UIMenuItem blizzard = new UIMenuItem(blizzardWeather, "Set the weather to blizzard.");
            UIMenuItem xmas = new UIMenuItem(xmasWeather, "Set the weather to xmas.");
            UIMenuItem halloween = new UIMenuItem(halloweenWeather, "Set the weather to halloween.");
            UIMenuItem neutral = new UIMenuItem(neutralWeather, "Set the weather to neutral.");

            submenu.AddItem(extraSunny);
            submenu.AddItem(clear);
            submenu.AddItem(clouds);
            submenu.AddItem(overcast);
            submenu.AddItem(clearing);
            submenu.AddItem(rain);
            submenu.AddItem(thunder);
            submenu.AddItem(smog);
            submenu.AddItem(foggy);
            submenu.AddItem(snow);
            submenu.AddItem(snowlight);
            submenu.AddItem(blizzard);
            submenu.AddItem(xmas);
            submenu.AddItem(halloween);
            submenu.AddItem(neutral);

            submenu.RefreshIndex();
            submenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == extraSunny)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "EXTRASUNNY");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + extraSunnyWeather);
                }
                else if (item == clear)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "CLEAR");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + clearWeather);
                }
                else if (item == clouds)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "CLOUDS");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + cloudsWeather);
                }
                else if (item == overcast)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "OVERCAST");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + overcastWeather);
                }
                else if (item == clearing)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "CLEARING");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + clearingWeather);
                }
                else if (item == rain)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "RAIN");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + rainWeather);
                }
                else if (item == thunder)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "THUNDER");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + thunderWeather);
                }
                else if (item == smog)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "SMOG");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + smogWeather);
                }
                else if (item == foggy)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "FOGGY");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + foggyWeather);
                }
                else if (item == snow)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "SNOW");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + snowWeather);
                }
                else if (item == snowlight)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "SNOWLIGHT");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + snowLightWeather);
                }
                else if (item == blizzard)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "BLIZZARD");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + blizzardWeather);
                }
                else if (item == xmas)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "XMAS");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + xmasWeather);
                }
                else if (item == halloween)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "HALLOWEEN");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + halloweenWeather);
                }
                else if (item == neutral)
                {
                    TriggerServerEvent("vMenu:UpdateWeather", "NEUTRAL");
                    Notify("Please wait 15 seconds while the almighty ~g~Snail ~w~processes your request for ~y~" + neutralWeather);
                }
            };

            submenu.OnMenuClose += (sender) =>
            {
                mainMenu.Visible = true;
            };

            _menuPool.Add(submenu);
            var weatherOptions = new UIMenuItem(weatherMenuTitle, "Change weather options.");
            mainMenu.AddItem(weatherOptions);
            mainMenu.BindMenuToItem(submenu, weatherOptions);
            _menuPool.RefreshIndex();
        }
        #endregion
        #region Time Menu
        private void CreateTimeMenu()
        {
            UIMenu submenu = new UIMenu(timeMenuTitle, "Change The Time Of Day");

            UIMenuItem earlyMorning = new UIMenuItem("Early Morning", "Set the time to 06:00 AM.");
            UIMenuItem morning = new UIMenuItem("Morning", "Set the time to 09:00 AM.");
            UIMenuItem noon = new UIMenuItem("Noon", "Set the time to 12:00 PM.");
            UIMenuItem earlyAfternoon = new UIMenuItem("Early Afternoon", "Set the time to 3:00 PM.");
            UIMenuItem afternoon = new UIMenuItem("Afternoon", "Set the time to 6:00 PM.");
            UIMenuItem evening = new UIMenuItem("Evening", "Set the time to 9:00 PM.");
            UIMenuItem midnight = new UIMenuItem("Midnight", "Set the time to 12:00 AM.");
            UIMenuItem night = new UIMenuItem("Night", "Set the time to 3:00 AM.");

            submenu.SetBannerType(banner);
            submenu.AddItem(earlyMorning);
            submenu.AddItem(morning);
            submenu.AddItem(noon);
            submenu.AddItem(earlyAfternoon);
            submenu.AddItem(afternoon);
            submenu.AddItem(evening);
            submenu.AddItem(midnight);
            submenu.AddItem(night);

            submenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == earlyMorning)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 6, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~6:00 AM~w~.");
                }
                else if (item == morning)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 9, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~9:00 AM~w~.");
                }
                else if (item == noon)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 12, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~12:00 PM~w~.");
                }
                else if (item == earlyAfternoon)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 15, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~3:00 PM~w~.");
                }
                else if (item == afternoon)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 18, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~6:00 PM~w~.");
                }
                else if (item == evening)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 21, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~9:00 PM~w~.");
                }
                else if (item == midnight)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 0, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~12:00 AM~w~.");
                }
                else if (item == night)
                {
                    TriggerServerEvent("vMenu:UpdateTime", 3, 0);
                    Notify("Begging the ~g~almighty Snail ~w~to change the time to ~r~3:00 AM~w~.");
                }
            };

            submenu.OnMenuClose += (sender) =>
            {
                mainMenu.Visible = true;
            };

            var timeButton = new UIMenuItem(timeMenuTitle, "Change The Time Of Day.");
            mainMenu.AddItem(timeButton);
            submenu.RefreshIndex();
            _menuPool.Add(submenu);
            _menuPool.RefreshIndex();
            mainMenu.BindMenuToItem(submenu, timeButton);

        }
        #endregion

        #region common vehicle functions
        #region Fix vehicle
        /// <summary>
        /// Fix the vehicle the player is in.
        /// </summary>
        private void FixCar()
        {
            // In vehicle
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                int veh = GetVehiclePedIsIn(PlayerPedId(), false);
                if (DoesEntityExist(veh) && !IsEntityDead(veh))
                {
                    SetVehicleFixed(veh);
                    Subtitle("Your ~g~" + GetLabelText(GetDisplayNameFromVehicleModel(modelHash: (uint)GetEntityModel(veh))) + " ~w~has been repaired.");
                }
            }
            // Not in vehicle
            else
            {
                Notify("~r~Error: ~w~You are not inside a vehicle.", false);
            }
        }

        /// <summary>
        /// Fix the car specified by p0.
        /// </summary>
        /// <param name="vehicle"></param>
        private void FixCar(int vehicle)
        {
            if (DoesEntityExist(vehicle) && !IsEntityDead(vehicle))
            {
                SetVehicleFixed(vehicle);
                Subtitle("Your ~g~" + GetLabelText(GetDisplayNameFromVehicleModel(modelHash: (uint)GetEntityModel(vehicle))) + " ~w~has been repaired.");
            }
        }
        #endregion
        #region Clean vehicle
        /// <summary>
        /// Clean the vehicle the player is in.
        /// </summary>
        private void CleanCar()
        {
            // In vehicle
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                int veh = GetVehiclePedIsIn(PlayerPedId(), false);
                WashDecalsFromVehicle(veh, 10f);
                SetVehicleDirtLevel(veh, 0f);
                Subtitle("Your ~g~" + GetLabelText(GetDisplayNameFromVehicleModel(modelHash: (uint)GetEntityModel(veh))) + " ~w~has been cleaned.");
            }
            // Not in vehicle
            else
            {
                Notify("~r~Error: ~w~You are not inside a vehicle.");
            }
        }

        /// <summary>
        /// Clean the vehicle specified by p0.
        /// </summary>
        /// <param name="vehicle"></param>
        private void CleanCar(int vehicle)
        {
            if (DoesEntityExist(vehicle) && !IsEntityDead(vehicle))
            {
                WashDecalsFromVehicle(vehicle, 10f);
                SetVehicleDirtLevel(vehicle, 0f);
                Subtitle("Your ~g~" + GetLabelText(GetDisplayNameFromVehicleModel(modelHash: (uint)GetEntityModel(vehicle))) + " ~w~has been cleaned.");
            }
        }
        #endregion
        #region Spawn Vehicle
        /// <summary>
        /// Spawn a vehicle in front of the player by passing the vehicle's model name as p0.
        /// </summary>
        /// <param name="vehicleName"></param>
        private async void SpawnVehicleAsync(string vehicleName)
        {
            uint model = (uint)(int)GetHashKey(vehicleName);
            if (IsModelInCdimage(model))
            {
                RequestModel(model);
                while (!HasModelLoaded(model))
                {
                    await Delay(0);
                }
                //var pos = GetEntityCoords(PlayerPedId(), true);
                var vehclass = GetVehicleClassFromName(model);
                var pos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0f, 8f, 0f);
                var heading = GetEntityHeading(PlayerPedId());
                var spawnInside = true;

                // If the vehicle is utility class (possibly trailers) or a trains class then we don't want the player to be teleported
                // Inside the vehicle, and we also rotate the vehicle 90 degrees so it doesn't kill the player by spawning on top of the player. 
                // (in case it's a very long vehicle)
                if (vehclass == (int)VehicleClass.Utility || vehclass == (int)VehicleClass.Trains)
                {
                    heading += 90f;
                    spawnInside = false;
                }
                var veh = CreateVehicle(model, pos.X, pos.Y, pos.Z, heading, true, false);
                SetEntityAsMissionEntity(veh, true, true);
                SetVehicleOnGroundProperly(veh);
                SetVehicleNeedsToBeHotwired(veh, false);
                if (IsThisModelAHeli(model))
                {
                    SetHeliBladesFullSpeed(veh);
                }
                // Should the player be spawned/teleported inside the vehicle?
                if (spawnInside)
                {
                    TaskWarpPedIntoVehicle(PlayerPedId(), veh, -1);
                }
            }
            else
            {
                Notify("~r~Error: ~w~This model (~r~" + vehicleName + "~w~) could not be found, are you sure it's valid?");
            }
        }
        #endregion
        #region Delete Vehicle
        /// <summary>
        /// Delete the vehicle the player's currently in.
        /// </summary>
        private void DeleteCar()
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                int veh = GetVehiclePedIsIn(PlayerPedId(), false);
                if (GetPedInVehicleSeat(veh, -1) == PlayerPedId())
                {
                    SetEntityAsMissionEntity(veh, false, false);
                    DeleteVehicle(ref veh);
                    Notify("The almighty ~g~Snail ~w~cleaned up your mess!");
                }
                else
                {
                    Notify("~r~Error: ~w~You need to be in the driver's seat to delete a vehicle!");
                }
            }
            else
            {
                Notify("~r~Error: ~w~You're not inside a vehicle!");
            }
        }
        /// <summary>
        /// Delete the vehicle specified by p0.
        /// </summary>
        /// <param name="vehicle"></param>
        private void DeleteCar(int vehicle)
        {

        }
        #endregion

        #endregion
        #region common general functions
        /// <summary>
        /// Show a notification above the minimap.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="blink"></param>
        public void Notify(string message, bool blink = false)
        {
            CitizenFX.Core.UI.Screen.ShowNotification(message, blink);
        }
        /// <summary>
        /// Show subtitle
        /// </summary>
        /// <param name="message"></param>
        /// <param name="duration"></param>
        private void Subtitle(string message, int duration = 2500)
        {
            CitizenFX.Core.UI.Screen.ShowSubtitle(message, duration);
        }

        /// <summary>
        /// Show help message.
        /// </summary>
        /// <param name="message">Message, max 299 characters long.</param>
        /// <param name="sound">Play notification sound.</param>
        /// <param name="duration">Help message duration (in ms).</param>
        private void Help(string message, bool sound = false, int duration = 5000)
        {
            BeginTextCommandDisplayHelp("THREESTRINGS");
            AddTextComponentSubstringPlayerName(message);
            EndTextCommandDisplayHelp(0, false, sound, duration);
        }
        #endregion
    }
}
