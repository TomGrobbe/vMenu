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
using vMenu.Client.MenuSettings;

namespace vMenu.Client.Menus
{
    public class MainMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu mainMenu = null;

        public MainMenu()
        {
            mainMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Main Menu", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, menuSettings.Glare, menuSettings.AlternativeTitle, menuSettings.fadingTime)
            {
                MaxItemsOnScreen = menuSettings.maxItemsOnScreen,
                BuildingAnimation = menuSettings.buildingAnimation,
                ScrollingType = menuSettings.scrollingType,
                Enabled3DAnimations = menuSettings.enabled3DAnimations,
                MouseControlsEnabled = menuSettings.mouseControlsEnabled,
                ControlDisablingEnabled = menuSettings.controlDisablingEnabled,
                EnableAnimation = menuSettings.enableAnimation,
            };

            UIMenuItem onlinePlayers = new UIMenuItem("Online Players", "All currently connected players");
            onlinePlayers.SetRightLabel(">>>");

            UIMenuItem bannedPlayers = new UIMenuItem("Banned Players", "View and manage all banned players in this menu.");
            bannedPlayers.SetRightLabel(">>>");

            UIMenuItem playerRelatedOptions = new UIMenuItem("Player Related Options", "Open this submenu for player related subcategories.");
            playerRelatedOptions.SetRightLabel(">>>");

            UIMenuItem vehicleRelatedOptions = new UIMenuItem("Vehicle Related Options", "Open this submenu for vehicle related subcategories.");
            vehicleRelatedOptions.SetRightLabel(">>>");

            UIMenuItem worldRelatedOptions = new UIMenuItem("World Related Options", "Open this submenu for world related subcategories.");
            worldRelatedOptions.SetRightLabel(">>>");

            UIMenuItem voiceChatSettings = new UIMenuItem("Voice Chat Settings", "Change Voice Chat options here.");
            voiceChatSettings.SetRightLabel(">>>");

            UIMenuItem recordingOptions = new UIMenuItem("Recording Options (Broken)", "In-game recording options.");
            recordingOptions.SetRightLabel(">>>");

            UIMenuItem miscOptions = new UIMenuItem("Misc. Options", "Miscellaneous vMenu options/settings can be configured here. You can also save your settings in this menu");
            miscOptions.SetRightLabel(">>>");

            UIMenuItem aboutvMenu = new UIMenuItem("About vMenu", "Information about vMenu.");
            aboutvMenu.SetRightLabel(">>>");

            mainMenu.AddItem(onlinePlayers);
            mainMenu.AddItem(bannedPlayers);
            mainMenu.AddItem(playerRelatedOptions);
            mainMenu.AddItem(vehicleRelatedOptions);
            mainMenu.AddItem(worldRelatedOptions);
            mainMenu.AddItem(voiceChatSettings);
            mainMenu.AddItem(recordingOptions);
            mainMenu.AddItem(miscOptions);
            mainMenu.AddItem(aboutvMenu);

            onlinePlayers.Activated += (sender, i) =>
            {
                sender.SwitchTo(OnlinePlayersMenu.Menu(), inheritOldMenuParams: true);
            };

            bannedPlayers.Activated += (sender, i) =>
            {
                sender.SwitchTo(BannedPlayersMenu.Menu(), inheritOldMenuParams: true);
            };

            playerRelatedOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(PlayerRelatedOptions.Menu(), inheritOldMenuParams: true);
            };

            vehicleRelatedOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(VehicleRelatedOptions.Menu(), inheritOldMenuParams: true);
            };

            worldRelatedOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(WorldRelatedOptions.Menu(), inheritOldMenuParams: true);
            };

            voiceChatSettings.Activated += (sender, i) =>
            {
                sender.SwitchTo(VoiceChatSettings.Menu(), inheritOldMenuParams: true);
            };

            recordingOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(RecordingMenu.Menu(), inheritOldMenuParams: true);
            };

            miscOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(MiscOptionsMenu.Menu(), inheritOldMenuParams: true);
            };

            aboutvMenu.Activated += (sender, i) =>
            {
                sender.SwitchTo(AboutMenu.Menu(), inheritOldMenuParams: true);
            };

            Main.Menus.Add(mainMenu);
        }

        public static UIMenu Menu()
        {
            return mainMenu;
        }
    }
}
