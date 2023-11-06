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
    public class MiscOptionsMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu miscOptionsMenu;

        public MiscOptionsMenu()
        {
            
        }

        public static UIMenu Menu()
        {
            miscOptionsMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Misc. Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true);

            Main.Menus.Add(miscOptionsMenu);

            UIMenuItem toggleMenuAlign = new UIMenuItem("Toggle Menu Align", "Change the Menu Alignment (Left | Right)");
            toggleMenuAlign.SetRightLabel(Main.MenuAlign.ToString());
            miscOptionsMenu.AddItem(toggleMenuAlign);

            toggleMenuAlign.Activated += (sender, i) =>
            {
                if (Main.MenuAlign == Shared.Enums.MenuAlign.Left)
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Right;
                    sender.Visible = false;
                    //miscOptionsMenu.
                    sender.Visible = true;
                }
                else
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Left;
                    sender.Visible = false;
                    //sender.Clear();
                    //new MiscOptionsMenu();
                    sender.Visible = true;
                }
            };

            return miscOptionsMenu;
        }
    }
}
