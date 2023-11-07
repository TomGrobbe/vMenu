using CitizenFX.Core;
using CitizenFX.FiveM;
using ScaleformUI;
using ScaleformUI.Menu;
using ScaleformUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vMenu.Client.Functions;
using static CitizenFX.FiveM.Native.Natives;
using static CitizenFX.FiveM.PlayerList;
using vMenu.Client.MenuSettings;

namespace vMenu.Client.Menus
{
    public class OnlinePlayersMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu onlinePlayersMenu = null;

        public OnlinePlayersMenu()
        {
            onlinePlayersMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Online Players", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, menuSettings.Glare, menuSettings.AlternativeTitle, menuSettings.fadingTime)
            {
                MaxItemsOnScreen = menuSettings.maxItemsOnScreen,
                BuildingAnimation = menuSettings.buildingAnimation,
                ScrollingType = menuSettings.scrollingType,
                Enabled3DAnimations = menuSettings.enabled3DAnimations,
                MouseControlsEnabled = menuSettings.mouseControlsEnabled,
                ControlDisablingEnabled = menuSettings.controlDisablingEnabled,
                EnableAnimation = menuSettings.enableAnimation,
            };

            UIMenuSeparatorItem onlinePlayerq = new UIMenuSeparatorItem("No Players Online", false);
            onlinePlayersMenu.AddItem(onlinePlayerq);

            Main.Menus.Add(onlinePlayersMenu);
        }

        public static UIMenu Menu()
        {
            return onlinePlayersMenu;
        }

        public static async Coroutine UpdateOnlinePlayers()
        {
            PlayerList PlayersList = new PlayerList();
            int onlinePlayers = PlayersList.Count();
            int onlinePlayersUpdateListCount = onlinePlayers;

            onlinePlayersMenu.Windows.Clear();
            onlinePlayersMenu.MenuItems.Clear();

            foreach (Player player in PlayersList.OrderBy(a => a.Name))
            {
                UIMenuItem onlinePlayer = new UIMenuItem(player.Name, "Click to view the options for this player", menuSettings.BackgroundColor, menuSettings.HighlightColor);
                onlinePlayer.SetRightLabel($"Server ID: {player.ServerId}");

                int mugshot = RegisterPedheadshot(player.Character.Handle);

                while (!IsPedheadshotReady(mugshot)) await Yield();

                string mugtxd = GetPedheadshotTxdString(mugshot);

                UnregisterPedheadshot(mugshot);

                onlinePlayer.Activated += (sender, e) =>
                {
                    sender.SwitchTo(OnlinePlayersSubmenus.OnlinePlayerMenu.Menu(player, mugtxd), inheritOldMenuParams: true);
                };

                onlinePlayersMenu.AddItem(onlinePlayer);

                onlinePlayersUpdateListCount--;
            }

            while (onlinePlayersUpdateListCount > 0)
            {
                await Wait(100);
            }

            await Wait(3000);
        }
    }
}
