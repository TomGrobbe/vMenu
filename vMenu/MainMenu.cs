using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using System.Dynamic;

namespace vMenuClient
{
    public class MainMenu : BaseScript
    {
        // Variables
        public static CommonFunctions cf { get; } = new CommonFunctions();
        public static Notification Notify { get; } = new Notification();
        public static Subtitles Subtitle { get; } = new Subtitles();

        public static MenuPool Mp { get; } = new MenuPool();
        //public static System.Drawing.PointF MenuPosition { get; } = new System.Drawing.PointF(CitizenFX.Core.UI.Screen.Resolution.Width - 800f, 45f);
        //public static System.Drawing.PointF MenuPosition { get; } = new System.Drawing.PointF(0f, 0f);
        //public static UIResRectangle BannerSprite { get; } = new UIResRectangle(new System.Drawing.PointF(0f, 0f), new System.Drawing.SizeF(1920f, 1080f), UnknownColors.SlateGray);

        private bool firstTick = true;
        private bool setupComplete = false;

        public static UIMenu Menu { get; private set; }

        public static PlayerOptions PlayerOptionsMenu { get; private set; }
        public static OnlinePlayers OnlinePlayersMenu { get; private set; }
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


        public static Dictionary<string, bool> Permissions { get; private set; } = new Dictionary<string, bool>();

        private BarTimerBar bt = new BarTimerBar("Opening Menu");
        private bool debug = GetResourceMetadata(GetCurrentResourceName(), "client_debug_mode", 0) == "true" ? true : false;

        public static bool DontOpenMenus { get; set; } = false;


        /// <summary>f
        /// Constructor.
        /// </summary>
        public MainMenu()
        {
            EventHandlers.Add("vMenu:SetPermissions", new Action<dynamic>(SetPermissions));
            //InvokeFunctionReference("0x7bdcbd45", $"Enjoying vMenu {GetResourceMetadata(GetCurrentResourceName(), "version", 0)} (Pre-Release)", 0, ref unused);
            Tick += OnTick;
        }

        /// <summary>
        /// Set the permissions for this client.
        /// </summary>
        /// <param name="dict"></param>
        private void SetPermissions(dynamic dict)
        {
            // Convert the dynamic object into a dictionary<string, bool>
            foreach (dynamic permission in dict)
            {
                // Add the new permission to the dictionary.
                Permissions.Add(permission.Key.ToString(), permission.Value);

                // Only if debugging is set to true, notify the client of the values.
                if (debug)
                {
                    Notify.Custom($"Key: {permission.Key.ToString()}\r\nValue: {permission.Value}");
                }

            }

            // Loop through the new permissions list.
            // Check for wildcards and if they are true, set all "child" permissions to true (allowed)
            foreach (KeyValuePair<string, bool> perm in Permissions)
            {
                // If the permission is currently not allowed.
                if (!perm.Value)
                {
                    // Check the wildcard for this permission.
                    var wildCardVersion = perm.Key.ToString().Split('_')[0].ToString() + perm.Key.ToString().Split('_')[1].ToString() + "_*";
                    if (Permissions.ContainsKey(wildCardVersion))
                    {
                        // If that wildcard is set to true (allowed) then allow the current permission as well.
                        if (Permissions[wildCardVersion])
                        {
                            // Update the current permission in the dictionary to be set to true instead of false.
                            Permissions[perm.Key] = true;
                        }
                    }
                }
            }

            #region if debug
            if (debug)
            {
                foreach (KeyValuePair<string, bool> perm in Permissions)
                {
                    if (perm.Value)
                    {
                        TriggerEvent("chatMessage", perm.Key.ToString());
                    }
                }
            }
            #endregion

            setupComplete = true;
        }

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

        /// <summary>
        /// OnTick runs every game tick.
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

                // Wait until the data is received.
                while (!setupComplete || GetPlayerName(PlayerId()) == "**Invalid**" || GetPlayerName(PlayerId()) == "** Invalid **")
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

            // When it's not the first tick, do these things:
            else
            {
                #region Handle Opening/Closing of the menu.

                // If menus can be opened.
                if (!DontOpenMenus && !IsPauseMenuActive())
                {
                    // If the player is using Keyboard & Mouse and they pressed the M key (interaction menu button) then...
                    if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && (Game.IsControlJustPressed(0, Control.InteractionMenu) || Game.IsDisabledControlJustPressed(0, Control.InteractionMenu)))
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
                        // Create a timer and set it to 0.
                        float timer = 0f;

                        // While (and only if) the player keeps using only the controller, and keeps holding down the interactionmenu button (select on controller).
                        while (Game.CurrentInputMode == InputMode.GamePad && Game.IsControlPressed(0, Control.InteractionMenu))
                        {
                            // Increment the timer.
                            timer++;

                            // If debugging is enabled, show the progress using a timerbar.
                            if (debug)
                            {
                                bt.Draw(0);
                                float percent = (timer / 60f);
                                bt.Percentage = percent;
                                Subtitle.Success(percent.ToString(), 0, true, "Progress:");
                            }

                            // If the timer has reached 60, open the menu. (60 is +/- 1 second, so the player is holding down the button for at least 1 second).
                            if (timer > 59)
                            {
                                Menu.Visible = !Mp.IsAnyMenuOpen();
                                // Break the loop (resetting the timer).
                                break;
                            }

                            // Wait for the next game tick. This will make the timer only increment once per tick, so it'll take 60 game ticks for the menu to be open (60 frames is +/- 1 sec).
                            await Delay(0);
                        }
                    }
                }
                // If the pause menu is active or all menus should be closed, close all menus.
                else
                {
                    await Delay(5);
                    Mp.CloseAllMenus();
                }
                #endregion

