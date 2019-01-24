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
    public class PersonalVehicle
    {
        // Variables
        private Menu menu;
        public bool EnableVehicleBlip { get; private set; } = UserDefaults.PVEnableVehicleBlip;

        // Empty constructor
        public PersonalVehicle() { }

        public Vehicle CurrentPersonalVehicle { get; internal set; } = null;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Menu
            menu = new Menu(GetSafePlayerName(Game.Player.Name), "Personal Vehicle Options");

            // menu items
            MenuItem setVehice = new MenuItem("Set Vehicle", "Sets your current vehicle as your personal vehicle. If you already have a personal vehicle set then this will override your selection.") { Label = "Current Vehicle: None" };
            MenuItem toggleEngine = new MenuItem("Toggle Engine", "Toggles the engine on or off, even when you're not inside of the vehicle. This does not work if someone else is currently using your vehicle.");
            MenuListItem toggleLights = new MenuListItem("Set Vehicle Lights", new List<string>() { "Force On", "Force Off", "Reset" }, 0, "This will enable or disable your vehicle headlights, the engine of your vehicle needs to be running for this to work.");
            MenuItem kickAllPassengers = new MenuItem("Kick Passengers", "This will remove all passengers from your personal vehicle.");
            //MenuItem
            MenuItem lockDoors = new MenuItem("Lock Vehicle Doors", "This will lock all your vehicle doors for all players. Anyone already inside will always be able to leave the vehicle, even if the doors are locked.");
            MenuItem unlockDoors = new MenuItem("Unlock Vehicle Doors", "This will unlock all your vehicle doors for all players.");
            MenuItem soundHorn = new MenuItem("Sound Horn", "Sounds the horn of the vehicle.");
            MenuItem toggleAlarm = new MenuItem("Toggle Alarm Sound", "Toggles the vehicle alarm sound on or off. This does not set an alarm. It only toggles the current sounding status of the alarm.");

            // This is always allowed if this submenu is created/allowed.
            menu.AddMenuItem(setVehice);

            // Add conditional features.

            // Toggle engine.
            if (IsAllowed(Permission.PVToggleEngine))
            {
                menu.AddMenuItem(toggleEngine);
            }

            // Toggle lights
            if (IsAllowed(Permission.PVToggleLights))
            {
                menu.AddMenuItem(toggleLights);
            }

            // Kick vehicle passengers
            if (IsAllowed(Permission.PVKickPassengers))
            {
                menu.AddMenuItem(kickAllPassengers);
            }

            // Lock and unlock vehicle doors
            if (IsAllowed(Permission.PVLockDoors))
            {
                menu.AddMenuItem(lockDoors);
                menu.AddMenuItem(unlockDoors);
            }

            if (IsAllowed(Permission.PVSoundHorn))
            {
                menu.AddMenuItem(soundHorn);
            }

            if (IsAllowed(Permission.PVToggleAlarm))
            {
                menu.AddMenuItem(toggleAlarm);
            }
            // more coming soon
            //if (IsAllowed(Permission.PVToggleEngine)) { }


            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == setVehice)
                {
                    if (CurrentPersonalVehicle == null)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            var veh = GetVehicle();
                            if (Game.PlayerPed == veh.Driver)
                            {

                            }
                            else
                            {
                                Notify.Error(CommonErrors.NeedToBeTheDriver);
                            }
                        }
                        else
                        {
                            Notify.Error(CommonErrors.NoVehicle);
                        }
                    }
                    else
                    {

                    }
                    
                }

                else if (item == toggleEngine)
                {

                }

                else if (item == toggleLights)
                {

                }

                else if (item == kickAllPassengers)
                {

                }

                else if (item == lockDoors || item == unlockDoors)
                {
                    bool _lock = item == lockDoors;

                }
            };
        }

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
