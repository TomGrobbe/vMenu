using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using FxEvents;

using CitizenFX.Core;

using vMenu.Client.Functions;
using vMenu.Client.KeyMappings;
using vMenu.Client.Settings;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Events
{
    public class MenuEvents
    {
        private static readonly object _padlock = new();
        private static MenuEvents _instance;
        public static Dictionary<vMenu.Shared.Enums.PermissionList, bool> Permissions;

        public MenuEvents()
        {
            Main.Instance.AddEventHandler("onResourceStop", OnResourceStop);
            Main.Instance.AddEventHandler("onClientResourceStart", OnClientResourceStart);
            Debug.WriteLine("MenuEvents Initialized");
        }

        internal static MenuEvents Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance ??= new MenuEvents();
                }
            }
        }

        private void OnResourceStop(string resource)
        {
            if (resource == GetCurrentResourceName())
            {
                ScaleformUI.MenuHandler.CloseAndClearHistory();

                Main.Menus.Clear();

                SetStreamedTextureDictAsNoLongerNeeded(Main.MenuBanner.TextureDictionary);

                if (Main.MenuBanner.TextureUrl != null)
                {
                    DestroyDui(Main.DuiObject);
                }
            }
        }

        private async void OnClientResourceStart(string resource)
        {

            if (resource == GetCurrentResourceName())
            {
                string permissions = await EventDispatcher.Get<string>("RequestPermissions");
                //Debug.WriteLine(permissions);
                Permissions = JsonConvert.DeserializeObject<Dictionary<vMenu.Shared.Enums.PermissionList, bool>>(permissions);
                Debug.WriteLine($"{Permissions[Shared.Enums.PermissionList.WRMenu]}");
                _ = MenuSettings.Instance;
                _ = MenuFunctions.Instance;
                _ = Commands.Instance;
                _ = NoClip.Instance;
                _ = Notify.Instance;
                _ = Subtitle.Instance;
                _ = RichPresence.Instance;

                MenuFunctions.Instance.SetBannerTexture();
                MenuFunctions.Instance.InitializeAllMenus();
                Debug.WriteLine("vMenu has started.");
            }
        }
    }
}
