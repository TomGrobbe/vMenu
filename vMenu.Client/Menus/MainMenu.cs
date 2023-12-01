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
using static vMenu.Client.Functions.MenuFunctions;
using vMenu.Client.Events;
using vMenu.Client.Settings;

using vMenu.Shared.Enums;

using static CitizenFX.Core.Native.API;
using vMenu.Client.Objects;

namespace vMenu.Client.Menus
{
    public class MainMenu
    {
        private static UIMenu mainMenu = null;

        public MainMenu()
        {
            var MenuLanguage = Languages.Menus["MainMenu"];

            mainMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Main Menu").Create();
            mainMenu.SetMouse(MenuSettings.MouseControlsEnabled, MenuSettings.MouseEdgeEnabled, MenuSettings.MouseWheelControlEnabled, false, true);

            UIMenuItem onlinePlayers = new vMenuItem(MenuLanguage.Items["OnlinePlayersItem"], "Online Players", "All currently connected players.").Create();
            onlinePlayers.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            onlinePlayers.SetRightLabel(">>>");

            UIMenuItem bannedPlayers = new vMenuItem(MenuLanguage.Items["BannedPlayersItem"], "Banned Players", "View and manage all banned players in this menu.").Create();
            bannedPlayers.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            bannedPlayers.SetRightLabel(">>>");

            UIMenuItem playerRelatedOptions = new vMenuItem(MenuLanguage.Items["PlayerOptionsItem"], "Player Options", "Open this submenu for player related subcategories.").Create();
            playerRelatedOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            playerRelatedOptions.SetRightLabel(">>>");

            UIMenuItem vehicleRelatedOptions = new vMenuItem(MenuLanguage.Items["VehicleOptionsItem"], "Vehicle Options", "Open this submenu for vehicle related subcategories.").Create();
            vehicleRelatedOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            vehicleRelatedOptions.SetRightLabel(">>>");

            UIMenuItem worldRelatedOptions = new vMenuItem(MenuLanguage.Items["WorldOptionsItem"], "World Options", "Open this submenu for world related subcategories.").Create();
            worldRelatedOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            worldRelatedOptions.SetRightLabel(">>>");

            UIMenuItem voiceChatSettings = new vMenuItem(MenuLanguage.Items["VoiceChatOptionsItem"], "Voice Chat Options", "Change Voice Chat options here.").Create();
            voiceChatSettings.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            voiceChatSettings.SetRightLabel(">>>");

            UIMenuItem recordingOptions = new vMenuItem(MenuLanguage.Items["RockstarEditorOptionsItem"], "R* Editor Options", "In-game Rockstar Editor Options.").Create();
            recordingOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            recordingOptions.SetRightLabel(">>>");

            UIMenuItem hudOptions = new vMenuItem(MenuLanguage.Items["HudOptionsItem"], "Hud Options", "Hud Options Menu.").Create();
            hudOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            hudOptions.SetRightLabel(">>>");

            UIMenuItem miscOptions = new vMenuItem(MenuLanguage.Items["MiscellaneousOptionsItem"], "Miscellaneous Options", "Miscellaneous vMenu options/settings can be configured here. You can also save your settings in this menu").Create();
            miscOptions.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            miscOptions.SetRightLabel(">>>");

            UIMenuItem aboutvMenu = new vMenuItem(MenuLanguage.Items["AboutvMenuItem"], "About vMenu", "Information about vMenu.").Create();
            aboutvMenu.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
            aboutvMenu.SetRightLabel(">>>");

            mainMenu.AddItem(onlinePlayers);

            if (IsAllowed(Permission.Staff))
            mainMenu.AddItem(bannedPlayers);

            if (IsAllowed(Permission.POMenu))
            mainMenu.AddItem(playerRelatedOptions);

            if (IsAllowed(Permission.VOMenu))            
            mainMenu.AddItem(vehicleRelatedOptions);

            if (IsAllowed(Permission.WRMenu))
            mainMenu.AddItem(worldRelatedOptions);

            if (IsAllowed(Permission.VCMenu))
            mainMenu.AddItem(voiceChatSettings);

            mainMenu.AddItem(recordingOptions);
            mainMenu.AddItem(hudOptions);
            mainMenu.AddItem(miscOptions);
            mainMenu.AddItem(aboutvMenu);

            onlinePlayers.Activated += (sender, i) =>
            {
                i.Label = MenuLanguage.Items["OnlinePlayersLoadingItem"].Name ?? "Loading Online Players";
                Instance.UpdateOnlinePlayers(sender, i);
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

            recordingOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(RecordingMenu.Menu(), inheritOldMenuParams: true);
            };

            hudOptions.Activated += (sender, i) =>
            {
                sender.SwitchTo(HudOptionsMenu.Menu(), inheritOldMenuParams: true);
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
