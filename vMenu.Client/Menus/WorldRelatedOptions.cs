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
    public class WorldRelatedOptions : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu worldRelatedOptions = null;

        public WorldRelatedOptions()
        {
            worldRelatedOptions = new UIMenu(Main.MenuBanner.BannerTitle, "World Related Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true)
            {
                MaxItemsOnScreen = 9,
                BuildingAnimation = MenuBuildingAnimation.NONE,
                ScrollingType = ScrollingType.ENDLESS,
                Enabled3DAnimations = false,
                MouseControlsEnabled = false,
                ControlDisablingEnabled = false,
                EnableAnimation = false,
            };
            UIMenuItem button = new UIMenuItem("~r~~h~Under Construction!~h~");

            worldRelatedOptions.AddItem(button);

            Main.Menus.Add(worldRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return worldRelatedOptions;
        }
    }
}