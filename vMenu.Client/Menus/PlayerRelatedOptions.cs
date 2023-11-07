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

        public static bool NoClipEnabled { get { return NoClip.IsNoclipActive(); } set { NoClip.SetNoclipActive(value); } }

        public PlayerRelatedOptions()
        {
            playerRelatedOptions = new UIMenu(Main.MenuBanner.BannerTitle, "Player Related Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true)
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

            UIMenuItem NoClip = new UIMenuItem("NoClip Toggle");

            playerRelatedOptions.AddItem(NoClip);
            playerRelatedOptions.AddItem(button);

            NoClip.Activated += (sender, i) =>
            {
                NoClipEnabled = !NoClipEnabled;
                if (!NoClipEnabled)
                {
                  playerRelatedOptions.Visible = false;  
                  playerRelatedOptions.Visible = true;  
                }      
            };

            Main.Menus.Add(playerRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return playerRelatedOptions;
        }
    }
}