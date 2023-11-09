// System Libraries //
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// CitizenFX Libraries //
using CitizenFX.Core;
using ScaleformUI;
using vMenu.Client.Menus;
using vMenu.Client.Menus.OnlinePlayersSubmenus;
using static CitizenFX.Core.Native.API;
using FxEvents;
using Newtonsoft.Json;
using vMenu.Shared.Objects;
using ScaleformUI.Menu;

namespace vMenu.Client.Functions
{
    public class MenuFunctions : BaseScript
    {
        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        public static void QuitSession() => NetworkSessionEnd(true, true);

        public void SetBannerTexture()
        {
            if (Main.MenuBanner.TextureUrl != null)
            {
                Main.DuiObject = CreateDui(Main.MenuBanner.TextureUrl, 512, 128);
                string _duihandle = GetDuiHandle(Main.DuiObject);
                long _txdhandle = CreateRuntimeTxd("vmenu_textures_custom");
                CreateRuntimeTextureFromDuiHandle(_txdhandle, "menubanner", _duihandle);
                Main.MenuBanner.TextureDictionary = "vmenu_textures_custom";
            }
        }

        private Type[] GetClassesInMenusNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public void RestartMenu()
        {
            MenuHandler.CurrentMenu.Visible = false;
            MenuHandler.CloseAndClearHistory();
            //var test = GetClassesInMenusNamespace(Assembly.GetExecutingAssembly(), "vMenu.Client.Menus");

            new MainMenu();
            new OnlinePlayersMenu();
            new OnlinePlayerMenu();
            new MiscOptionsMenu();
            MainMenu.Menu().Visible = true;
        }

        public async void UpdateOnlinePlayers(UIMenu menu, UIMenuItem item)
        {
            try
            {
                string playersSt = await EventDispatcher.Get<string>("RequestPlayersList");

                List<OnlinePlayersCB> playersObj = JsonConvert.DeserializeObject<List<OnlinePlayersCB>>(playersSt);
                int PlayersLeftToAdd = playersObj.Count();

                Main.OnlinePlayers.Clear();

                playersObj.ForEach(async (OnlinePlayersCB playerData) =>
                {
                    CitizenFX.Core.Player ply = Main.PlayerList[playerData.Player.ServerId];

                    if (ply != null && ply.Character != null && ply.Character.Exists())
                    {
                        int mugshotHandle = RegisterPedheadshot(ply.Character.Handle);

                        while (!IsPedheadshotReady(mugshotHandle) || !IsPedheadshotValid(mugshotHandle))
                        {
                            await Delay(0);
                        }

                        string mugtxd = GetPedheadshotTxdString(mugshotHandle);

                        Main.OnlinePlayers.Add(new KeyValuePair<OnlinePlayersCB, string>(playerData, mugtxd));

                        UnregisterPedheadshot(mugshotHandle);

                        PlayersLeftToAdd--;
                    }
                    else
                    {
                        PlayersLeftToAdd--;
                    }
                });

                while (PlayersLeftToAdd > 0)
                {
                    await Delay(0);
                }

                OnlinePlayersMenu.ReplaceMenuItems(menu, item);
            }
            catch(Exception err)
            {
                Debug.WriteLine(err.ToString());
            }
        }

        public PointF GetMenuOffset()
        {
            if (Main.MenuAlign == Shared.Enums.MenuAlign.Left)
            {
                return new PointF(20, 20);
            }
            else
            {
                return new PointF(970, 20);
            }
        }
    }
}
