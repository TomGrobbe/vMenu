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

        public static CommonFunctions cf = new CommonFunctions();
        public static MenuPool _mp = new MenuPool();
        public static System.Drawing.PointF MenuPosition = new System.Drawing.PointF(CitizenFX.Core.UI.Screen.Resolution.Width - 465f, 45f);
        public static Notification Notify = new Notification();
        public static Subtitles Subtitle = new Subtitles();

        private bool firstTick = true;
        private bool setupComplete = false;
        public static UIMenu menu;
        public static PlayerOptions _po;
        public static OnlinePlayers _op;
        public static VehicleOptions _vo;
        public static Dictionary<string, bool> Permissions { get; private set; } = new Dictionary<string, bool>();

        private BarTimerBar bt = new BarTimerBar("Opening Menu");
        private bool debug = false;
        public static Sprite BannerSprite { get; private set; } = new Sprite("menubanner", "menu_header", new System.Drawing.PointF(0f, 0f), new System.Drawing.SizeF(0f, 0f), 0f, UnknownColors.SlateGray);

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
                menu = new UIMenu(GetPlayerName(PlayerId()), "Main Menu", MenuPosition);

                // Add the main menu to the menu pool.
                _mp.Add(menu);

                menu.RefreshIndex();
                menu.ScaleWithSafezone = false;

                menu.UpdateScaleform();

                // Create all (sub)menus.

                // Add the online players menu.
                if (Permissions["vMenu_menus_*"] || Permissions["vMenu_menus_onlinePlayers"])
                {
                    UIMenuItem onlinePlayersBtn = new UIMenuItem("Online Players", "All currently connected players.");
                    menu.AddItem(onlinePlayersBtn);
                    _op = new OnlinePlayers();
                    UIMenu onlinePlayers = _op.GetMenu();
                    menu.BindMenuToItem(onlinePlayers, onlinePlayersBtn);
                    _mp.Add(onlinePlayers);
                    menu.OnItemSelect += (sender, item, index) =>
                    {
                        if (item == onlinePlayersBtn)
                        {
                            _op.UpdatePlayerlist();
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
                    menu.AddItem(playerOptionsBtn);
                    _po = new PlayerOptions();
                    UIMenu playerOptions = _po.GetMenu();
                    menu.BindMenuToItem(playerOptions, playerOptionsBtn);
                    _mp.Add(playerOptions);
                    //playerOptions.UpdateScaleform();
                }
                
                // Add the vehicle options Menu.
                if (Permissions["vMenu_menus_*"] || Permissions["vMenu_menus_vehicleOptions"])
                {
                    UIMenuItem vehicleOptionsBtn = new UIMenuItem("Vehicle Options", "Here you can change common vehicle options, as well as tune & style your vehicle.");
                    menu.AddItem(vehicleOptionsBtn);
                    _vo = new VehicleOptions();
                    UIMenu vehicleOptions = _vo.GetMenu();
                    menu.BindMenuToItem(vehicleOptions, vehicleOptionsBtn);
                    _mp.Add(vehicleOptions);
                }

                // Refresh everything.
                _mp.RefreshIndex();
                menu.UpdateScaleform();

                // Set the banner globally.
                _mp.SetBannerType(BannerSprite);
                // Globally disable the native ui controls disabling.
                _mp.ControlDisablingEnabled = false;
                // Globally disable the "mouse edge" feature.
                _mp.MouseEdgeEnabled = false;
            }
            else
            {
                if (Game.CurrentInputMode == InputMode.MouseAndKeyboard && Game.IsControlJustPressed(0, Control.InteractionMenu))
                {
                    if (_mp.IsAnyMenuOpen())
                    {
                        _mp.CloseAllMenus();
                    }
                    else
                    {
                        menu.Visible = !_mp.IsAnyMenuOpen();
                    }

                }
                else if (!_mp.IsAnyMenuOpen() && Game.CurrentInputMode == InputMode.GamePad)
                {
                    float timer = 0f;
                    while (Game.CurrentInputMode == InputMode.GamePad && Game.IsControlPressed(0, Control.InteractionMenu))
                    {
                        timer++;

                        if (debug)
                        {
                            bt.Draw(0);
                            float percent = (timer / 60f);
                            bt.Percentage = percent;
                            Subtitle.Success(percent.ToString(), 0, true, "Progress:");
                        }

                        if (timer > 59)
                        {
                            menu.Visible = !_mp.IsAnyMenuOpen();
                            break;
                        }

                        await Delay(0);
                    }
                }

                _mp.ProcessMenus();
            }


        }
    }


}
