using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using System.Dynamic;

namespace vMenuClient
{
    public class MainMenu : BaseScript
    {
        #region Variables
        // Function Variables
        public static CommonFunctions Cf { get; } = new CommonFunctions();

        public static MenuPool Mp { get; } = new MenuPool();

        private bool firstTick = true;
        private static bool permissionsSetupDone = false;
        private static bool optionsSetupDone = false;
        public static bool addonCarsLoaded = false;
        public static bool addonPedsLoaded = false;
        public static bool addonWeaponsLoaded = false;

        private static int MenuToggleKey = 244; // M by default (InteractionMenu)
        private static int NoClipKey = 289; // F2 by default (ReplayStartStopRecordingSecondary)
        public static UIMenu Menu { get; private set; }

        public static PlayerOptions PlayerOptionsMenu { get; private set; }
        public static OnlinePlayers OnlinePlayersMenu { get; private set; }
        public static BannedPlayers BannedPlayersMenu { get; private set; }
        public static SavedVehicles SavedVehiclesMenu { get; private set; }
        public static VehicleOptions VehicleOptionsMenu { get; private set; }
        public static VehicleSpawner VehicleSpawnerMenu { get; private set; }
        public static PlayerAppearance PlayerAppearanceMenu { get; private set; }
        public static TimeOptions TimeOptionsMenu { get; private set; }
        public static WeatherOptions WeatherOptionsMenu { get; private set; }
        public static WeaponOptions WeaponOptionsMenu { get; private set; }
        public static MiscSettings MiscSettingsMenu { get; private set; }
        public static VoiceChat VoiceChatSettingsMenu { get; private set; }
        public static About AboutMenu { get; private set; }
        public static UIMenu NoClipMenu { get; } = new NoclipMenu().GetMenu();
        public static bool NoClipEnabled { get; set; } = false;

        // Only used when debugging is enabled:
        private BarTimerBar bt = new BarTimerBar("Opening Menu");

        public static bool DebugMode = GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true" ? true : false;
        public static bool EnableExperimentalFeatures = (GetResourceMetadata(GetCurrentResourceName(), "experimental_features_enabled", 0) ?? "0") == "1";
        public static bool DontOpenMenus { get; set; } = false;
        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        public static Dictionary<string, string> MenuOptions { get; private set; }

        public static bool DisableControls { get; set; } = false;
        private UIMenu currentMenu = null;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenu()
        {
            RegisterCommand("vmenuclient", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
            {
                if (args != null)
                {
                    if (args.Count > 0)
                    {
                        if (args[0].ToString().ToLower() == "debug")
                        {
                            DebugMode = !DebugMode;
                            Notify.Custom($"Debug mode is now set to: {DebugMode}.");
                        }
                    }
                    else
                    {
                        Notify.Custom($"vMenu is currently running version: {Version}.");
                    }
                }
            }), false);

            // Set discord rich precense once, allowing it to be overruled by other resources once those load.
            if (DebugMode)
            {
                SetRichPresence($"Debugging vMenu {Version}!");
            }

            if (GetCurrentResourceName() != "vMenu")
            {
                Exception InvalidNameException = new Exception("\r\n\r\n[vMenu] INSTALLATION ERROR!\r\nThe name of the resource is not valid. Please change the folder name from '" + GetCurrentResourceName() + "' to 'vMenu' (case sensitive) instead!\r\n\r\n\r\n");
                try
                {
                    throw InvalidNameException;
                }
                catch (Exception e)
                {
                    Cf.Log(e.Message);
                }
                TriggerEvent("chatMessage", "^3IMPORTANT: vMenu IS NOT SETUP CORRECTLY. PLEASE CHECK THE SERVER LOG FOR MORE INFO.");
            }
            else
            {
                Tick += OnTick;
                Tick += ProcessMainButtons;
                Tick += ProcessDirectionalButtons;
            }

        }

