using CitizenFX.Core;
using CitizenFX.FiveM;
using ScaleformUI.Menu;
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

namespace vMenu.Client.Menus
{
    public class OnlinePlayersMenu : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        private static UIMenu onlinePlayersMenu = null;

        private static int updatingPlayerList;

        public OnlinePlayersMenu()
        {
            
        }

        public static UIMenu Menu()
        {
            onlinePlayersMenu = new UIMenu(Main.MenuBanner.BannerTitle, "Online Players", MenuFunctions.GetMenuOffset(), Main.MenuBanner.TextureDictionary, Main.MenuBanner.TextureName, false, true);
            UIMenuSeparatorItem onlinePlayerq = new UIMenuSeparatorItem("No Players Online", false);
            onlinePlayersMenu.AddItem(onlinePlayerq);

            Main.Menus.Add(onlinePlayersMenu);

            return onlinePlayersMenu;
        }

        public static async Coroutine UpdateOnlinePlayers()
        {
            PlayerList PlayersList = Players;
            updatingPlayerList = PlayersList.Count();

            async void UpdateStuffAsync()
            {
                onlinePlayersMenu.MenuItems.Clear();
                onlinePlayersMenu.Windows.Clear();

                foreach (Player player in PlayersList.OrderBy(a => a.Name))
                {
                    UIMenuItem onlinePlayer = new UIMenuItem(player.Name, "Click to view the options for this player");

                    int mugshot = RegisterPedheadshot(player.Character.Handle);

                    while (!IsPedheadshotReady(mugshot)) await WaitUntilNextFrame();

                    string mugtxd = GetPedheadshotTxdString(mugshot);

                    UIMenuDetailsWindow playerDetails = new UIMenuDetailsWindow($"~h~{player.Name}~h~", $"Server ID: {player.ServerId}\nLocal ID: {player.Handle}", $"Steam ID: test\nDiscord ID: yeet", new UIDetailImage()
                    {
                        Txd = mugtxd,
                        Txn = mugtxd,
                        Size = new Size(60, 60)
                    });

                    onlinePlayersMenu.AddWindow(playerDetails);
                    onlinePlayersMenu.AddItem(onlinePlayer);

                    onlinePlayersMenu.Visible = false;
                    onlinePlayersMenu.Visible = true;

                    updatingPlayerList--;
                }
            }

            Debug.WriteLine(updatingPlayerList);

            UpdateStuffAsync();

            while (updatingPlayerList > 0)
            {
                await Wait(100);
            }

            await Wait(5000);
        }
    }
}
