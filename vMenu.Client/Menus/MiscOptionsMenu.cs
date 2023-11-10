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
    public class MiscOptionsMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu miscOptionsMenu = null;

        public MiscOptionsMenu()
        {
            miscOptionsMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Misc. Options", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, MenuSettings.Glare, MenuSettings.AlternativeTitle, MenuSettings.FadingTime)
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

            UIMenuItem toggleMenuAlign = new UIMenuItem("Toggle Menu Align", "Change the Menu Alignment (Left | Right)", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            toggleMenuAlign.SetRightLabel(Main.MenuAlign.ToString());
            miscOptionsMenu.AddItem(toggleMenuAlign);

            toggleMenuAlign.Activated += (sender, i) =>
            {
                if (Main.MenuAlign == Shared.Enums.MenuAlign.Left)
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Right;
                    MenuFunctions.RestartMenu();
                }
                else
                {
                    Main.MenuAlign = Shared.Enums.MenuAlign.Left;
                    MenuFunctions.RestartMenu();
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
