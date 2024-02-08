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
        public static bool NoClipEnabled { get { return Functions.NoClip.IsNoclipActive(); } set { Functions.NoClip.SetNoclipActive(value); } }
        private static readonly object _padlock = new();
        private static MenuEvents _instance;
        public static Dictionary<vMenu.Shared.Enums.Permission, bool> Permissions;

        public MenuEvents()
        {
            Main.Instance.AddEventHandler("onResourceStop", OnResourceStop);
            Main.Instance.AddEventHandler("onClientResourceStart", OnClientResourceStart);
            Main.Instance.AddEventHandler("vMenu:RestartMenu", RestartMenu);
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

        private async void RestartMenu( )
        {
            string permissions = await EventDispatcher.Get<string>("RequestPermissions", Game.Player.ServerId);
            Permissions = JsonConvert.DeserializeObject<Dictionary<vMenu.Shared.Enums.Permission, bool>>(permissions);
            ScaleformUI.MenuHandler.CurrentMenu.Visible = false;
            Notify.Alert("Rebuilding menu due to your permissions being changed");
            _ = MenuFunctions.Instance;
            _ = Commands.Instance;
            _ = NoClip.Instance;
            _ = TimeWeather.Instance;
            NoClipEnabled = false;
            MenuFunctions.Instance.RestartMenu();
            Debug.WriteLine(permissions);
        }

        private async void OnClientResourceStart(string resource)
        {

            if (resource == GetCurrentResourceName())
            {
                string permissions = await EventDispatcher.Get<string>("RequestPermissions", Game.Player.ServerId);
                //Debug.WriteLine(permissions);
                Permissions = JsonConvert.DeserializeObject<Dictionary<vMenu.Shared.Enums.Permission, bool>>(permissions);
                if (!Convar.GetSettingsBool("vmenu_use_permissions"))
                {
                    Notify.Alert("vMenu is set up to ignore permissions, default permissions will be used.");
                }
                
                _ = MenuSettings.Instance;
                _ = MenuFunctions.Instance;
                _ = Commands.Instance;
                _ = NoClip.Instance;
                _ = TimeWeather.Instance;
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
