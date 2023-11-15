using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

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
using vMenu.Client.Events;
using vMenu.Client.Settings;

using vMenu.Shared.Enums;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Menus
{
    public class MainMenu
    {
        private static UIMenu mainMenu = null;

        public MainMenu()
        {
            var MenuLanguage = Languages.Menus["MainMenu"];

            mainMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Main Menu").Create();

            UIMenuItem onlinePlayers = new UIMenuItem(MenuLanguage.Items["OnlinePlayersItem"].Name ?? "Online Players", MenuLanguage.Items["OnlinePlayersItem"].Description ?? "All currently connected players.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            onlinePlayers.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            onlinePlayers.SetRightLabel(">>>");

            UIMenuItem bannedPlayers = new UIMenuItem(MenuLanguage.Items["BannedPlayersItem"].Name ?? "Banned Players", MenuLanguage.Items["BannedPlayersItem"].Description ?? "View and manage all banned players in this menu.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            bannedPlayers.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            bannedPlayers.SetRightLabel(">>>");

            UIMenuItem playerRelatedOptions = new UIMenuItem(MenuLanguage.Items["PlayerOptionsItem"].Name ?? "Player Options", MenuLanguage.Items["PlayerOptionsItem"].Description ?? "Open this submenu for player related subcategories.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            playerRelatedOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            playerRelatedOptions.SetRightLabel(">>>");

            UIMenuItem vehicleRelatedOptions = new UIMenuItem(MenuLanguage.Items["VehicleOptionsItem"].Name ?? "Vehicle Options", MenuLanguage.Items["VehicleOptionsItem"].Description ?? "Open this submenu for vehicle related subcategories.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            vehicleRelatedOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            vehicleRelatedOptions.SetRightLabel(">>>");

            UIMenuItem worldRelatedOptions = new UIMenuItem(MenuLanguage.Items["WorldOptionsItem"].Name ?? "World Options", MenuLanguage.Items["WorldOptionsItem"].Description ?? "Open this submenu for world related subcategories.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            worldRelatedOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            worldRelatedOptions.SetRightLabel(">>>");

            UIMenuItem voiceChatSettings = new UIMenuItem(MenuLanguage.Items["VoiceChatOptionsItem"].Name ?? "Voice Chat Options", MenuLanguage.Items["VoiceChatOptionsItem"].Description ?? "Change Voice Chat options here.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            voiceChatSettings.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            voiceChatSettings.SetRightLabel(">>>");

            UIMenuItem recordingOptions = new UIMenuItem(MenuLanguage.Items["RockstarEditorOptionsItem"].Name ?? "R* Editor Options", MenuLanguage.Items["RockstarEditorOptionsItem"].Description ?? "In-game Rockstar Editor Options.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            recordingOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            recordingOptions.SetRightLabel(">>>");

            UIMenuItem miscOptions = new UIMenuItem(MenuLanguage.Items["MiscellaneousOptionsItem"].Name ?? "Miscellaneous Options", MenuLanguage.Items["MiscellaneousOptionsItem"].Description ?? "Miscellaneous vMenu options/settings can be configured here. You can also save your settings in this menu", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            miscOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            miscOptions.SetRightLabel(">>>");

            UIMenuItem aboutvMenu = new UIMenuItem(MenuLanguage.Items["AboutvMenuItem"].Name ?? "About vMenu", MenuLanguage.Items["AboutvMenuItem"].Description ?? "Information about vMenu.", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
            aboutvMenu.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            aboutvMenu.SetRightLabel(">>>");

            mainMenu.AddItem(onlinePlayers);
            mainMenu.AddItem(bannedPlayers);
            mainMenu.AddItem(playerRelatedOptions);
            mainMenu.AddItem(vehicleRelatedOptions);

            Debug.WriteLine($"{MenuEvents.Permissions[PermissionList.WRMenu]}");
            if (MenuEvents.Permissions[PermissionList.WRMenu])
            mainMenu.AddItem(worldRelatedOptions);

            mainMenu.AddItem(voiceChatSettings);
            mainMenu.AddItem(recordingOptions);
            mainMenu.AddItem(miscOptions);
            mainMenu.AddItem(aboutvMenu);

            onlinePlayers.Activated += (sender, i) =>
            {
                i.Label = MenuLanguage.Items["OnlinePlayersLoadingItem"].Name ?? "Loading Online Players";
                MenuFunctions.Instance.UpdateOnlinePlayers(sender, i);
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
