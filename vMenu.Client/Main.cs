// System Libraries //
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.FiveM.Native.Natives;

// vMenu Namespaces //
using vMenu.Client.Functions;
using vMenu.Shared.Objects;
using vMenu.Shared.Enums;
using ScaleformUI.Menu;
using CitizenFX.FiveM;

namespace vMenu.Client
{
    public class Main : BaseScript
    {
        public static MenuFunctions MenuFunctions = new MenuFunctions();

        public static MenuAlign MenuAlign = MenuAlign.Left;

        public static List<UIMenu> Menus = new List<UIMenu>();

        public static long DuiObject = 0;
        public static MenuBannerObject MenuBanner = new MenuBannerObject()
        {
            BannerTitle = "vMenu ~y~Revamped",

            TextureDictionary = "vmenu_textures",
            TextureName = "menubanner",

            // This will be used instead of the texture set above (only when set) - Link must be a static image or gif (.png, .jpg, .gif, etc...) //
            // The image/gif MUST be 288x130px //
            // SET THIS AS null IF NOT USING //
            TextureUrl = null
        };

        public Main()
        {
            MenuFunctions.SetBannerTexture();
            Tick += Client.Menus.OnlinePlayersMenu.UpdateOnlinePlayers;
        }
    }
}
