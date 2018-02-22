using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class VehicleSpawner
    {
        // Variables
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;

        public bool SpawnInVehicle { get; private set; } = false;

        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Vehicle Spawner", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseEdgeEnabled = false
            };
            UIMenuItem spawnByName = new UIMenuItem("Spawn Vehicle By Name", "Enter a vehicle model name to spawn.");
            menu.AddItem(spawnByName);
            menu.OnItemSelect += (sender, item, index) =>
           {
               cf.SpawnVehicle("custom");
           };
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
