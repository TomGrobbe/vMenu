using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using FxEvents;

using Logger;

using ScaleformUI.Menu;

using vMenu.Client.Functions;
using vMenu.Shared.Enums;
using vMenu.Shared.Objects;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client
{
    public class Main : BaseScript
    {
        public static PlayerList PlayerList;
        public static List<KeyValuePair<OnlinePlayersCB, string>> OnlinePlayers = new List<KeyValuePair<OnlinePlayersCB, string>>();

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
            //TextureUrl = "https://domain.com/image.png"
        };

        public Main()
        {
            PlayerList = Players;
            EventDispatcher.Initalize("vMenu:Inbound", "vMenu:Outbound", "vMenu:Signature");

            MenuFunctions.SetBannerTexture();
        }
    }
}