        #region Set Permissions function
        /// <summary>
        /// Set the permissions for this client.
        /// </summary>
        /// <param name="dict"></param>
        public static void SetPermissions(dynamic dict)
        {
            // Loop through the dynamic object and get the keys and values.
            foreach (dynamic permission in dict)
            {
                // Add the new permission to the dictionary.
                PermissionsManager.SetPermission(permission.Key.ToString(), permission.Value);
            }
            Cf.Log(JsonConvert.SerializeObject(PermissionsManager.Permissions).ToString());

            permissionsSetupDone = true;
            VehicleSpawner.allowedCategories = new List<bool>()
            {
                Cf.IsAllowed(Permission.VSCompacts),
                Cf.IsAllowed(Permission.VSSedans),
                Cf.IsAllowed(Permission.VSSUVs),
                Cf.IsAllowed(Permission.VSCoupes),
                Cf.IsAllowed(Permission.VSMuscle),
                Cf.IsAllowed(Permission.VSSportsClassic),
                Cf.IsAllowed(Permission.VSSports),
                Cf.IsAllowed(Permission.VSSuper),
                Cf.IsAllowed(Permission.VSMotorcycles),
                Cf.IsAllowed(Permission.VSOffRoad),
                Cf.IsAllowed(Permission.VSIndustrial),
                Cf.IsAllowed(Permission.VSUtility),
                Cf.IsAllowed(Permission.VSVans),
                Cf.IsAllowed(Permission.VSCycles),
                Cf.IsAllowed(Permission.VSBoats),
                Cf.IsAllowed(Permission.VSHelicopters),
                Cf.IsAllowed(Permission.VSPlanes),
                Cf.IsAllowed(Permission.VSService),
                Cf.IsAllowed(Permission.VSEmergency),
                Cf.IsAllowed(Permission.VSMilitary),
                Cf.IsAllowed(Permission.VSCommercial),
                Cf.IsAllowed(Permission.VSTrains),
            };
        }
        #endregion

        #region set settings
        /// <summary>
        /// Sets the settings received from the server.
        /// </summary>
        /// <param name="options"></param>
        public static void SetOptions(dynamic options)
        {
            MenuOptions = new Dictionary<string, string>();
            foreach (dynamic option in options)
            {
                MenuOptions.Add(option.Key.ToString(), option.Value.ToString());
            }
            Cf.Log($"Settings loaded: {JsonConvert.SerializeObject(MenuOptions)}");

            MenuToggleKey = int.Parse(MenuOptions["menuKey"].ToString());
            NoClipKey = int.Parse(MenuOptions["noclipKey"].ToString());
            optionsSetupDone = true;
            if (MenuOptions.ContainsKey("disableSync"))
            {
                if (MenuOptions["disableSync"] == "true")
                {
                    EventManager.enableSync = false;
                }
            }
        }
        #endregion

        #region Process Menu Buttons
        /// <summary>
        /// Process the select & go back/cancel buttons.
        /// </summary>
        /// <returns></returns>
        private async Task ProcessMainButtons()
        {
            if (Mp.IsAnyMenuOpen())
            {
                currentMenu = Cf.GetOpenMenu();
                if (currentMenu != null && !DontOpenMenus && Mp.IsAnyMenuOpen() && !NoClipEnabled)
                {
                    if (currentMenu.Visible && !DisableControls)
                    {
                        // Select / Enter
                        if (Game.IsDisabledControlJustReleased(0, Control.FrontendAccept) || Game.IsControlJustReleased(0, Control.FrontendAccept))
                        {
                            if (currentMenu.MenuItems.Count() > 0)
                            {
                                currentMenu.SelectItem();
                            }
                        }
                        // Cancel / Go Back
                        else if (Game.IsDisabledControlJustReleased(0, Control.PhoneCancel))
                        {
                            // Wait for the next frame to make sure the "cinematic camera" button doesn't get "re-enabled" before the menu gets closed.
                            await Delay(0);
                            currentMenu.GoBack();
                        }
                    }
                }
                else
                {
                    await Delay(0);
                }
            }

        }

