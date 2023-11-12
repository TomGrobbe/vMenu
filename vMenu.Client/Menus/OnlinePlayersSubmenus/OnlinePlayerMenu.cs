using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

namespace vMenu.Client.Menus.OnlinePlayersSubmenus
{
    public class OnlinePlayerMenu
    {
        private static UIMenu onlinePlayerMenu = null;

        public OnlinePlayerMenu()
        {
            var MenuLanguage = Languages.Menus["OnlinePlayerMenu"];

            onlinePlayerMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Online Player").Create();

            Main.Menus.Add(onlinePlayerMenu);
        }

        public static UIMenu Menu(OnlinePlayersCB player, string texture)
        {
            var MenuLanguage = Languages.Menus["OnlinePlayerMenu"];

            onlinePlayerMenu.Windows.Clear();
            onlinePlayerMenu.MenuItems.Clear();

            UIMenuDetailsWindow playerDetails = new UIMenuDetailsWindow($"~h~{player.Player.Name}~h~", $"{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["ServerIdVar"] ?? "Server Id"}: {player.Player.ServerId}\n{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["PedHandleVar"] ?? "Ped Handle"}: {player.Player.CharacterHandle}", $"{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["SteamIdVar"] ?? "Steam Id"}: {player.SteamIdentifier ?? "None"}\n{MenuLanguage.Items["PlayerDetailsWindow"].DynamicDetails["DiscordIdVar"] ?? "Discord Id"}: {player.DiscordIdentifier ?? "None"}", new UIDetailImage()
            {
                Txd = texture,
                Txn = texture,
                Size = new Size(60, 60)
            });

            UIMenuItem spectatePlayer = new UIMenuItem(MenuLanguage.Items["SpectatePlayerItem"].Name ?? "Spectate Player", MenuLanguage.Items["SpectatePlayerItem"].Description ?? "Click to spectate this player", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);

            onlinePlayerMenu.AddWindow(playerDetails);
            onlinePlayerMenu.AddItem(spectatePlayer);

            return onlinePlayerMenu;
        }
    }
}
