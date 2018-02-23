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
    public class MiscSettings
    {
        // Variables
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Misc Settings")//, MainMenu.MenuPosition)
            {
                //ScaleWithSafezone = false,
                MouseEdgeEnabled = false
            };
            UIMenuItem tptowp = new UIMenuItem("Teleport To WayPoint", "Teleport to the waypoint on your map.");
            menu.AddItem(tptowp);
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == tptowp)
                {
                    cf.TeleportToWp();
                }
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