        /// <summary>
        /// Process left/right/up/down buttons (also holding down buttons will speed up after 3 iterations)
        /// </summary>
        /// <returns></returns>
        private async Task ProcessDirectionalButtons()
        {
            // Get the currently open menu.
            UIMenu currentMenu = Cf.GetOpenMenu();
            // If it exists.
            if (currentMenu != null && !DontOpenMenus && Mp.IsAnyMenuOpen() && !NoClipEnabled)
            {
                if (currentMenu.Visible && !DisableControls)
                {
                    // Check if the Go Up controls are pressed.
                    if (Game.IsDisabledControlJustPressed(0, Control.Phone) || Game.IsControlJustPressed(0, Control.SniperZoomInSecondary))
                    {
                        // Update the currently selected item to the new one.
                        currentMenu.GoUp();
                        currentMenu.GoUpOverflow();

                        // Get the current game time.
                        var time = GetGameTimer();
                        var times = 0;
                        var delay = 200;

                        // Do the following as long as the controls are being pressed.
                        while (Game.IsDisabledControlPressed(0, Control.Phone) && Cf.GetOpenMenu() != null)
                        {
                            // Update the current menu.
                            currentMenu = Cf.GetOpenMenu();

                            // Check if the game time has changed by "delay" amount.
                            if (GetGameTimer() - time > delay)
                            {
                                // Increment the "changed indexes" counter
                                times++;

                                // If the controls are still being held down after moving 3 indexes, reduce the delay between index changes.
                                if (times > 2)
                                {
                                    delay = 150;
                                }

                                // Update the currently selected item to the new one.
                                currentMenu.GoUp();
                                currentMenu.GoUpOverflow();

                                // Reset the time to the current game timer.
                                time = GetGameTimer();
                            }

                            // Wait for the next game tick.
                            await Delay(0);
                        }
                    }

                    // Check if the Go Left controls are pressed.
                    else if (Game.IsDisabledControlJustPressed(0, Control.PhoneLeft))
                    {
                        currentMenu.GoLeft();
                        var time = GetGameTimer();
                        var times = 0;
                        var delay = 200;
                        while (Game.IsDisabledControlPressed(0, Control.PhoneLeft) && Cf.GetOpenMenu() != null)
                        {
                            currentMenu = Cf.GetOpenMenu();
                            if (GetGameTimer() - time > delay)
                            {
                                times++;
                                if (times > 2)
                                {
                                    delay = 150;
                                }
                                currentMenu.GoLeft();
                                time = GetGameTimer();
                            }
                            await Delay(0);
                        }
                    }

                    // Check if the Go Right controls are pressed.
                    else if (Game.IsDisabledControlJustPressed(0, Control.PhoneRight))
                    {
                        currentMenu.GoRight();
                        var time = GetGameTimer();
                        var times = 0;
                        var delay = 200;
                        while ((Game.IsDisabledControlPressed(0, Control.PhoneRight) || Game.IsControlPressed(0, Control.PhoneRight)) && Cf.GetOpenMenu() != null)
                        {
                            currentMenu = Cf.GetOpenMenu();
                            if (GetGameTimer() - time > delay)
                            {
                                times++;
                                if (times > 2)
                                {
                                    delay = 150;
                                }
                                currentMenu.GoRight();
                                time = GetGameTimer();
                            }
                            await Delay(0);
                        }
                    }

                    // Check if the Go Down controls are pressed.
                    else if (Game.IsDisabledControlJustPressed(0, Control.PhoneDown) || Game.IsControlJustPressed(0, Control.SniperZoomOutSecondary))
                    {
                        currentMenu.GoDown();
                        currentMenu.GoDownOverflow();
                        var time = GetGameTimer();
                        var times = 0;
                        var delay = 200;
                        while (Game.IsDisabledControlPressed(0, Control.PhoneDown) && Cf.GetOpenMenu() != null)
                        {
                            currentMenu = Cf.GetOpenMenu();
                            if (GetGameTimer() - time > delay)
                            {
                                times++;
                                if (times > 2)
                                {
                                    delay = 150;
                                }
                                currentMenu.GoDown();
                                currentMenu.GoDownOverflow();
                                time = GetGameTimer();
                            }
                            await Delay(0);
                        }
                    }
                }
                else
                {
                    await Delay(0);
                }
            }
            else
            {
                await Delay(0);
            }
        }
        #endregion

