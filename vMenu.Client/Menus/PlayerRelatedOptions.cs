using CitizenFX.Core;
using ScaleformUI.Menu;
using ScaleformUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;
using vMenu.Client.MenuSettings;

namespace vMenu.Client.Menus
{
    public class PlayerRelatedOptions : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu playerRelatedOptions = null;

        public static bool NoClipEnabled { get { return NoClip.IsNoclipActive(); } set { NoClip.SetNoclipActive(value); } }

        public PlayerRelatedOptions()
        {
            playerRelatedOptions = new UIMenu(Main.MenuBanner.BannerTitle, "Player Related Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, menuSettings.Glare, menuSettings.AlternativeTitle, menuSettings.fadingTime)
            {
                MaxItemsOnScreen = menuSettings.maxItemsOnScreen,
                BuildingAnimation = menuSettings.buildingAnimation,
                ScrollingType = menuSettings.scrollingType,
                Enabled3DAnimations = menuSettings.enabled3DAnimations,
                MouseControlsEnabled = menuSettings.mouseControlsEnabled,
                ControlDisablingEnabled = menuSettings.controlDisablingEnabled,
                EnableAnimation = menuSettings.enableAnimation,
            };
            
            UIMenuItem button = new UIMenuItem("~r~~h~Under Construction!~h~", "", menuSettings.BackgroundColor, menuSettings.HighlightColor);

            UIMenuItem NoClip = new UIMenuItem("NoClip Toggle", "", menuSettings.BackgroundColor, menuSettings.HighlightColor);

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