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
    public class VehicleOptions
    {
        #region Variables
        // Menu variable, will be defined in CreateMenu()
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;
        private static VehicleData vd = new VehicleData();

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = false;
        public bool VehicleEngineAlwaysOn { get; private set; } = true;
        public bool VehicleNoSiren { get; private set; } = false;
        public bool VehicleNoBikeHelemet { get; private set; } = false;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
        public float VehicleTorqueMultiplierAmount { get; private set; } = 2f;
        public float VehiclePowerMultiplierAmount { get; private set; } = 2f;
        #endregion

        #region CreateMenu()
        /// <summary>
        /// Create menu creates the vehicle options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Vehicle Options", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseEdgeEnabled = false
            };

            #region menu variables
            // Create Checkboxes.
            UIMenuCheckboxItem vehicleGod = new UIMenuCheckboxItem("God Mode", VehicleGodMode, "Disables any type of visual or physical damage to your vehicle.");
            UIMenuCheckboxItem vehicleEngineAO = new UIMenuCheckboxItem("Engine Always On", VehicleEngineAlwaysOn, "Keeps your vehicle engine on when you exit your vehicle.");
            UIMenuCheckboxItem vehicleNoSiren = new UIMenuCheckboxItem("Disable Siren", VehicleNoSiren, "Disables your vehicle's siren. Only works if your vehicle actually has a siren.");
            UIMenuCheckboxItem vehicleNoBikeHelmet = new UIMenuCheckboxItem("No Bike Helmet", VehicleNoBikeHelemet, "No longer auto-equip a helmet when getting on a bike or quad.");
            UIMenuCheckboxItem vehicleFreeze = new UIMenuCheckboxItem("Freeze Vehicle", VehicleFrozen, "Freeze your vehicle's position.");
            UIMenuCheckboxItem torqueEnabled = new UIMenuCheckboxItem("Enable Torque Multiplier", VehicleTorqueMultiplier, "Enables the torque multiplier selected from the list below.");
            UIMenuCheckboxItem powerEnabled = new UIMenuCheckboxItem("Enable Power Multiplier", VehiclePowerMultiplier, "Enables the power multiplier selected from the list below.");

            // Create buttons.
            UIMenuItem fixVehicle = new UIMenuItem("Repair Vehicle", "Repair any visual and physical damage present on your vehicle.");
            UIMenuItem cleanVehicle = new UIMenuItem("Wash Vehicle", "Clean your vehicle.");
            UIMenuItem toggleEngine = new UIMenuItem("Toggle Engine On/Off", "Turn your engine on/off.");
            UIMenuItem setLicensePlateText = new UIMenuItem("Set License Plate Text", "Enter a custom license plate for your vehicle.");
            UIMenuItem modMenuBtn = new UIMenuItem("Mod Menu", "Tune and customize your vehicle here.");
            UIMenuItem doorsMenuBtn = new UIMenuItem("Vehicle Doors", "Open, close, remove and restore vehicle doors here.");
            UIMenuItem windowsMenuBtn = new UIMenuItem("Vehicle Windows", "Roll your windows up/down or remove/restore your vehicle windows here.");
            UIMenuItem componentsMenuBtn = new UIMenuItem("Vehicle Extras", "Add/remove vehicle components/extras.");
            UIMenuItem liveriesMenuBtn = new UIMenuItem("Vehicle Liveries", "Style your vehicle with fancy liveries!");
            UIMenuItem colorsMenuBtn = new UIMenuItem("Vehicle Colors", "Style your vehicle even further by giving it some ~g~Snailsome ~s~colors!");
            UIMenuItem deleteBtn = new UIMenuItem("~r~Delete Vehicle", "Delete your vehicle, this ~r~can NOT be undone~s~!");
            deleteBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
            UIMenuItem flipVehicle = new UIMenuItem("Flip Vehicle", "Sets your current vehicle on all 4 wheels.");
            UIMenuItem vehicleAlarm = new UIMenuItem("Toggle Vehicle Alarm", "Starts/stops your vehicle's alarm.");
            UIMenuItem cycleSeats = new UIMenuItem("Cycle Through Vehicle Seats", "Cycle through the available vehicle seats.");

            // Create lists.
            var dirtlevel = new List<dynamic> { "Clean", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            UIMenuListItem setDirtLevel = new UIMenuListItem("Set Dirt Level", dirtlevel, 0, "Select how much dirt should be visible on your vehicle. This won't freeze the dirt level, it will only set it once.");
            var licensePlates = new List<dynamic> { GetLabelText("CMOD_PLA_0"), GetLabelText("CMOD_PLA_1"), GetLabelText("CMOD_PLA_2"), GetLabelText("CMOD_PLA_3"), GetLabelText("CMOD_PLA_4"), "North Yankton" };
            UIMenuListItem setLicensePlateType = new UIMenuListItem("License Plate Type", licensePlates, 0, "Select a license plate type and press enter to apply it to your vehicle.");
            var torqueMultiplierList = new List<dynamic> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            UIMenuListItem torqueMultiplier = new UIMenuListItem("Engine Torque Multiplier", torqueMultiplierList, 0, "Select the engine torque multiplier.");
            var powerMultiplierList = new List<dynamic> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            UIMenuListItem powerMultiplier = new UIMenuListItem("Engine Power Multiplier", powerMultiplierList, 0, "Select the engine power multiplier.");

            // Submenu's
            UIMenu vehicleModMenu = new UIMenu("Mod Menu", "Vehicle Mods", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenu vehicleDoorsMenu = new UIMenu("Vehicle Doors", "Vehicle Doors Management", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenu vehicleWindowsMenu = new UIMenu("Vehicle Windows", "Vehicle Windows Management", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenu vehicleComponents = new UIMenu("Vehicle Extras", "Vehicle Extras/Components", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenu vehicleLiveries = new UIMenu("Vehicle Liveries", "Vehicle Liveries.", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            UIMenu vehicleColors = new UIMenu("Vehicle Colors", "Vehicle Colors", MainMenu.MenuPosition)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            MainMenu.Mp.Add(vehicleColors);
            #endregion

            #region Add items to the menu.
            // Add everything to the menu.
            menu.AddItem(vehicleGod); // GOD MODE
            menu.AddItem(fixVehicle); // REPAIR VEHICLE
            menu.AddItem(cleanVehicle); // CLEAN VEHICLE
            menu.AddItem(setDirtLevel); // SET DIRT LEVEL
            menu.AddItem(toggleEngine); // TOGGLE ENGINE ON/OFF
            menu.AddItem(setLicensePlateText); // SET LICENSE PLATE TEXT
            menu.AddItem(setLicensePlateType); // SET LICENSE PLATE TYPE
            menu.AddItem(modMenuBtn); // MOD MENU
            menu.AddItem(colorsMenuBtn); // COLORS MENU
            menu.AddItem(liveriesMenuBtn); // LIVERIES MENU
            menu.AddItem(componentsMenuBtn); // COMPONENTS MENU
            menu.AddItem(doorsMenuBtn); // DOORS MENU
            menu.AddItem(windowsMenuBtn); // WINDOWS MENU
            menu.AddItem(deleteBtn); // DELETE VEHICLE
            menu.AddItem(vehicleFreeze); // FREEZE VEHICLE
            menu.AddItem(torqueEnabled); // TORQUE ENABLED
            menu.AddItem(torqueMultiplier); // TORQUE LIST
            menu.AddItem(powerEnabled); // POWER ENABLED
            menu.AddItem(powerMultiplier); // POWER LIST
            menu.AddItem(flipVehicle); // FLIP VEHICLE
            menu.AddItem(vehicleAlarm); // TOGGLE VEHICLE ALARM
            menu.AddItem(cycleSeats); // CYCLE THROUGH VEHICLE SEATS
            menu.AddItem(vehicleEngineAO); // LEAVE ENGINE RUNNING
            menu.AddItem(vehicleNoSiren); // DISABLE SIREN
            menu.AddItem(vehicleNoBikeHelmet); // DISABLE BIKE HELMET
            #endregion

            #region Bind Submenus to their buttons.
            menu.BindMenuToItem(vehicleColors, colorsMenuBtn);
            menu.BindMenuToItem(vehicleDoorsMenu, doorsMenuBtn);
            menu.BindMenuToItem(vehicleWindowsMenu, windowsMenuBtn);
            #endregion

            #region Handle button presses
            // Manage button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If the player is actually in a vehicle, continue.
                if (DoesEntityExist(cf.GetVehicle()))
                {
                    // Create a vehicle object.
                    Vehicle vehicle = new Vehicle(cf.GetVehicle());

                    // Check if the player is the driver of the vehicle, if so, continue.
                    if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == new Ped(PlayerPedId()))
                    {
                        // Repair vehicle.
                        if (item == fixVehicle)
                        {
                            vehicle.Repair();
                        }
                        // Clean vehicle.
                        else if (item == cleanVehicle)
                        {
                            vehicle.Wash();
                        }
                        // Delete vehicle.
                        else if (item == deleteBtn)
                        {
                            vehicle.Delete();
                            vehicle = null;
                        }
                        // Flip vehicle.
                        else if (item == flipVehicle)
                        {
                            SetVehicleOnGroundProperly(vehicle.Handle);
                        }
                        // Toggle alarm.
                        else if (item == vehicleAlarm)
                        {
                            if (vehicle.IsAlarmSounding)
                            {
                                // Set the duration to 0;
                                vehicle.AlarmTimeLeft = 0;
                                vehicle.IsAlarmSet = false;
                            }
                            else
                            {
                                // Randomize duration of the alarm and start the alarm.
                                vehicle.IsAlarmSet = true;
                                vehicle.AlarmTimeLeft = new Random().Next(8000, 45000);
                                vehicle.StartAlarm();
                            }
                        }
                        else if (item == toggleEngine)
                        {
                            vehicle.IsEngineRunning = !vehicle.IsEngineRunning;
                        }
                        else if (item == setLicensePlateText)
                        {
                            cf.SetLicensePlateTextAsync();
                        }
                    }

                    // If the player is not the driver seat and a button other than the option below (cycle seats) was pressed, notify them.
                    else if (item != cycleSeats)
                    {
                        Notify.Error("You must be in the driver seat to access these options!", true, false);
                    }

                    // Only this button can be used when you're not the driver of the car.
                    if (item == cycleSeats)
                    {
                        // Cycle vehicle seats
                        cf.CycleThroughSeats();
                    }
                }
                // If the player is not inside a vehicle, notify them.
                else
                {
                    Notify.Error("You must be inside a vehicle to access these options!", true, false);
                }
            };
            #endregion

            #region Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                //### removed because we actually want to handle these changes even if the player is not in a vehicle. ###//
                // ~~If the player is actually in a vehicle, continue.~~
                //if (DoesEntityExist(cf.GetVehicle()))
                //{

                // Create a vehicle object.
                Vehicle vehicle = new Vehicle(cf.GetVehicle());

                if (item == vehicleGod) // God Mode Toggled
                {
                    VehicleGodMode = _checked;
                }
                else if (item == vehicleFreeze) // Freeze Vehicle Toggled
                {
                    VehicleFrozen = _checked;
                }
                else if (item == torqueEnabled) // Enable Torque Multiplier Toggled
                {
                    VehicleTorqueMultiplier = _checked;
                }
                else if (item == powerEnabled) // Enable Power Multiplier Toggled
                {
                    VehiclePowerMultiplier = _checked;
                    if (_checked)
                    {
                        SetVehicleEnginePowerMultiplier(cf.GetVehicle(), VehiclePowerMultiplierAmount);
                    }
                    else
                    {
                        SetVehicleEnginePowerMultiplier(cf.GetVehicle(), 1f);
                    }
                }
                else if (item == vehicleEngineAO) // Leave Engine Running (vehicle always on) Toggled
                {
                    VehicleEngineAlwaysOn = _checked;
                }
                else if (item == vehicleNoSiren) // Disable Siren Toggled
                {
                    VehicleNoSiren = _checked;
                    vehicle.IsSirenSilent = _checked;
                }
                else if (item == vehicleNoBikeHelmet) // No Helemet Toggled
                {
                    VehicleNoBikeHelemet = _checked;
                }

                //}
            };
            #endregion

            #region Handle List Changes.
            // Handle list changes.
            menu.OnListChange += (sender, item, index) =>
            {
                // If the torque multiplier changed. Change the torque multiplier to the new value.
                if (item == torqueMultiplier)
                {
                    // Get the selected value and remove the "x" in the string with nothing.
                    var value = torqueMultiplierList[index].ToString().Replace("x", "");
                    // Convert the value to a float and set it as a public variable.
                    VehicleTorqueMultiplierAmount = float.Parse(value);
                }
                // If the power multiplier is changed. Change the power multiplier to the new value.
                else if (item == powerMultiplier)
                {
                    // Get the selected value. Remove the "x" from the string.
                    var value = powerMultiplierList[index].ToString().Replace("x", "");
                    // Conver the string into a float and set it to be the value of the public variable.
                    VehiclePowerMultiplierAmount = float.Parse(value);
                    if (VehiclePowerMultiplier)
                    {
                        SetVehicleEnginePowerMultiplier(cf.GetVehicle(), VehiclePowerMultiplierAmount);
                    }
                }
                else if (item == setLicensePlateType)
                {
                    // Check if the player is actually in a vehicle.
                    var veh = cf.GetVehicle();
                    if (DoesEntityExist(veh))
                    {
                        Vehicle vehicle = new Vehicle(veh);
                        // Set the license plate style.
                        switch (index)
                        {
                            case 0:
                                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite1;
                                break;
                            case 1:
                                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite2;
                                break;
                            case 2:
                                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite3;
                                break;
                            case 3:
                                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                                break;
                            case 4:
                                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlack;
                                break;
                            case 5:
                                vehicle.Mods.LicensePlateStyle = LicensePlateStyle.NorthYankton;
                                break;
                            default:
                                break;
                        }

                    }
                }
            };
            #endregion

            #region Handle List Items Selected
            menu.OnListSelect += (sender, item, index) =>
            {
                if (item == setDirtLevel)
                {
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        Vehicle veh = new Vehicle(cf.GetVehicle());
                        veh.DirtLevel = float.Parse(index.ToString());
                    }
                }
            };
            #endregion

            #region Create Colors Menu Items

            #region Create lists for each color.
            // Metallic Colors
            List<dynamic> Metallic = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.MetallicColors)
            {
                Metallic.Add(color.Key.ToString());
            }

            // Matte colors
            List<dynamic> Matte = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.MatteColors)
            {
                Matte.Add(color.Key.ToString());
            }

            // Metal colors
            List<dynamic> Metals = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.MetalColors)
            {
                Metals.Add(color.Key.ToString());
            }

            // Util Colors
            List<dynamic> Utils = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.UtilColors)
            {
                Utils.Add(color.Key.ToString());
            }

            // Worn colors
            List<dynamic> Worn = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.WornColors)
            {
                Worn.Add(color.Key.ToString());
            }

            // Pearlescent colors
            List<dynamic> Pearlescent = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.MetallicColors)
            {
                Pearlescent.Add(color.Key.ToString());
            }

            // Wheel colors
            List<dynamic> Wheels = new List<dynamic>();
            Wheels.Add("Default Alloy Color");
            foreach (KeyValuePair<string, int> color in vd.MetallicColors)
            {
                Wheels.Add(color.Key.ToString());
            }
            #endregion

            #region Create the headers + menu list items
            // Headers
            UIMenuItem primaryColorsHeader = new UIMenuItem("~g~Primary Colors:")
            {
                Enabled = false
            };
            UIMenuItem secondaryColorsHeader = new UIMenuItem("~g~Secondary Colors:")
            {
                Enabled = false
            };
            UIMenuItem otherColorsHeader = new UIMenuItem("~g~Other Colors:")
            {
                Enabled = false
            };

            // Primary Colors
            UIMenuListItem classicColors = new UIMenuListItem("Classic", Metallic, 0, "Select a Classic primary color.");
            // Metallic == Classic + Pearlescent
            UIMenuListItem metallicColors = new UIMenuListItem("Metallic", Metallic, 0, "Select a Metallic primary color.");
            UIMenuListItem matteColors = new UIMenuListItem("Matte", Matte, 0, "Select a Matte primary color.");
            UIMenuListItem metalsColors = new UIMenuListItem("Metals", Metals, 0, "Select a Metals primary color.");
            UIMenuListItem utilsColors = new UIMenuListItem("Util", Utils, 0, "Select a Util primary color.");
            UIMenuListItem wornColors = new UIMenuListItem("Worn", Worn, 0, "Select a Worn primary color.");

            // Secondary Colors.
            UIMenuListItem classicColors2 = new UIMenuListItem("Classic", Metallic, 0, "Select a Classic secondary color.");
            UIMenuListItem metallicColors2 = new UIMenuListItem("Metallic", Metallic, 0, "Select a Metallic secondary color.");
            UIMenuListItem matteColors2 = new UIMenuListItem("Matte", Matte, 0, "Select a Matte secondary color.");
            UIMenuListItem metalsColors2 = new UIMenuListItem("Metals", Metals, 0, "Select a Metals secondary color.");
            UIMenuListItem utilsColors2 = new UIMenuListItem("Util", Utils, 0, "Select a Util secondary color.");
            UIMenuListItem wornColors2 = new UIMenuListItem("Worn", Worn, 0, "Select a Worn secondary color.");

            // Other Colors
            // Pearlescent == Classic + Classic on top of secondary color.
            UIMenuListItem pearlescentColors = new UIMenuListItem("Pearlescent", Metallic, 0, "Select a pearlescent color.");
            UIMenuListItem wheelColors = new UIMenuListItem("Wheels Color", Wheels, 0, "Select a color for your wheels.");
            #endregion

            #region Add the items to the colors menu.
            // Primary Colors
            vehicleColors.AddItem(primaryColorsHeader); // header
            vehicleColors.AddItem(classicColors);
            vehicleColors.AddItem(metallicColors);
            vehicleColors.AddItem(matteColors);
            vehicleColors.AddItem(metalsColors);
            vehicleColors.AddItem(utilsColors);
            vehicleColors.AddItem(wornColors);

            // Secondary Colors
            vehicleColors.AddItem(secondaryColorsHeader); // header
            vehicleColors.AddItem(classicColors2);
            vehicleColors.AddItem(metallicColors2);
            vehicleColors.AddItem(matteColors2);
            vehicleColors.AddItem(metalsColors2);
            vehicleColors.AddItem(utilsColors2);
            vehicleColors.AddItem(wornColors2);

            // Other Colors
            vehicleColors.AddItem(otherColorsHeader); // header
            vehicleColors.AddItem(pearlescentColors);
            vehicleColors.AddItem(wheelColors);
            #endregion

            #region Handle Vehicle Color Changes
            vehicleColors.OnListChange += (sender, item, index) =>
            {
                // Get the current vehicle.
                var veh = cf.GetVehicle();

                // Check if the vehicle exists and isn't dead and the player is the driver of the vehicle.
                if (DoesEntityExist(veh) && !IsEntityDead(veh) && GetPedInVehicleSeat(veh, -1) == PlayerPedId())
                {
                    // Get the primary and secondary colors from the current vehicle..
                    int primary = 0;
                    int secondary = 0;
                    int pearlescent = 0;
                    int wheels = 0;
                    GetVehicleColours(veh, ref primary, ref secondary);
                    GetVehicleExtraColours(veh, ref pearlescent, ref wheels);

                    // If any color of the primary colors is selected, which isn't the pearlescent or metallic option, then reset the pearlescent color to black;
                    if (item == classicColors || item == matteColors || item == metalsColors || item == utilsColors || item == wornColors)
                    {
                        pearlescent = 0;
                    }
                    // Classic / Metallic (primary)
                    if (item == classicColors || item == metallicColors)
                    {
                        primary = vd.MetallicColors[Metallic[index]];
                        if (item == metallicColors) // If the primary metallic changes, 
                        {
                            pearlescent = vd.MetallicColors[Metallic[index]];
                        }
                    }
                    // Classic / Metallic (secondary)
                    else if (item == classicColors2 || item == metallicColors2)
                    {
                        secondary = vd.MetallicColors[Metallic[index]];
                    }

                    // Matte (primary)
                    else if (item == matteColors)
                    {
                        primary = vd.MatteColors[Matte[index]];
                    }
                    // Matte (secondary)
                    else if (item == matteColors2)
                    {
                        secondary = vd.MatteColors[Matte[index]];
                    }

                    // Metals (primary)
                    else if (item == metalsColors)
                    {
                        primary = vd.MetalColors[Metals[index]];
                    }
                    // Metals (secondary)
                    else if (item == metalsColors2)
                    {
                        secondary = vd.MetalColors[Metals[index]];
                    }

                    // Utils (primary)
                    else if (item == utilsColors)
                    {
                        primary = vd.UtilColors[Utils[index]];
                    }
                    // Utils (secondary)
                    else if (item == utilsColors2)
                    {
                        secondary = vd.UtilColors[Utils[index]];
                    }

                    // Worn (primary)
                    else if (item == wornColors)
                    {
                        primary = vd.WornColors[Worn[index]];
                    }
                    // Worn (secondary)
                    else if (item == wornColors2)
                    {
                        secondary = vd.WornColors[Worn[index]];
                    }

                    // Pearlescent
                    else if (item == pearlescentColors)
                    {
                        pearlescent = vd.MetallicColors[Metallic[index]];
                    }

                    // Wheel colors
                    else if (item == wheelColors)
                    {
                        if (index == 0)
                        {
                            // "Default Alloy Color" is not in the metallic list, so we have to manually account for this one.
                            wheels = 156;
                        }
                        else
                        {
                            wheels = vd.MetallicColors[Metallic[index + 1]];
                        }
                    }
                    // Set the mod kit so we can modify things.
                    SetVehicleModKit(veh, 0);

                    // Set all the colors.
                    SetVehicleColours(cf.GetVehicle(), primary, secondary);
                    SetVehicleExtraColours(veh, pearlescent, wheels);
                }

            };
            #endregion

            #endregion
        }
        #endregion

        /// <summary>
        /// Public get method for the menu. Checks if the menu exists, if not create the menu first.
        /// </summary>
        /// <returns>Returns the Vehicle Options menu.</returns>
        public UIMenu GetMenu()
        {
            // If menu doesn't exist. Create one.
            if (menu == null)
            {
                CreateMenu();
            }
            // Return the menu.
            return menu;
        }
    }
}
