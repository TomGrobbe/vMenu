using MenuAPI;

namespace vMenuClient.menus
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
            var version = new MenuItem("vMenu Version", $"This server is using vMenu ~b~~h~{MainMenu.Version}~h~~s~.")
            {
                Label = $"~h~{MainMenu.Version}~h~"
            };
            var credits = new MenuItem("About vMenu / Credits", "vMenu is made by ~b~Vespura~s~. For more info, checkout ~b~www.vespura.com/vmenu~s~. Thank you to: Deltanic, Brigliar, IllusiveTea, Shayan Doust, zr0iq and Golden for your contributions!");

            var serverInfoMessage = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_message);
            if (!string.IsNullOrEmpty(serverInfoMessage))
            {
                var serverInfo = new MenuItem("Server Info", serverInfoMessage);
                var siteUrl = vMenuShared.ConfigManager.GetSettingsString(vMenuShared.ConfigManager.Setting.vmenu_server_info_website_url);
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