        /// <summary>
        /// Main OnTick task runs every game tick and handles all the menu stuff.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            #region FirstTick
            // Only run this the first tick.
            if (firstTick)
            {
                firstTick = false;
                // Clear all previous pause menu info/brief messages on resource start.
                ClearBrief();

                // Request the permissions data from the server.
                TriggerServerEvent("vMenu:RequestPermissions", PlayerId());
                //TriggerServerEvent("vMenu:RequestBanList", PlayerId());

                // Wait until the data is received and the player's name is loaded correctly.
                while (!permissionsSetupDone || !optionsSetupDone
                    || GetPlayerName(PlayerId()) == "**Invalid**" || GetPlayerName(PlayerId()) == "** Invalid **" ||
                    !addonCarsLoaded || !addonPedsLoaded || !addonWeaponsLoaded)
                {
                    await Delay(0);
                }

                // Create the main menu.
                Menu = new UIMenu(GetPlayerName(PlayerId()), "Main Menu", true)
                {
                    ScaleWithSafezone = false,
                    MouseControlsEnabled = false,
                    MouseEdgeEnabled = false,
                    ControlDisablingEnabled = false
                };

                // Add the main menu to the menu pool.
                Mp.Add(Menu);

                Menu.RefreshIndex();
                Menu.UpdateScaleform();

                // Create all (sub)menus.
                CreateSubmenus();
            }
            #endregion


            // If the setup (permissions) is done, and it's not the first tick, then do this:
            if (permissionsSetupDone && optionsSetupDone && !firstTick)
            {
                #region Handle Opening/Closing of the menu.
                // If menus can be opened.
                if (!DontOpenMenus && !IsPauseMenuActive())
                {
                    // If the player is using Keyboard & Mouse and they pressed the M key (interaction menu button) then...
                    if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && (Game.IsControlJustPressed(0, (Control)MenuToggleKey) || Game.IsDisabledControlJustPressed(0, (Control)MenuToggleKey)))
                    {
                        // If any menu is already open: close all menus.
                        if (Mp.IsAnyMenuOpen())
                        {
                            Mp.CloseAllMenus();
                        }
                        // Otherwise: toggle the main menu (to be safe, only open it if no other menus are open.)
                        else
                        {
                            Menu.Visible = !Mp.IsAnyMenuOpen();
                        }
                    }

                    // If the player is using a controller, and no menus are currently open.
                    else if (!Mp.IsAnyMenuOpen() && Game.CurrentInputMode == InputMode.GamePad)
                    {
                        // Create a timer and set it to the current game timer value.
                        int timer = GetGameTimer();

                        // While (and only if) the player keeps using only the controller, and keeps holding down the interactionmenu button (select on controller).
                        while (Game.CurrentInputMode == InputMode.GamePad && Game.IsControlPressed(0, Control.InteractionMenu))
                        {
                            // If debugging is enabled, show the progress using a timerbar.
                            if (DebugMode)
                            {
                                bt.Draw(0);
                                float percent = ((GetGameTimer() - timer) / 350f);
                                bt.Percentage = percent;
                            }

                            // If 900ms in real time have passed.
                            if (GetGameTimer() - timer > 350)
                            {
                                Menu.Visible = !Mp.IsAnyMenuOpen();
                                // Break the loop (resetting the timer).
                                break;
                            }

                            // Wait for the next game tick. 
                            await Delay(0);
                        }
                    }

                    if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                    {
                        if (Game.IsControlJustPressed(0, (Control)NoClipKey) && Cf.IsAllowed(Permission.NoClip))
                        {
                            if (IsPedInAnyVehicle(PlayerPedId(), false))
                            {
                                if (GetPedInVehicleSeat(Cf.GetVehicle(), -1) == PlayerPedId())
                                {
                                    NoClipEnabled = !Mp.IsAnyMenuOpen();
                                }
                                else
                                {
                                    NoClipEnabled = false;
                                    Notify.Error("You need to be the driver of this vehicle to enable noclip!");
                                }
                            }
                            else
                            {
                                NoClipEnabled = !Mp.IsAnyMenuOpen();
                            }

                        }
                    }
                }
                // If the pause menu is active or all menus should be closed, close all menus.
                else
                {
                    await Delay(1);
                    Mp.CloseAllMenus();
                }
                #endregion

                // Menu toggle button.
                Game.DisableControlThisFrame(0, (Control)MenuToggleKey);

