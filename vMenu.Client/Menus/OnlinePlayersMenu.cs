using CitizenFX.Core;
using ScaleformUI;
using ScaleformUI.Elements;
using ScaleformUI.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vMenu.Client.Functions;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;

using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.PlayerList;

namespace vMenu.Client.Menus
{
    public class OnlinePlayersMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu onlinePlayersMenu = null;

        public OnlinePlayersMenu()
        {
            onlinePlayersMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Online Players", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, MenuSettings.Glare, MenuSettings.AlternativeTitle, MenuSettings.FadingTime)
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

            UIMenuSeparatorItem onlinePlayerq = new UIMenuSeparatorItem("No Players Online", false)
            {
                MainColor = MenuSettings.Colours.Spacers.BackgroundColor,
                HighlightColor = MenuSettings.Colours.Spacers.HighlightColor,
                HighlightedTextColor = MenuSettings.Colours.Spacers.HighlightedTextColor,
                TextColor = MenuSettings.Colours.Spacers.TextColor
            };

            onlinePlayersMenu.AddItem(onlinePlayerq);

            Main.Menus.Add(onlinePlayersMenu);
        }

        public static UIMenu Menu()
        {
            return onlinePlayersMenu;
        }

        public static async void ReplaceMenuItems(UIMenu menu, UIMenuItem item)
        {
            onlinePlayersMenu.MenuItems.Clear();

            List<KeyValuePair<OnlinePlayersCB, string>> OnlinePlayers = Main.OnlinePlayers;
            int OnlinePlayersCount = OnlinePlayers.Count();

            foreach (KeyValuePair<OnlinePlayersCB, string> player in OnlinePlayers.OrderBy(a => a.Key.Player.Name))
            {
                var playerData = player.Key;
                var playerTexture = player.Value;

                UIMenuItem onlinePlayer = new UIMenuItem(playerData.Player.Name, "Click to view the options for this player", MenuSettings.Colours.Items.BackgroundColor, MenuSettings.Colours.Items.HighlightColor);
                onlinePlayer.SetRightLabel($"Server #{playerData.Player.ServerId}");

                onlinePlayer.Activated += (sender, e) =>
                {
                    sender.SwitchTo(OnlinePlayersSubmenus.OnlinePlayerMenu.Menu(playerData, playerTexture), inheritOldMenuParams: true);
                };

                onlinePlayersMenu.AddItem(onlinePlayer);
                OnlinePlayersCount--;
            }

            while (OnlinePlayersCount > 0)
            {
                await Delay(0);
            }

            await menu.SwitchTo(Menu(), inheritOldMenuParams: true);

            item.Label = "Online Players";
        }
    }
}
