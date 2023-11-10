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
using static CitizenFX.Core.Native.API;

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
using vMenu.Client.Settings;

namespace vMenu.Client.Menus
{
    public class MainMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu mainMenu = null;

        public MainMenu()
        {
            mainMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Main Menu", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, MenuSettings.Glare, MenuSettings.AlternativeTitle, MenuSettings.FadingTime)
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

            UIMenuItem onlinePlayers = new UIMenuItem("Online Players", "All currently connected players", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            onlinePlayers.SetRightLabel(">>>");

            UIMenuItem bannedPlayers = new UIMenuItem("Banned Players", "View and manage all banned players in this menu.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            bannedPlayers.SetRightLabel(">>>");

            UIMenuItem playerRelatedOptions = new UIMenuItem("Player Options", "Open this submenu for player related subcategories.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            playerRelatedOptions.SetRightLabel(">>>");

            UIMenuItem vehicleRelatedOptions = new UIMenuItem("Vehicle Options", "Open this submenu for vehicle related subcategories.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vehicleRelatedOptions.SetRightLabel(">>>");

            UIMenuItem worldRelatedOptions = new UIMenuItem("World Options", "Open this submenu for world related subcategories.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            worldRelatedOptions.SetRightLabel(">>>");

            UIMenuItem voiceChatSettings = new UIMenuItem("Voice Chat Options", "Change Voice Chat options here.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            voiceChatSettings.SetRightLabel(">>>");

            UIMenuItem recordingOptions = new UIMenuItem("Recording Options (Broken)", "In-game recording options.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            recordingOptions.SetRightLabel(">>>");

            UIMenuItem miscOptions = new UIMenuItem("Misc. Options", "Miscellaneous vMenu options/settings can be configured here. You can also save your settings in this menu", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            miscOptions.SetRightLabel(">>>");

            UIMenuItem aboutvMenu = new UIMenuItem("About vMenu", "Information about vMenu.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
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
                i.Label = "Loading Online Players";
                MenuFunctions.UpdateOnlinePlayers(sender, i);
            };

            bannedPlayers.Activated += (sender, i) =>
            {
                sender.SwitchTo(BannedPlayersMenu.Menu(), inheritOldMenuParams: true);
            };

            playerRelatedOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(PlayerOptionsMenu.Menu(), inheritOldMenuParams: true);
            };

            vehicleRelatedOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(VehicleOptionsMenu.Menu(), inheritOldMenuParams: true);
            };

            worldRelatedOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(WorldOptionsMenu.Menu(), inheritOldMenuParams: true);
            };

            voiceChatSettings.Activated += (sender, i) =>
            {
                sender.SwitchTo(VoiceChatOptionsMenu.Menu(), inheritOldMenuParams: true);
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
