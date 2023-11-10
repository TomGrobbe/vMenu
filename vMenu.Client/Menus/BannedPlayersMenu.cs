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

namespace vMenu.Client.Menus
{
    public class BannedPlayersMenu
    {
        private static UIMenu bannedPlayersMenu = null;

        public BannedPlayersMenu()
        {
            bannedPlayersMenu = new Objects.vMenu("Banned Players").Create();

            UIMenuSeparatorItem button = new UIMenuSeparatorItem("Under Construction!", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };

            bannedPlayersMenu.AddItem(button);

            Main.Menus.Add(bannedPlayersMenu);
        }

        public static UIMenu Menu()
        {
            return bannedPlayersMenu;
        }
    }
}