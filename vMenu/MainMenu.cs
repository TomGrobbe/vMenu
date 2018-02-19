using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class MainMenu : BaseScript
    {
        // Variables
        public static MenuPool _mp = new MenuPool();
        public static System.Drawing.PointF MenuPosition = new System.Drawing.PointF(CitizenFX.Core.UI.Screen.Resolution.Width - 465f, 45f);
        private static Notification Notify = new Notification();
        private static Subtitles Subtitle = new Subtitles();

        private bool firstTick = true;
        private bool setupComplete = true;
        public static UIMenu menu;
        public static PlayerOptions _po;
        public static OnlinePlayers _op;
        public static VehicleOptions _vo;

        private BarTimerBar bt = new BarTimerBar("Opening Menu");
        private bool debug = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainMenu()
        {
            Tick += OnTick;
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
                // Request the data from the server.
                //TriggerServerEvent("vMenu:GetSettings");

                // Wait until the data is received.
                while (!setupComplete && (GetPlayerName(PlayerId()) == "**Invalid**" || GetPlayerName(PlayerId()) == "** Invalid **"))
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
                menu.RefreshIndex();


                // Create all (sub)menus.

                // Add the online players menu.
                UIMenuItem onlinePlayersBtn = new UIMenuItem("Online Players", "All currently connected players.");
                menu.AddItem(onlinePlayersBtn);
                _op = new OnlinePlayers();
                UIMenu onlinePlayers = _op.GetMenu();

                onlinePlayers.ScaleWithSafezone = false;
                onlinePlayers.UpdateScaleform();

                menu.BindMenuToItem(onlinePlayers, onlinePlayersBtn);
                _mp.Add(onlinePlayers);

                // Add the player options menu.
                UIMenuItem playerOptionsBtn = new UIMenuItem("Player Options", "Common player options can be accessed here.");
                menu.AddItem(playerOptionsBtn);
                _po = new PlayerOptions();
                UIMenu playerOptions = _po.GetMenu();
                playerOptions.ScaleWithSafezone = false;
                playerOptions.UpdateScaleform();

                menu.BindMenuToItem(playerOptions, playerOptionsBtn);
                _mp.Add(playerOptions);

                _mp.RefreshIndex();

                onlinePlayers.UpdateScaleform();
                playerOptions.UpdateScaleform();
                menu.UpdateScaleform();

                _mp.SetBannerType(new UIResRectangle(new System.Drawing.PointF(0f, 0f), new System.Drawing.SizeF(0f, 0f), System.Drawing.Color.FromArgb(38, 38, 38)));
                _mp.ControlDisablingEnabled = false;
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
