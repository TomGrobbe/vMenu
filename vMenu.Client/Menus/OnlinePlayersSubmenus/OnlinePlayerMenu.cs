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
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu onlinePlayerMenu = null;

        public OnlinePlayerMenu()
        {
            onlinePlayerMenu = new Objects.vMenu("Online Player").Create();

            Main.Menus.Add(onlinePlayerMenu);
        }

        public static UIMenu Menu(OnlinePlayersCB player, string texture)
        {
            onlinePlayerMenu.Windows.Clear();
            onlinePlayerMenu.MenuItems.Clear();

            UIMenuItem spectatePlayer = new UIMenuItem("Spectate Player", "Click to spectate this player", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);

            UIMenuDetailsWindow playerDetails = new UIMenuDetailsWindow($"~h~{player.Player.Name}~h~", $"Server ID: {player.Player.ServerId}\nPed Handle: {player.Player.CharacterHandle}", $"Steam ID: {player.SteamIdentifier ?? "None"}\nDiscord ID: {player.DiscordIdentifier ?? "None"}", new UIDetailImage()
            {
                Txd = texture,
                Txn = texture,
                Size = new Size(60, 60)
            });

            onlinePlayerMenu.AddWindow(playerDetails);
            onlinePlayerMenu.AddItem(spectatePlayer);

            return onlinePlayerMenu;
        }
    }
}
