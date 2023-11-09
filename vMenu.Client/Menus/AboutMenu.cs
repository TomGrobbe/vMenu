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
    public class AboutMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu aboutMenu = null;

        public AboutMenu()
        {
            aboutMenu = new UIMenu(Main.MenuBanner.BannerTitle, "About vMenu", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, MenuSettings.Glare, MenuSettings.AlternativeTitle, MenuSettings.FadingTime)
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

            UIMenuItem vMenuVersion = new UIMenuItem("About vMenu", "", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vMenuVersion.SetRightLabel($"~h~v{MenuFunctions.Version}~h~");
            UIMenuItem vMenuCredits = new UIMenuItem("About vMenu / Credits", $"This server is using vMenu ~b~~h~v{MenuFunctions.Version}~h~~s~", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            UIMenuItem vMenuFounder = new UIMenuItem("vMenu Founder Info", "https://vespura.com/", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);

            aboutMenu.AddItem(vMenuVersion);
            aboutMenu.AddItem(vMenuCredits);
            aboutMenu.AddItem(vMenuFounder);

            Main.Menus.Add(aboutMenu);
        }

        public static UIMenu Menu()
        {
            return aboutMenu;
        }
    }
}