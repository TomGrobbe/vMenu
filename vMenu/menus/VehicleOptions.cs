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
    public class VehicleOptions
    {
        #region Variables
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Submenus
        public Menu VehicleModMenu { get; private set; }
        public Menu VehicleDoorsMenu { get; private set; }
        public Menu VehicleWindowsMenu { get; private set; }
        public Menu VehicleComponentsMenu { get; private set; }
        public Menu VehicleLiveriesMenu { get; private set; }
        public Menu VehicleColorsMenu { get; private set; }
        public Menu DeleteConfirmMenu { get; private set; }
        public Menu VehicleUnderglowMenu { get; private set; }

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = UserDefaults.VehicleGodMode;
        public bool VehicleSpecialGodMode { get; private set; } = UserDefaults.VehicleSpecialGodMode;
        public bool VehicleEngineAlwaysOn { get; private set; } = UserDefaults.VehicleEngineAlwaysOn;
        public bool VehicleNeverDirty { get; private set; } = UserDefaults.VehicleNeverDirty;
        public bool VehicleNoSiren { get; private set; } = UserDefaults.VehicleNoSiren;
        public bool VehicleNoBikeHelemet { get; private set; } = UserDefaults.VehicleNoBikeHelmet;
        public bool FlashHighbeamsOnHonk { get; private set; } = UserDefaults.VehicleHighbeamsOnHonk;
        public bool DisablePlaneTurbulence { get; private set; } = UserDefaults.VehicleDisablePlaneTurbulence;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
        public float VehicleTorqueMultiplierAmount { get; private set; } = 2f;
        public float VehiclePowerMultiplierAmount { get; private set; } = 2f;

        private Dictionary<MenuItem, int> vehicleExtras = new Dictionary<MenuItem, int>();
        #endregion

        #region CreateMenu()
        /// <summary>
        /// Create menu creates the vehicle options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Vehicle Options");

            #region menu variables
            // Create Checkboxes.
            MenuCheckboxItem vehicleGod = new MenuCheckboxItem("Vehicle God Mode", "Your vehicle will not be able to take visual or physical damage.", VehicleGodMode);
            MenuCheckboxItem vehicleSpecialGod = new MenuCheckboxItem("Special Vehicle God Mode", "This option repairs your vehicle immediately when it gets damaged. This special god mode is needed for vehicles like the Phantom Wedge to keep it from breaking down with regular god mode turned on.", VehicleSpecialGodMode);
            MenuCheckboxItem vehicleNeverDirty = new MenuCheckboxItem("Keep Vehicle Clean", "This will constantly clean your car if the vehicle dirt level goes above 0. Note that this only cleans ~o~dust~s~ or ~o~dirt~s~. This does not clean mud, snow or other ~r~damage decals~s~. Repair your vehicle to remove them.", VehicleNeverDirty);
            MenuCheckboxItem vehicleEngineAO = new MenuCheckboxItem("Engine Always On", "Keeps your vehicle engine on when you exit your vehicle.", VehicleEngineAlwaysOn);
            MenuCheckboxItem vehicleNoTurbulence = new MenuCheckboxItem("Disable Plane Turbulence", "Disables the turbulence for all planes. Note only works for planes. Helicopters and other flying vehicles are not supported.", DisablePlaneTurbulence);
            MenuCheckboxItem vehicleNoSiren = new MenuCheckboxItem("Disable Siren", "Disables your vehicle's siren. Only works if your vehicle actually has a siren.", VehicleNoSiren);
            MenuCheckboxItem vehicleNoBikeHelmet = new MenuCheckboxItem("No Bike Helmet", "No longer auto-equip a helmet when getting on a bike or quad.", VehicleNoBikeHelemet);
            MenuCheckboxItem vehicleFreeze = new MenuCheckboxItem("Freeze Vehicle", "Freeze your vehicle's position.", VehicleFrozen);
            MenuCheckboxItem torqueEnabled = new MenuCheckboxItem("Enable Torque Multiplier", "Enables the torque multiplier selected from the list below.", VehicleTorqueMultiplier);
            MenuCheckboxItem powerEnabled = new MenuCheckboxItem("Enable Power Multiplier", "Enables the power multiplier selected from the list below.", VehiclePowerMultiplier);
            MenuCheckboxItem highbeamsOnHonk = new MenuCheckboxItem("Flash Highbeams On Honk", "Turn on your highbeams on your vehicle when honking your horn. Does not work during the day when you have your lights turned off.", FlashHighbeamsOnHonk);

            // Create buttons.
            MenuItem fixVehicle = new MenuItem("Repair Vehicle", "Repair any visual and physical damage present on your vehicle.");
            MenuItem cleanVehicle = new MenuItem("Wash Vehicle", "Clean your vehicle.");
            MenuItem toggleEngine = new MenuItem("Toggle Engine On/Off", "Turn your engine on/off.");
            MenuItem setLicensePlateText = new MenuItem("Set License Plate Text", "Enter a custom license plate for your vehicle.");
            MenuItem modMenuBtn = new MenuItem("Mod Menu", "Tune and customize your vehicle here.");
            modMenuBtn.Label = "→→→";
            MenuItem doorsMenuBtn = new MenuItem("Vehicle Doors", "Open, close, remove and restore vehicle doors here.");
            doorsMenuBtn.Label = "→→→";
            MenuItem windowsMenuBtn = new MenuItem("Vehicle Windows", "Roll your windows up/down or remove/restore your vehicle windows here.");
            windowsMenuBtn.Label = "→→→";
            MenuItem componentsMenuBtn = new MenuItem("Vehicle Extras", "Add/remove vehicle components/extras.");
            componentsMenuBtn.Label = "→→→";
            MenuItem liveriesMenuBtn = new MenuItem("Vehicle Liveries", "Style your vehicle with fancy liveries!");
            liveriesMenuBtn.Label = "→→→";
            MenuItem colorsMenuBtn = new MenuItem("Vehicle Colors", "Style your vehicle even further by giving it some ~g~Snailsome ~s~colors!");
            colorsMenuBtn.Label = "→→→";
            MenuItem underglowMenuBtn = new MenuItem("Vehicle Neon Kits", "Make your vehicle shine with some fancy neon underglow!");
            underglowMenuBtn.Label = "→→→";
            MenuItem flipVehicle = new MenuItem("Flip Vehicle", "Sets your current vehicle on all 4 wheels.");
            MenuItem vehicleAlarm = new MenuItem("Toggle Vehicle Alarm", "Starts/stops your vehicle's alarm.");
            MenuItem cycleSeats = new MenuItem("Cycle Through Vehicle Seats", "Cycle through the available vehicle seats.");
            List<string> lights = new List<string>()
            {
                "Hazard Lights",
                "Left Indicator",
                "Right Indicator",
                //"Interior Lights",
                //"Taxi Light", // this doesn't seem to work no matter what.
                "Helicopter Spotlight",
            };
            MenuListItem vehicleLights = new MenuListItem("Vehicle Lights", lights, 0, "Turn vehicle lights on/off.");
            MenuItem deleteBtn = new MenuItem("~r~Delete Vehicle", "Delete your vehicle, this ~r~can NOT be undone~s~!");
            deleteBtn.LeftIcon = MenuItem.Icon.WARNING;
            deleteBtn.Label = "→→→";
            MenuItem deleteNoBtn = new MenuItem("NO, CANCEL", "NO, do NOT delete my vehicle and go back!");
            MenuItem deleteYesBtn = new MenuItem("~r~YES, DELETE", "Yes I'm sure, delete my vehicle please, I understand that this cannot be undone.");
            deleteYesBtn.LeftIcon = MenuItem.Icon.WARNING;

            // Create lists.
            var dirtlevel = new List<string> { "No Dirt", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            MenuListItem setDirtLevel = new MenuListItem("Set Dirt Level", dirtlevel, 0, "Select how much dirt should be visible on your vehicle, press ~r~enter~s~ " +
                "to apply the selected level.");
            var licensePlates = new List<string> { GetLabelText("CMOD_PLA_0"), GetLabelText("CMOD_PLA_1"), GetLabelText("CMOD_PLA_2"), GetLabelText("CMOD_PLA_3"),
                GetLabelText("CMOD_PLA_4"), "North Yankton" };
            MenuListItem setLicensePlateType = new MenuListItem("License Plate Type", licensePlates, 0, "Choose a license plate type and press ~r~enter ~s~to apply " +
                "it to your vehicle.");
            var torqueMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            MenuListItem torqueMultiplier = new MenuListItem("Set Engine Torque Multiplier", torqueMultiplierList, 0, "Set the engine torque multiplier.");
            var powerMultiplierList = new List<string> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            MenuListItem powerMultiplier = new MenuListItem("Set Engine Power Multiplier", powerMultiplierList, 0, "Set the engine power multiplier.");
            List<string> speedLimiterOptions = new List<string>() { "Set", "Reset", "Custom Speed Limit" };
            MenuListItem speedLimiter = new MenuListItem("Speed Limiter", speedLimiterOptions, 0, "Set your vehicles max speed to your ~y~current speed~s~. Resetting your vehicles max speed will set the max speed of your current vehicle back to default. Only your current vehicle is affected by this option.");
            #endregion

            #region Submenus
            // Submenu's
            VehicleModMenu = new Menu("Mod Menu", "Vehicle Mods");
            VehicleDoorsMenu = new Menu("Vehicle Doors", "Vehicle Doors Management");
            VehicleWindowsMenu = new Menu("Vehicle Windows", "Vehicle Windows Management");
            VehicleComponentsMenu = new Menu("Vehicle Extras", "Vehicle Extras/Components");
            VehicleLiveriesMenu = new Menu("Vehicle Liveries", "Vehicle Liveries");
            VehicleColorsMenu = new Menu("Vehicle Colors", "Vehicle Colors");
            DeleteConfirmMenu = new Menu("Confirm Action", "Delete Vehicle, Are You Sure?");
            VehicleUnderglowMenu = new Menu("Vehicle Neon Kits", "Vehicle Neon Underglow Options");

            MenuController.AddSubmenu(menu, VehicleModMenu);
            MenuController.AddSubmenu(menu, VehicleDoorsMenu);
            MenuController.AddSubmenu(menu, VehicleWindowsMenu);
            MenuController.AddSubmenu(menu, VehicleComponentsMenu);
            MenuController.AddSubmenu(menu, VehicleLiveriesMenu);
            MenuController.AddSubmenu(menu, VehicleColorsMenu);
            MenuController.AddSubmenu(menu, DeleteConfirmMenu);
            MenuController.AddSubmenu(menu, VehicleUnderglowMenu);
            #endregion

            #region Add items to the menu.
            // Add everything to the menu. (based on permissions)
            if (IsAllowed(Permission.VOGod)) // GOD MODE
            {
                menu.AddMenuItem(vehicleGod);
            }
            if (IsAllowed(Permission.VOSpecialGod)) // special god mode
            {
                menu.AddMenuItem(vehicleSpecialGod);
            }
            if (IsAllowed(Permission.VORepair)) // REPAIR VEHICLE
            {
                menu.AddMenuItem(fixVehicle);
            }
            if (IsAllowed(Permission.VOKeepClean))
            {
                menu.AddMenuItem(vehicleNeverDirty);
            }
            if (IsAllowed(Permission.VOWash))
            {
                menu.AddMenuItem(cleanVehicle); // CLEAN VEHICLE
                menu.AddMenuItem(setDirtLevel); // SET DIRT LEVEL
            }
            if (IsAllowed(Permission.VOEngine)) // TOGGLE ENGINE ON/OFF
            {
                menu.AddMenuItem(toggleEngine);
            }
            if (IsAllowed(Permission.VOSpeedLimiter)) // SPEED LIMITER
            {
                menu.AddMenuItem(speedLimiter);
            }
            if (IsAllowed(Permission.VOChangePlate))
            {
                menu.AddMenuItem(setLicensePlateText); // SET LICENSE PLATE TEXT
                menu.AddMenuItem(setLicensePlateType); // SET LICENSE PLATE TYPE
            }
            if (IsAllowed(Permission.VOMod)) // MOD MENU
            {
                menu.AddMenuItem(modMenuBtn);
            }
            if (IsAllowed(Permission.VOColors)) // COLORS MENU
            {
                menu.AddMenuItem(colorsMenuBtn);
            }
            if (IsAllowed(Permission.VOUnderglow)) // UNDERGLOW EFFECTS
            {
                menu.AddMenuItem(underglowMenuBtn);
                MenuController.BindMenuItem(menu, VehicleUnderglowMenu, underglowMenuBtn);
            }
            if (IsAllowed(Permission.VOLiveries)) // LIVERIES MENU
            {
                menu.AddMenuItem(liveriesMenuBtn);
            }
            if (IsAllowed(Permission.VOComponents)) // COMPONENTS MENU
            {
                menu.AddMenuItem(componentsMenuBtn);
            }
            if (IsAllowed(Permission.VODoors)) // DOORS MENU
            {
                menu.AddMenuItem(doorsMenuBtn);
            }
            if (IsAllowed(Permission.VOWindows)) // WINDOWS MENU
            {
                menu.AddMenuItem(windowsMenuBtn);
            }
            if (IsAllowed(Permission.VOTorqueMultiplier))
            {
                menu.AddMenuItem(torqueEnabled); // TORQUE ENABLED
                menu.AddMenuItem(torqueMultiplier); // TORQUE LIST
            }
            if (IsAllowed(Permission.VOPowerMultiplier))
            {
                menu.AddMenuItem(powerEnabled); // POWER ENABLED
                menu.AddMenuItem(powerMultiplier); // POWER LIST
            }
            if (IsAllowed(Permission.VODisableTurbulence))
            {
                menu.AddMenuItem(vehicleNoTurbulence);
            }
            if (IsAllowed(Permission.VOFlip)) // FLIP VEHICLE
            {
                menu.AddMenuItem(flipVehicle);
            }
            if (IsAllowed(Permission.VOAlarm)) // TOGGLE VEHICLE ALARM
            {
                menu.AddMenuItem(vehicleAlarm);
            }
            if (IsAllowed(Permission.VOCycleSeats)) // CYCLE THROUGH VEHICLE SEATS
            {
                menu.AddMenuItem(cycleSeats);
            }
            if (IsAllowed(Permission.VOLights)) // VEHICLE LIGHTS LIST
            {
                menu.AddMenuItem(vehicleLights);
            }
            if (IsAllowed(Permission.VOFreeze)) // FREEZE VEHICLE
            {
                menu.AddMenuItem(vehicleFreeze);
            }
            if (IsAllowed(Permission.VOEngineAlwaysOn)) // LEAVE ENGINE RUNNING
            {
                menu.AddMenuItem(vehicleEngineAO);
            }
            if (IsAllowed(Permission.VONoSiren) && !vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.Setting.vmenu_use_els_compatibility_mode)) // DISABLE SIREN
            {
                menu.AddMenuItem(vehicleNoSiren);
            }
            if (IsAllowed(Permission.VONoHelmet)) // DISABLE BIKE HELMET
            {
                menu.AddMenuItem(vehicleNoBikeHelmet);
            }
            if (IsAllowed(Permission.VOFlashHighbeamsOnHonk)) // FLASH HIGHBEAMS ON HONK
            {
                menu.AddMenuItem(highbeamsOnHonk);
            }

            if (IsAllowed(Permission.VODelete)) // DELETE VEHICLE
            {
                menu.AddMenuItem(deleteBtn);
            }
            #endregion

            #region delete vehicle handle stuff
            DeleteConfirmMenu.AddMenuItem(deleteNoBtn);
            DeleteConfirmMenu.AddMenuItem(deleteYesBtn);
            DeleteConfirmMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteNoBtn)
                {
                    DeleteConfirmMenu.GoBack();
                }
                else
                {
                    Vehicle veh = GetVehicle();
                    if (veh != null && veh.Exists() && GetVehicle().Driver == Game.PlayerPed)
                    {
                        SetVehicleHasBeenOwnedByPlayer(veh.Handle, false);
                        SetEntityAsMissionEntity(veh.Handle, false, false);
                        veh.Delete();
                    }
                    else
                    {
                        if (!Game.PlayerPed.IsInVehicle())
                        {
                            Notify.Alert(CommonErrors.NoVehicle);
                        }
                        else
                        {
                            Notify.Alert("You need to be in the driver's seat if you want to delete a vehicle.");
                        }

                    }
                    DeleteConfirmMenu.GoBack();
                    menu.GoBack();
                }
            };
            #endregion

            #region Bind Submenus to their buttons.
            MenuController.BindMenuItem(menu, VehicleModMenu, modMenuBtn);
            MenuController.BindMenuItem(menu, VehicleDoorsMenu, doorsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleWindowsMenu, windowsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleComponentsMenu, componentsMenuBtn);
            MenuController.BindMenuItem(menu, VehicleLiveriesMenu, liveriesMenuBtn);
            MenuController.BindMenuItem(menu, VehicleColorsMenu, colorsMenuBtn);
            MenuController.BindMenuItem(menu, DeleteConfirmMenu, deleteBtn);
            #endregion

            #region Handle button presses
            // Manage button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteBtn) // reset the index so that "no" / "cancel" will always be selected by default.
                {
                    DeleteConfirmMenu.RefreshIndex();
                }
                // If the player is actually in a vehicle, continue.
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    // Create a vehicle object.
                    Vehicle vehicle = GetVehicle();

                    // Check if the player is the driver of the vehicle, if so, continue.
                    if (vehicle.GetPedOnSeat(VehicleSeat.Driver) == new Ped(Game.PlayerPed.Handle))
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
                        // Toggle engine
                        else if (item == toggleEngine)
                        {
                            SetVehicleEngineOn(vehicle.Handle, !vehicle.IsEngineRunning, false, true);
                        }
                        // Set license plate text
                        else if (item == setLicensePlateText)
                        {
                            SetLicensePlateCustomText();
                        }
                    }

                    // If the player is not the driver seat and a button other than the option below (cycle seats) was pressed, notify them.
                    else if (item != cycleSeats)
                    {
                        Notify.Error("You have to be the driver of a vehicle to access this menu!", true, false);
                    }

                    // Cycle vehicle seats
                    if (item == cycleSeats)
                    {
                        CycleThroughSeats();
                    }
                }
            };
            #endregion

            #region Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // Create a vehicle object.
                Vehicle vehicle = GetVehicle();


                if (item == vehicleGod) // God Mode Toggled
                {
                    VehicleGodMode = _checked;
                }
                else if (item == vehicleSpecialGod) // special god mode
                {
                    VehicleSpecialGodMode = _checked;
                }
                else if (item == vehicleFreeze) // Freeze Vehicle Toggled
                {
                    VehicleFrozen = _checked;
                    if (!_checked)
                    {
                        if (vehicle != null && vehicle.Exists())
                        {
                            FreezeEntityPosition(vehicle.Handle, false);
                        }
                    }
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
                        if (vehicle != null && vehicle.Exists())
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, VehiclePowerMultiplierAmount);
                    }
                    else
                    {
                        if (vehicle != null && vehicle.Exists())
                            SetVehicleEnginePowerMultiplier(vehicle.Handle, 1f);
                    }
                }
                else if (item == vehicleEngineAO) // Leave Engine Running (vehicle always on) Toggled
                {
                    VehicleEngineAlwaysOn = _checked;
                }
                else if (item == vehicleNoSiren) // Disable Siren Toggled
                {
                    VehicleNoSiren = _checked;
                    if (vehicle != null && vehicle.Exists())
                        vehicle.IsSirenSilent = _checked;
                }
                else if (item == vehicleNoBikeHelmet) // No Helemet Toggled
                {
                    VehicleNoBikeHelemet = _checked;
                }
                else if (item == highbeamsOnHonk)
                {
                    FlashHighbeamsOnHonk = _checked;
                }
                else if (item == vehicleNoTurbulence)
                {
                    DisablePlaneTurbulence = _checked;
                    if (vehicle != null && vehicle.Exists() && vehicle.Model.IsPlane)
                    {
                        if (MainMenu.VehicleOptionsMenu.DisablePlaneTurbulence)
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 0f);
                        }
                        else
                        {
                            SetPlaneTurbulenceMultiplier(vehicle.Handle, 1.0f);
                        }
                    }
                }
                else if (item == vehicleNeverDirty)
                {
                    VehicleNeverDirty = _checked;
                }
            };
            #endregion

            #region Handle List Changes.
            // Handle list changes.
            menu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (GetVehicle() != null && GetVehicle().Exists())
                {
                    Vehicle veh = GetVehicle();
                    // If the torque multiplier changed. Change the torque multiplier to the new value.
                    if (item == torqueMultiplier)
                    {
                        // Get the selected value and remove the "x" in the string with nothing.
                        var value = torqueMultiplierList[newIndex].ToString().Replace("x", "");
                        // Convert the value to a float and set it as a public variable.
                        VehicleTorqueMultiplierAmount = float.Parse(value);
                    }
                    // If the power multiplier is changed. Change the power multiplier to the new value.
                    else if (item == powerMultiplier)
                    {
                        // Get the selected value. Remove the "x" from the string.
                        var value = powerMultiplierList[newIndex].ToString().Replace("x", "");
                        // Conver the string into a float and set it to be the value of the public variable.
                        VehiclePowerMultiplierAmount = float.Parse(value);
                        if (VehiclePowerMultiplier)
                        {
                            SetVehicleEnginePowerMultiplier(veh.Handle, VehiclePowerMultiplierAmount);
                        }
                    }
                    else if (item == setLicensePlateType)
                    {
                        // Set the license plate style.
                        switch (newIndex)
                        {
                            case 0:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite1;
                                break;
                            case 1:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite2;
                                break;
                            case 2:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.BlueOnWhite3;
                                break;
                            case 3:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;
                                break;
                            case 4:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlack;
                                break;
                            case 5:
                                veh.Mods.LicensePlateStyle = LicensePlateStyle.NorthYankton;
                                break;
                            default:
                                break;
                        }
                    }
                }
            };
            #endregion

            #region Handle List Items Selected
            menu.OnListItemSelect += async (sender, item, listIndex, itemIndex) =>
            {
                // Set dirt level
                if (item == setDirtLevel)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        GetVehicle().DirtLevel = float.Parse(listIndex.ToString());
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Toggle vehicle lights
                else if (item == vehicleLights)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Vehicle veh = GetVehicle();
                        var state = GetVehicleIndicatorLights(veh.Handle); // 0 = none, 1 = left, 2 = right, 3 = both

                        if (listIndex == 0) // Hazard lights
                        {
                            if (state != 3) // either all lights are off, or one of the two (left/right) is off.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else // both are on.
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 1) // left indicator
                        {
                            if (state != 1) // Left indicator is (only) off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, true); // left on
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        else if (listIndex == 2) // right indicator
                        {
                            if (state != 2) // Right indicator (only) is off
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, true); // right on
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh.Handle, 1, false); // left off
                                SetVehicleIndicatorLights(veh.Handle, 0, false); // right off
                            }
                        }
                        //else if (index == 3) // Interior lights
                        //{
                        //    CommonFunctions.Log("Something cool here.");
                        //}
                        //else if (index == 4) // taxi light
                        //{
                        //    SetTaxiLights(veh, true);
                        //    SetTaxiLights(veh, false);
                        //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //SetTaxiLights(veh, true);
                        //    //CommonFunctions.Log(IsTaxiLightOn(veh).ToString());
                        //    //SetTaxiLights(veh, false);
                        //    //SetTaxiLights(veh, !IsTaxiLightOn(veh));
                        //    CommonFunctions.Log
                        //}
                        else if (listIndex == 3) // helicopter spotlight
                        {
                            SetVehicleSearchlight(veh.Handle, !IsVehicleSearchlightOn(veh.Handle), true);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Speed Limiter
                else if (item == speedLimiter)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Vehicle vehicle = GetVehicle();

                        if (vehicle != null && vehicle.Exists())
                        {
                            if (listIndex == 0) // Set
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                SetEntityMaxSpeed(vehicle.Handle, vehicle.Speed);

                                if (ShouldUseMetricMeasurements()) // kph
                                {
                                    Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(vehicle.Speed * 3.6f, 1)} KPH~s~.");
                                }
                                else // mph
                                {
                                    Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(vehicle.Speed * 2.237f, 1)} MPH~s~.");
                                }

                            }
                            else if (listIndex == 1) // Reset
                            {
                                SetEntityMaxSpeed(vehicle.Handle, 500.01f); // Default max speed seemingly for all vehicles.
                                Notify.Info("Vehicle speed is now no longer limited.");
                            }
                            else if (listIndex == 2) // custom speed
                            {
                                string inputSpeed = await GetUserInput("Enter a speed (in meters/sec)", "20.0", 5);
                                if (!string.IsNullOrEmpty(inputSpeed))
                                {
                                    if (float.TryParse(inputSpeed, out float outFloat))
                                    {
                                        //vehicle.MaxSpeed = outFloat;
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outFloat + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(outFloat * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round(outFloat * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else if (int.TryParse(inputSpeed, out int outInt))
                                    {
                                        SetEntityMaxSpeed(vehicle.Handle, 500.01f);
                                        await BaseScript.Delay(0);
                                        SetEntityMaxSpeed(vehicle.Handle, outInt + 0.01f);
                                        if (ShouldUseMetricMeasurements()) // kph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round((float)outInt * 3.6f, 1)} KPH~s~.");
                                        }
                                        else // mph
                                        {
                                            Notify.Info($"Vehicle speed is now limited to ~b~{Math.Round((float)outInt * 2.237f, 1)} MPH~s~.");
                                        }
                                    }
                                    else
                                    {
                                        Notify.Error("This is not a valid number. Please enter a valid speed in meters per second.");
                                    }
                                }
                                else
                                {
                                    Notify.Error(CommonErrors.InvalidInput);
                                }
                            }
                        }
                    }
                }
            };
            #endregion

            #region Vehicle Colors Submenu Stuff

            #region Create lists for each color.
            // Metallic Colors
            List<string> Metallic = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.MetallicColors)
            {
                Metallic.Add(color.Key.ToString());
            }

            // Matte colors
            List<string> Matte = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.MatteColors)
            {
                Matte.Add(color.Key.ToString());
            }

            // Metal colors
            List<string> Metals = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.MetalColors)
            {
                Metals.Add(color.Key.ToString());
            }

            // Util Colors
            List<string> Utils = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.UtilColors)
            {
                Utils.Add(color.Key.ToString());
            }

            // Worn colors
            List<string> Worn = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.WornColors)
            {
                Worn.Add(color.Key.ToString());
            }

            // Pearlescent colors
            List<string> Pearlescent = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.MetallicColors)
            {
                Pearlescent.Add(color.Key.ToString());
            }

            // Wheel colors
            List<string> Wheels = new List<string>
            {
                "Default Alloy Color"
            };
            foreach (KeyValuePair<string, int> color in VehicleData.MetallicColors)
            {
                Wheels.Add(color.Key.ToString());
            }

            // Dashboard Trim Colors
            List<string> DashboardColor = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.MetallicColors)
            {
                DashboardColor.Add(color.Key.ToString());
            }

            // Extra Accent Colors
            List<string> TrimColor = new List<string>();
            foreach (KeyValuePair<string, int> color in VehicleData.MetallicColors)
            {
                TrimColor.Add(color.Key.ToString());
            }
            #endregion

            #region Create the headers + menu list items
            // Headers
            MenuItem primaryColorsHeader = GetSpacerMenuItem("PRIMARY COLORS");
            MenuItem secondaryColorsHeader = GetSpacerMenuItem("SECONDARY COLORS");
            MenuItem otherColorsHeader = GetSpacerMenuItem("OTHER COLORS");

            // Primary Colors
            MenuListItem classicColors = new MenuListItem("Classic", Metallic, 0, "Select a Classic primary color.");
            // Metallic == Classic + Pearlescent
            MenuListItem metallicColors = new MenuListItem("Metallic", Metallic, 0, "Select a Metallic primary color.");
            MenuListItem matteColors = new MenuListItem("Matte", Matte, 0, "Select a Matte primary color.");
            MenuListItem metalsColors = new MenuListItem("Metals", Metals, 0, "Select a Metals primary color.");
            MenuListItem utilsColors = new MenuListItem("Util", Utils, 0, "Select a Util primary color.");
            MenuListItem wornColors = new MenuListItem("Worn", Worn, 0, "Select a Worn primary color.");

            // Secondary Colors.
            MenuListItem classicColors2 = new MenuListItem("Classic", Metallic, 0, "Select a Classic secondary color.");
            MenuListItem metallicColors2 = new MenuListItem("Metallic", Metallic, 0, "Select a Metallic secondary color.");
            MenuListItem matteColors2 = new MenuListItem("Matte", Matte, 0, "Select a Matte secondary color.");
            MenuListItem metalsColors2 = new MenuListItem("Metals", Metals, 0, "Select a Metals secondary color.");
            MenuListItem utilsColors2 = new MenuListItem("Util", Utils, 0, "Select a Util secondary color.");
            MenuListItem wornColors2 = new MenuListItem("Worn", Worn, 0, "Select a Worn secondary color.");

            // Other Colors
            // Pearlescent == Classic + Classic on top of secondary color.
            MenuListItem pearlescentColors = new MenuListItem("Pearlescent", Metallic, 0, "Select a pearlescent color.");
            MenuListItem wheelColors = new MenuListItem("Wheel Color", Wheels, 0, "Select a color for your wheels.");
            // Chrome Button
            MenuItem chromeBtn = new MenuItem("Chrome", "Make your vehicle chrome!");
            MenuListItem dashboardColors = new MenuListItem("Dashboard Color", DashboardColor, 0, "Select a dashboard color (only availalbe on some cars).");
            MenuListItem trimColors = new MenuListItem("Trim Color", TrimColor, 0, "Select an trim/accent color (only availalbe on some cars).");
            #endregion

            #region Add the items to the colors menu.
            // Primary Colors
            VehicleColorsMenu.AddMenuItem(primaryColorsHeader); // header
            VehicleColorsMenu.AddMenuItem(classicColors);
            VehicleColorsMenu.AddMenuItem(metallicColors);
            VehicleColorsMenu.AddMenuItem(matteColors);
            VehicleColorsMenu.AddMenuItem(metalsColors);
            VehicleColorsMenu.AddMenuItem(utilsColors);
            VehicleColorsMenu.AddMenuItem(wornColors);

            // Secondary Colors
            VehicleColorsMenu.AddMenuItem(secondaryColorsHeader); // header
            VehicleColorsMenu.AddMenuItem(classicColors2);
            VehicleColorsMenu.AddMenuItem(metallicColors2);
            VehicleColorsMenu.AddMenuItem(matteColors2);
            VehicleColorsMenu.AddMenuItem(metalsColors2);
            VehicleColorsMenu.AddMenuItem(utilsColors2);
            VehicleColorsMenu.AddMenuItem(wornColors2);

            // Other Colors
            VehicleColorsMenu.AddMenuItem(otherColorsHeader); // header
            VehicleColorsMenu.AddMenuItem(pearlescentColors);
            VehicleColorsMenu.AddMenuItem(wheelColors);
            VehicleColorsMenu.AddMenuItem(chromeBtn);
            VehicleColorsMenu.AddMenuItem(dashboardColors);
            VehicleColorsMenu.AddMenuItem(trimColors);
            #endregion

            #region Handle Vehicle Color Changes
            VehicleColorsMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                // Get the current vehicle.
                Vehicle veh = GetVehicle();

                // Check if the vehicle exists and isn't dead and the player is the driver of the vehicle.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    // Get the primary and secondary colors from the current vehicle..
                    int primary = 0;
                    int secondary = 0;
                    int pearlescent = 0;
                    int wheels = 0;

                    int trimColor = 0;
                    int dashboardColor = 0;
                    GetVehicleInteriorColour(veh.Handle, ref trimColor);
                    GetVehicleDashboardColour(veh.Handle, ref dashboardColor);

                    GetVehicleColours(veh.Handle, ref primary, ref secondary);
                    GetVehicleExtraColours(veh.Handle, ref pearlescent, ref wheels);

                    // If any color of the primary colors is selected, which isn't the pearlescent or metallic option, then reset the pearlescent color to black;
                    if (item == classicColors || item == matteColors || item == metalsColors || item == utilsColors || item == wornColors)
                    {
                        pearlescent = 0;
                    }
                    // Classic / Metallic (primary)
                    if (item == classicColors || item == metallicColors)
                    {
                        primary = VehicleData.MetallicColors[Metallic[newIndex]];
                        if (item == metallicColors) // If the primary metallic changes, 
                        {
                            pearlescent = VehicleData.MetallicColors[Metallic[newIndex]];
                        }
                    }
                    // Classic / Metallic (secondary)
                    else if (item == classicColors2 || item == metallicColors2)
                    {
                        secondary = VehicleData.MetallicColors[Metallic[newIndex]];
                    }

                    // Matte (primary)
                    else if (item == matteColors)
                    {
                        primary = VehicleData.MatteColors[Matte[newIndex]];
                    }
                    // Matte (secondary)
                    else if (item == matteColors2)
                    {
                        secondary = VehicleData.MatteColors[Matte[newIndex]];
                    }

                    // Metals (primary)
                    else if (item == metalsColors)
                    {
                        primary = VehicleData.MetalColors[Metals[newIndex]];
                    }
                    // Metals (secondary)
                    else if (item == metalsColors2)
                    {
                        secondary = VehicleData.MetalColors[Metals[newIndex]];
                    }

                    // Utils (primary)
                    else if (item == utilsColors)
                    {
                        primary = VehicleData.UtilColors[Utils[newIndex]];
                    }
                    // Utils (secondary)
                    else if (item == utilsColors2)
                    {
                        secondary = VehicleData.UtilColors[Utils[newIndex]];
                    }

                    // Worn (primary)
                    else if (item == wornColors)
                    {
                        primary = VehicleData.WornColors[Worn[newIndex]];
                    }
                    // Worn (secondary)
                    else if (item == wornColors2)
                    {
                        secondary = VehicleData.WornColors[Worn[newIndex]];
                    }

                    // Pearlescent
                    else if (item == pearlescentColors)
                    {
                        pearlescent = VehicleData.MetallicColors[Metallic[newIndex]];
                    }

                    // Wheel colors
                    else if (item == wheelColors)
                    {
                        if (newIndex == 0)
                        {
                            // "Default Alloy Color" is not in the metallic list, so we have to manually account for this one.
                            wheels = 156;
                        }
                        else
                        {
                            wheels = VehicleData.MetallicColors[Metallic[newIndex - 1]];
                        }
                    }

                    else if (item == dashboardColors)
                    {
                        trimColor = VehicleData.MetallicColors[DashboardColor[newIndex]];
                        SetVehicleInteriorColour(veh.Handle, trimColor);
                    }
                    else if (item == trimColors)
                    {
                        dashboardColor = VehicleData.MetallicColors[TrimColor[newIndex]];
                        SetVehicleDashboardColour(veh.Handle, dashboardColor);
                    }

                    // Set the mod kit so we can modify things.
                    SetVehicleModKit(veh.Handle, 0);

                    // Set all the colors.
                    SetVehicleColours(veh.Handle, primary, secondary);
                    SetVehicleExtraColours(veh.Handle, pearlescent, wheels);
                }
            };
            #endregion

            #region Handle Chrome Button Pressed
            // Handle chrome button press.
            VehicleColorsMenu.OnItemSelect += (sender, item, index) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    // Set primary and secondary color to chrome.
                    SetVehicleColours(veh.Handle, (int)VehicleColor.Chrome, (int)VehicleColor.Chrome);
                }
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                }

            };
            #endregion

            #endregion

            #region Vehicle Doors Submenu Stuff
            MenuItem openAll = new MenuItem("Open All Doors", "Open all vehicle doors.");
            MenuItem closeAll = new MenuItem("Close All Doors", "Close all vehicle doors.");
            MenuItem LF = new MenuItem("Left Front Door", "Open/close the left front door.");
            MenuItem RF = new MenuItem("Right Front Door", "Open/close the right front door.");
            MenuItem LR = new MenuItem("Left Rear Door", "Open/close the left rear door.");
            MenuItem RR = new MenuItem("Right Rear Door", "Open/close the right rear door.");
            MenuItem HD = new MenuItem("Hood", "Open/close the hood.");
            MenuItem TR = new MenuItem("Trunk", "Open/close the trunk.");

            VehicleDoorsMenu.AddMenuItem(LF);
            VehicleDoorsMenu.AddMenuItem(RF);
            VehicleDoorsMenu.AddMenuItem(LR);
            VehicleDoorsMenu.AddMenuItem(RR);
            VehicleDoorsMenu.AddMenuItem(HD);
            VehicleDoorsMenu.AddMenuItem(TR);
            VehicleDoorsMenu.AddMenuItem(openAll);
            VehicleDoorsMenu.AddMenuItem(closeAll);

            // Handle button presses.
            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                // Get the vehicle.
                Vehicle veh = GetVehicle();
                // If the player is in a vehicle, it's not dead and the player is the driver, continue.
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    // If button 0-5 are pressed, then open/close that specific index/door.
                    if (index < 6)
                    {
                        // If the door is open.
                        bool open = GetVehicleDoorAngleRatio(veh.Handle, index) > 0.1f ? true : false;

                        if (open)
                        {
                            // Close the door.
                            SetVehicleDoorShut(veh.Handle, index, false);
                        }
                        else
                        {
                            // Open the door.
                            SetVehicleDoorOpen(veh.Handle, index, false, false);
                        }
                    }
                    // If the index >= 6, and the button is "openAll": open all doors.
                    else if (item == openAll)
                    {
                        // Loop through all doors and open them.
                        for (var door = 0; door < 6; door++)
                        {
                            SetVehicleDoorOpen(veh.Handle, door, false, false);
                        }
                    }
                    // If the index >= 6, and the button is "closeAll": close all doors.
                    else if (item == closeAll)
                    {
                        // Close all doors.
                        SetVehicleDoorsShut(veh.Handle, false);
                    }
                }
                else
                {
                    Notify.Alert(CommonErrors.NoVehicle, placeholderValue: "to open/close a vehicle door");
                }
            };

            #endregion

            #region Vehicle Windows Submenu Stuff
            MenuItem fwu = new MenuItem("~y~↑~s~ Roll Front Windows Up", "Roll both front windows up.");
            MenuItem fwd = new MenuItem("~o~↓~s~ Roll Front Windows Down", "Roll both front windows down.");
            MenuItem rwu = new MenuItem("~y~↑~s~ Roll Rear Windows Up", "Roll both rear windows up.");
            MenuItem rwd = new MenuItem("~o~↓~s~ Roll Rear Windows Down", "Roll both rear windows down.");
            VehicleWindowsMenu.AddMenuItem(fwu);
            VehicleWindowsMenu.AddMenuItem(fwd);
            VehicleWindowsMenu.AddMenuItem(rwu);
            VehicleWindowsMenu.AddMenuItem(rwd);
            VehicleWindowsMenu.OnItemSelect += (sender, item, index) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead)
                {
                    if (item == fwu)
                    {
                        RollUpWindow(veh.Handle, 0);
                        RollUpWindow(veh.Handle, 1);
                    }
                    else if (item == fwd)
                    {
                        RollDownWindow(veh.Handle, 0);
                        RollDownWindow(veh.Handle, 1);
                    }
                    else if (item == rwu)
                    {
                        RollUpWindow(veh.Handle, 2);
                        RollUpWindow(veh.Handle, 3);
                    }
                    else if (item == rwd)
                    {
                        RollDownWindow(veh.Handle, 2);
                        RollDownWindow(veh.Handle, 3);
                    }
                }
            };
            #endregion

            #region Vehicle Liveries Submenu Stuff
            menu.OnItemSelect += (sender, item, idex) =>
            {
                // If the liverys menu button is selected.
                if (item == liveriesMenuBtn)
                {
                    // Get the player's vehicle.
                    Vehicle veh = GetVehicle();
                    // If it exists, isn't dead and the player is in the drivers seat continue.
                    if (veh != null && veh.Exists() && !veh.IsDead)
                    {
                        if (veh.Driver == Game.PlayerPed)
                        {
                            VehicleLiveriesMenu.ClearMenuItems();
                            SetVehicleModKit(veh.Handle, 0);
                            var liveryCount = GetVehicleLiveryCount(veh.Handle);

                            if (liveryCount > 0)
                            {
                                var liveryList = new List<string>();
                                for (var i = 0; i < liveryCount; i++)
                                {
                                    var livery = GetLiveryName(veh.Handle, i);
                                    livery = GetLabelText(livery) != "NULL" ? GetLabelText(livery) : $"Livery #{i}";
                                    liveryList.Add(livery);
                                }
                                MenuListItem liveryListItem = new MenuListItem("Set Livery", liveryList, GetVehicleLivery(veh.Handle), "Choose a livery for this vehicle.");
                                VehicleLiveriesMenu.AddMenuItem(liveryListItem);
                                VehicleLiveriesMenu.OnListIndexChange += (_menu, listItem, oldIndex, newIndex, itemIndex) =>
                                {
                                    if (listItem == liveryListItem)
                                    {
                                        veh = GetVehicle();
                                        SetVehicleLivery(veh.Handle, newIndex);
                                    }
                                };
                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                            else
                            {
                                Notify.Error("This vehicle does not have any liveries.");
                                VehicleLiveriesMenu.CloseMenu();
                                menu.OpenMenu();
                                MenuItem backBtn = new MenuItem("No Liveries Available :(", "Click me to go back.")
                                {
                                    Label = "Go Back"
                                };
                                VehicleLiveriesMenu.AddMenuItem(backBtn);
                                VehicleLiveriesMenu.OnItemSelect += (sender2, item2, index2) =>
                                {
                                    if (item2 == backBtn)
                                    {
                                        VehicleLiveriesMenu.GoBack();
                                    }
                                };

                                VehicleLiveriesMenu.RefreshIndex();
                                //VehicleLiveriesMenu.UpdateScaleform();
                            }
                        }
                        else
                        {
                            Notify.Error("You have to be the driver of a vehicle to access this menu.");
                        }
                    }
                    else
                    {
                        Notify.Error("You have to be the driver of a vehicle to access this menu.");
                    }
                }
            };
            #endregion

            #region Vehicle Mod Submenu Stuff
            menu.OnItemSelect += (sender, item, index) =>
            {
                // When the mod submenu is openend, reset all items in there.
                if (item == modMenuBtn)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        UpdateMods();
                    }
                    else
                    {
                        VehicleModMenu.CloseMenu();
                        menu.OpenMenu();
                    }

                }
            };
            #endregion

            #region Vehicle Components Submenu
            // when the components menu is opened.
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If the components menu is opened.
                if (item == componentsMenuBtn)
                {
                    // Empty the menu in case there were leftover buttons from another vehicle.
                    if (VehicleComponentsMenu.Size > 0)
                    {
                        VehicleComponentsMenu.ClearMenuItems();
                        vehicleExtras.Clear();
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();
                    }

                    // Get the vehicle.
                    Vehicle veh = GetVehicle();

                    // Check if the vehicle exists, it's actually a vehicle, it's not dead/broken and the player is in the drivers seat.
                    if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                    {
                        //List<int> extraIds = new List<int>();
                        // Loop through all possible extra ID's (AFAIK: 0-14).
                        for (var extra = 0; extra < 14; extra++)
                        {
                            // If this extra exists...
                            if (veh.ExtraExists(extra))
                            {
                                // Add it's ID to the list.
                                //extraIds.Add(extra);

                                // Create a checkbox for it.
                                MenuCheckboxItem extraCheckbox = new MenuCheckboxItem($"Extra #{extra.ToString()}", extra.ToString(), veh.IsExtraOn(extra));
                                // Add the checkbox to the menu.
                                VehicleComponentsMenu.AddMenuItem(extraCheckbox);

                                // Add it's ID to the dictionary.
                                vehicleExtras[extraCheckbox] = extra;
                            }
                        }



                        if (vehicleExtras.Count > 0)
                        {
                            MenuItem backBtn = new MenuItem("Go Back", "Go back to the Vehicle Options menu.");
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        else
                        {
                            MenuItem backBtn = new MenuItem("No Extras Available :(", "Go back to the Vehicle Options menu.");
                            backBtn.Label = "Go Back";
                            VehicleComponentsMenu.AddMenuItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        // And update the submenu to prevent weird glitches.
                        VehicleComponentsMenu.RefreshIndex();
                        //VehicleComponentsMenu.UpdateScaleform();

                    }
                }
            };
            // when a checkbox in the components menu changes
            VehicleComponentsMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                // When a checkbox is checked/unchecked, get the selected checkbox item index and use that to get the component ID from the list.
                // Then toggle that extra.
                if (vehicleExtras.TryGetValue(item, out int extra))
                {
                    Vehicle veh = GetVehicle();
                    veh.ToggleExtra(extra, _checked);
                }
            };
            #endregion

            #region Underglow Submenu
            MenuCheckboxItem underglowFront = new MenuCheckboxItem("Enable Front Light", "Enable or disable the underglow on the front side of the vehicle. Note not all vehicles have lights.", false);
            MenuCheckboxItem underglowBack = new MenuCheckboxItem("Enable Rear Light", "Enable or disable the underglow on the left side of the vehicle. Note not all vehicles have lights.", false);
            MenuCheckboxItem underglowLeft = new MenuCheckboxItem("Enable Left Light", "Enable or disable the underglow on the right side of the vehicle. Note not all vehicles have lights.", false);
            MenuCheckboxItem underglowRight = new MenuCheckboxItem("Enable Right Light", "Enable or disable the underglow on the back side of the vehicle. Note not all vehicles have lights.", false);
            var underglowColorsList = new List<string>();
            for (int i = 0; i < 13; i++)
            {
                underglowColorsList.Add(GetLabelText($"CMOD_NEONCOL_{i}"));
            }
            MenuListItem underglowColor = new MenuListItem(GetLabelText("CMOD_NEON_1"), underglowColorsList, 0, "Select the color of the neon underglow.");

            VehicleUnderglowMenu.AddMenuItem(underglowFront);
            VehicleUnderglowMenu.AddMenuItem(underglowBack);
            VehicleUnderglowMenu.AddMenuItem(underglowLeft);
            VehicleUnderglowMenu.AddMenuItem(underglowRight);

            VehicleUnderglowMenu.AddMenuItem(underglowColor);

            menu.OnItemSelect += (sender, item, index) =>
            {
                #region reset checkboxes state when opening the menu.
                if (item == underglowMenuBtn)
                {
                    Vehicle veh = GetVehicle();
                    if (veh != null)
                    {
                        if (veh.Mods.HasNeonLights)
                        {
                            underglowFront.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Front) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
                            underglowBack.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Back) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
                            underglowLeft.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Left) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
                            underglowRight.Checked = veh.Mods.HasNeonLight(VehicleNeonLight.Right) && veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

                            underglowFront.Enabled = true;
                            underglowBack.Enabled = true;
                            underglowLeft.Enabled = true;
                            underglowRight.Enabled = true;

                            underglowFront.LeftIcon = MenuItem.Icon.NONE;
                            underglowBack.LeftIcon = MenuItem.Icon.NONE;
                            underglowLeft.LeftIcon = MenuItem.Icon.NONE;
                            underglowRight.LeftIcon = MenuItem.Icon.NONE;
                        }
                        else
                        {
                            underglowFront.Checked = false;
                            underglowBack.Checked = false;
                            underglowLeft.Checked = false;
                            underglowRight.Checked = false;

                            underglowFront.Enabled = false;
                            underglowBack.Enabled = false;
                            underglowLeft.Enabled = false;
                            underglowRight.Enabled = false;

                            underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                            underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                            underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                            underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                        }
                    }
                    else
                    {
                        underglowFront.Checked = false;
                        underglowBack.Checked = false;
                        underglowLeft.Checked = false;
                        underglowRight.Checked = false;

                        underglowFront.Enabled = false;
                        underglowBack.Enabled = false;
                        underglowLeft.Enabled = false;
                        underglowRight.Enabled = false;

                        underglowFront.LeftIcon = MenuItem.Icon.LOCK;
                        underglowBack.LeftIcon = MenuItem.Icon.LOCK;
                        underglowLeft.LeftIcon = MenuItem.Icon.LOCK;
                        underglowRight.LeftIcon = MenuItem.Icon.LOCK;
                    }

                    underglowColor.ListIndex = GetIndexFromColor();
                }
                #endregion
            };
            // handle item selections
            VehicleUnderglowMenu.OnCheckboxChange += (sender, item, index, _checked) =>
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    Vehicle veh = GetVehicle();
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(underglowColor.ListIndex);
                        if (item == underglowLeft)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, veh.Mods.HasNeonLight(VehicleNeonLight.Left) && _checked);
                        }
                        else if (item == underglowRight)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, veh.Mods.HasNeonLight(VehicleNeonLight.Right) && _checked);
                        }
                        else if (item == underglowBack)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, veh.Mods.HasNeonLight(VehicleNeonLight.Back) && _checked);
                        }
                        else if (item == underglowFront)
                        {
                            veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, veh.Mods.HasNeonLight(VehicleNeonLight.Front) && _checked);
                        }
                    }
                }
            };

            VehicleUnderglowMenu.OnListIndexChange += (sender, item, oldIndex, newIndex, itemIndex) =>
            {
                if (item == underglowColor)
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        Vehicle veh = GetVehicle();
                        if (veh.Mods.HasNeonLights)
                        {
                            veh.Mods.NeonLightsColor = GetColorFromIndex(newIndex);
                        }
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
        public Menu GetMenu()
        {
            // If menu doesn't exist. Create one.
            if (menu == null)
            {
                CreateMenu();
            }
            // Return the menu.
            return menu;
        }

        #region Update Vehicle Mods Menu
        /// <summary>
        /// Refreshes the mods page. The selectedIndex allows you to go straight to a specific index after refreshing the menu.
        /// This is used because when the wheel type is changed, the menu is refreshed to update the available wheels list.
        /// </summary>
        /// <param name="selectedIndex">Pass this if you want to go straight to a specific mod/index.</param>
        public void UpdateMods(int selectedIndex = 0)
        {
            // If there are items, remove all of them.
            if (VehicleModMenu.Size > 0)
            {
                if (selectedIndex != 0)
                {
                    VehicleModMenu.ClearMenuItems(true);
                }
                else
                {
                    VehicleModMenu.ClearMenuItems(false);
                }

            }

            // Get the vehicle.
            Vehicle veh = GetVehicle();

            // Check if the vehicle exists, is still drivable/alive and it's actually a vehicle.
            if (veh != null && veh.Exists() && !veh.IsDead)
            {
                #region initial setup & dynamic vehicle mods setup
                // Set the modkit so we can modify the car.
                SetVehicleModKit(veh.Handle, 0);

                // Get all mods available on this vehicle.
                VehicleMod[] mods = veh.Mods.GetAllMods();

                // Loop through all the mods.
                foreach (var mod in mods)
                {
                    veh = GetVehicle();

                    // Get the mod type (suspension, armor, etc) name (convert the PascalCase to the Proper Case string values).
                    var typeName = ToProperString(mod.ModType.ToString());

                    // Create a list to all available upgrades for this modtype.
                    var modlist = new List<string>();

                    // Get the current item index ({current}/{max upgrades})
                    var currentItem = $"[1/{ mod.ModCount + 1}]";

                    // Add the stock value for this mod.
                    var name = $"Stock {typeName} {currentItem}";
                    modlist.Add(name);

                    // Loop through all available upgrades for this specific mod type.
                    for (var x = 0; x < mod.ModCount; x++)
                    {
                        // Create the item index.
                        currentItem = $"[{2 + x}/{ mod.ModCount + 1}]";

                        // Create the name (again, converting to proper case), then add the name.
                        name = mod.GetLocalizedModName(x) != "" ? $"{ToProperString(mod.GetLocalizedModName(x))} {currentItem}" : $"{typeName} #{x.ToString()} {currentItem}";
                        modlist.Add(name);
                    }

                    // Create the MenuListItem for this mod type.
                    var currIndex = GetVehicleMod(veh.Handle, (int)mod.ModType) + 1;
                    MenuListItem modTypeListItem = new MenuListItem(typeName, modlist, currIndex, $"Choose a ~y~{typeName}~s~ upgrade, it will be automatically applied to your vehicle.");

                    // Add the list item to the menu.
                    VehicleModMenu.AddMenuItem(modTypeListItem);
                }
                #endregion

                #region more variables and setup
                veh = GetVehicle();
                // Create the wheel types list & listitem and add it to the menu.
                List<string> wheelTypes = new List<string>() { "Sports", "Muscle", "Lowrider", "SUV", "Offroad", "Tuner", "Bike Wheels", "High End" };
                MenuListItem vehicleWheelType = new MenuListItem("Wheel Type", wheelTypes, GetVehicleWheelType(veh.Handle), $"Choose a ~y~wheel type~s~ for your vehicle.");
                VehicleModMenu.AddMenuItem(vehicleWheelType);

                // Create the checkboxes for some options.
                MenuCheckboxItem toggleCustomWheels = new MenuCheckboxItem("Toggle Custom Wheels", "Press this to add or remove ~y~custom~s~ wheels.", GetVehicleModVariation(veh.Handle, 23));
                MenuCheckboxItem xenonHeadlights = new MenuCheckboxItem("Xenon Headlights", "Enable or disable ~b~xenon ~s~headlights.", IsToggleModOn(veh.Handle, 22));
                MenuCheckboxItem turbo = new MenuCheckboxItem("Turbo", "Enable or disable the ~y~turbo~s~ for this vehicle.", IsToggleModOn(veh.Handle, 18));
                MenuCheckboxItem bulletProofTires = new MenuCheckboxItem("Bullet Proof Tires", "Enable or disable ~y~bullet proof tires~s~ for this vehicle.", !GetVehicleTyresCanBurst(veh.Handle));

                // Add the checkboxes to the menu.
                VehicleModMenu.AddMenuItem(toggleCustomWheels);
                VehicleModMenu.AddMenuItem(xenonHeadlights);
                int currentHeadlightColor = _GET_VEHICLE_HEADLIGHTS_COLOR(veh);
                if (currentHeadlightColor < 0 || currentHeadlightColor > 12)
                {
                    currentHeadlightColor = 13;
                }
                MenuListItem headlightColor = new MenuListItem("Headlight Color", new List<string>() { "White", "Blue", "Electric Blue", "Mint Green", "Lime Green", "Yellow", "Golden Shower", "Orange", "Red", "Pony Pink", "Hot Pink", "Purple", "Blacklight", "Default Xenon" }, currentHeadlightColor, "New in the Arena Wars GTA V update: Colored headlights. Note you must enable Xenon Headlights first.");
                VehicleModMenu.AddMenuItem(headlightColor);
                VehicleModMenu.AddMenuItem(turbo);
                VehicleModMenu.AddMenuItem(bulletProofTires);
                // Create a list of tire smoke options.
                List<string> tireSmokes = new List<string>() { "Red", "Orange", "Yellow", "Gold", "Light Green", "Dark Green", "Light Blue", "Dark Blue", "Purple", "Pink", "Black" };
                Dictionary<string, int[]> tireSmokeColors = new Dictionary<string, int[]>()
                {
                    ["Red"] = new int[] { 244, 65, 65 },
                    ["Orange"] = new int[] { 244, 167, 66 },
                    ["Yellow"] = new int[] { 244, 217, 65 },
                    ["Gold"] = new int[] { 181, 120, 0 },
                    ["Light Green"] = new int[] { 158, 255, 84 },
                    ["Dark Green"] = new int[] { 44, 94, 5 },
                    ["Light Blue"] = new int[] { 65, 211, 244 },
                    ["Dark Blue"] = new int[] { 24, 54, 163 },
                    ["Purple"] = new int[] { 108, 24, 192 },
                    ["Pink"] = new int[] { 192, 24, 172 },
                    ["Black"] = new int[] { 1, 1, 1 }
                };
                int smoker = 0, smokeg = 0, smokeb = 0;
                GetVehicleTyreSmokeColor(veh.Handle, ref smoker, ref smokeg, ref smokeb);
                var item = tireSmokeColors.ToList().Find((f) => { return (f.Value[0] == smoker && f.Value[1] == smokeg && f.Value[2] == smokeb); });
                int index = tireSmokeColors.ToList().IndexOf(item);
                if (index < 0)
                {
                    index = 0;
                }

                MenuListItem tireSmoke = new MenuListItem("Tire Smoke Color", tireSmokes, index, $"Choose a ~y~wheel type~s~ for your vehicle.");
                VehicleModMenu.AddMenuItem(tireSmoke);

                // Create the checkbox to enable/disable the tiresmoke.
                MenuCheckboxItem tireSmokeEnabled = new MenuCheckboxItem("Tire Smoke", "Enable or disable ~y~tire smoke~s~ for your vehicle. ~h~~r~Important:~s~ When disabling tire smoke, you'll need to drive around before it takes affect.", IsToggleModOn(veh.Handle, 20));
                VehicleModMenu.AddMenuItem(tireSmokeEnabled);

                // Create list for window tint
                List<string> windowTints = new List<string>() { "Stock [1/7]", "None [2/7]", "Limo [3/7]", "Light Smoke [4/7]", "Dark Smoke [5/7]", "Pure Black [6/7]", "Green [7/7]" };
                var currentTint = GetVehicleWindowTint(veh.Handle);
                if (currentTint == -1)
                {
                    currentTint = 4; // stock
                }

                // Convert window tint to the correct index of the list above.
                switch (currentTint)
                {
                    case 0:
                        currentTint = 1; // None
                        break;
                    case 1:
                        currentTint = 5; // Pure Black
                        break;
                    case 2:
                        currentTint = 4; // Dark Smoke
                        break;
                    case 3:
                        currentTint = 3; // Light Smoke
                        break;
                    case 4:
                        currentTint = 0; // Stock
                        break;
                    case 5:
                        currentTint = 2; // Limo
                        break;
                    case 6:
                        currentTint = 6; // Green
                        break;
                    default:
                        break;
                }

                MenuListItem windowTint = new MenuListItem("Window Tint", windowTints, currentTint, "Apply tint to your windows.");
                VehicleModMenu.AddMenuItem(windowTint);

                #endregion

                #region Checkbox Changes
                // Handle checkbox changes.
                VehicleModMenu.OnCheckboxChange += (sender2, item2, index2, _checked) =>
                {
                    veh = GetVehicle();

                    // Xenon Headlights
                    if (item2 == xenonHeadlights)
                    {
                        ToggleVehicleMod(veh.Handle, 22, _checked);
                    }
                    // Turbo
                    else if (item2 == turbo)
                    {
                        ToggleVehicleMod(veh.Handle, 18, _checked);
                    }
                    // Bullet Proof Tires
                    else if (item2 == bulletProofTires)
                    {
                        SetVehicleTyresCanBurst(veh.Handle, !_checked);
                    }
                    // Custom Wheels
                    else if (item2 == toggleCustomWheels)
                    {
                        SetVehicleMod(veh.Handle, 23, GetVehicleMod(veh.Handle, 23), !GetVehicleModVariation(veh.Handle, 23));

                        // If the player is on a motorcycle, also change the back wheels.
                        if (IsThisModelABike((uint)GetEntityModel(veh.Handle)))
                        {
                            SetVehicleMod(veh.Handle, 24, GetVehicleMod(veh.Handle, 24), GetVehicleModVariation(veh.Handle, 23));
                        }
                    }
                    // Toggle Tire Smoke
                    else if (item2 == tireSmokeEnabled)
                    {
                        // If it should be enabled:
                        if (_checked)
                        {
                            // Enable it.
                            ToggleVehicleMod(veh.Handle, 20, true);
                            // Get the selected color values.
                            var r = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][0];
                            var g = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][1];
                            var b = tireSmokeColors[tireSmokes[tireSmoke.ListIndex]][2];
                            // Set the color.
                            SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                        }
                        // If it should be disabled:
                        else
                        {
                            // Set the smoke to white.
                            SetVehicleTyreSmokeColor(veh.Handle, 255, 255, 255);
                            // Disable it.
                            ToggleVehicleMod(veh.Handle, 20, false);
                            // Remove the mod.
                            RemoveVehicleMod(veh.Handle, 20);
                        }
                    }
                };
                #endregion

                #region List Changes
                // Handle list selections
                VehicleModMenu.OnListIndexChange += (sender2, item2, oldIndex, newIndex, itemIndex) =>
                {
                    // Get the vehicle and set the mod kit.
                    veh = GetVehicle();
                    SetVehicleModKit(veh.Handle, 0);

                    #region handle the dynamic (vehicle-specific) mods
                    // If the affected list is actually a "dynamically" generated list, continue. If it was one of the manual options, go to else.
                    if (itemIndex < sender2.Size - 9)
                    {
                        // Get all mods available on this vehicle.
                        mods = veh.Mods.GetAllMods();

                        var dict = new Dictionary<int, int>();
                        var x = 0;

                        foreach (var mod in mods)
                        {
                            dict.Add(x, (int)mod.ModType);
                            x++;
                        }

                        int modType = dict[itemIndex];
                        int selectedUpgrade = item2.ListIndex - 1;
                        bool customWheels = GetVehicleModVariation(veh.Handle, 23);

                        SetVehicleMod(veh.Handle, modType, selectedUpgrade, customWheels);
                    }
                    #endregion
                    // If it was not one of the lists above, then it was one of the manual lists/options selected, 
                    // either: vehicle Wheel Type, tire smoke color, or window tint:
                    #region Handle the items available on all vehicles.
                    // Wheel types
                    else if (item2 == vehicleWheelType)
                    {
                        // Set the wheel type.
                        int nindex = newIndex;
                        if (newIndex >= item2.ItemsCount)
                        {
                            nindex = 0;
                        }
                        else if (newIndex < 0)
                        {
                            nindex = item2.ItemsCount - 1;
                        }

                        // set the wheel type
                        SetVehicleWheelType(veh.Handle, nindex);

                        bool customWheels = GetVehicleModVariation(veh.Handle, 23);

                        // reset the wheel mod index for front wheels
                        SetVehicleMod(veh.Handle, 23, -1, customWheels);

                        // if the model is a bike, do the same thing for the rear wheels.
                        if (veh.Model.IsBike)
                        {
                            SetVehicleMod(veh.Handle, 24, -1, customWheels);
                        }

                        // Refresh the menu with the item index so that the view doesn't change
                        UpdateMods(selectedIndex: itemIndex);
                    }
                    // Tire smoke
                    else if (item2 == tireSmoke)
                    {
                        // Get the selected color values.
                        var r = tireSmokeColors[tireSmokes[newIndex]][0];
                        var g = tireSmokeColors[tireSmokes[newIndex]][1];
                        var b = tireSmokeColors[tireSmokes[newIndex]][2];

                        // Set the color.
                        SetVehicleTyreSmokeColor(veh.Handle, r, g, b);
                    }
                    // Window Tint
                    else if (item2 == windowTint)
                    {
                        // Stock = 4,
                        // None = 0,
                        // Limo = 5,
                        // LightSmoke = 3,
                        // DarkSmoke = 2,
                        // PureBlack = 1,
                        // Green = 6,

                        switch (newIndex)
                        {
                            case 1:
                                SetVehicleWindowTint(veh.Handle, 0); // None
                                break;
                            case 2:
                                SetVehicleWindowTint(veh.Handle, 5); // Limo
                                break;
                            case 3:
                                SetVehicleWindowTint(veh.Handle, 3); // Light Smoke
                                break;
                            case 4:
                                SetVehicleWindowTint(veh.Handle, 2); // Dark Smoke
                                break;
                            case 5:
                                SetVehicleWindowTint(veh.Handle, 1); // Pure Black
                                break;
                            case 6:
                                SetVehicleWindowTint(veh.Handle, 6); // Green
                                break;
                            case 0:
                            default:
                                SetVehicleWindowTint(veh.Handle, 4); // Stock
                                break;
                        }
                    }
                    else if (item2 == headlightColor)
                    {
                        if (newIndex == 13) // default
                        {
                            _SET_VEHICLE_HEADLIGHTS_COLOR(veh, 255);
                        }
                        else if (newIndex > -1 && newIndex < 13)
                        {
                            _SET_VEHICLE_HEADLIGHTS_COLOR(veh, newIndex);
                        }
                    }
                    #endregion
                };

                #endregion
            }
            // Refresh Index and update the scaleform to prevent weird broken menus.
            if (selectedIndex == 0)
            {
                VehicleModMenu.RefreshIndex();
            }

            //VehicleModMenu.UpdateScaleform();

            // Set the selected index to the provided index (0 by default)
            // Used for example, when the wheelstype is changed, the menu is refreshed and we want to set the
            // selected item back to the "wheelsType" list so the user doesn't have to scroll down each time they
            // change the wheels type.
            //VehicleModMenu.CurrentIndex = selectedIndex;
        }

        internal static void _SET_VEHICLE_HEADLIGHTS_COLOR(Vehicle veh, int newIndex)
        {
            if (veh != null && veh.Exists() && veh.Driver == Game.PlayerPed)
            {
                if (newIndex > -1 && newIndex < 13)
                {
                    CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0xE41033B25D003A07, veh.Handle, newIndex);
                }
                else
                {
                    CitizenFX.Core.Native.Function.Call((CitizenFX.Core.Native.Hash)0xE41033B25D003A07, veh.Handle, -1);
                }
            }
        }

        internal static int _GET_VEHICLE_HEADLIGHTS_COLOR(Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (IsToggleModOn(vehicle.Handle, 22))
                {
                    int val = CitizenFX.Core.Native.Function.Call<int>((CitizenFX.Core.Native.Hash)0x3DFF319A831E0CDB, vehicle.Handle);
                    if (val > -1 && val < 13)
                    {
                        return val;
                    }
                    return -1;
                }
            }
            return -1;
        }
        #endregion

        #region GetColorFromIndex function (underglow)

        private readonly List<int[]> _VehicleNeonLightColors = new List<int[]>()
        {
            { new int[3] { 255, 255, 255 } },   // White
            { new int[3] { 2, 21, 255 } },      // Blue
            { new int[3] { 3, 83, 255 } },      // Electric blue
            { new int[3] { 0, 255, 140 } },     // Mint Green
            { new int[3] { 94, 255, 1 } },      // Lime Green
            { new int[3] { 255, 255, 0 } },     // Yellow
            { new int[3] { 255, 150, 5 } },     // Golden Shower
            { new int[3] { 255, 62, 0 } },      // Orange
            { new int[3] { 255, 0, 0 } },       // Red
            { new int[3] { 255, 50, 100 } },    // Pony Pink
            { new int[3] { 255, 5, 190 } },     // Hot Pink
            { new int[3] { 35, 1, 255 } },      // Purple
            { new int[3] { 15, 3, 255 } },      // Blacklight
        };

        /// <summary>
        /// Converts a list index to a <see cref="System.Drawing.Color"/> struct.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private System.Drawing.Color GetColorFromIndex(int index)
        {
            if (index >= 0 && index < 13)
            {
                return System.Drawing.Color.FromArgb(_VehicleNeonLightColors[index][0], _VehicleNeonLightColors[index][1], _VehicleNeonLightColors[index][2]);
            }
            return System.Drawing.Color.FromArgb(255, 255, 255);
        }

        /// <summary>
        /// Returns the color index that is applied on the current vehicle. 
        /// If a color is active on the vehicle which is not in the list, it'll return the default index 0 (white).
        /// </summary>
        /// <returns></returns>
        private int GetIndexFromColor()
        {
            Vehicle veh = GetVehicle();

            if (veh == null || !veh.Exists() || !veh.Mods.HasNeonLights)
            {
                return 0;
            }

            int r = 255, g = 255, b = 255;

            GetVehicleNeonLightsColour(veh.Handle, ref r, ref g, ref b);

            if (r == 255 && g == 0 && b == 255) // default return value when the vehicle has no neon kit selected.
            {
                return 0;
            }

            if (_VehicleNeonLightColors.Any(a => { return a[0] == r && a[1] == g && a[2] == b; }))
            {
                return _VehicleNeonLightColors.FindIndex(a => { return a[0] == r && a[1] == g && a[2] == b; });
            }

            return 0;
        }
        #endregion
    }
}
