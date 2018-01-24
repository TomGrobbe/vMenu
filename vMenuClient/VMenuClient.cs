using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public class VMenuClient : BaseScript
    {
        #region variables
        private MenuPool _menuPool;
        private UIMenu mainMenu;
        private bool firstTick = true;
        private string lastVehicleSpawned = "Adder";
        private bool vehicleGodMode = false;
        private bool playerGodMode = false;
        private bool neverWanted = false;
        private bool superJump = false;
        private bool unlimitedStamina = true;
        private static VehiclesList vehiclesList = new VehiclesList();
        private string playerInvincibleText = GetLabelText("BLIP_6") + " " + GetLabelText("CHEAT_INVINCIBILITY_OFF"); // Player Invincibility
        private string playerOptionsText = GetLabelText("BLIP_6") + " " + GetLabelText("PM_MP_OPTIONS"); // Player Options
        private string vehicleOptionsText = GetLabelText("collision_w0oam1") + " " + GetLabelText("PM_MP_OPTIONS"); // Vehicle Options
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
            Tick += OnTick;
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
                await CreateMainMenu();
                ClearBrief();
                ShowHelp();
                EventHandlers.Add("playerSpawned", new Action(ShowHelp));
                await Delay(0);
            }
            #endregion

            // Process all menus.
            _menuPool.ProcessMenus();

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
                        PlaySoundFrontend(-1, mainMenu.AUDIO_SELECT, mainMenu.AUDIO_LIBRARY, false);
                        break;
                    }
                }
            }
            // Handle opening/closing of the menu for keyboard/mouse.
            if (IsControlJustPressed(0, (int)Control.InteractionMenu) && IsInputDisabled(2))
            {
                mainMenu.Visible = !_menuPool.IsAnyMenuOpen();
                PlaySoundFrontend(-1, mainMenu.AUDIO_SELECT, mainMenu.AUDIO_LIBRARY, false);
            }
            #endregion

            #region disable buttons when menu is open, also close all menu's if the game is paused.
            // If any menu is open, run the following.
            if (_menuPool.IsAnyMenuOpen())
            {
                // If keyboard/mouse imput > disable looking left/right because we don't want the camera to freak out.
                if (IsInputDisabled(2))
                {
                    DisableControlAction(0, (int)Control.LookLeftRight, true);
                    DisableControlAction(0, (int)Control.LookUpDown, true);
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
                CitizenFX.Core.UI.Screen.DisplayHelpTextThisFrame("Press ~INPUT_INTERACTION_MENU~ to open the interaction menu.");
            }
            else
            {
                CitizenFX.Core.UI.Screen.DisplayHelpTextThisFrame("Press and hold down ~INPUT_INTERACTION_MENU~ to open the interaction menu.");
            }
        }
        #endregion

        #region main menu
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
            // When the username is valid, create the new menu.
            mainMenu = new UIMenu(playerName, "Main Menu");

            

            // Add the main menu to the _menuPool.
            _menuPool.Add(mainMenu);

            // Add Player Options.
            CreatePlayerOptions(mainMenu);
            // Add vehicle options.
            CreateVehicleOptions(mainMenu);
            // Add car spawn menu.
            CreateVehicleSpawn(mainMenu);

            // Apply the banner to the main menu.
            mainMenu.SetBannerType(banner);

            // Allow all buttons to function when the user has the menu open.
            _menuPool.ControlDisablingEnabled = false;

            // Broken setting?
            _menuPool.ResetCursorOnOpen = true;

            // Refresh the index so the menu opens at the top.
            _menuPool.RefreshIndex();
        }
        #endregion

        #region vehilce spawn menu
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
            var carSpawnMenuBtn = new UIMenuItem(GetLabelText("BLIP_125") + " Menu", "Vehicle Spawn Menu."); // Localized Title: Spawn vehicle

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
                        CitizenFX.Core.UI.Screen.ShowNotification("~r~Error: ~w~You did not provide a vehicle name.", true);
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
        
        #region Spawn Vehicle function
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
                CitizenFX.Core.UI.Screen.ShowNotification("~r~Error: ~w~This model (~r~" + vehicleName + "~w~) could not be found, are you sure it's valid?");
            }
        }
        #endregion

        #region Player Options
        private void CreatePlayerOptions(UIMenu menu)
        {
            // Create submenu.
            var submenu = new UIMenu(GetPlayerName(PlayerId()), playerOptionsText);
            submenu.SetBannerType(banner);

            // Create checkboxes for the main player toggle options.
            var godmodebtn = new UIMenuCheckboxItem(playerInvincibleText, false, "Toggle Player Invincibility.");
            var neverwantedbtn = new UIMenuCheckboxItem("Never Wanted", false, "Toggle Never Wanted.");
            var fastrunbtn = new UIMenuCheckboxItem("Fast Run", false, "Enable super walk/run speeds for your player.");
            var superjumpbtn = new UIMenuCheckboxItem("Super Jump", false, "Enable super jump for your player.");
            var unlimstaminabtn = new UIMenuCheckboxItem("Unlimited Stamina", true, "Enable/disable unlimited stamina for your player. It's recommended to leave this enabled.");

            // Add checkboxes to the submenu.
            submenu.AddItem(godmodebtn);
            submenu.AddItem(neverwantedbtn);
            submenu.AddItem(fastrunbtn);
            submenu.AddItem(superjumpbtn);
            submenu.AddItem(unlimstaminabtn);

            // Handle checkbox changes.
            submenu.OnCheckboxChange += (sender, checkbox, _checked) =>
            {
                if (checkbox == godmodebtn)
                {
                    playerGodMode = _checked;
                }
                else if (checkbox == neverwantedbtn)
                {
                    neverWanted = _checked;
                }
                else if (checkbox == fastrunbtn)
                {
                    if (_checked)
                    {
                        SetRunSprintMultiplierForPlayer(PlayerId(), 1.49f);
                    }
                    else
                    {
                        SetRunSprintMultiplierForPlayer(PlayerId(), 1.0f);
                    }
                }
                else if (checkbox == superjumpbtn)
                {
                    superJump = _checked;
                }
                else if (checkbox == unlimstaminabtn)
                {
                    unlimitedStamina = _checked;
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

        #region vehicle options
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

            // Add the buttons/checkboxes to the menu.
            submenu.AddItem(vehgod);
            submenu.AddItem(fixCar);
            submenu.AddItem(cleanCar);

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
                    // In vehicle
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        int veh = GetVehiclePedIsIn(PlayerPedId(), false);
                        SetVehicleFixed(veh);
                        CitizenFX.Core.UI.Screen.ShowSubtitle("Your ~r~" + GetLabelText(GetDisplayNameFromVehicleModel(modelHash: (uint)GetEntityModel(veh))) + " ~w~has been repaired.");
                    }
                    // Not in vehicle
                    else
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification("~r~Error: ~w~You are not inside a vehicle.");
                    }
                }
                // Clean car
                else if (item == cleanCar)
                {
                    // In vehicle
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        int veh = GetVehiclePedIsIn(PlayerPedId(), false);
                        WashDecalsFromVehicle(veh, 10.0f);
                        SetVehicleDirtLevel(veh, 0.0f);
                        CitizenFX.Core.UI.Screen.ShowSubtitle("Your ~r~" + GetLabelText(GetDisplayNameFromVehicleModel(modelHash: (uint)GetEntityModel(veh))) + " ~w~has been cleaned.");
                    }
                    // Not in vehicle
                    else
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification("~r~Error: ~w~You are not inside a vehicle.");
                    }
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

    }
}
