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
        public static System.Drawing.PointF MenuPosition { get; } = new System.Drawing.PointF(CitizenFX.Core.UI.Screen.Resolution.Width - 465f, 45f);
        public static UIResRectangle BannerSprite { get; } = new UIResRectangle(new System.Drawing.PointF(0f, 0f), new System.Drawing.SizeF(0f, 0f), UnknownColors.SlateGray);

        private bool firstTick = true;
        private bool setupComplete = false;

        public static UIMenu Menu { get; private set; }

        public static PlayerOptions PlayerOptionsMenu { get; private set; }
        public static OnlinePlayers OnlinePlayersMenu { get; private set; }
        public static VehicleOptions VehicleOptionsMenu { get; private set; }

        public static Dictionary<string, bool> Permissions { get; private set; } = new Dictionary<string, bool>();

        private BarTimerBar bt = new BarTimerBar("Opening Menu");
        private bool debug = false;
        //public static Sprite BannerSprite { get; private set; } = new Sprite("menubanner", "menu_header", new System.Drawing.PointF(0f, 0f), new System.Drawing.SizeF(0f, 0f), 0f, UnknownColors.SlateGray);


        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenu()
        {
            EventHandlers.Add("vMenu:SetPermissions", new Action<dynamic>(SetPermissions));
            Tick += OnTick;
        }

        /// <summary>
        /// Set the permissions for this client.
        /// </summary>
        /// <param name="dict"></param>
        private void SetPermissions(dynamic dict)
        {
            //foreach (KeyValuePair<string, bool> perm in dict)
            //{
            //    Permissions.Add(perm.Key, perm.Value);
            //}
            foreach (dynamic permission in dict)
            {
                Permissions.Add(permission.Key.ToString(), permission.Value);
                if (debug)
                {
                    Notify.Custom($"Key: {permission.Key.ToString()}\r\nValue: {permission.Value}");
                }

            }
            setupComplete = true;
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

                // Request the permissions data from the server.
                TriggerServerEvent("vMenu:RequestPermissions", PlayerId());

                // Wait until the data is received.
                while (!setupComplete || GetPlayerName(PlayerId()) == "**Invalid**" || GetPlayerName(PlayerId()) == "** Invalid **")
                {
                    await Delay(0);
                }

                // Create the main menu.
                Menu = new UIMenu(GetPlayerName(PlayerId()), "Main Menu", MenuPosition);

                // Add the main menu to the menu pool.
                Mp.Add(Menu);

                Menu.RefreshIndex();
                Menu.ScaleWithSafezone = false;

                Menu.UpdateScaleform();
                Menu.MouseControlsEnabled = false;

                // Create all (sub)menus.

                // Add the online players menu.
                if (Permissions["vMenu_menus_*"] || Permissions["vMenu_menus_onlinePlayers"])
                {
                    UIMenuItem onlinePlayersBtn = new UIMenuItem("Online Players", "All currently connected players.");
                    Menu.AddItem(onlinePlayersBtn);
                    OnlinePlayersMenu = new OnlinePlayers();
                    UIMenu onlinePlayers = OnlinePlayersMenu.GetMenu();
                    onlinePlayers.MouseControlsEnabled = false;
                    Menu.BindMenuToItem(onlinePlayers, onlinePlayersBtn);
                    Mp.Add(onlinePlayers);
                    Menu.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == onlinePlayersBtn)
                        {
                            OnlinePlayersMenu.UpdatePlayerlist();
                            onlinePlayers.UpdateScaleform();
                            onlinePlayers.RefreshIndex();
                        }
                    };
                    onlinePlayers.UpdateScaleform();
                }

                // Add the player options menu.
                if (Permissions["vMenu_menus_*"] || Permissions["vMenu_menus_playerOptions"])
                {
                    UIMenuItem playerOptionsBtn = new UIMenuItem("Player Options", "Common player options can be accessed here.");
                    Menu.AddItem(playerOptionsBtn);
                    PlayerOptionsMenu = new PlayerOptions();
                    UIMenu playerOptions = PlayerOptionsMenu.GetMenu();
                    Menu.BindMenuToItem(playerOptions, playerOptionsBtn);
                    Mp.Add(playerOptions);
                    playerOptions.MouseControlsEnabled = false;
                }

                // Add the vehicle options Menu.
                if (Permissions["vMenu_menus_*"] || Permissions["vMenu_menus_vehicleOptions"])
                {
                    UIMenuItem vehicleOptionsBtn = new UIMenuItem("Vehicle Options", "Here you can change common vehicle options, as well as tune & style your vehicle.");
                    Menu.AddItem(vehicleOptionsBtn);
                    VehicleOptionsMenu = new VehicleOptions();
                    UIMenu vehicleOptions = VehicleOptionsMenu.GetMenu();
                    Menu.BindMenuToItem(vehicleOptions, vehicleOptionsBtn);
                    Mp.Add(vehicleOptions);
                    vehicleOptions.MouseControlsEnabled = false;
                }

                // Refresh everything.
                Mp.RefreshIndex();
                Menu.UpdateScaleform();

                // Set the banner globally.
                Mp.SetBannerType(BannerSprite);
                // Globally disable the native ui controls disabling.
                Mp.ControlDisablingEnabled = false;
                // Globally disable the "mouse edge" feature.
                Mp.MouseEdgeEnabled = false;
            }
            #endregion

            // When it's not the first tick, do these things:
            else
            {
                #region Handle Opening/Closing of the menu.
                // If the player is using Keyboard & Mouse and they pressed the M key (interaction menu button) then...
                if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && Game.IsControlJustPressed(0, Control.InteractionMenu))
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
                #endregion

                #region Disable Inputs when any menu is open.
                if (Mp.IsAnyMenuOpen())
                {
                    // Disable Gamepad/Controller Specific controls:
                    if (Game.CurrentInputMode == InputMode.GamePad)
                    {
                        // when in a vehicle.
                        if (IsPedInAnyVehicle(PlayerPedId(), false))
                        {
                            Game.DisableControlThisFrame(0, Control.VehicleHeadlight);
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

                // Process all menus in the menu pool (displays them when they're active).
                Mp.ProcessMenus();

            }


        }
    }


}
