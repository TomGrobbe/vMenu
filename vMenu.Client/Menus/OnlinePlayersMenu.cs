using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CitizenFX.Core;

using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Client.Menus.OnlinePlayersSubmenus;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.PlayerList;

namespace vMenu.Client.Menus
{
    public class OnlinePlayersMenu
    {
        private static UIMenu onlinePlayersMenu = null;

        public OnlinePlayersMenu()
        {
            var MenuLanguage = Languages.Menus["OnlinePlayersMenu"];

            onlinePlayersMenu = new Objects.vMenu(MenuLanguage.Subtitle ?? "Online Players").Create();

            UIMenuSeparatorItem noOnlinePlayersSpacer = new UIMenuSeparatorItem(MenuLanguage.Items["NoOnlinePlayersSpacer"].Name ?? "No Players Online", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };

            noOnlinePlayersSpacer.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

            onlinePlayersMenu.AddItem(noOnlinePlayersSpacer);

            Main.Menus.Add(onlinePlayersMenu);
        }

        public static UIMenu Menu()
        {
            return onlinePlayersMenu;
        }

        public static async void ReplaceMenuItems(UIMenu menu, UIMenuItem item)
        {
            var MenuLanguage = Languages.Menus["OnlinePlayersMenu"];

            onlinePlayersMenu.MenuItems.Clear();

            List<KeyValuePair<OnlinePlayersCB, string>> OnlinePlayers = Main.OnlinePlayers;
            int OnlinePlayersCount = OnlinePlayers.Count();

            foreach (KeyValuePair<OnlinePlayersCB, string> player in OnlinePlayers.OrderBy(a => a.Key.Player.Name))
            {
                var playerData = player.Key;
                var playerTexture = player.Value;

                UIMenuItem onlinePlayer = new UIMenuItem(playerData.Player.Name, MenuLanguage.Items["OnlinePlayerItem"].Description ?? "Click to view the options for this player", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
                onlinePlayer.LabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);
                onlinePlayer.SetRightLabel($"Server #{playerData.Player.ServerId}");
                onlinePlayer.RightLabelFont = new ItemFont(Main.CustomFontName, Main.CustomFontId);

                onlinePlayer.Activated += (sender, e) =>
                {
                    sender.SwitchTo(OnlinePlayersSubmenus.OnlinePlayerMenu.Menu(playerData, playerTexture), inheritOldMenuParams: true);
                };

                onlinePlayersMenu.AddItem(onlinePlayer);
                OnlinePlayersCount--;
            }

            while (OnlinePlayersCount > 0)
            {
                await BaseScript.Delay(0);
            }

            await menu.SwitchTo(Menu(), inheritOldMenuParams: true);

            item.Label = Languages.Menus["MainMenu"].Items["OnlinePlayersItem"].Name ?? "Online Players";
        }
    }
}
