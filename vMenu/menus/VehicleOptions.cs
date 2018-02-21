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
        private static Notification Notify = new Notification();
        //private static Subtitles Subtitle = new Subtitles();
        private static CommonFunctions cf = new CommonFunctions();

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = false;
        public bool VehicleEngineAlwaysOn { get; private set; } = false;
        public bool VehicleNoSiren { get; private set; } = false;
        public bool VehicleNoBikeHelemet { get; private set; } = false;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
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
            var torqueMultiplierList = new List<dynamic> { 1 };
            UIMenuListItem torqueMultiplier = new UIMenuListItem("Engine Torque Multiplier", torqueMultiplierList, 0, "Select the engine torque multiplier.");
            var engineMultiplierList = new List<dynamic> { 1 };
            UIMenuListItem powerMultiplier = new UIMenuListItem("Engine Power Multiplier", engineMultiplierList, 0, "Select the engine power multiplier.");

            // Submenu's
            UIMenu vehicleModMenu = new UIMenu("Mod Menu", "Vehicle Mods");
            UIMenu vehicleDoorsMenu = new UIMenu("Vehicle Doors", "Vehicle Doors Management");
            UIMenu vehicleWindowsMenu = new UIMenu("Vehicle Windows", "Vehicle Windows Management");
            UIMenu vehicleComponents = new UIMenu("Vehicle Extras", "Vehicle Extras/Components");
            UIMenu vehicleLiveries = new UIMenu("Vehicle Liveries", "Vehicle Liveries.");
            UIMenu vehicleColors = new UIMenu("Vehicle Colors", "Vehicle Colors");
            #endregion

            #region Add items to the menu.
            // Add everything to the menu.
            menu.AddItem(vehicleGod); // GOD MODE
            menu.AddItem(fixVehicle); // REPAIR VEHICLE
            menu.AddItem(cleanVehicle); // CLEAN VEHICLE
            menu.AddItem(setDirtLevel); // SET DIRT LEVEL
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
                                vehicle.AlarmTimeLeft = 0;
                                vehicle.IsAlarmSet = false;
                            }
                            else
                            {
                                vehicle.IsAlarmSet = true;
                                vehicle.AlarmTimeLeft = 8000;
                                vehicle.StartAlarm();
                            }
                        }
                        // Cycle vehicle seats (to be implemented later)
                        else if (item == cycleSeats)
                        {
                            cf.CycleThroughSeats();
                        }
                        
                    }
                    // If the player is not the driver, notify them.
                    else
                    {
                        Notify.Error("You must be in the driver seat to access these options!", true, false);
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
                // If the player is actually in a vehicle, continue.
                if (DoesEntityExist(cf.GetVehicle()))
                {
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
                    }
                    else if (item == vehicleEngineAO) // Leave Engine Running (vehicle always on) Toggled
                    {
                        VehicleEngineAlwaysOn = _checked;
                    }
                    else if (item == vehicleNoSiren) // Disable Siren Toggled
                    {
                        VehicleNoSiren = _checked;
                    }
                    else if (item == vehicleNoBikeHelmet) // No Helemet Toggled
                    {
                        VehicleNoBikeHelemet = _checked;
                    }
                }
            };


            #endregion

        }
        #endregion

        /// <summary>
        /// Public get method for the menu. Checks if the menu exists, if not create the menu first.
        /// </summary>
        /// <returns>Returns the Vehicle Options menu.</returns>
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
