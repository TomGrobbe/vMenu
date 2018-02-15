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
            if (firstTick)
            {
                firstTick = false;
                TriggerServerEvent("vMenu:GetSettings");

                while (!setupComplete)
                {
                    await Delay(0);
                }

                menu = new UIMenu(GetPlayerName(PlayerId()), "MAIN MENU")
                {
                    ControlDisablingEnabled = false
                };
                menu.RefreshIndex();
                _mp.Add(menu);
            }


        }
    }

    
}
