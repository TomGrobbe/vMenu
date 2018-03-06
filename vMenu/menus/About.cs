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
        private CommonFunctions cf = MainMenu.Cf;

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


            var currentVersion = ($"v{GetResourceMetadata(GetCurrentResourceName(), "version", 0)} (Pre-Alpha)");

            // Create menu items.
            UIMenuItem version = new UIMenuItem("Version", $"Currently installed version of vMenu: ~c~~h~{currentVersion}~h~");
            version.SetRightLabel($"~m~~h~{currentVersion}~h~");
            UIMenuItem credits = new UIMenuItem("Credits", $"vMenu is made by ~b~Vespura~w~. Contributors: ~o~Briglair~w~ & ~o~Shayan~w~. Thanks to ~y~IllusiveTea ~w~for helping me test everything.");
            UIMenuItem info1 = new UIMenuItem("More Info (1/2)", "vMenu is a server sided trainer, including full permissions support for all of it's features. " +
                "vMenu is inspired by ~b~Oui's Lambda Menu~w~ and ~y~Sjaak327's Simple Trainer for GTA V~w~. ");
            UIMenuItem info2 = new UIMenuItem("More Info (2/2)", "I've tried to add all features that ~r~~h~I~h~~w~ believe are important for any trainer. All (in my opinion) unnecassary features have been left out. " +
                "This way, I hope to provide a customizable server-sided menu, that can benefit almost all servers.");
            UIMenuItem help = new UIMenuItem("Player Help", $"If you found a ~p~bug~w~, want to ~y~request a feature~w~, want to leave ~g~feedback ~w~or you want to ~o~contact ~w~me for another reason, please go to ~b~vespura.com/vmenu/contact~w~.");
            UIMenuItem support = new UIMenuItem("Developer Help", "If you need help setting this up on your server, please visit the vMenu wiki page at: ~b~vespura.com/vmenu/wiki~w~.");

            menu.AddItem(version);
            menu.AddItem(credits);
            menu.AddItem(info1);
            menu.AddItem(info2);
            menu.AddItem(help);
            menu.AddItem(support);

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
