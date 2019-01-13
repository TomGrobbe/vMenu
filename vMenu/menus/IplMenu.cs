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

    /// <summary>
    /// 
    /// 
    ///             NOTE TO SELF (OR OTHER DEVS)
    ///
    /// This menu is different compared to others.
    /// Some parts of the menu are created here in this class, others parts are dynamically created in <see cref="IplManager"/>.
    /// It's messy, and I'm aware of that, but it'll work for now because I've got limited 'dynamic menu' support due to the dependency that I'm using,
    /// and the limitations of the export functions. These are not limitations of the dependency (bob74_ipl) but rather limitations of using the exports
    /// system with multiple coding languages.
    /// 
    /// </summary>






    public class IplMenu
    {
        // Variables
        private Menu menu;

        public bool EnableIplTeleports { get; private set; } = UserDefaults.IPLEnableTeleports;

        public Menu hangarsMenu = new Menu("Aircraft Hangars", "Aircraft Hangars Menu");
        public Menu apartmentsMenu = new Menu("Apartments", "Apartments and houses");
        public Menu bunkersMenu = new Menu("Bunkers", "bunkers menu");
        public Menu nightclubsMenu = new Menu("Nightclubs", "Night Clubs Menu");
        public Menu officesMenu = new Menu("CEO Offices", "CEO Offices Menu");
        public Menu warehousesMenu = new Menu("Warehouses", "Warehouses Menu");

        public IplMenu() { }

        public void CreateMenu()
        {
            // create the menu
            menu = new Menu("IPL Manager", "customize ipls using bob74_ipl");

            MenuController.AddSubmenu(menu, apartmentsMenu);

            // create checkbox items
            MenuCheckboxItem enableInteriorTeleportMarkers = new MenuCheckboxItem("Enable Interior Teleport Markers", "Enables or disables the markers and teleporting options for interiors loaded by the bob74_ipl resource.", EnableIplTeleports);
            MenuItem hangars = new MenuItem("Aircraft Hangars", "Customize the aircraft hanger interior.") { Label = "→→→", Enabled = false, LeftIcon = MenuItem.Icon.LOCK };
            MenuItem apartments = new MenuItem("Apartments", "Every apartment can have it's own style, customize them all here.") { Label = "→→→" };
            MenuItem bunkers = new MenuItem("Bunkers", "Customize the bunker interior.") { Label = "→→→", Enabled = false, LeftIcon = MenuItem.Icon.LOCK };
            MenuItem nightclubs = new MenuItem("Nightclubs", "Customze the Night Clubs interior.") { Label = "→→→", Enabled = false, LeftIcon = MenuItem.Icon.LOCK };
            MenuItem offices = new MenuItem("CEO Offices", "Customze the CEO Offices interior.") { Label = "→→→", Enabled = false, LeftIcon = MenuItem.Icon.LOCK };
            MenuItem warehouses = new MenuItem("Warehouses", "Customize the warehouses interior. (Coming soon)") { Label = "→→→", Enabled = false, LeftIcon = MenuItem.Icon.LOCK };

            MenuItem about = new MenuItem("About IPL Manager", "This submenu uses the bob74_ipl resource to customize interiors. Changes made are synced for everyone on the server, and will be automatically saved to the server. They will be restored next time the server starts.");

            // add items to the menu.
            menu.AddMenuItem(enableInteriorTeleportMarkers);

            menu.AddMenuItem(hangars);
            menu.AddMenuItem(apartments);
            menu.AddMenuItem(bunkers);
            menu.AddMenuItem(nightclubs);
            menu.AddMenuItem(offices);
            menu.AddMenuItem(warehouses);

            MenuController.BindMenuItem(menu, apartmentsMenu, apartments);


            menu.AddMenuItem(about);

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
