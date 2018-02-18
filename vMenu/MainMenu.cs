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

        private bool firstTick = true;
        private bool setupComplete = true;
        public static UIMenu menu;
        public static PlayerOptions _po;

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
                while (!setupComplete)
                {
                    await Delay(0);
                }

                // Create the main menu.
                menu = new UIMenu(GetPlayerName(PlayerId()), "Main Menu")
                {
                    ControlDisablingEnabled = false
                };

                var playerOptionsBtn = new UIMenuItem("Player Options", "Player Options");
                menu.AddItem(playerOptionsBtn);
                _po = new PlayerOptions();
                var playerOptions = _po.GetMenu();
                menu.BindMenuToItem(playerOptions, playerOptionsBtn);
                _mp.Add(playerOptions);


                // Add the main menu to the menu pool.
                _mp.Add(menu);

                // Create all (sub)menus.
            }
            else
            {
                _mp.ProcessMenus();
                
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
            }

            
        }
    }


}
