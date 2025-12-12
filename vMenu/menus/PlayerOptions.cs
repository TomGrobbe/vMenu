using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

using MenuAPI;

using vMenuClient.data;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class PlayerOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        // Public variables (getters only), return the private variables.
        public bool PlayerGodMode { get; private set; } = UserDefaults.PlayerGodMode;
        public bool PlayerInvisible { get; private set; } = false;
        public bool PlayerStamina { get; private set; } = UserDefaults.UnlimitedStamina;
        public bool PlayerFastRun { get; private set; } = UserDefaults.FastRun;
        public bool PlayerFastSwim { get; private set; } = UserDefaults.FastSwim;
        public bool PlayerSuperJump { get; private set; } = UserDefaults.SuperJump;
        public bool PlayerNoRagdoll { get; private set; } = UserDefaults.NoRagdoll;
        public bool PlayerNeverWanted { get; private set; } = UserDefaults.NeverWanted;
        public bool PlayerIsIgnored { get; private set; } = UserDefaults.EveryoneIgnorePlayer;
        public bool PlayerStayInVehicle { get; private set; } = UserDefaults.PlayerStayInVehicle;
        public bool PlayerFrozen { get; private set; } = false;

        public int PlayerBlood { get; private set; } = 0;

        private readonly Menu CustomDrivingStyleMenu = new("Driving Style", "Custom Driving Style");

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create menu and menu items
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Player Options");

            // Create all checkboxes.
            var playerGodModeCheckbox = new MenuCheckboxItem("Godmode", "Makes you invincible.", PlayerGodMode);
            var invisibleCheckbox = new MenuCheckboxItem("Invisible", "Makes you invisible to yourself and others.", PlayerInvisible);
            var unlimitedStaminaCheckbox = new MenuCheckboxItem("Unlimited Stamina", "Allows you to run forever without slowing down or taking damage.", PlayerStamina);
            var fastRunCheckbox = new MenuCheckboxItem("Fast Run", "Get ~g~Snail~s~ powers and run very fast!", PlayerFastRun);
            SetRunSprintMultiplierForPlayer(Game.Player.Handle, PlayerFastRun && IsAllowed(Permission.POFastRun) ? 1.49f : 1f);
            var fastSwimCheckbox = new MenuCheckboxItem("Fast Swim", "Get ~g~Snail 2.0~s~ powers and swim super fast!", PlayerFastSwim);
            SetSwimMultiplierForPlayer(Game.Player.Handle, PlayerFastSwim && IsAllowed(Permission.POFastSwim) ? 1.49f : 1f);
            var superJumpCheckbox = new MenuCheckboxItem("Super Jump", "Get ~g~Snail 3.0~s~ powers and jump like a champ!", PlayerSuperJump);
            var noRagdollCheckbox = new MenuCheckboxItem("No Ragdoll", "Disables player ragdoll, makes you not fall off your bike anymore.", PlayerNoRagdoll);
            var neverWantedCheckbox = new MenuCheckboxItem("Never Wanted", "Disables all wanted levels.", PlayerNeverWanted);
            var everyoneIgnoresPlayerCheckbox = new MenuCheckboxItem("Everyone Ignore Player", "Everyone will leave you alone.", PlayerIsIgnored);
            var playerStayInVehicleCheckbox = new MenuCheckboxItem("Stay In Vehicle", "When this is enabled, NPCs will not be able to drag you out of your vehicle if they get angry at you.", PlayerStayInVehicle);
            var playerFrozenCheckbox = new MenuCheckboxItem("Freeze Player", "Freezes your current location.", PlayerFrozen);

            // Wanted level options
            var wantedLevelList = new List<string> { "No Wanted Level", "1", "2", "3", "4", "5" };
            var setWantedLevel = new MenuListItem("Set Wanted Level", wantedLevelList, GetPlayerWantedLevel(Game.Player.Handle), "Set your wanted level by selecting a value, and pressing enter.");
            var setArmorItem = new MenuListItem("Set Armor Type", new List<string> { "No Armor", GetLabelText("WT_BA_0"), GetLabelText("WT_BA_1"), GetLabelText("WT_BA_2"), GetLabelText("WT_BA_3"), GetLabelText("WT_BA_4"), }, 0, "Set the armor level/type for your player.");

            // Blood level options
            var clearBloodBtn = new MenuItem("Clear Blood", "Clear the blood off your player.");
            var bloodList = new List<string> { "BigHitByVehicle", "SCR_Torture", "SCR_TrevorTreeBang", "HOSPITAL_0", "HOSPITAL_1", "HOSPITAL_2", "HOSPITAL_3", "HOSPITAL_4", "HOSPITAL_5", "HOSPITAL_6", "HOSPITAL_7", "HOSPITAL_8", "HOSPITAL_9", "Explosion_Med", "Skin_Melee_0", "Explosion_Large", "Car_Crash_Light", "Car_Crash_Heavy", "Fall_Low", "Fall", "HitByVehicle", "BigRunOverByVehicle", "RunOverByVehicle", "TD_KNIFE_FRONT", "TD_KNIFE_FRONT_VA", "TD_KNIFE_FRONT_VB", "TD_KNIFE_REAR", "TD_KNIFE_REAR_VA", "TD_KNIFE_REAR_VB", "TD_KNIFE_STEALTH", "TD_MELEE_FRONT", "TD_MELEE_REAR", "TD_MELEE_STEALTH", "TD_MELEE_BATWAIST", "TD_melee_face_l", "MTD_melee_face_r", "MTD_melee_face_jaw", "TD_PISTOL_FRONT", "TD_PISTOL_FRONT_KILL", "TD_PISTOL_REAR", "TD_PISTOL_REAR_KILL", "TD_RIFLE_FRONT_KILL", "TD_RIFLE_NONLETHAL_FRONT", "TD_RIFLE_NONLETHAL_REAR", "TD_SHOTGUN_FRONT_KILL", "TD_SHOTGUN_REAR_KILL" };
            var setBloodLevel = new MenuListItem("Set Blood Level", bloodList, PlayerBlood, "Sets your players blood level.");

            var healPlayerBtn = new MenuItem("Heal Player", "Give the player max health.");
            var cleanPlayerBtn = new MenuItem("Clean Player Clothes", "Clean your player clothes.");
            var dryPlayerBtn = new MenuItem("Dry Player Clothes", "Make your player clothes dry.");
            var wetPlayerBtn = new MenuItem("Wet Player Clothes", "Make your player clothes wet.");
            var suicidePlayerBtn = new MenuItem("~r~Commit Suicide", "Kill yourself by taking the pill. Or by using a pistol if you have one.");

            var vehicleAutoPilot = new Menu("Auto Pilot", "Vehicle auto pilot options.");

            MenuController.AddSubmenu(menu, vehicleAutoPilot);

            var vehicleAutoPilotBtn = new MenuItem("Vehicle Auto Pilot Menu", "Manage vehicle auto pilot options.")
            {
                Label = "→→→"
            };

            var drivingStyles = new List<string>() { "Normal", "Rushed", "Avoid highways", "Drive in reverse", "Custom" };
            var drivingStyle = new MenuListItem("Driving Style", drivingStyles, 0, "Set the driving style that is used for the Drive to Waypoint and Drive Around Randomly functions.");

            // Scenarios (list can be found in the PedScenarios class)
            var playerScenarios = new MenuListItem("Player Scenarios", PedScenarios.Scenarios, 0, "Select a scenario and hit enter to start it. Selecting another scenario will override the current scenario. If you're already playing the selected scenario, selecting it again will stop the scenario.");
            var stopScenario = new MenuItem("Force Stop Scenario", "This will force a playing scenario to stop immediately, without waiting for it to finish it's 'stopping' animation.");
            #endregion

            #region add items to menu based on permissions
            // Add all checkboxes to the menu. (keeping permissions in mind)
            if (IsAllowed(Permission.POGod))
            {
                menu.AddMenuItem(playerGodModeCheckbox);
            }
            if (IsAllowed(Permission.POInvisible))
            {
                menu.AddMenuItem(invisibleCheckbox);
            }
            if (IsAllowed(Permission.POUnlimitedStamina))
            {
                menu.AddMenuItem(unlimitedStaminaCheckbox);
            }
            if (IsAllowed(Permission.POFastRun))
            {
                menu.AddMenuItem(fastRunCheckbox);
            }
            if (IsAllowed(Permission.POFastSwim))
            {
                menu.AddMenuItem(fastSwimCheckbox);
            }
            if (IsAllowed(Permission.POSuperjump))
            {
                menu.AddMenuItem(superJumpCheckbox);
            }
            if (IsAllowed(Permission.PONoRagdoll))
            {
                menu.AddMenuItem(noRagdollCheckbox);
            }
            if (IsAllowed(Permission.PONeverWanted))
            {
                menu.AddMenuItem(neverWantedCheckbox);
            }
            if (IsAllowed(Permission.POSetWanted))
            {
                menu.AddMenuItem(setWantedLevel);
            }
            if (IsAllowed(Permission.POClearBlood))
            {
                menu.AddMenuItem(clearBloodBtn);
            }
            if (IsAllowed(Permission.POSetBlood))
            {
                menu.AddMenuItem(setBloodLevel);
            }
            if (IsAllowed(Permission.POIgnored))
            {
                menu.AddMenuItem(everyoneIgnoresPlayerCheckbox);
            }
            if (IsAllowed(Permission.POStayInVehicle))
            {
                menu.AddMenuItem(playerStayInVehicleCheckbox);
            }
            if (IsAllowed(Permission.POMaxHealth))
            {
                menu.AddMenuItem(healPlayerBtn);
            }
            if (IsAllowed(Permission.POMaxArmor))
            {
                menu.AddMenuItem(setArmorItem);
            }
            if (IsAllowed(Permission.POCleanPlayer))
            {
                menu.AddMenuItem(cleanPlayerBtn);
            }
            if (IsAllowed(Permission.PODryPlayer))
            {
                menu.AddMenuItem(dryPlayerBtn);
            }
            if (IsAllowed(Permission.POWetPlayer))
            {
                menu.AddMenuItem(wetPlayerBtn);
            }

            menu.AddMenuItem(suicidePlayerBtn);

            if (IsAllowed(Permission.POVehicleAutoPilotMenu))
            {
                menu.AddMenuItem(vehicleAutoPilotBtn);
                MenuController.BindMenuItem(menu, vehicleAutoPilot, vehicleAutoPilotBtn);

                vehicleAutoPilot.AddMenuItem(drivingStyle);

                var startDrivingWaypoint = new MenuItem("Drive To Waypoint", "Make your player ped drive your vehicle to your waypoint.");
                var startDrivingRandomly = new MenuItem("Drive Around Randomly", "Make your player ped drive your vehicle randomly around the map.");
                var stopDriving = new MenuItem("Stop Driving", "The player ped will find a suitable place to stop the vehicle. The task will be stopped once the vehicle has reached the suitable stop location.");
                var forceStopDriving = new MenuItem("Force Stop Driving", "This will stop the driving task immediately without finding a suitable place to stop.");
                var customDrivingStyle = new MenuItem("Custom Driving Style", "Select a custom driving style. Make sure to also enable it by selecting the 'Custom' driving style in the driving styles list.") { Label = "→→→" };
                MenuController.AddSubmenu(vehicleAutoPilot, CustomDrivingStyleMenu);
                vehicleAutoPilot.AddMenuItem(customDrivingStyle);
                MenuController.BindMenuItem(vehicleAutoPilot, CustomDrivingStyleMenu, customDrivingStyle);
                var knownNames = new Dictionary<int, string>()
                {
                    { 0, "Stop for vehicles" },  // Using this flag will allow the driver to stop for other vehicles when appropriate, instead of hitting them.
                    { 1, "Stop for pedestrians" },  // Using this flag will allow the driver to stop for pedestrians when appropriate, instead of hitting them.
                    { 2, "Swerve around all vehicles" },  // Using this flag will allow the driver to stop for other vehicles when appropriate, instead of hitting them.
                    { 3, "Steer around stationary vehicles" },  // Using this flag will allow the driver to steer around a vehicle in order to avoid them when driving.
                    { 4, "Steer around pedestrians" },  // Using this flag will allow the driver to steer around a pedestrian in order to avoid them when driving.
                    { 5, "Steer around objects" },  // Using this flag will allow the driver to steer around objects in order to avoid them when driving.
                    { 6, "Don't steer around Player" },  // Using this flag will prevent the driver trying steer around an pedestrian if that pedestrian is the player
                    { 7, "Stop at lights" },  // Using this flag will cause the driver to stop at traffic lights when appropriate.
                    { 8, "Go off road when avoiding" },  // Using this flag will allow the driver to deliberately go off-road when trying to avoid something.
                    { 9, "Drive into oncoming traffic" },  // Using this flag will allow the driver to deliberately drive into oncoming traffic, when certain conditions are met. Such as avoiding or swerving, or overtaking.
                    { 10, "Drive in reverse" },  // Using this flag will tell the driver that they should drive in reverse.
                    { 11, "Use wander fallback instead of straight line" },  // Using this flag will make the driver use wander pathfinding instead of straight line pathfinding when no route is found.
                    { 12, "Avoid restricted areas" },  // Using this flag will tell the driver that they should avoid areas considered restricted. This includes areas like the Army Base, Prison, Airport, or any other areas that might give the Player a Wanted Level for driving through.
                    { 13, "Prevent background pathfinding" },  // The driver will not perform background pathfinding.
                    { 14, "Adjust speed for current road" },  // Using this flag will tell the driver that they should match their speed to the road that they are driving on. Highways use a faster speed than main roads.
                    { 15, "Prevent join in road direction when moving" },  // The driver avoids joining roads in the direction of traffic when moving.
                    { 16, "Don't avoid target" },  // The driver does not avoid the target destination.
                    { 17, "Target position overrides entity" },  // The target position takes precedence over the target entity.
                    { 18, "Use shortcut links (Use shortest path)" },  // Using this flag will tell the driver that they should try to use Road Nodes flagged as shortcuts. This may divert them down side streets, or through oncoming traffic, if this behavior is not restricted.
                    { 19, "Change lanes around obstructions" },  // Using this flag allows the Driver to change lanes to avoid an obstruction.
                    { 20, "Avoid target coords" },  // The driver pathfind away from instead of towards the target coordinates.
                    { 21, "Use switched-off nodes" },  // Using this flag allows the Driver to use Road Nodes that have been disabled by Zones.
                    { 22, "Prefer navmesh route" },  // Using this flag allows the Driver to use the Navmesh to find a path to their destination instead of the Road Nodes system. This is useful for off-road driving.
                    { 23, "Plane taxi mode" },  // The driver operates as if taxiing an aircraft.
                    { 24, "Force straight line" },  // Using this flag forces the Driver to drive in a straight line to their next destination, ignoring the Navmesh or the Road Nodes system.
                    { 25, "Use string pulling at junctions" },  // The driver uses string pulling for smoother turns at junctions.
                    { 26, "Avoid Adverse Conditions" },  // The driver avoids "adverse conditions" (shocking events, etc.) when cruising.
                    { 27, "Avoid turns" },  // The driver avoids turns when cruising.
                    { 28, "Extend route with wander results" },  // The driver extends the route using wandering paths.
                    { 29, "Avoid highways (if possible)" },  // Using this flag prevents the Driver from using a highway when possible.
                    { 30, "Force join in road direction" },  // The driver joins roads in direction of the flow of traffic only.
                    { 31, "Don't terminate task when achieved" },  // The driver will not terminate the driving task upon reaching the destination.
                };
                for (var i = 0; i < 32; i++)
                {
                    var name = "~r~Unknown Flag";
                    if (knownNames.ContainsKey(i))
                    {
                        name = knownNames[i];
                    }
                    var checkbox = new MenuCheckboxItem(name, "Toggle this driving style flag.", false);
                    CustomDrivingStyleMenu.AddMenuItem(checkbox);
                }
                CustomDrivingStyleMenu.OnCheckboxChange += (sender, item, index, _checked) =>
                {
                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                    CustomDrivingStyleMenu.MenuSubtitle = $"custom style: {style}";
                    if (drivingStyle.ListIndex == 4)
                    {
                        Notify.Custom("Driving style updated.");
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                    }
                    else
                    {
                        Notify.Custom("Driving style NOT updated because you haven't enabled the Custom driving style in the previous menu.");
                    }
                };

                vehicleAutoPilot.AddMenuItem(startDrivingWaypoint);
                vehicleAutoPilot.AddMenuItem(startDrivingRandomly);
                vehicleAutoPilot.AddMenuItem(stopDriving);
                vehicleAutoPilot.AddMenuItem(forceStopDriving);

                vehicleAutoPilot.RefreshIndex();

                vehicleAutoPilot.OnItemSelect += async (sender, item, index) =>
                {
                    if (Game.PlayerPed.IsInVehicle() && item != stopDriving && item != forceStopDriving)
                    {
                        if (Game.PlayerPed.CurrentVehicle != null && Game.PlayerPed.CurrentVehicle.Exists() && !Game.PlayerPed.CurrentVehicle.IsDead && Game.PlayerPed.CurrentVehicle.IsDriveable)
                        {
                            if (Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed)
                            {
                                if (item == startDrivingWaypoint)
                                {
                                    if (IsWaypointActive())
                                    {
                                        var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                        DriveToWp(style);
                                        Notify.Info("Your player ped is now driving the vehicle for you. You can cancel any time by pressing the Stop Driving button. The vehicle will stop when it has reached the destination.");
                                    }
                                    else
                                    {
                                        Notify.Error("You need a waypoint before you can drive to it!");
                                    }

                                }
                                else if (item == startDrivingRandomly)
                                {
                                    var style = GetStyleFromIndex(drivingStyle.ListIndex);
                                    DriveWander(style);
                                    Notify.Info("Your player ped is now driving the vehicle for you. You can cancel any time by pressing the Stop Driving button.");
                                }
                            }
                            else
                            {
                                Notify.Error("You must be the driver of this vehicle!");
                            }
                        }
                        else
                        {
                            Notify.Error("Your vehicle is broken or it does not exist!");
                        }
                    }
                    else if (item != stopDriving && item != forceStopDriving)
                    {
                        Notify.Error("You need to be in a vehicle first!");
                    }
                    if (item == stopDriving)
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            var veh = GetVehicle();
                            if (veh != null && veh.Exists() && !veh.IsDead)
                            {
                                var outPos = new Vector3();
                                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                                {
                                    Notify.Info("The player ped will find a suitable place to park the car and will then stop driving. Please wait.");
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    TaskVehiclePark(Game.PlayerPed.Handle, veh.Handle, outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, 3, 60f, true);
                                    while (Game.PlayerPed.Position.DistanceToSquared2D(outPos) > 3f)
                                    {
                                        await BaseScript.Delay(0);
                                    }
                                    SetVehicleHalt(veh.Handle, 3f, 0, false);
                                    ClearPedTasks(Game.PlayerPed.Handle);
                                    Notify.Info("The player ped has stopped driving.");
                                }
                            }
                        }
                        else
                        {
                            ClearPedTasks(Game.PlayerPed.Handle);
                            Notify.Alert("Your ped is not in any vehicle.");
                        }
                    }
                    else if (item == forceStopDriving)
                    {
                        ClearPedTasks(Game.PlayerPed.Handle);
                        Notify.Info("Driving task cancelled.");
                    }
                };

                vehicleAutoPilot.OnListItemSelect += (sender, item, listIndex, itemIndex) =>
                {
                    if (item == drivingStyle)
                    {
                        var style = GetStyleFromIndex(listIndex);
                        SetDriveTaskDrivingStyle(Game.PlayerPed.Handle, style);
                        Notify.Info($"Driving task style is now set to: ~r~{drivingStyles[listIndex]}~s~.");
                    }
                };
            }

            if (IsAllowed(Permission.POFreeze))
            {
                menu.AddMenuItem(playerFrozenCheckbox);
            }
            if (IsAllowed(Permission.POScenarios))
            {
                menu.AddMenuItem(playerScenarios);
                menu.AddMenuItem(stopScenario);
            }
            #endregion

            #region handle all events
            // Checkbox changes.
            menu.OnCheckboxChange += (sender, item, itemIndex, _checked) =>
            {
                // God Mode toggled.
                if (item == playerGodModeCheckbox)
                {
                    PlayerGodMode = _checked;
                }
                // Invisibility toggled.
                else if (item == invisibleCheckbox)
                {
                    PlayerInvisible = _checked;
                    SetEntityVisible(Game.PlayerPed.Handle, !PlayerInvisible, false);
                }
                // Unlimited Stamina toggled.
                else if (item == unlimitedStaminaCheckbox)
                {
                    PlayerStamina = _checked;
                    StatSetInt((uint)GetHashKey("MP0_STAMINA"), _checked ? 100 : 0, true);
                }
                // Fast run toggled.
                else if (item == fastRunCheckbox)
                {
                    PlayerFastRun = _checked;
                    SetRunSprintMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Fast swim toggled.
                else if (item == fastSwimCheckbox)
                {
                    PlayerFastSwim = _checked;
                    SetSwimMultiplierForPlayer(Game.Player.Handle, _checked ? 1.49f : 1f);
                }
                // Super jump toggled.
                else if (item == superJumpCheckbox)
                {
                    PlayerSuperJump = _checked;
                }
                // No ragdoll toggled.
                else if (item == noRagdollCheckbox)
                {
                    PlayerNoRagdoll = _checked;
                }
                // Never wanted toggled.
                else if (item == neverWantedCheckbox)
                {
                    PlayerNeverWanted = _checked;
                    if (!_checked)
                    {
                        SetMaxWantedLevel(5);
                    }
                    else
                    {
                        SetMaxWantedLevel(0);
                    }
                }
                // Everyone ignores player toggled.
                else if (item == everyoneIgnoresPlayerCheckbox)
                {
                    PlayerIsIgnored = _checked;

                    // Manage player is ignored by everyone.
                    SetEveryoneIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPoliceIgnorePlayer(Game.Player.Handle, PlayerIsIgnored);
                    SetPlayerCanBeHassledByGangs(Game.Player.Handle, !PlayerIsIgnored);
                }
                else if (item == playerStayInVehicleCheckbox)
                {
                    PlayerStayInVehicle = _checked;
                }
                // Freeze player toggled.
                else if (item == playerFrozenCheckbox)
                {
                    PlayerFrozen = _checked;

                    if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                    else if (!MainMenu.NoClipEnabled)
                    {
                        FreezeEntityPosition(Game.PlayerPed.Handle, PlayerFrozen);
                    }
                }
            };

            // List selections
            menu.OnListItemSelect += (sender, listItem, listIndex, itemIndex) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(Game.Player.Handle, listIndex, false);
                    SetPlayerWantedLevelNow(Game.Player.Handle, false);
                }
                // Set blood level
                else if (listItem == setBloodLevel)
                {
                    ApplyPedDamagePack(Game.PlayerPed.Handle, bloodList[listIndex], 100, 100);
                }
                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[listIndex]]);
                }
                else if (listItem == setArmorItem)
                {
                    Game.PlayerPed.Armor = listItem.ListIndex * 20;
                }
            };

            // button presses
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Force Stop Scenario button
                if (item == stopScenario)
                {
                    // Play a new scenario named "forcestop" (this scenario doesn't exist, but the "Play" function checks
                    // for the string "forcestop", if that's provided as th scenario name then it will forcefully clear the player task.
                    PlayScenario("forcestop");
                }
                else if (item == clearBloodBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Game.PlayerPed.ResetVisibleDamage();
                    // not ideal for removing visible bruises & scars, may have some sync issues but could not find an alternative method, anyone who does feel free to update

                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 0, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 1, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 2, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 3, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 4, "ALL");
                    ClearPedDamageDecalByZone(Game.PlayerPed.Handle, 5, "ALL");
                }
                else if (item == healPlayerBtn)
                {
                    Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                    Notify.Success("Player healed.");
                }
                else if (item == cleanPlayerBtn)
                {
                    Game.PlayerPed.ClearBloodDamage();
                    Notify.Success("Player clothes have been cleaned.");
                }
                else if (item == dryPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 0f;
                    Notify.Success("Player is now dry.");
                }
                else if (item == wetPlayerBtn)
                {
                    Game.PlayerPed.WetnessHeight = 2f;
                    Notify.Success("Player is now wet.");
                }
                else if (item == suicidePlayerBtn)
                {
                    CommitSuicide();
                }
            };
            #endregion

        }

        private int GetCustomDrivingStyle()
        {
            var items = CustomDrivingStyleMenu.GetMenuItems();
            var flags = new int[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is MenuCheckboxItem checkbox)
                {
                    flags[i] = checkbox.Checked ? 1 : 0;
                }
            }
            var binaryString = "";
            var reverseFlags = flags.Reverse();
            foreach (var i in reverseFlags)
            {
                binaryString += i;
            }
            var binaryNumber = Convert.ToUInt32(binaryString, 2);
            return (int)binaryNumber;
        }

        private int GetStyleFromIndex(int index)
        {
            var style = index switch
            {
                0 => 443,// normal
                1 => 575,// rushed
                2 => 536871355,// Avoid highways
                3 => 1467,// Go in reverse
                4 => GetCustomDrivingStyle(),// custom driving style;
                _ => 0,// no style (impossible, but oh well)
            };
            return style;
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Player Options Menu</returns>
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
