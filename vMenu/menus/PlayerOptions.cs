﻿using System;
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
        private static Notification Notify = MainMenu.Notify;
        private static Subtitles Subtitle = MainMenu.Subtitle;
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
            UIMenuCheckboxItem fastRunCheckbox = new UIMenuCheckboxItem("Fast Run", false, "Get ~g~Snail~s~ powers and run very fast!");
            UIMenuCheckboxItem fastSwimCheckbox = new UIMenuCheckboxItem("Fast Swim", false, "Get ~g~Snail 2.0~s~ powers and swim super fast!");
            UIMenuCheckboxItem superJumpCheckbox = new UIMenuCheckboxItem("Super Jump", PlayerSuperJump, "Get ~g~Snail 3.0~s~ powers and jump like a champ!");
            UIMenuCheckboxItem noRagdollCheckbox = new UIMenuCheckboxItem("No Ragdoll", PlayerNoRagdoll, "Disables player ragdoll, makes you not fall off your bike anymore.");
            UIMenuCheckboxItem neverWantedCheckbox = new UIMenuCheckboxItem("Never Wanted", PlayerNeverWanted, "Disables all wanted levels.");
            UIMenuCheckboxItem everyoneIgnoresPlayerCheckbox = new UIMenuCheckboxItem("Everyone Ignore Player", PlayerIsIgnored, "Everyone will leave you alone.");
            UIMenuCheckboxItem playerFrozenCheckbox = new UIMenuCheckboxItem("Freeze Player", PlayerFrozen, "Freezes your current location, why would you do this...?");

            // Wanted level options
            List<dynamic> wantedLevelList = new List<dynamic> { "No Wanted Level", 1, 2, 3, 4, 5 };
            UIMenuListItem setWantedLevel = new UIMenuListItem("Set Wanted Level", wantedLevelList, GetPlayerWantedLevel(PlayerId()), "Set your wanted level by selecting a value, and pressing enter.");

            // Player options
            List<dynamic> playerOptionsList = new List<dynamic> { "Max Health", "Max Armor", "Clean Player Clothes", "Player Dry", "Player Wet", "~r~Commit Suicide", "Drive To Waypoint", "Drive Around Randomly" };
            UIMenuListItem playerFunctions = new UIMenuListItem("Player Functions", playerOptionsList, 0, "Select an option and press enter to run/stop it.");

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

            // Always allowed.
            menu.AddItem(unlimitedStaminaCheckbox);

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
            if (cf.IsAllowed(Permission.POFunctions))
            {
                menu.AddItem(playerFunctions);
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
                // Player options (healing, cleaning, armor, dry/wet, etc)
                else if (listItem == playerFunctions)
                {
                    switch (index)
                    {
                        // Max Health
                        case 0:
                            SetEntityHealth(PlayerPedId(), GetEntityMaxHealth(PlayerPedId()));
                            Subtitle.Info("Max ~g~health ~s~applied.", prefix: "Info:");
                            break;
                        // Max Armor
                        case 1:
                            SetPedArmour(PlayerPedId(), GetPlayerMaxArmour(PlayerId()));
                            Subtitle.Info("Max ~b~armor ~s~applied.", prefix: "Info:");
                            break;
                        // Clean Player Clothes
                        case 2:
                            ClearPedBloodDamage(PlayerPedId());
                            Subtitle.Info("Cleaned player clothes.", prefix: "Info:");
                            break;
                        // Make Player Dry
                        case 3:
                            ClearPedWetness(PlayerPedId());
                            Subtitle.Info("Player clothes are now ~c~dry~s~.", prefix: "Info:");
                            break;
                        // Make Player Wet
                        case 4:
                            SetPedWetnessHeight(PlayerPedId(), 2f);
                            SetPedWetnessEnabledThisFrame(PlayerPedId());
                            Subtitle.Info("Player clothes are now ~b~wet~s~.", prefix: "Info:");
                            break;
                        // Kill Player
                        case 5:
                            SetEntityHealth(PlayerPedId(), 0);
                            Subtitle.Info("You ~r~killed ~s~yourself.", prefix: "Info:");
                            break;
                        // Drive To Waypoint
                        case 6:
                            if (!Game.IsWaypointActive)
                            {
                                Subtitle.Error("You need to set a ~p~waypoint ~s~first!", prefix: "Error:");
                            }
                            else if (IsPedInAnyVehicle(PlayerPedId(), false))
                            {
                                try
                                {
                                    cf.DriveToWp();
                                }
                                catch (NotImplementedException e)
                                {
                                    cf.Log("\n\r[vMenu] Exception: " + e.Message + "\r\n");
                                    Notify.Error(CommonErrors.UnknownError, placeholderValue: "This function is not implemented yet.");
                                }
                            }
                            else
                            {
                                Subtitle.Error("You need a ~r~vehicle ~s~first!", prefix: "Error:");
                            }
                            break;
                        // Drive Around Randomly (wander)
                        case 7:
                            if (IsPedInAnyVehicle(PlayerPedId(), false))
                            {
                                try
                                {
                                    cf.DriveWander();
                                }
                                catch (NotImplementedException e)
                                {
                                    cf.Log("\n\r[vMenu] Exception: " + e.Message + "\r\n");
                                    Notify.Error(CommonErrors.UnknownError, placeholderValue: "This function is not implemented yet.");
                                }

                            }
                            else
                            {
                                Subtitle.Error("You need a ~r~vehicle ~s~first!", prefix: "Error:");
                            }
                            break;
                        default:
                            break;
                    }
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
            };
            #endregion

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
