using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient
{
    public class About
    {
        // Variables
        private UIMenu menu;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu("vMenu", "About vMenu", RightAlignMenus());

            // Create menu items.
            UIMenuItem version = new UIMenuItem("vMenu Version", $"This server is using vMenu ~b~~h~{MainMenu.Version}~h~~s~.");
            version.SetRightLabel($"~h~{MainMenu.Version}~h~");
            UIMenuItem credits = new UIMenuItem("About vMenu / Credits", $"vMenu is made by ~b~Vespura~s~. For more info, checkout ~b~www.vespura.com/vmenu~s~.");

            string serverInfoMessage = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_message);
            if (!string.IsNullOrEmpty(serverInfoMessage))
            {
                UIMenuItem serverInfo = new UIMenuItem("Server Info", serverInfoMessage);
                string siteUrl = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_website_url);
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    serverInfo.SetRightLabel($"{siteUrl}");
                }
                menu.AddItem(serverInfo);
            }
            menu.AddItem(version);
            menu.AddItem(credits);
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
