using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using ScaleformUI.Menu;
using ScaleformUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;
using vMenu.Client.Settings;

namespace vMenu.Client.Menus
{
    public class PlayerOptionsMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu playerRelatedOptions = null;

        public static bool NoClipEnabled { get { return NoClip.IsNoclipActive(); } set { NoClip.SetNoclipActive(value); } }

        public PlayerOptionsMenu()
        {
            playerRelatedOptions = new UIMenu(Main.MenuBanner.BannerTitle, "Player Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, MenuSettings.Glare, MenuSettings.AlternativeTitle, MenuSettings.FadingTime)
            {
                MaxItemsOnScreen = MenuSettings.MaxItemsOnScreen,
                BuildingAnimation = MenuSettings.BuildingAnimation,
                ScrollingType = MenuSettings.ScrollingType,
                Enabled3DAnimations = MenuSettings.Enabled3DAnimations,
                MouseControlsEnabled = MenuSettings.MouseControlsEnabled,
                MouseEdgeEnabled = MenuSettings.MouseEdgeEnabled,
                MouseWheelControlEnabled = MenuSettings.MouseWheelControlEnabled,
                ControlDisablingEnabled = MenuSettings.ControlDisablingEnabled,
                EnableAnimation = MenuSettings.EnableAnimation,
            };

            UIMenuSeparatorItem button = new UIMenuSeparatorItem("Under Construction!", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };

            UIMenuItem NoClip = new UIMenuItem("NoClip Toggle", "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);

            playerRelatedOptions.AddItem(button);
            playerRelatedOptions.AddItem(NoClip);

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