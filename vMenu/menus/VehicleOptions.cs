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
        private CommonFunctions cf = MainMenu.Cf;
        private static VehicleData vd = new VehicleData();

        // Submenus
        public UIMenu VehicleModMenu { get; private set; }
        public UIMenu VehicleDoorsMenu { get; private set; }
        public UIMenu VehicleWindowsMenu { get; private set; }
        public UIMenu VehicleComponentsMenu { get; private set; }
        public UIMenu VehicleLiveriesMenu { get; private set; }
        public UIMenu VehicleColorsMenu { get; private set; }
        public UIMenu DeleteConfirmMenu { get; private set; }
        public UIMenu VehicleUnderglowMenu { get; private set; }

        // Public variables (getters only), return the private variables.
        public bool VehicleGodMode { get; private set; } = UserDefaults.VehicleGodMode;
        public bool VehicleSpecialGodMode { get; private set; } = UserDefaults.VehicleSpecialGodMode;
        public bool VehicleEngineAlwaysOn { get; private set; } = UserDefaults.VehicleEngineAlwaysOn;
        public bool VehicleNoSiren { get; private set; } = UserDefaults.VehicleNoSiren;
        public bool VehicleNoBikeHelemet { get; private set; } = UserDefaults.VehicleNoBikeHelmet;
        public bool FlashHighbeamsOnHonk { get; private set; } = UserDefaults.VehicleHighbeamsOnHonk;
        public bool VehicleFrozen { get; private set; } = false;
        public bool VehicleTorqueMultiplier { get; private set; } = false;
        public bool VehiclePowerMultiplier { get; private set; } = false;
        public float VehicleTorqueMultiplierAmount { get; private set; } = 2f;
        public float VehiclePowerMultiplierAmount { get; private set; } = 2f;

        private Dictionary<UIMenuItem, int> vehicleExtras = new Dictionary<UIMenuItem, int>();
        #endregion

        #region CreateMenu()
        /// <summary>
        /// Create menu creates the vehicle options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Vehicle Options", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            #region menu variables
            // Create Checkboxes.
            UIMenuCheckboxItem vehicleGod = new UIMenuCheckboxItem("Vehicle God Mode", VehicleGodMode, "Your vehicle will not be able to take visual or physical damage.");
            UIMenuCheckboxItem vehicleSpecialGod = new UIMenuCheckboxItem("Special Vehicle God Mode", VehicleSpecialGodMode, "This option repairs your vehicle immediately when " +
                "it gets damaged. This special god mode is needed for vehicles like the Phantom Wedge to keep it from breaking down with regular god mode turned on.");
            UIMenuCheckboxItem vehicleEngineAO = new UIMenuCheckboxItem("Engine Always On", VehicleEngineAlwaysOn, "Keeps your vehicle engine on when you exit your vehicle.");
            UIMenuCheckboxItem vehicleNoSiren = new UIMenuCheckboxItem("Disable Siren", VehicleNoSiren, "Disables your vehicle's siren. Only works if your vehicle actually has a siren.");
            UIMenuCheckboxItem vehicleNoBikeHelmet = new UIMenuCheckboxItem("No Bike Helmet", VehicleNoBikeHelemet, "No longer auto-equip a helmet when getting on a bike or quad.");
            UIMenuCheckboxItem vehicleFreeze = new UIMenuCheckboxItem("Freeze Vehicle", VehicleFrozen, "Freeze your vehicle's position.");
            UIMenuCheckboxItem torqueEnabled = new UIMenuCheckboxItem("Enable Torque Multiplier", VehicleTorqueMultiplier, "Enables the torque multiplier selected from the list below.");
            UIMenuCheckboxItem powerEnabled = new UIMenuCheckboxItem("Enable Power Multiplier", VehiclePowerMultiplier, "Enables the power multiplier selected from the list below.");
            UIMenuCheckboxItem highbeamsOnHonk = new UIMenuCheckboxItem("Flash Highbeams On Honk", FlashHighbeamsOnHonk, "Turn on your highbeams on your vehicle when honking your horn." +
                " Does not work during the day when you have your lights turned off.");

            // Create buttons.
            UIMenuItem fixVehicle = new UIMenuItem("Repair Vehicle", "Repair any visual and physical damage present on your vehicle.");
            UIMenuItem cleanVehicle = new UIMenuItem("Wash Vehicle", "Clean your vehicle.");
            UIMenuItem toggleEngine = new UIMenuItem("Toggle Engine On/Off", "Turn your engine on/off.");
            UIMenuItem setLicensePlateText = new UIMenuItem("Set License Plate Text", "Enter a custom license plate for your vehicle.");
            UIMenuItem modMenuBtn = new UIMenuItem("Mod Menu", "Tune and customize your vehicle here.");
            modMenuBtn.SetRightLabel("→→→");
            UIMenuItem doorsMenuBtn = new UIMenuItem("Vehicle Doors", "Open, close, remove and restore vehicle doors here.");
            doorsMenuBtn.SetRightLabel("→→→");
            UIMenuItem windowsMenuBtn = new UIMenuItem("Vehicle Windows", "Roll your windows up/down or remove/restore your vehicle windows here.");
            windowsMenuBtn.SetRightLabel("→→→");
            UIMenuItem componentsMenuBtn = new UIMenuItem("Vehicle Extras", "Add/remove vehicle components/extras.");
            componentsMenuBtn.SetRightLabel("→→→");
            UIMenuItem liveriesMenuBtn = new UIMenuItem("Vehicle Liveries", "Style your vehicle with fancy liveries!");
            liveriesMenuBtn.SetRightLabel("→→→");
            UIMenuItem colorsMenuBtn = new UIMenuItem("Vehicle Colors", "Style your vehicle even further by giving it some ~g~Snailsome ~s~colors!");
            colorsMenuBtn.SetRightLabel("→→→");
            UIMenuItem underglowMenuBtn = new UIMenuItem("Vehicle Underglow Options", "Make your vehicle shine with some fancy underglow!");
            underglowMenuBtn.SetRightLabel("→→→");
            UIMenuItem flipVehicle = new UIMenuItem("Flip Vehicle", "Sets your current vehicle on all 4 wheels.");
            UIMenuItem vehicleAlarm = new UIMenuItem("Toggle Vehicle Alarm", "Starts/stops your vehicle's alarm.");
            UIMenuItem cycleSeats = new UIMenuItem("Cycle Through Vehicle Seats", "Cycle through the available vehicle seats.");
            List<dynamic> lights = new List<dynamic>()
            {
                "Hazard Lights",
                "Left Indicator",
                "Right Indicator",
                //"Interior Lights",
                //"Taxi Light", // this doesn't seem to work no matter what.
                "Helicopter Spotlight",
            };
            UIMenuListItem vehicleLights = new UIMenuListItem("Vehicle Lights", lights, 0, "Turn vehicle lights on/off.");
            UIMenuItem deleteBtn = new UIMenuItem("~r~Delete Vehicle", "Delete your vehicle, this ~r~can NOT be undone~s~!");
            deleteBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
            deleteBtn.SetRightLabel("→→→");
            UIMenuItem deleteNoBtn = new UIMenuItem("NO, CANCEL", "NO, do NOT delete my vehicle and go back!");
            UIMenuItem deleteYesBtn = new UIMenuItem("~r~YES, DELETE", "Yes I'm sure, delete my vehicle please, I understand that this cannot be undone.");
            deleteYesBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
            deleteYesBtn.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);

            // Create lists.
            var dirtlevel = new List<dynamic> { "No Dirt", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            UIMenuListItem setDirtLevel = new UIMenuListItem("Set Dirt Level", dirtlevel, 0, "Select how much dirt should be visible on your vehicle, press ~r~enter~s~ " +
                "to apply the selected level.");
            var licensePlates = new List<dynamic> { GetLabelText("CMOD_PLA_0"), GetLabelText("CMOD_PLA_1"), GetLabelText("CMOD_PLA_2"), GetLabelText("CMOD_PLA_3"),
                GetLabelText("CMOD_PLA_4"), "North Yankton" };
            UIMenuListItem setLicensePlateType = new UIMenuListItem("License Plate Type", licensePlates, 0, "Choose a license plate type and press ~r~enter ~s~to apply " +
                "it to your vehicle.");
            var torqueMultiplierList = new List<dynamic> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            UIMenuListItem torqueMultiplier = new UIMenuListItem("Set Engine Torque Multiplier", torqueMultiplierList, 0, "Set the engine torque multiplier.");
            var powerMultiplierList = new List<dynamic> { "x2", "x4", "x8", "x16", "x32", "x64", "x128", "x256", "x512", "x1024" };
            UIMenuListItem powerMultiplier = new UIMenuListItem("Set Engine Power Multiplier", powerMultiplierList, 0, "Set the engine power multiplier.");
            #endregion

            #region Submenus
            // Submenu's
            VehicleModMenu = new UIMenu("Mod Menu", "Vehicle Mods", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            VehicleDoorsMenu = new UIMenu("Vehicle Doors", "Vehicle Doors Management", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            VehicleWindowsMenu = new UIMenu("Vehicle Windows", "Vehicle Windows Management", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            VehicleComponentsMenu = new UIMenu("Vehicle Extras", "Vehicle Extras/Components", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            VehicleLiveriesMenu = new UIMenu("Vehicle Liveries", "Vehicle Liveries", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            VehicleColorsMenu = new UIMenu("Vehicle Colors", "Vehicle Colors", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            DeleteConfirmMenu = new UIMenu("Confirm Action", "Delete Vehicle, Are You Sure?", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            VehicleUnderglowMenu = new UIMenu("Vehicle Underglow", "Vehicle Underglow Options", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            MainMenu.Mp.Add(VehicleModMenu);
            MainMenu.Mp.Add(VehicleDoorsMenu);
            MainMenu.Mp.Add(VehicleWindowsMenu);
            MainMenu.Mp.Add(VehicleComponentsMenu);
            MainMenu.Mp.Add(VehicleLiveriesMenu);
            MainMenu.Mp.Add(VehicleColorsMenu);
            MainMenu.Mp.Add(DeleteConfirmMenu);
            MainMenu.Mp.Add(VehicleUnderglowMenu);
            #endregion

            #region Add items to the menu.
            // Add everything to the menu. (based on permissions)
            if (cf.IsAllowed(Permission.VOGod)) // GOD MODE
            {
                menu.AddItem(vehicleGod);
            }
            if (cf.IsAllowed(Permission.VOSpecialGod)) // special god mode
            {
                menu.AddItem(vehicleSpecialGod);
            }
            if (cf.IsAllowed(Permission.VORepair)) // REPAIR VEHICLE
            {
                menu.AddItem(fixVehicle);
            }
            if (cf.IsAllowed(Permission.VOWash))
            {
                menu.AddItem(cleanVehicle); // CLEAN VEHICLE
                menu.AddItem(setDirtLevel); // SET DIRT LEVEL
            }
            if (cf.IsAllowed(Permission.VOEngine)) // TOGGLE ENGINE ON/OFF
            {
                menu.AddItem(toggleEngine);
            }
            if (cf.IsAllowed(Permission.VOChangePlate))
            {
                menu.AddItem(setLicensePlateText); // SET LICENSE PLATE TEXT
                menu.AddItem(setLicensePlateType); // SET LICENSE PLATE TYPE
            }
            if (cf.IsAllowed(Permission.VOMod)) // MOD MENU
            {
                menu.AddItem(modMenuBtn);
            }
            if (cf.IsAllowed(Permission.VOColors)) // COLORS MENU
            {
                menu.AddItem(colorsMenuBtn);
            }
            if (cf.IsAllowed(Permission.VOUnderglow)) // UNDERGLOW EFFECTS
            {
                menu.AddItem(underglowMenuBtn);
                menu.BindMenuToItem(VehicleUnderglowMenu, underglowMenuBtn);
            }
            if (cf.IsAllowed(Permission.VOLiveries)) // LIVERIES MENU
            {
                menu.AddItem(liveriesMenuBtn);
            }
            if (cf.IsAllowed(Permission.VOComponents)) // COMPONENTS MENU
            {
                menu.AddItem(componentsMenuBtn);
            }
            if (cf.IsAllowed(Permission.VODoors)) // DOORS MENU
            {
                menu.AddItem(doorsMenuBtn);
            }
            if (cf.IsAllowed(Permission.VOWindows)) // WINDOWS MENU
            {
                menu.AddItem(windowsMenuBtn);
            }
            if (cf.IsAllowed(Permission.VOTorqueMultiplier))
            {
                menu.AddItem(torqueEnabled); // TORQUE ENABLED
                menu.AddItem(torqueMultiplier); // TORQUE LIST
            }
            if (cf.IsAllowed(Permission.VOPowerMultiplier))
            {
                menu.AddItem(powerEnabled); // POWER ENABLED
                menu.AddItem(powerMultiplier); // POWER LIST
            }
            if (cf.IsAllowed(Permission.VOFlip)) // FLIP VEHICLE
            {
                menu.AddItem(flipVehicle);
            }
            if (cf.IsAllowed(Permission.VOAlarm)) // TOGGLE VEHICLE ALARM
            {
                menu.AddItem(vehicleAlarm);
            }
            if (cf.IsAllowed(Permission.VOCycleSeats)) // CYCLE THROUGH VEHICLE SEATS
            {
                menu.AddItem(cycleSeats);
            }
            if (cf.IsAllowed(Permission.VOLights)) // VEHICLE LIGHTS LIST
            {
                menu.AddItem(vehicleLights);
            }
            if (cf.IsAllowed(Permission.VOFreeze)) // FREEZE VEHICLE
            {
                menu.AddItem(vehicleFreeze);
            }
            if (cf.IsAllowed(Permission.VOEngineAlwaysOn)) // LEAVE ENGINE RUNNING
            {
                menu.AddItem(vehicleEngineAO);
            }
            if (cf.IsAllowed(Permission.VONoSiren) && !vMenuShared.ConfigManager.GetSettingsBool(vMenuShared.ConfigManager.SettingsCategory.external, vMenuShared.ConfigManager.Setting.use_els_compatibility_mode)) // DISABLE SIREN
            {
                menu.AddItem(vehicleNoSiren);
            }
            if (cf.IsAllowed(Permission.VONoHelmet)) // DISABLE BIKE HELMET
            {
                menu.AddItem(vehicleNoBikeHelmet);
            }
            if (cf.IsAllowed(Permission.VOFlashHighbeamsOnHonk)) // FLASH HIGHBEAMS ON HONK
            {
                menu.AddItem(highbeamsOnHonk);
            }

            if (cf.IsAllowed(Permission.VODelete)) // DELETE VEHICLE
            {
                menu.AddItem(deleteBtn);
            }
            #endregion

            #region delete vehicle handle stuff
            DeleteConfirmMenu.AddItem(deleteNoBtn);
            DeleteConfirmMenu.AddItem(deleteYesBtn);
            DeleteConfirmMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == deleteNoBtn)
                {
                    DeleteConfirmMenu.GoBack();
                }
                else
                {
                    var veh = cf.GetVehicle();
                    if (DoesEntityExist(veh) && GetPedInVehicleSeat(veh, -1) == PlayerPedId())
                    {
                        SetVehicleHasBeenOwnedByPlayer(veh, false);
                        SetEntityAsMissionEntity(veh, false, false);
                        DeleteVehicle(ref veh);
                    }
                    else
                    {
                        if (!IsPedInAnyVehicle(PlayerPedId(), false))
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
            menu.BindMenuToItem(VehicleModMenu, modMenuBtn);
            menu.BindMenuToItem(VehicleDoorsMenu, doorsMenuBtn);
            menu.BindMenuToItem(VehicleWindowsMenu, windowsMenuBtn);
            menu.BindMenuToItem(VehicleComponentsMenu, componentsMenuBtn);
            menu.BindMenuToItem(VehicleLiveriesMenu, liveriesMenuBtn);
            menu.BindMenuToItem(VehicleColorsMenu, colorsMenuBtn);
            menu.BindMenuToItem(DeleteConfirmMenu, deleteBtn);
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
                            cf.SetLicensePlateTextAsync();
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
                        cf.CycleThroughSeats();
                    }
                }
            };
            #endregion

            #region Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                // Create a vehicle object.
                Vehicle vehicle = new Vehicle(cf.GetVehicle());

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
                        FreezeEntityPosition(cf.GetVehicle(), false);
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
                else if (item == highbeamsOnHonk)
                {
                    FlashHighbeamsOnHonk = _checked;
                }
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
                // Set dirt level
                if (item == setDirtLevel)
                {
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        Vehicle veh = new Vehicle(cf.GetVehicle())
                        {
                            DirtLevel = float.Parse(index.ToString())
                        };
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
                // Toggle vehicle lights
                else if (item == vehicleLights)
                {
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        var veh = cf.GetVehicle();
                        var state = GetVehicleIndicatorLights(veh); // 0 = none, 1 = left, 2 = right, 3 = both

                        if (index == 0) // Hazard lights
                        {
                            if (state != 3) // either all lights are off, or one of the two (left/right) is off.
                            {
                                SetVehicleIndicatorLights(veh, 1, true); // left on
                                SetVehicleIndicatorLights(veh, 0, true); // right on
                            }
                            else // both are on.
                            {
                                SetVehicleIndicatorLights(veh, 1, false); // left off
                                SetVehicleIndicatorLights(veh, 0, false); // right off
                            }
                        }
                        else if (index == 1) // left indicator
                        {
                            if (state != 1) // Left indicator is (only) off
                            {
                                SetVehicleIndicatorLights(veh, 1, true); // left on
                                SetVehicleIndicatorLights(veh, 0, false); // right off
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh, 1, false); // left off
                                SetVehicleIndicatorLights(veh, 0, false); // right off
                            }
                        }
                        else if (index == 2) // right indicator
                        {
                            if (state != 2) // Right indicator (only) is off
                            {
                                SetVehicleIndicatorLights(veh, 1, false); // left off
                                SetVehicleIndicatorLights(veh, 0, true); // right on
                            }
                            else
                            {
                                SetVehicleIndicatorLights(veh, 1, false); // left off
                                SetVehicleIndicatorLights(veh, 0, false); // right off
                            }
                        }
                        //else if (index == 3) // Interior lights
                        //{
                        //    cf.Log("Something cool here.");
                        //}
                        //else if (index == 4) // taxi light
                        //{
                        //    SetTaxiLights(veh, true);
                        //    SetTaxiLights(veh, false);
                        //    //MainMenu.Cf.Log(IsTaxiLightOn(veh).ToString());
                        //    //SetTaxiLights(veh, true);
                        //    //MainMenu.Cf.Log(IsTaxiLightOn(veh).ToString());
                        //    //SetTaxiLights(veh, false);
                        //    //SetTaxiLights(veh, !IsTaxiLightOn(veh));
                        //    MainMenu.Cf.Log
                        //}
                        else if (index == 3) // helicopter spotlight
                        {
                            SetVehicleSearchlight(veh, !IsVehicleSearchlightOn(veh), true);
                        }
                    }
                    else
                    {
                        Notify.Error(CommonErrors.NoVehicle);
                    }
                }
            };
            #endregion

            #region Vehicle Colors Submenu Stuff

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

            // Dashboard Trim Colors
            List<dynamic> DashboardColor = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.MetallicColors)
            {
                DashboardColor.Add(color.Key.ToString());
            }

            // Extra Accent Colors
            List<dynamic> TrimColor = new List<dynamic>();
            foreach (KeyValuePair<string, int> color in vd.MetallicColors)
            {
                TrimColor.Add(color.Key.ToString());
            }
            #endregion

            #region Create the headers + menu list items
            // Headers
            UIMenuItem primaryColorsHeader = cf.GetSpacerMenuItem("PRIMARY COLORS");
            UIMenuItem secondaryColorsHeader = cf.GetSpacerMenuItem("SECONDARY COLORS");
            UIMenuItem otherColorsHeader = cf.GetSpacerMenuItem("OTHER COLORS");

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
            UIMenuListItem wheelColors = new UIMenuListItem("Wheel Color", Wheels, 0, "Select a color for your wheels.");
            // Chrome Button
            UIMenuItem chromeBtn = new UIMenuItem("Chrome", "Make your vehicle chrome!");
            UIMenuListItem dashboardColors = new UIMenuListItem("Dashboard Color", DashboardColor, 0, "Select a dashboard color (only availalbe on some cars).");
            UIMenuListItem trimColors = new UIMenuListItem("Trim Color", TrimColor, 0, "Select an trim/accent color (only availalbe on some cars).");
            #endregion

            #region Add the items to the colors menu.
            // Primary Colors
            VehicleColorsMenu.AddItem(primaryColorsHeader); // header
            VehicleColorsMenu.AddItem(classicColors);
            VehicleColorsMenu.AddItem(metallicColors);
            VehicleColorsMenu.AddItem(matteColors);
            VehicleColorsMenu.AddItem(metalsColors);
            VehicleColorsMenu.AddItem(utilsColors);
            VehicleColorsMenu.AddItem(wornColors);

            // Secondary Colors
            VehicleColorsMenu.AddItem(secondaryColorsHeader); // header
            VehicleColorsMenu.AddItem(classicColors2);
            VehicleColorsMenu.AddItem(metallicColors2);
            VehicleColorsMenu.AddItem(matteColors2);
            VehicleColorsMenu.AddItem(metalsColors2);
            VehicleColorsMenu.AddItem(utilsColors2);
            VehicleColorsMenu.AddItem(wornColors2);

            // Other Colors
            VehicleColorsMenu.AddItem(otherColorsHeader); // header
            VehicleColorsMenu.AddItem(pearlescentColors);
            VehicleColorsMenu.AddItem(wheelColors);
            VehicleColorsMenu.AddItem(chromeBtn);
            VehicleColorsMenu.AddItem(dashboardColors);
            VehicleColorsMenu.AddItem(trimColors);
            #endregion

            #region Handle Vehicle Color Changes
            VehicleColorsMenu.OnListChange += (sender, item, index) =>
            {
                // Get the current vehicle.
                var veh = cf.GetVehicle();

                // Check if the vehicle exists and isn't dead and the player is the driver of the vehicle.
                if (DoesEntityExist(veh) && !IsEntityDead(veh) && GetPedInVehicleSeat(veh, -1) == PlayerPedId())
                {
                    var vehi = new Vehicle(veh);
                    // Get the primary and secondary colors from the current vehicle..
                    int primary = 0;
                    int secondary = 0;
                    int pearlescent = 0;
                    int wheels = 0;

                    int trimColor = 0;
                    int dashboardColor = 0;
                    GetVehicleInteriorColour(veh, ref trimColor);
                    GetVehicleDashboardColour(veh, ref dashboardColor);

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
                            wheels = vd.MetallicColors[Metallic[index - 1]];
                        }
                    }

                    else if (item == dashboardColors)
                    {
                        trimColor = vd.MetallicColors[DashboardColor[index]];
                        SetVehicleInteriorColour(veh, trimColor);
                    }
                    else if (item == trimColors)
                    {
                        dashboardColor = vd.MetallicColors[TrimColor[index]];
                        SetVehicleDashboardColour(veh, dashboardColor);
                    }

                    // Set the mod kit so we can modify things.
                    SetVehicleModKit(veh, 0);

                    // Set all the colors.
                    SetVehicleColours(cf.GetVehicle(), primary, secondary);
                    SetVehicleExtraColours(veh, pearlescent, wheels);
                }
            };
            #endregion

            #region Handle Chrome Button Pressed
            // Handle chrome button press.
            VehicleColorsMenu.OnItemSelect += (sender, item, index) =>
            {
                // Set primary and secondary color to chrome.
                SetVehicleColours(cf.GetVehicle(), (int)VehicleColor.Chrome, (int)VehicleColor.Chrome);
            };
            #endregion

            #endregion

            #region Vehicle Doors Submenu Stuff
            UIMenuItem openAll = new UIMenuItem("Open All Doors", "Open all vehicle doors.");
            UIMenuItem closeAll = new UIMenuItem("Close All Doors", "Close all vehicle doors.");
            UIMenuItem LF = new UIMenuItem("Left Front Door", "Open/close the left front door.");
            UIMenuItem RF = new UIMenuItem("Right Front Door", "Open/close the right front door.");
            UIMenuItem LR = new UIMenuItem("Left Rear Door", "Open/close the left rear door.");
            UIMenuItem RR = new UIMenuItem("Right Rear Door", "Open/close the right rear door.");
            UIMenuItem HD = new UIMenuItem("Hood", "Open/close the hood.");
            UIMenuItem TR = new UIMenuItem("Trunk", "Open/close the trunk.");

            VehicleDoorsMenu.AddItem(LF);
            VehicleDoorsMenu.AddItem(RF);
            VehicleDoorsMenu.AddItem(LR);
            VehicleDoorsMenu.AddItem(RR);
            VehicleDoorsMenu.AddItem(HD);
            VehicleDoorsMenu.AddItem(TR);
            VehicleDoorsMenu.AddItem(openAll);
            VehicleDoorsMenu.AddItem(closeAll);

            // Handle button presses.
            VehicleDoorsMenu.OnItemSelect += (sender, item, index) =>
            {
                // Get the vehicle.
                var veh = cf.GetVehicle();
                // If the player is in a vehicle, it's not dead and the player is the driver, continue.
                if (DoesEntityExist(veh) && !IsEntityDead(veh) && GetPedInVehicleSeat(veh, -1) == PlayerPedId())
                {
                    // If button 0-5 are pressed, then open/close that specific index/door.
                    if (index < 6)
                    {
                        // If the door is open.
                        bool open = GetVehicleDoorAngleRatio(veh, index) > 0.1f ? true : false;

                        if (open)
                        {
                            // Close the door.
                            SetVehicleDoorShut(veh, index, false);
                        }
                        else
                        {
                            // Open the door.
                            SetVehicleDoorOpen(veh, index, false, false);
                        }
                    }
                    // If the index >= 6, and the button is "openAll": open all doors.
                    else if (item == openAll)
                    {
                        // Loop through all doors and open them.
                        for (var door = 0; door < 6; door++)
                        {
                            SetVehicleDoorOpen(veh, door, false, false);
                        }
                    }
                    // If the index >= 6, and the button is "closeAll": close all doors.
                    else if (item == closeAll)
                    {
                        // Close all doors.
                        SetVehicleDoorsShut(veh, false);
                    }
                }
                else
                {
                    Notify.Alert(CommonErrors.NoVehicle, placeholderValue: "to open/close a vehicle door");
                }

            };

            #endregion

            #region Vehicle Windows Submenu Stuff
            UIMenuItem fwu = new UIMenuItem("~y~↑~s~ Roll Front Windows Up", "Roll both front windows up.");
            UIMenuItem fwd = new UIMenuItem("~o~↓~s~ Roll Front Windows Down", "Roll both front windows down.");
            UIMenuItem rwu = new UIMenuItem("~y~↑~s~ Roll Rear Windows Up", "Roll both rear windows up.");
            UIMenuItem rwd = new UIMenuItem("~o~↓~s~ Roll Rear Windows Down", "Roll both rear windows down.");
            VehicleWindowsMenu.AddItem(fwu);
            VehicleWindowsMenu.AddItem(fwd);
            VehicleWindowsMenu.AddItem(rwu);
            VehicleWindowsMenu.AddItem(rwd);
            VehicleWindowsMenu.OnItemSelect += (sender, item, index) =>
            {
                var veh = cf.GetVehicle();
                if (DoesEntityExist(veh) && !IsEntityDead(veh))
                {
                    if (item == fwu)
                    {
                        RollUpWindow(veh, 0);
                        RollUpWindow(veh, 1);
                    }
                    else if (item == fwd)
                    {
                        RollDownWindow(veh, 0);
                        RollDownWindow(veh, 1);
                    }
                    else if (item == rwu)
                    {
                        RollUpWindow(veh, 2);
                        RollUpWindow(veh, 3);
                    }
                    else if (item == rwd)
                    {
                        RollDownWindow(veh, 2);
                        RollDownWindow(veh, 3);
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
                    var veh = cf.GetVehicle();
                    // If it exists, isn't dead and the player is in the drivers seat continue.
                    if (DoesEntityExist(veh) && !IsEntityDead(veh))
                    {
                        if (GetPedInVehicleSeat(veh, -1) == PlayerPedId())
                        {
                            VehicleLiveriesMenu.MenuItems.Clear();
                            SetVehicleModKit(veh, 0);
                            var liveryCount = GetVehicleLiveryCount(veh);

                            if (liveryCount > 0)
                            {
                                var liveryList = new List<dynamic>();
                                for (var i = 0; i < liveryCount; i++)
                                {
                                    var livery = GetLiveryName(veh, i);
                                    livery = GetLabelText(livery) != "NULL" ? GetLabelText(livery) : $"Livery #{i}";
                                    liveryList.Add(livery);
                                }
                                UIMenuListItem liveryListItem = new UIMenuListItem("Set Livery", liveryList, GetVehicleLivery(veh), "Choose a livery for this vehicle.");
                                VehicleLiveriesMenu.AddItem(liveryListItem);
                                VehicleLiveriesMenu.OnListChange += (sender2, item2, index2) =>
                                {
                                    veh = cf.GetVehicle();
                                    SetVehicleLivery(veh, index2);
                                };
                                VehicleLiveriesMenu.RefreshIndex();
                                VehicleLiveriesMenu.UpdateScaleform();
                            }
                            else
                            {
                                Notify.Error("This vehicle does not have any liveries.");
                                VehicleLiveriesMenu.Visible = false;
                                menu.Visible = true;
                                UIMenuItem backBtn = new UIMenuItem("No Liveries Available :(", "Click me to go back.");
                                backBtn.SetRightLabel("Go Back");
                                VehicleLiveriesMenu.AddItem(backBtn);
                                VehicleLiveriesMenu.OnItemSelect += (sender2, item2, index2) =>
                                {
                                    if (item2 == backBtn)
                                    {
                                        VehicleLiveriesMenu.GoBack();
                                    }
                                };

                                VehicleLiveriesMenu.RefreshIndex();
                                VehicleLiveriesMenu.UpdateScaleform();
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
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        UpdateMods();
                    }
                    else
                    {
                        VehicleModMenu.Visible = false;
                        menu.Visible = true;
                    }

                }
            };
            #endregion

            #region Vehicle Components Submenu
            menu.OnItemSelect += (sender, item, index) =>
            {
                // If the components menu is opened.
                if (item == componentsMenuBtn)
                {
                    // Empty the menu in case there were leftover buttons from another vehicle.
                    if (VehicleComponentsMenu.MenuItems.Count > 0)
                    {
                        VehicleComponentsMenu.Clear();
                        vehicleExtras.Clear();
                        VehicleComponentsMenu.RefreshIndex();
                        VehicleComponentsMenu.UpdateScaleform();
                    }

                    // Get the vehicle.
                    int veh = cf.GetVehicle();
                    // Check if the vehicle exists, it's actually a vehicle, it's not dead/broken and the player is in the drivers seat.
                    if (DoesEntityExist(veh) && !IsEntityDead(veh) && IsEntityAVehicle(veh) && GetPedInVehicleSeat(veh, -1) == PlayerPedId())
                    {
                        // Create a vehicle.
                        Vehicle vehicle = new Vehicle(veh);

                        //List<int> extraIds = new List<int>();
                        // Loop through all possible extra ID's (AFAIK: 0-14).
                        for (var extra = 0; extra < 14; extra++)
                        {
                            // If this extra exists...
                            if (vehicle.ExtraExists(extra))
                            {
                                // Add it's ID to the list.
                                //extraIds.Add(extra);

                                // Create a checkbox for it.
                                UIMenuCheckboxItem extraCheckbox = new UIMenuCheckboxItem($"Extra #{extra.ToString()}", vehicle.IsExtraOn(extra), extra.ToString());
                                // Add the checkbox to the menu.
                                VehicleComponentsMenu.AddItem(extraCheckbox);

                                // Add it's ID to the dictionary.
                                vehicleExtras[extraCheckbox] = extra;
                            }
                        }

                        // When a checkbox is checked/unchecked, get the selected checkbox item index and use that to get the component ID from the list.
                        VehicleComponentsMenu.OnCheckboxChange += (sender2, item2, _checked) =>
                        {
                            // Then toggle that extra.
                            //vehicle.ToggleExtra(extraIds[sender2.CurrentSelection], _checked);
                            if (vehicleExtras.TryGetValue(item2, out int extra))
                            {
                                vehicle.ToggleExtra(extra, _checked);
                            }
                            //if (vehicleExtras.ContainsKey(item2))
                            //{
                            //    int extra = 
                            //}
                        };

                        if (vehicleExtras.Count > 0)
                        {
                            UIMenuItem backBtn = new UIMenuItem("Go Back", "Go back to the Vehicle Options menu.");
                            VehicleComponentsMenu.AddItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        else
                        {
                            UIMenuItem backBtn = new UIMenuItem("No Extras Available :(", "Go back to the Vehicle Options menu.");
                            backBtn.SetRightLabel("Go Back");
                            VehicleComponentsMenu.AddItem(backBtn);
                            VehicleComponentsMenu.OnItemSelect += (sender3, item3, index3) =>
                            {
                                VehicleComponentsMenu.GoBack();
                            };
                        }
                        // And update the submenu to prevent weird glitches.
                        VehicleComponentsMenu.RefreshIndex();
                        VehicleComponentsMenu.UpdateScaleform();

                    }
                }
            };
            #endregion

            #region Underglow Submenu
            UIMenuCheckboxItem underglowFront = new UIMenuCheckboxItem("Enable Front Light", false, "Enable or disable the underglow on the front side of the" +
                " vehicle. Note not all vehicles have lights.");
            UIMenuCheckboxItem underglowBack = new UIMenuCheckboxItem("Enable Rear Light", false, "Enable or disable the underglow on the left side of the vehicle. " +
                "Note not all vehicles have lights.");
            UIMenuCheckboxItem underglowLeft = new UIMenuCheckboxItem("Enable Left Light", false, "Enable or disable the underglow on the right side of the " +
                "vehicle. Note not all vehicles have lights.");
            UIMenuCheckboxItem underglowRight = new UIMenuCheckboxItem("Enable Right Light", false, "Enable or disable the underglow on the back side of the " +
                "vehicle. Note not all vehicles have lights.");
            var underglowColorsList = new List<dynamic>() { "Red", "Pink", "Purple", "Blacklight", "Dark Blue", "Light Blue", "White", "Lime", "Green", "Dark Green", "Gold", "Orange", "Yellow" };
            UIMenuListItem underglowColor = new UIMenuListItem("Underglow Color", underglowColorsList, 0, "Select the color of the underglow.");

            VehicleUnderglowMenu.AddItem(underglowFront);
            VehicleUnderglowMenu.AddItem(underglowBack);
            VehicleUnderglowMenu.AddItem(underglowLeft);
            VehicleUnderglowMenu.AddItem(underglowRight);

            VehicleUnderglowMenu.AddItem(underglowColor);

            menu.OnItemSelect += (sender, item, index) =>
            {
                #region reset checkboxes state when opening the menu.
                if (item == underglowMenuBtn)
                {
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        Vehicle veh = new Vehicle(cf.GetVehicle());
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

                            underglowFront.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                            underglowBack.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                            underglowLeft.SetLeftBadge(UIMenuItem.BadgeStyle.None);
                            underglowRight.SetLeftBadge(UIMenuItem.BadgeStyle.None);
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

                            underglowFront.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            underglowBack.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            underglowLeft.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                            underglowRight.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
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

                        underglowFront.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        underglowBack.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        underglowLeft.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                        underglowRight.SetLeftBadge(UIMenuItem.BadgeStyle.Lock);
                    }
                }
                #endregion
            };
            // handle item selections
            VehicleUnderglowMenu.OnCheckboxChange += (sender, item, _checked) =>
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false))
                {
                    Vehicle veh = new Vehicle(cf.GetVehicle());
                    if (veh.Mods.HasNeonLights)
                    {
                        veh.Mods.NeonLightsColor = GetColorFromIndex(underglowColor.Index);
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

            VehicleUnderglowMenu.OnListChange += (sender, item, newIndex) =>
            {
                if (item == underglowColor)
                {
                    if (IsPedInAnyVehicle(PlayerPedId(), false))
                    {
                        Vehicle veh = new Vehicle(cf.GetVehicle());
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

        #region Update Vehicle Mods Menu
        /// <summary>
        /// Refreshes the mods page. The selectedIndex allows you to go straight to a specific index after refreshing the menu.
        /// This is used because when the wheel type is changed, the menu is refreshed to update the available wheels list.
        /// </summary>
        /// <param name="selectedIndex">Pass this if you want to go straight to a specific mod/index.</param>
        public void UpdateMods(int selectedIndex = 0)
        {
            // If there are items, remove all of them.
            if (VehicleModMenu.MenuItems.Count > 0)
            {
                VehicleModMenu.Clear();
            }

            // Get the vehicle.
            int veh = -1;
            veh = cf.GetVehicle();

            // Check if the vehicle exists, is still drivable/alive and it's actually a vehicle.
            if (DoesEntityExist(veh) && IsEntityAVehicle(veh) && !IsEntityDead(veh))
            {
                #region initial setup & dynamic vehicle mods setup
                // Set the modkit so we can modify the car.
                SetVehicleModKit(veh, 0);

                // Create a Vehicle object for it, this is used to get some of the vehicle mods.
                Vehicle vehicle = new Vehicle(veh);

                // Get all mods available on this vehicle.
                VehicleMod[] mods = vehicle.Mods.GetAllMods();

                // Loop through all the mods.
                foreach (var mod in mods)
                {
                    veh = cf.GetVehicle();

                    // Get the mod type (suspension, armor, etc) name (convert the PascalCase to the Proper Case string values).
                    var typeName = cf.ToProperString(mod.ModType.ToString());

                    // Create a list to all available upgrades for this modtype.
                    var modlist = new List<dynamic>();

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
                        name = mod.GetLocalizedModName(x) != "" ? $"{cf.ToProperString(mod.GetLocalizedModName(x))} {currentItem}" : $"{typeName} #{x.ToString()} {currentItem}";
                        modlist.Add(name);
                    }

                    // Create the UIMenuListItem for this mod type.
                    var currIndex = GetVehicleMod(veh, (int)mod.ModType) + 1;
                    UIMenuListItem modTypeListItem = new UIMenuListItem(typeName, modlist, currIndex, $"Choose a ~y~{typeName}~s~ upgrade, it will be automatically applied to your vehicle.");

                    // Add the list item to the menu.
                    VehicleModMenu.AddItem(modTypeListItem);
                }
                #endregion

                #region more variables and setup
                veh = cf.GetVehicle();
                // Create the wheel types list & listitem and add it to the menu.
                List<dynamic> wheelTypes = new List<dynamic>() { "Sports", "Muscle", "Lowrider", "SUV", "Offroad", "Tuner", "Bike Wheels", "High End" };
                UIMenuListItem vehicleWheelType = new UIMenuListItem("Wheel Type", wheelTypes, GetVehicleWheelType(veh), $"Choose a ~y~wheel type~s~ for your vehicle. ~r~Important:~s~ if you change the wheel type, you will need to back out of the Vehicle Mods menu for the Wheels List to update.");
                VehicleModMenu.AddItem(vehicleWheelType);

                // Create the checkboxes for some options.
                UIMenuCheckboxItem toggleCustomWheels = new UIMenuCheckboxItem("Toggle Custom Wheels", GetVehicleModVariation(veh, 23), "Press this to add or remove ~y~custom~s~ wheels.");
                UIMenuCheckboxItem xenonHeadlights = new UIMenuCheckboxItem("Xenon Headlights", IsToggleModOn(veh, 22), "Enable or disable ~b~xenon ~s~headlights.");
                UIMenuCheckboxItem turbo = new UIMenuCheckboxItem("Turbo", IsToggleModOn(veh, 18), "Enable or disable the ~y~turbo~s~ for this vehicle.");
                UIMenuCheckboxItem bulletProofTires = new UIMenuCheckboxItem("Bullet Proof Tires", !GetVehicleTyresCanBurst(veh), "Enable or disable ~y~bullet proof tires~s~ for this vehicle.");

                // Add the checkboxes to the menu.
                VehicleModMenu.AddItem(toggleCustomWheels);
                VehicleModMenu.AddItem(xenonHeadlights);
                VehicleModMenu.AddItem(turbo);
                VehicleModMenu.AddItem(bulletProofTires);
                // Create a list of tire smoke options.
                List<dynamic> tireSmokes = new List<dynamic>() { "Red", "Orange", "Yellow", "Gold", "Light Green", "Dark Green", "Light Blue", "Dark Blue", "Purple", "Pink", "Black" };
                Dictionary<String, int[]> tireSmokeColors = new Dictionary<string, int[]>()
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
                UIMenuListItem tireSmoke = new UIMenuListItem("Tire Smoke Color", tireSmokes, 0, $"Choose a ~y~wheel type~s~ for your vehicle.");
                VehicleModMenu.AddItem(tireSmoke);

                // Create the checkbox to enable/disable the tiresmoke.
                UIMenuCheckboxItem tireSmokeEnabled = new UIMenuCheckboxItem("Tire Smoke", IsToggleModOn(veh, 20), "Enable or disable ~y~tire smoke~s~ for your vehicle. ~h~~r~Important:~s~ When disabling tire smoke, you'll need to drive around before it takes affect.");
                VehicleModMenu.AddItem(tireSmokeEnabled);

                // Create list for window tint
                List<dynamic> windowTints = new List<dynamic>() { "Stock [1/7]", "None [2/7]", "Limo [3/7]", "Light Smoke [4/7]", "Dark Smoke [5/7]", "Pure Black [6/7]", "Green [7/7]" };
                var currentTint = GetVehicleWindowTint(veh);
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

                UIMenuListItem windowTint = new UIMenuListItem("Window Tint", windowTints, currentTint, "Apply tint to your windows.");
                VehicleModMenu.AddItem(windowTint);

                #endregion

                #region Checkbox Changes
                // Handle checkbox changes.
                VehicleModMenu.OnCheckboxChange += (sender2, item2, _checked) =>
                {
                    veh = cf.GetVehicle();

                    // Xenon Headlights
                    if (item2 == xenonHeadlights)
                    {
                        ToggleVehicleMod(veh, 22, _checked);
                    }
                    // Turbo
                    else if (item2 == turbo)
                    {
                        ToggleVehicleMod(veh, 18, _checked);
                    }
                    // Bullet Proof Tires
                    else if (item2 == bulletProofTires)
                    {
                        SetVehicleTyresCanBurst(veh, _checked);
                    }
                    // Custom Wheels
                    else if (item2 == toggleCustomWheels)
                    {
                        SetVehicleMod(veh, 23, GetVehicleMod(veh, 23), !GetVehicleModVariation(veh, 23));

                        // If the player is on a motorcycle, also change the back wheels.
                        if (IsThisModelABike((uint)GetEntityModel(veh)))
                        {
                            SetVehicleMod(veh, 24, GetVehicleMod(veh, 24), GetVehicleModVariation(veh, 23));
                        }
                    }
                    // Toggle Tire Smoke
                    else if (item2 == tireSmokeEnabled)
                    {
                        // If it should be enabled:
                        if (_checked)
                        {
                            // Enable it.
                            ToggleVehicleMod(veh, 20, true);
                            // Get the selected color values.
                            var r = tireSmokeColors[tireSmokes[tireSmoke.Index]][0];
                            var g = tireSmokeColors[tireSmokes[tireSmoke.Index]][1];
                            var b = tireSmokeColors[tireSmokes[tireSmoke.Index]][2];
                            // Set the color.
                            SetVehicleTyreSmokeColor(veh, r, g, b);
                        }
                        // If it should be disabled:
                        else
                        {
                            // Set the smoke to white.
                            SetVehicleTyreSmokeColor(veh, 255, 255, 255);
                            // Disable it.
                            ToggleVehicleMod(veh, 20, false);
                            // Remove the mod.
                            RemoveVehicleMod(veh, 20);
                        }
                    }
                };
                #endregion

                #region List Changes
                // Handle list selections
                VehicleModMenu.OnListChange += (sender2, item2, index2) =>
                {
                    // Get the vehicle and set the mod kit.
                    veh = cf.GetVehicle();
                    SetVehicleModKit(veh, 0);

                    #region handle the dynamic (vehicle-specific) mods
                    // If the affected list is actually a "dynamically" generated list, continue. If it was one of the manual options, go to else.
                    if (sender2.CurrentSelection < sender2.MenuItems.Count - 8)
                    {
                        // Get all mods available on this vehicle.
                        vehicle = new Vehicle(veh);
                        mods = vehicle.Mods.GetAllMods();

                        var dict = new Dictionary<int, int>();
                        var x = 0;

                        foreach (var mod in mods)
                        {
                            dict.Add(x, (int)mod.ModType);
                            x++;
                        }

                        int modType = dict[sender2.CurrentSelection];
                        int selectedUpgrade = item2.Index - 1;
                        bool customWheels = GetVehicleModVariation(veh, 23);

                        SetVehicleMod(veh, modType, selectedUpgrade, customWheels);
                    }
                    #endregion
                    // If it was not one of the lists above, then it was one of the manual lists/options selected, 
                    // either: vehicle Wheel Type, tire smoke color, or window tint:
                    #region Handle the items available on all vehicles.
                    // Wheel types
                    else if (item2 == vehicleWheelType)
                    {
                        // Set the wheel type.
                        SetVehicleWheelType(veh, index2);
                        UpdateMods(selectedIndex: VehicleModMenu.CurrentSelection);
                    }
                    // Tire smoke
                    else if (item2 == tireSmoke)
                    {
                        // Get the selected color values.
                        var r = tireSmokeColors[tireSmokes[index2]][0];
                        var g = tireSmokeColors[tireSmokes[index2]][1];
                        var b = tireSmokeColors[tireSmokes[index2]][2];

                        // Set the color.
                        SetVehicleTyreSmokeColor(veh, r, g, b);
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

                        switch (index2)
                        {
                            case 1:
                                SetVehicleWindowTint(veh, 0); // None
                                break;
                            case 2:
                                SetVehicleWindowTint(veh, 5); // Limo
                                break;
                            case 3:
                                SetVehicleWindowTint(veh, 3); // Light Smoke
                                break;
                            case 4:
                                SetVehicleWindowTint(veh, 2); // Dark Smoke
                                break;
                            case 5:
                                SetVehicleWindowTint(veh, 1); // Pure Black
                                break;
                            case 6:
                                SetVehicleWindowTint(veh, 6); // Green
                                break;
                            case 0:
                            default:
                                SetVehicleWindowTint(veh, 4); // Stock
                                break;
                        }
                    }
                    #endregion
                };

                #endregion
            }
            // Refresh Index and update the scaleform to prevent weird broken menus.
            VehicleModMenu.RefreshIndex();
            VehicleModMenu.UpdateScaleform();

            // Set the selected index to the provided index (0 by default)
            // Used for example, when the wheelstype is changed, the menu is refreshed and we want to set the
            // selected item back to the "wheelsType" list so the user doesn't have to scroll down each time they
            // change the wheels type.
            VehicleModMenu.CurrentSelection = selectedIndex;
        }
        #endregion

        #region GetColorFromIndex function (underglow)
        /// <summary>
        /// Converts a list index to a color.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private System.Drawing.Color GetColorFromIndex(int index)
        {
            switch (index)
            {
                // { "Red", "Pink", "Purple", "Blacklight", "Dark Blue", "Light Blue", "White", "Lime", "Green", "Dark Green", "Gold", "Orange", "Yellow" }
                case 0:
                    return System.Drawing.Color.FromArgb(red: 237, green: 0, blue: 0); // red
                case 1:
                    return System.Drawing.Color.FromArgb(red: 237, green: 0, blue: 201); // pink
                case 2:
                    return System.Drawing.Color.FromArgb(red: 156, green: 0, blue: 237); // purple
                case 3:
                    return System.Drawing.Color.FromArgb(red: 66, green: 0, blue: 255); // blacklight
                case 4:
                    return System.Drawing.Color.FromArgb(red: 0, green: 66, blue: 255); // dark blue
                case 5:
                    return System.Drawing.Color.FromArgb(red: 0, green: 198, blue: 255); // light blue
                case 6:
                    return System.Drawing.Color.FromArgb(red: 255, green: 255, blue: 255); // white
                case 7:
                    return System.Drawing.Color.FromArgb(red: 0, green: 255, blue: 0); // lime green
                case 8:
                    return System.Drawing.Color.FromArgb(red: 33, green: 169, blue: 15); // green
                case 9:
                    return System.Drawing.Color.FromArgb(red: 8, green: 64, blue: 0); // dark green
                case 10:
                    return System.Drawing.Color.FromArgb(red: 222, green: 165, blue: 10); // golden shower
                case 11:
                    return System.Drawing.Color.FromArgb(red: 222, green: 85, blue: 10); // orange
                case 12:
                    return System.Drawing.Color.FromArgb(red: 236, green: 244, blue: 28); // yellow
                default:
                    return System.Drawing.Color.FromArgb(red: 255, green: 255, blue: 255); // white
            }
        }
        #endregion
    }
}
