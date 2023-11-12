using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using ScaleformUI.Menu;

using vMenu.Client.Menus.OnlinePlayersSubmenus;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

namespace vMenu.Client.Menus.PlayerRelated
{
    public class WeaponOptions
    {
        private static UIMenu weaponOptionsMenu = null;

        public WeaponOptions()
        {
            weaponOptionsMenu = new Objects.vMenu("Weapon Related Options").Create();

            UIMenuSeparatorItem button = new UIMenuSeparatorItem("Under Construction!", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };

            weaponOptionsMenu.AddItem(button);

            Main.Menus.Add(weaponOptionsMenu);
        }

        public static UIMenu Menu()
        {
            return weaponOptionsMenu;
        }
    }
}