                #region Disable Inputs when any menu is open.
                if (Mp.IsAnyMenuOpen())
                {
                    // Close all menus when the player dies.
                    if (Game.PlayerPed.IsDead)
                    {
                        Mp.CloseAllMenus();
                    }

                    // Disable Gamepad/Controller Specific controls:
                    if (Game.CurrentInputMode == InputMode.GamePad)
                    {
                        Game.DisableControlThisFrame(0, Control.MultiplayerInfo);
                        // when in a vehicle.
                        if (IsPedInAnyVehicle(PlayerPedId(), false))
                        {
                            Game.DisableControlThisFrame(0, Control.VehicleHeadlight);
                            Game.DisableControlThisFrame(0, Control.VehicleDuck);
                        }
                    }
                    // Disable Shared Controls

                    // Radio Inputs
                    Game.DisableControlThisFrame(0, Control.RadioWheelLeftRight);
                    Game.DisableControlThisFrame(0, Control.RadioWheelUpDown);
                    Game.DisableControlThisFrame(0, Control.VehicleNextRadio);
                    Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);
                    Game.DisableControlThisFrame(0, Control.VehiclePrevRadio);

                    // Phone / Arrows Inputs
                    Game.DisableControlThisFrame(0, Control.Phone);
                    Game.DisableControlThisFrame(0, Control.PhoneCancel);
                    Game.DisableControlThisFrame(0, Control.PhoneDown);
                    Game.DisableControlThisFrame(0, Control.PhoneLeft);
                    Game.DisableControlThisFrame(0, Control.PhoneRight);

                    // Attack Controls
                    Game.DisableControlThisFrame(0, Control.Attack);
                    Game.DisableControlThisFrame(0, Control.Attack2);
                    Game.DisableControlThisFrame(0, Control.MeleeAttack1);
                    Game.DisableControlThisFrame(0, Control.MeleeAttack2);
                    Game.DisableControlThisFrame(0, Control.MeleeAttackAlternate);
                    Game.DisableControlThisFrame(0, Control.MeleeAttackHeavy);
                    Game.DisableControlThisFrame(0, Control.MeleeAttackLight);
                    Game.DisableControlThisFrame(0, Control.VehicleAttack);
                    Game.DisableControlThisFrame(0, Control.VehicleAttack2);
                    Game.DisableControlThisFrame(0, Control.VehicleFlyAttack);
                    Game.DisableControlThisFrame(0, Control.VehiclePassengerAttack);
                    Game.DisableControlThisFrame(0, Control.Aim);

