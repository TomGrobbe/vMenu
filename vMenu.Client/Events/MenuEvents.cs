using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMenu.Client.Functions;
using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Events
{
    public class MenuEvents : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        public MenuEvents()
        {

        }

        [EventHandler("onResourceStop")]
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

        [EventHandler("onClientResourceStart")]
        private void OnClientResourceStart()
        {
            Debug.WriteLine("vMenu has started.");
        }
    }
}
