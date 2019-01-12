using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using MenuAPI;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using static vMenuClient.IplManager;
using static vMenuShared.PermissionsManager;
using static vMenuShared.ConfigManager;

namespace vMenuClient
{
    public class IplMenu
    {
        // Variables
        private Menu menu;

        public bool EnableIplTeleports { get; private set; } = UserDefaults.IPLEnableTeleports;

        public IplMenu()
        {

        }

        public void CreateMenu()
        {
            // create the menu
            menu = new Menu("IPL Management", "bob74_ipl integration menu");

            // create checkbox items
            MenuCheckboxItem enableInteriorTeleportMarkers = new MenuCheckboxItem("Enable Interior Teleport Markers", "Enables or disables the markers and teleporting options for interiors loaded by the bob74_ipl resource.", EnableIplTeleports);

            // add items to the menu.
            menu.AddMenuItem(enableInteriorTeleportMarkers);

            // check for checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == enableInteriorTeleportMarkers)
                {
                    EnableIplTeleports = _checked;
                }
            };


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
