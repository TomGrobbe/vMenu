// System Libraries //
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// CitizenFX Libraries //
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.FiveM.Native.Natives;

namespace vMenu.Client.Functions
{
    public class MenuFunctions : BaseScript
    {
        public MenuFunctions()
        {

        }

        public void SetBannerTexture()
        {
            if (Main.MenuBanner.TextureUrl != null)
            {
                Main.DuiObject = CreateDui(Main.MenuBanner.TextureUrl, 288, 130);
                string _duihandle = GetDuiHandle(Main.DuiObject);
                long _txdhandle = CreateRuntimeTxd("vmenu_textures_custom");
                CreateRuntimeTextureFromDuiHandle(_txdhandle, "menubanner", _duihandle);
                Main.MenuBanner.TextureDictionary = "vmenu_textures_custom";
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
                return new PointF(1080 - 288 - 20, 20);
            }
        }
    }
}
