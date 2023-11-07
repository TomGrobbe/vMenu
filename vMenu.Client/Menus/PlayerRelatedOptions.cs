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
    public class PlayerRelatedOptions : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu playerRelatedOptions = null;

        public PlayerRelatedOptions()
        {
            playerRelatedOptions = new UIMenu(Main.MenuBanner.BannerTitle, "Player Related Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true);
            
            UIMenuItem button = new UIMenuItem("~r~~h~Under Construction!~h~");

            playerRelatedOptions.AddItem(button);

            Main.Menus.Add(playerRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return playerRelatedOptions;
        }
    }
}