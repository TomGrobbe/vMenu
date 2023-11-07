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
using CitizenFX.Core.Native;
using vMenu.Client.Menus;
using vMenu.Client.Menus.OnlinePlayersSubmenus;
using static CitizenFX.FiveM.Native.Natives;

namespace vMenu.Client.Functions
{
    public class MenuFunctions : BaseScript
    {

        public static string Version { get { return GetResourceMetadata(GetCurrentResourceName(), "version", 0); } }

        public static void QuitSession() => NetworkSessionEnd(true, true);

        public MenuFunctions()
        {

        }

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
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        public void RestartMenu()
        {
            ScaleformUI.MenuHandler.CurrentMenu.Visible = false;
            ScaleformUI.MenuHandler.CloseAndClearHistory();
            //var test = GetClassesInMenusNamespace(Assembly.GetExecutingAssembly(), "vMenu.Client.Menus");

            new MainMenu();
            new OnlinePlayersMenu();
            new OnlinePlayerMenu();
            new MiscOptionsMenu();
            MainMenu.Menu().Visible = true;
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
