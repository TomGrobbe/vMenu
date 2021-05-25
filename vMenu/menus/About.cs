using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class About
    {
        // Variables
        private Menu menu;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu("vMenu", "About vMenu");

            // Create menu items.
            MenuItem version = new MenuItem("vMenu Version", $"This server is using vMenu ~b~~h~{MainMenu.Version}~h~~s~.")
            {
                Label = $"~h~{MainMenu.Version}~h~"
            };
            MenuItem credits = new MenuItem("About vMenu / Credits", "vMenu is made by ~b~Vespura~s~. For more info, checkout ~b~www.vespura.com/vmenu~s~. Thank you to: Deltanic, Brigliar, IllusiveTea, Shayan Doust and zr0iq for your contributions.");

            string serverInfoMessage = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_message);
            if (!string.IsNullOrEmpty(serverInfoMessage))
            {
                MenuItem serverInfo = new MenuItem("Server Info", serverInfoMessage);
                string siteUrl = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_website_url);
                if (!string.IsNullOrEmpty(siteUrl))
                {
                    serverInfo.Label = $"{siteUrl}";
                }
                menu.AddMenuItem(serverInfo);
            }
            menu.AddMenuItem(version);
            menu.AddMenuItem(credits);
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}
