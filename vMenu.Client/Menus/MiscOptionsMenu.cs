using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class MiscOptionsMenu
    {
        private static UIMenu miscOptionsMenu = null;

        public MiscOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["MiscOptionsMenu"];

            miscOptionsMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Miscellaneous Options").Create();

            UIMenuItem toggleMenuAlign = new UIMenuItem(MenuLanguage.Items["ToggleMenuAlignItem"].Name ?? "Toggle Menu Align", MenuLanguage.Items["ToggleMenuAlignItem"].Description ?? "Change the Menu Alignment (Left | Right)", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            toggleMenuAlign.SetRightLabel(Main.MenuAlign.ToString());
            miscOptionsMenu.AddItem(toggleMenuAlign);

            toggleMenuAlign.Activated += (sender, i) =>
            {
                if (Main.MenuAlign == Shared.Enums.MenuAlign.Left)
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Right;
                    MenuFunctions.Instance.RestartMenu();
                }
                else
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Left;
                    MenuFunctions.Instance.RestartMenu();
                }
            };

            Main.Menus.Add(miscOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return miscOptionsMenu;
        }
    }
}
