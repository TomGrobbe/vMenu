using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using ScaleformUI.Elements;
using ScaleformUI.Menu;
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
    public class WorldOptionsMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu worldRelatedOptions = null;

        public WorldOptionsMenu()
        {
            worldRelatedOptions = new UIMenu(Main.MenuBanner.BannerTitle, "World Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, MenuSettings.Glare, MenuSettings.AlternativeTitle, MenuSettings.FadingTime)
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

            worldRelatedOptions.AddItem(button);

            Main.Menus.Add(worldRelatedOptions);
        }

        public static UIMenu Menu()
        {
            return worldRelatedOptions;
        }
    }
}