                    // When in a vehicle
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        Game.DisableControlThisFrame(0, Control.VehicleSelectNextWeapon);
                        Game.DisableControlThisFrame(0, Control.VehicleSelectPrevWeapon);
                        Game.DisableControlThisFrame(0, Control.VehicleCinCam);
                    }
                }
                #endregion

                // Process the menu. Draw it and reset the menu width offset to make sure any newly generated menus always have the right width offset.
                Mp.WidthOffset = 50;
                if (Mp.IsAnyMenuOpen())
                {
                    Mp.Draw();
                }
            }
        }

        #region Add Menu Function
        /// <summary>
        /// Add the menu to the menu pool and set it up correctly.
        /// Also add and bind the menu buttons.
        /// </summary>
        /// <param name="submenu"></param>
        /// <param name="menuButton"></param>
        private void AddMenu(UIMenu submenu, UIMenuItem menuButton)
        {
            Menu.AddItem(menuButton);
            Menu.BindMenuToItem(submenu, menuButton);
            Mp.Add(submenu);
            submenu.RefreshIndex();
            submenu.UpdateScaleform();
        }
        #endregion
        #region Create Submenus
        /// <summary>
        /// Creates all the submenus depending on the permissions of the user.
        /// </summary>
        private void CreateSubmenus()
        {
            // Add the online players menu.
            if (Cf.IsAllowed(Permission.OPMenu))
            {
                OnlinePlayersMenu = new OnlinePlayers();
                UIMenu menu = OnlinePlayersMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Online Players", "All currently connected players.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
                Menu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == button)
                    {
                        OnlinePlayersMenu.UpdatePlayerlist();
                        menu.RefreshIndex();
                        menu.UpdateScaleform();
                    }
                };
            }
            if (Cf.IsAllowed(Permission.OPUnban))
            {
                //TriggerServerEvent("vMenu:RequestBanList", PlayerId());
                BannedPlayersMenu = new BannedPlayers();
                UIMenu menu = BannedPlayersMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Banned Players", "View and manage all banned players in this menu.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
                Menu.OnItemSelect += (sender, item, index) =>
                {
                    if (item == button)
                    {
                        TriggerServerEvent("vMenu:RequestBanList", PlayerId());
                        menu.RefreshIndex();
                        menu.UpdateScaleform();
                    }
                };
            }

            // Add the player options menu.
            if (Cf.IsAllowed(Permission.POMenu))
            {
                PlayerOptionsMenu = new PlayerOptions();
                UIMenu menu = PlayerOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Player Options", "Common player options can be accessed here.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the vehicle options Menu.
            if (Cf.IsAllowed(Permission.VOMenu))
            {
                VehicleOptionsMenu = new VehicleOptions();
                UIMenu menu = VehicleOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Vehicle Options", "Here you can change common vehicle options, as well as tune & style your vehicle.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            var vl = new Vehicles().VehicleClasses;
            // Add the vehicle spawner menu.
            if (Cf.IsAllowed(Permission.VSMenu))
            {
                VehicleSpawnerMenu = new VehicleSpawner();
                UIMenu menu = VehicleSpawnerMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Vehicle Spawner", "Spawn a vehicle by name or choose one from a specific category.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add Saved Vehicles menu.
            if (Cf.IsAllowed(Permission.SVMenu))
            {
                SavedVehiclesMenu = new SavedVehicles();
                UIMenu menu = SavedVehiclesMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Saved Vehicles", "Save new vehicles, or spawn or delete already saved vehicles.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the player appearance menu.
            if (Cf.IsAllowed(Permission.PAMenu))
            {
                PlayerAppearanceMenu = new PlayerAppearance();
                UIMenu menu = PlayerAppearanceMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Player Appearance", "Choose a ped model, customize it and save & load your customized characters.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the time options menu.
            // check for 'not true' to make sure that it _ONLY_ gets disabled if the owner _REALLY_ wants it disabled, not if they accidentally spelled "false" wrong or whatever.
            if (Cf.IsAllowed(Permission.TOMenu) && MenuOptions["disableSync"] != "true")
            {
                TimeOptionsMenu = new TimeOptions();
                UIMenu menu = TimeOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Time Options", "Change the time, and edit other time related options.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the weather options menu.
            // check for 'not true' to make sure that it _ONLY_ gets disabled if the owner _REALLY_ wants it disabled, not if they accidentally spelled "false" wrong or whatever.
            if (Cf.IsAllowed(Permission.WOMenu) && MenuOptions["disableSync"] != "true")
            {
                WeatherOptionsMenu = new WeatherOptions();
                UIMenu menu = WeatherOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Weather Options", "Change all weather related options here.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the weapons menu.
            if (Cf.IsAllowed(Permission.WPMenu))
            {
                WeaponOptionsMenu = new WeaponOptions();
                UIMenu menu = WeaponOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Weapon Options", "Add/remove weapons, modify weapons and set ammo options.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add Voice Chat Menu.
            if (Cf.IsAllowed(Permission.VCMenu))
            {
                VoiceChatSettingsMenu = new VoiceChat();
                UIMenu menu = VoiceChatSettingsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Voice Chat Settings", "Change Voice Chat options here.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add misc settings menu.
            //if (Cf.IsAllowed(Permission.MSMenu))
            // removed the permissions check, because the misc menu should've never been restricted in the first place.
            // not sure why I even added this before... saving of preferences and similar functions should always be allowed.
            // no matter what.
            {
                MiscSettingsMenu = new MiscSettings();
                UIMenu menu = MiscSettingsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Misc Settings", "Miscellaneous vMenu options/settings can be configured here. You can also save your settings in this menu.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }



            // Add About Menu.
            AboutMenu = new About();
            UIMenu sub = AboutMenu.GetMenu();
            UIMenuItem btn = new UIMenuItem("About vMenu", "Information about vMenu.");
            btn.SetRightLabel("→→→");
            AddMenu(sub, btn);

            // Refresh everything.
            Mp.RefreshIndex();
            Menu.UpdateScaleform();

            // Globally disable the native ui controls disabling.
            Mp.ControlDisablingEnabled = false;
            // Globally disable the "mouse edge" feature.
            Mp.MouseEdgeEnabled = false;
        }
        #endregion
    }
}
