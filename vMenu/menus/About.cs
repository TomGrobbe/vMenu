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
        private readonly CommonFunctions cf = MainMenu.Cf;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu("vMenu", "About vMenu", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            // Create menu items.
            UIMenuItem version = new UIMenuItem("Version", $"This server is using vMenu ~b~~h~{MainMenu.Version}~h~~s~.");
            version.SetRightLabel($"~h~{MainMenu.Version}~h~");
            UIMenuItem credits = new UIMenuItem("About / Credits", $"vMenu is made by ~b~Vespura~s~. For more info, checkout ~b~www.vespura.com/vmenu~s~.");
            //UIMenuItem info = new UIMenuItem("More Info About vMenu", "If you'd like to find out more about vMenu and all of it's features, " +
            //    "checkout the forum post at ~b~vespura.com/vmenu~s~.");
            //UIMenuItem help = new UIMenuItem("Need Help?", $"If you want to learn more about vMenu, report a bug or you need to contact me, go to ~b~vespura.com/vmenu~s~.");
            //UIMenuItem support = new UIMenuItem("Info For Server Owners", "If you want to learn how to setup vMenu for your server, please visit the vMenu wiki page " +
            //    "at: ~b~vespura.com/vmenu/wiki~s~.");

            menu.AddItem(version);
            menu.AddItem(credits);
            //menu.AddItem(info);
            //menu.AddItem(help);
            //menu.AddItem(support);

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
