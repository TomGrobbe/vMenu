using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using vMenu.menus;

namespace vMenu
{

    public class MainMenu : BaseScript
    {
        // Variables
        public static MenuPool _mp = new MenuPool();

        private bool firstTick = true;
        private static UIMenu onlinePlayers = new OnlinePlayersMenu().Menu;
        private bool setupComplete = false;
        public static UIMenu menu;

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
                TriggerServerEvent("vMenu:GetSettings");

                // Wait until the data is received.
                while (!setupComplete)
                {
                    await Delay(0);
                }

                // Create the main menu.
                menu = new UIMenu(GetPlayerName(PlayerId()), "MAIN MENU")
                {
                    ControlDisablingEnabled = false
                };
                menu.RefreshIndex();

                // Add the main menu to the menu pool.
                _mp.Add(menu);
            }
            // Todo: more stuff

        }
    }


}
