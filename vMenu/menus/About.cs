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
    public class About
    {
        // Variables
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "About vMenu", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseEdgeEnabled = false
            };

            var currentVersion = ($"~m~v{GetResourceMetadata(GetCurrentResourceName(), "version", 0)} (Pre-Alpha)");
            UIMenuItem version = new UIMenuItem("Version", $"Current version of vMenu: {currentVersion}");
            version.SetRightLabel($"{currentVersion}");
            menu.AddItem(version);
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
