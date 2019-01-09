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
    public class VehicleSpawner
    {
        // Variables
        private Menu menu;
        public static Dictionary<string, uint> AddonVehicles;

        public bool SpawnInVehicle { get; private set; } = UserDefaults.VehicleSpawnerSpawnInside;
        public bool ReplaceVehicle { get; private set; } = UserDefaults.VehicleSpawnerReplacePrevious;
        public static List<bool> allowedCategories;

        private void CreateMenu()
        {
            #region initial setup.
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Vehicle Spawner");

            // Create the buttons and checkboxes.
            MenuItem spawnByName = new MenuItem("Spawn Vehicle By Model Name", "Enter the name of a vehicle to spawn.");
            MenuCheckboxItem spawnInVeh = new MenuCheckboxItem("Spawn Inside Vehicle", "This will teleport you into the vehicle when you spawn it.", SpawnInVehicle);
            MenuCheckboxItem replacePrev = new MenuCheckboxItem("Replace Previous Vehicle", "This will automatically delete your previously spawned vehicle when you spawn a new vehicle.", ReplaceVehicle);

            // Add the items to the menu.
            if (IsAllowed(Permission.VSSpawnByName))
            {
                menu.AddMenuItem(spawnByName);
            }
            menu.AddMenuItem(spawnInVeh);
            menu.AddMenuItem(replacePrev);
            #endregion

            #region addon cars menu
            // Vehicle Addons List
            Menu addonCarsMenu = new Menu("Addon Vehicles", "Spawn An Addon Vehicle");
            MenuItem addonCarsBtn = new MenuItem("Addon Vehicles", "A list of addon vehicles available on this server.");
            addonCarsBtn.Label = "→→→";

            menu.AddMenuItem(addonCarsBtn);

            if (IsAllowed(Permission.VSAddon))
            {
                if (AddonVehicles != null)
                {
                    if (AddonVehicles.Count > 0)
                    {
                        MenuController.BindMenuItem(menu, addonCarsMenu, addonCarsBtn);
                        MenuController.AddSubmenu(menu, addonCarsMenu);
                        foreach (KeyValuePair<string, uint> veh in AddonVehicles)
                        {
                            string localizedName = GetLabelText(GetDisplayNameFromVehicleModel(veh.Value));
                            string name = localizedName != "NULL" ? localizedName : GetDisplayNameFromVehicleModel(veh.Value);
                            name = name != "CARNOTFOUND" ? name : veh.Key;
                            MenuItem carBtn = new MenuItem(name, $"Click to spawn {name}.");
                            carBtn.Label = $"({veh.Key.ToString()})";
                            if (!IsModelInCdimage(veh.Value))
                            {
                                carBtn.Enabled = false;
                                carBtn.Description = "This vehicle is not available. Please ask the server owner to check if the vehicle is being streamed correctly.";
                                carBtn.LeftIcon = MenuItem.Icon.LOCK;
                            }
                            addonCarsMenu.AddMenuItem(carBtn);
                        }

                        addonCarsMenu.OnItemSelect += (sender, item, index) =>
                        {
                            SpawnVehicle(AddonVehicles.ElementAt(index).Key, SpawnInVehicle, ReplaceVehicle);
                        };
                        addonCarsMenu.RefreshIndex();
                        //addonCarsMenu.UpdateScaleform();
                    }
                    else
                    {
                        addonCarsBtn.Enabled = false;
                        addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                        addonCarsBtn.Description = "There are no addon vehicles available on this server.";
                    }
                }
                else
                {
                    addonCarsBtn.Enabled = false;
                    addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                    addonCarsBtn.Description = "The list containing all addon cars could not be loaded, is it configured properly?";
                }
            }
            else
            {
                addonCarsBtn.Enabled = false;
                addonCarsBtn.LeftIcon = MenuItem.Icon.LOCK;
                addonCarsBtn.Description = "Access to this list has been restricted by the server owner.";
            }
            #endregion

            #region vehicle classes submenus
            // Create the submenus for each category.
            var vl = new Vehicles();

            // Loop through all the vehicle classes.
            for (var vehClass = 0; vehClass < 22; vehClass++)
            {
                // Get the class name.
                string className = GetLocalizedName($"VEH_CLASS_{vehClass.ToString()}");

                // Create a button & a menu for it, add the menu to the menu pool and add & bind the button to the menu.
                MenuItem btn = new MenuItem(className, $"Spawn a vehicle from the ~o~{className} ~s~class.");
                btn.Label = "→→→";

                Menu vehicleClassMenu = new Menu("Vehicle Spawner", className);

                MenuController.AddSubmenu(menu, vehicleClassMenu);
                menu.AddMenuItem(btn);

                if (allowedCategories[vehClass])
                {
                    MenuController.BindMenuItem(menu, vehicleClassMenu, btn);
                }
                else
                {
                    btn.LeftIcon = MenuItem.Icon.LOCK;
                    btn.Description = "This category has been disabled by the server owner.";
                    btn.Enabled = false;
                }

                // Create a dictionary for the duplicate vehicle names (in this vehicle class).
                var duplicateVehNames = new Dictionary<string, int>();

                #region Add vehicles per class
                // Loop through all the vehicles in the vehicle class.
                foreach (var veh in vl.VehicleClasses[className])
                {
                    // Convert the model name to start with a Capital letter, converting the other characters to lowercase. 
                    var properCasedModelName = veh[0].ToString().ToUpper() + veh.ToLower().Substring(1);

                    // Get the localized vehicle name, if it's "NULL" (no label found) then use the "properCasedModelName" created above.
                    var vehName = GetVehDisplayNameFromModel(veh) != "NULL" ? GetVehDisplayNameFromModel(veh) : properCasedModelName;

                    // Loop through all the menu items and check each item's title/text and see if it matches the current vehicle (display) name.
                    var duplicate = false;
                    for (var itemIndex = 0; itemIndex < vehicleClassMenu.Size; itemIndex++)
                    {
                        // If it matches...
                        if (vehicleClassMenu.GetMenuItems()[itemIndex].Text.ToString() == vehName)
                        {

                            // Check if the model was marked as duplicate before.
                            if (duplicateVehNames.Keys.Contains(vehName))
                            {
                                // If so, add 1 to the duplicate counter for this model name.
                                duplicateVehNames[vehName]++;
                            }

                            // If this is the first duplicate, then set it to 2.
                            else
                            {
                                duplicateVehNames[vehName] = 2;
                            }

                            // The model name is a duplicate, so get the modelname and add the duplicate amount for this model name to the end of the vehicle name.
                            vehName += $" ({duplicateVehNames[vehName].ToString()})";

                            // Then create and add a new button for this vehicle.

                            if (DoesModelExist(veh))
                            {
                                var vehBtn = new MenuItem(vehName) { Enabled = true };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                            }
                            else
                            {
                                var vehBtn = new MenuItem(vehName, "This vehicle is not available because the model could not be found in your game files. If this is a DLC vehicle, make sure the server is streaming it.") { Enabled = false };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                                vehBtn.RightIcon = MenuItem.Icon.LOCK;
                            }

                            // Mark duplicate as true and break from the loop because we already found the duplicate.
                            duplicate = true;
                            break;
                        }
                    }

                    // If it's not a duplicate, add the model name.
                    if (!duplicate)
                    {
                        if (DoesModelExist(veh))
                        {
                            var vehBtn = new MenuItem(vehName) { Enabled = true };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                        }
                        else
                        {
                            var vehBtn = new MenuItem(vehName, "This vehicle is not available because the model could not be found in your game files. If this is a DLC vehicle, make sure the server is streaming it.") { Enabled = false };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                            vehBtn.RightIcon = MenuItem.Icon.LOCK;
                        }
                    }
                }
                #endregion

                // Handle button presses
                vehicleClassMenu.OnItemSelect += (sender2, item2, index2) =>
                {
                    SpawnVehicle(vl.VehicleClasses[className][index2], SpawnInVehicle, ReplaceVehicle);
                };
            }
            #endregion

            #region handle events
            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == spawnByName)
                {
                    // Passing "custom" as the vehicle name, will ask the user for input.
                    SpawnVehicle("custom", SpawnInVehicle, ReplaceVehicle);
                }
            };

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (item == spawnInVeh)
                {
                    SpawnInVehicle = _checked;
                }
                else if (item == replacePrev)
                {
                    ReplaceVehicle = _checked;
                }
            };
            #endregion
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
