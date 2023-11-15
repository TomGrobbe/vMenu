using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus.WorldSubmenus
{
    public class TimeOptionsMenu
    {
        private static UIMenu timeOptionsMenu = null;

        public TimeOptionsMenu()
        {
            var MenuLanguage = Languages.Menus["TimeOptionsMenu"];

            timeOptionsMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Time Options").Create();

            UIMenuSeparatorItem button = new UIMenuSeparatorItem("Under Construction!", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };

            button.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            timeOptionsMenu.AddItem(button);

            Main.Menus.Add(timeOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return timeOptionsMenu;
        }
    }
}