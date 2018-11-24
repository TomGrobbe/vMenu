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
    public class PlayerOptions
    {
        // Menu variable, will be defined in CreateMenu()
        private UIMenu menu;
        private static CommonFunctions cf = MainMenu.Cf;

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
        public bool PlayerFrozen { get; private set; } = false;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            #region create menu and menu items
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Player Options", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            // Create all checkboxes.
            UIMenuCheckboxItem playerGodModeCheckbox = new UIMenuCheckboxItem("Godmode", PlayerGodMode, "Makes you invincible.");
            UIMenuCheckboxItem invisibleCheckbox = new UIMenuCheckboxItem("Invisible", PlayerInvisible, "Makes you invisible to yourself and others.");
            UIMenuCheckboxItem unlimitedStaminaCheckbox = new UIMenuCheckboxItem("Unlimited Stamina", PlayerStamina, "Allows you to run forever without slowing down or taking damage.");
            UIMenuCheckboxItem fastRunCheckbox = new UIMenuCheckboxItem("Fast Run", PlayerFastRun, "Get ~g~Snail~s~ powers and run very fast!");
            SetRunSprintMultiplierForPlayer(PlayerId(), (PlayerFastRun ? 1.49f : 1f));
            UIMenuCheckboxItem fastSwimCheckbox = new UIMenuCheckboxItem("Fast Swim", PlayerFastSwim, "Get ~g~Snail 2.0~s~ powers and swim super fast!");
            SetSwimMultiplierForPlayer(PlayerId(), (PlayerFastSwim ? 1.49f : 1f));
            UIMenuCheckboxItem superJumpCheckbox = new UIMenuCheckboxItem("Super Jump", PlayerSuperJump, "Get ~g~Snail 3.0~s~ powers and jump like a champ!");
            UIMenuCheckboxItem noRagdollCheckbox = new UIMenuCheckboxItem("No Ragdoll", PlayerNoRagdoll, "Disables player ragdoll, makes you not fall off your bike anymore.");
            UIMenuCheckboxItem neverWantedCheckbox = new UIMenuCheckboxItem("Never Wanted", PlayerNeverWanted, "Disables all wanted levels.");
            UIMenuCheckboxItem everyoneIgnoresPlayerCheckbox = new UIMenuCheckboxItem("Everyone Ignore Player", PlayerIsIgnored, "Everyone will leave you alone.");
            UIMenuCheckboxItem playerFrozenCheckbox = new UIMenuCheckboxItem("Freeze Player", PlayerFrozen, "Freezes your current location, why would you do this...?");

            // Wanted level options
            List<dynamic> wantedLevelList = new List<dynamic> { "No Wanted Level", 1, 2, 3, 4, 5 };
            UIMenuListItem setWantedLevel = new UIMenuListItem("Set Wanted Level", wantedLevelList, GetPlayerWantedLevel(PlayerId()), "Set your wanted level by selecting a value, and pressing enter.");

            UIMenuItem healPlayerBtn = new UIMenuItem("Heal Player", "Give the player max health.");
            UIMenuItem maxArmorBtn = new UIMenuItem("Max Armor", "Give the player max armor.");
            UIMenuItem cleanPlayerBtn = new UIMenuItem("Clean Player Clothes", "Clean your player clothes.");
            UIMenuItem dryPlayerBtn = new UIMenuItem("Dry Player Clothes", "Make your player clothes dry.");
            UIMenuItem wetPlayerBtn = new UIMenuItem("Wet Player Clothes", "Make your player clothes wet.");
            UIMenuItem suicidePlayerBtn = new UIMenuItem("~r~Commit Suicide", "Kill yourself by taking the pill. Or by using a pistol if you have one.");

            UIMenu vehicleAutoPilot = new UIMenu("Auto Pilot", "Vehicle auto pilot options.", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            MainMenu.Mp.Add(vehicleAutoPilot);

            UIMenuItem vehicleAutoPilotBtn = new UIMenuItem("Vehicle Auto Pilot Menu", "Manage vehicle auto pilot options.");
            vehicleAutoPilotBtn.SetRightLabel("→→→");

            List<dynamic> drivingStyles = new List<dynamic>() { "Normal", "Rushed", "Avoid highways", "Drive in reverse" };
            UIMenuListItem drivingStyle = new UIMenuListItem("Driving Style", drivingStyles, 0, "Set the driving style that is used for the Drive to Waypoint and Drive Around Randomly functions.");

            // Scenarios (list can be found in the PedScenarios class)
            UIMenuListItem playerScenarios = new UIMenuListItem("Player Scenarios", PedScenarios.Scenarios, 0, "Select a scenario and hit enter to start it. Selecting another scenario will override the current scenario. If you're already playing the selected scenario, selecting it again will stop the scenario.");
            UIMenuItem stopScenario = new UIMenuItem("Force Stop Scenario", "This will force a playing scenario to stop immediately, without waiting for it to finish it's 'stopping' animation.");
            #endregion

            #region add items to menu based on permissions
            // Add all checkboxes to the menu. (keeping permissions in mind)
            if (cf.IsAllowed(Permission.POGod))
            {
                menu.AddItem(playerGodModeCheckbox);
            }
            if (cf.IsAllowed(Permission.POInvisible))
            {
                menu.AddItem(invisibleCheckbox);
            }
            if (cf.IsAllowed(Permission.POUnlimitedStamina))
            {
                menu.AddItem(unlimitedStaminaCheckbox);
            }
            if (cf.IsAllowed(Permission.POFastRun))
            {
                menu.AddItem(fastRunCheckbox);
            }
            if (cf.IsAllowed(Permission.POFastSwim))
            {
                menu.AddItem(fastSwimCheckbox);
            }
            if (cf.IsAllowed(Permission.POSuperjump))
            {
                menu.AddItem(superJumpCheckbox);
            }
            if (cf.IsAllowed(Permission.PONoRagdoll))
            {
                menu.AddItem(noRagdollCheckbox);
            }
            if (cf.IsAllowed(Permission.PONeverWanted))
            {
                menu.AddItem(neverWantedCheckbox);
            }
            if (cf.IsAllowed(Permission.POSetWanted))
            {
                menu.AddItem(setWantedLevel);
            }
            if (cf.IsAllowed(Permission.POIgnored))
            {
                menu.AddItem(everyoneIgnoresPlayerCheckbox);
            }

            if (cf.IsAllowed(Permission.POMaxHealth))
            {
                menu.AddItem(healPlayerBtn);
            }
            if (cf.IsAllowed(Permission.POMaxArmor))
            {
                menu.AddItem(maxArmorBtn);
            }
            if (cf.IsAllowed(Permission.POCleanPlayer))
            {
                menu.AddItem(cleanPlayerBtn);
            }
            if (cf.IsAllowed(Permission.PODryPlayer))
            {
                menu.AddItem(dryPlayerBtn);
            }
            if (cf.IsAllowed(Permission.POWetPlayer))
            {
                menu.AddItem(wetPlayerBtn);
            }

            menu.AddItem(suicidePlayerBtn);

            if (cf.IsAllowed(Permission.POVehicleAutoPilotMenu))
            {
                menu.AddItem(vehicleAutoPilotBtn);
                menu.BindMenuToItem(vehicleAutoPilot, vehicleAutoPilotBtn);

                vehicleAutoPilot.AddItem(drivingStyle);

                UIMenuItem startDrivingWaypoint = new UIMenuItem("Drive To Waypoint", "Make your player ped drive your vehicle to your waypoint.");
                UIMenuItem startDrivingRandomly = new UIMenuItem("Drive Around Randomly", "Make your player ped drive your vehicle randomly around the map.");
                UIMenuItem stopDriving = new UIMenuItem("Stop Driving", "The player ped will find a suitable place to stop the vehicle. The task will be stopped once the vehicle has reached the suitable stop location.");
                UIMenuItem forceStopDriving = new UIMenuItem("Force Stop Driving", "This will stop the driving task immediately without finding a suitable place to stop.");

                vehicleAutoPilot.AddItem(startDrivingWaypoint);
                vehicleAutoPilot.AddItem(startDrivingRandomly);
                vehicleAutoPilot.AddItem(stopDriving);
                vehicleAutoPilot.AddItem(forceStopDriving);

                vehicleAutoPilot.RefreshIndex();
                vehicleAutoPilot.UpdateScaleform();

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
                                        int style = GetStyleFromIndex(drivingStyle.Index);
                                        cf.DriveToWp(style);
                                        Notify.Info("Your player ped is now driving the vehicle for you. You can cancel any time by pressing the Stop Driving button. The vehicle will stop when it has reached the destination.");
                                    }
                                    else
                                    {
                                        Notify.Error("You need a waypoint before you can drive to it!");
                                    }
                                    
                                }
                                else if (item == startDrivingRandomly)
                                {
                                    int style = GetStyleFromIndex(drivingStyle.Index);
                                    cf.DriveWander(style);
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
                            Vector3 outPos = new Vector3();
                            if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                            {
                                Notify.Info("The player ped will find a suitable place to park the car and will then stop driving. Please wait.");
                                ClearPedTasks(PlayerPedId());
                                TaskVehiclePark(PlayerPedId(), cf.GetVehicle(), outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, 3, 60f, true);
                                while (Game.PlayerPed.Position.DistanceToSquared2D(outPos) > 3f)
                                {
                                    await BaseScript.Delay(0);
                                }
                                SetVehicleHalt(cf.GetVehicle(), 3f, 0, false);
                                ClearPedTasks(PlayerPedId());
                                Notify.Info("The player ped has stopped driving.");
                            }
                        }
                        else
                        {
                            ClearPedTasks(PlayerPedId());
                            Notify.Alert("Your ped is not in any vehicle.");
                        }
                    }
                    else if (item == forceStopDriving)
                    {
                        ClearPedTasks(PlayerPedId());
                        Notify.Info("Driving task cancelled.");
                    }
                };

                vehicleAutoPilot.OnListSelect += (sender, item, index) =>
                {
                    if (item == drivingStyle)
                    {
                        int style = GetStyleFromIndex(index);
                        SetDriveTaskDrivingStyle(PlayerPedId(), style);
                        Notify.Info($"Driving task style is now set to: ~r~{drivingStyles[index]}~s~.");
                    }
                };
            }

            if (cf.IsAllowed(Permission.POFreeze))
            {
                menu.AddItem(playerFrozenCheckbox);
            }
            if (cf.IsAllowed(Permission.POScenarios))
            {
                menu.AddItem(playerScenarios);
                menu.AddItem(stopScenario);
            }
            #endregion

            #region handle all events
            // Checkbox changes.
            menu.OnCheckboxChange += (sender, item, _checked) =>
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
                            }
                            // Unlimited Stamina toggled.
                            else if (item == unlimitedStaminaCheckbox)
                            {
                                PlayerStamina = _checked;
                            }
                            // Fast run toggled.
                            else if (item == fastRunCheckbox)
                            {
                                PlayerFastRun = _checked;
                                SetRunSprintMultiplierForPlayer(PlayerId(), (_checked ? 1.49f : 1f));
                            }
                            // Fast swim toggled.
                            else if (item == fastSwimCheckbox)
                            {
                                PlayerFastSwim = _checked;
                                SetSwimMultiplierForPlayer(PlayerId(), (_checked ? 1.49f : 1f));
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
                            }
                            // Everyone ignores player toggled.
                            else if (item == everyoneIgnoresPlayerCheckbox)
                            {
                                PlayerIsIgnored = _checked;
                            }
                            // Freeze player toggled.
                            else if (item == playerFrozenCheckbox)
                            {
                                PlayerFrozen = _checked;
                                if (!_checked)
                                    FreezeEntityPosition(PlayerPedId(), false);
                            }
                        };

            // List selections
            menu.OnListSelect += (sender, listItem, index) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(PlayerId(), index, false);
                    SetPlayerWantedLevelNow(PlayerId(), false);
                }
                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    cf.PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[index]]);
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
                    cf.PlayScenario("forcestop");
                }
                else if (item == healPlayerBtn)
                {
                    Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;
                    Notify.Success("Player healed.");
                }
                else if (item == maxArmorBtn)
                {
                    Game.PlayerPed.Armor = 200;
                    Notify.Success("Max armor applied.");
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
                    cf.CommitSuicide();
                }
            };
            #endregion

        }

        private int GetStyleFromIndex(int index)
        {
            int style = 0;
            switch (index)
            {
                case 0:
                    style = 443; // normal
                    break;
                case 1:
                    style = 575; // rushed
                    break;
                case 2:
                    style = 536871355; // Avoid highways
                    break;
                case 3:
                    style = 1467; // Go in reverse
                    break;
                default:
                    style = 0; // no style (impossible, but oh well)
                    break;
            }
            return style;
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Player Options Menu</returns>
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
