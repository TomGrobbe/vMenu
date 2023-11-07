using CitizenFX.Core;
using ScaleformUI.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;

namespace vMenu.Client.Menus
{
    public class AboutMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu aboutMenu = null;

        public AboutMenu()
        {
            aboutMenu = new UIMenu(Main.MenuBanner.BannerTitle, "About vMenu", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true);

            UIMenuItem vMenuVersion = new UIMenuItem("About vMenu");
            vMenuVersion.SetRightLabel($"~h~v{MenuFunctions.Version}~h~");
            UIMenuItem vMenuCredits = new UIMenuItem("About vMenu / Credits", $"This server is using vMenu ~b~~h~v{MenuFunctions.Version}~h~~s~");
            UIMenuItem vMenuFounder = new UIMenuItem("vMenu Founder Info", "https://vespura.com/");

            aboutMenu.AddItem(vMenuVersion);
            aboutMenu.AddItem(vMenuCredits);
            aboutMenu.AddItem(vMenuFounder);

            Main.Menus.Add(aboutMenu);
        }

        public static UIMenu Menu()
        {
            return aboutMenu;
        }
    }
}