                #region Disable Inputs when any menu is open.
                if (Mp.IsAnyMenuOpen())
                {
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

                    // Menu toggle button.
                    Game.DisableControlThisFrame(0, Control.InteractionMenu);

                    // When in a vehicle
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        Game.DisableControlThisFrame(0, Control.VehicleSelectNextWeapon);
                        Game.DisableControlThisFrame(0, Control.VehicleSelectPrevWeapon);
                        Game.DisableControlThisFrame(0, Control.VehicleCinCam);
                    }
                }
                #endregion

                // Process all menus in the menu pool (displays them when they're active).
                Mp.ProcessMenus();
                Mp.WidthOffset = 50;

            }

        }

        private void CreateSubmenus()
        {
            // Add the online players menu.
            if (cf.IsAllowed("vMenu_menus_onlinePlayers"))
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

            // Add the player options menu.
            if (cf.IsAllowed("vMenu_menus_playerOptions"))
            {
                PlayerOptionsMenu = new PlayerOptions();
                UIMenu menu = PlayerOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Player Options", "Common player options can be accessed here.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the vehicle options Menu.
            if (cf.IsAllowed("vMenu_menus_vehicleOptions"))
            {
                VehicleOptionsMenu = new VehicleOptions();
                UIMenu menu = VehicleOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Vehicle Options", "Here you can change common vehicle options, as well as tune & style your vehicle.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            var vl = new Vehicles().VehicleClasses;
            // Add the vehicle spawner menu.
            if (cf.IsAllowed("vMenu_menus_vehicleSpawner"))
            {
                VehicleSpawnerMenu = new VehicleSpawner();
                UIMenu menu = VehicleSpawnerMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Vehicle Spawner", "Spawn a vehicle by name or choose one from a specific category.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add Saved Vehicles menu.
            if (cf.IsAllowed("vMenu_menus_savedVehicles"))
            {
                SavedVehiclesMenu = new SavedVehicles();
                UIMenu menu = SavedVehiclesMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Saved Vehicles", "Save new vehicles, or spawn or delete already saved vehicles.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the player appearance menu.
            if (cf.IsAllowed("vMenu_menus_playerAppearance"))
            {
                PlayerAppearanceMenu = new PlayerAppearance();
                UIMenu menu = PlayerAppearanceMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Player Appearance", "Choose a ped model, customize it and save & load your customized characters.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the time options menu.
            if (cf.IsAllowed("vMenu_menus_timeOptions"))
            {
                TimeOptionsMenu = new TimeOptions();
                UIMenu menu = TimeOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Time Options", "Change the time, and edit other time related options.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the weather options menu.
            if (cf.IsAllowed("vMenu_menus_weatherOptions"))
            {
                WeatherOptionsMenu = new WeatherOptions();
                UIMenu menu = WeatherOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Weather Options", "Change all weather related options here.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add the weapons menu.
            if (cf.IsAllowed("vMenu_menus_weaponOptions"))
            {
                WeaponOptionsMenu = new WeaponOptions();
                UIMenu menu = WeaponOptionsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Weapon Options", "Add/remove weapons, modify weapons and set ammo options.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add misc settings menu.
            if (cf.IsAllowed("vMenu_menus_miscSettings"))
            {
                MiscSettingsMenu = new MiscSettings();
                UIMenu menu = MiscSettingsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("Misc Settings", "Change general settings.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add Voice Chat Menu.
            if (cf.IsAllowed("vMenu_menus_voiceChat"))
            {
                VoiceChatSettingsMenu = new VoiceChat();
                UIMenu menu = VoiceChatSettingsMenu.GetMenu();
                UIMenuItem button = new UIMenuItem("VoiceChat Settings", "Change VoiceChat options here.");
                button.SetRightLabel("→→→");
                AddMenu(menu, button);
            }

            // Add About Menu.
            AboutMenu = new About();
            UIMenu sub = AboutMenu.GetMenu();
            UIMenuItem btn = new UIMenuItem("About vMenu", "Information about this menu and it's creators.");
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
    }


}
