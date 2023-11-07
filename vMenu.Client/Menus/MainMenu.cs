// System Libraries //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.FiveM.Native.Natives;

// ScaleformUI Libraries //
using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.LobbyMenu;
using ScaleformUI.Menu;
using ScaleformUI.PauseMenu;
using ScaleformUI.PauseMenus;
using ScaleformUI.Radial;
using ScaleformUI.Radio;
using ScaleformUI.Scaleforms;
using vMenu.Client.Functions;

namespace vMenu.Client.Menus
{
    public class MainMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu mainMenu = null;

        public MainMenu()
        {
            mainMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Main Menu", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true)
            {
                MaxItemsOnScreen = 7,
                BuildingAnimation = MenuBuildingAnimation.NONE,
                ScrollingType = ScrollingType.CLASSIC,
                Enabled3DAnimations = false,
                MouseControlsEnabled = false,
                ControlDisablingEnabled = false,
            };

            UIMenuItem onlinePlayers = new UIMenuItem("Online Players", "All currently connected players");
            onlinePlayers.SetRightLabel(">>>");

            UIMenuItem miscOptions = new UIMenuItem("Misc. Options", "Miscellaneous vMenu options/settings can be configured here. You can also save your settings in this menu");
            miscOptions.SetRightLabel(">>>");

            mainMenu.AddItem(onlinePlayers);
            mainMenu.AddItem(miscOptions);

            onlinePlayers.Activated += (sender, i) =>
            {
                sender.SwitchTo(OnlinePlayersMenu.Menu(), inheritOldMenuParams: true);
            };

            miscOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(MiscOptionsMenu.Menu(), inheritOldMenuParams: true);
            };

            Main.Menus.Add(mainMenu);
        }

        public static UIMenu Menu()
        {
            return mainMenu;
        }
    }
}
