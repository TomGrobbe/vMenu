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
        private static Notification Notify = MainMenu.Notify;
        private static Subtitles Subtitle = MainMenu.Subtitle;

        // Public variables (getters only), return the private variables.
        public bool PlayerGodMode { get; private set; } = false;
        public bool PlayerInvisible { get; private set; } = false;
        public bool PlayerStamina { get; private set; } = true;
        public bool PlayerSuperJump { get; private set; } = false;
        public bool PlayerNoRagdoll { get; private set; } = false;
        public bool PlayerNeverWanted { get; private set; } = false;
        public bool PlayerIsIgnored { get; private set; } = false;
        public bool PlayerFrozen { get; private set; } = false;

        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Player Options")//, MainMenu.MenuPosition)
            {
                //ScaleWithSafezone = false,
                MouseEdgeEnabled = false
            };

            // Create all checkboxes.
            UIMenuCheckboxItem playerGodModeCheckbox = new UIMenuCheckboxItem("Godmode", PlayerGodMode, "Makes you invincible.");
            UIMenuCheckboxItem invisibleCheckbox = new UIMenuCheckboxItem("Invisible", PlayerInvisible, "Makes you invisible to yourself and others.");
            UIMenuCheckboxItem unlimitedStaminaCheckbox = new UIMenuCheckboxItem("Unlimited Stamina", PlayerStamina, "Allows you to run forever without slowing down or taking damage.");
            UIMenuCheckboxItem fastRunCheckbox = new UIMenuCheckboxItem("Fast Run", false, "Get ~g~Snail~s~ powers and run very fast!");
            UIMenuCheckboxItem fastSwimCheckbox = new UIMenuCheckboxItem("Fast Swim", false, "Get ~g~Sail 2.0~s~ powers and swim super fast!");
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
            UIMenuListItem playerOptions = new UIMenuListItem("Player Functions", playerOptionsList, 0, "Select an option and press enter to run/stop it.");

            // Scenarios (list can be found in the PedScenarios class)
            UIMenuListItem playerScenarios = new UIMenuListItem("Player Scenarios", PedScenarios.Scenarios, 0, "Select a scenario and hit enter to start it. Selecting another scenario will override the current scenario. If you're already playing the selected scenario, selecting it again will stop the scenario.");
            UIMenuItem stopScenario = new UIMenuItem("Force Stop Scenario", "This will force a playing scenario to stop immediately, without waiting for it to finish it's 'stopping' animation.");


            // Add all checkboxes to the menu.
            menu.AddItem(playerGodModeCheckbox);
            menu.AddItem(invisibleCheckbox);
            menu.AddItem(unlimitedStaminaCheckbox);
            menu.AddItem(fastRunCheckbox);
            menu.AddItem(fastSwimCheckbox);
            menu.AddItem(superJumpCheckbox);
            menu.AddItem(noRagdollCheckbox);
            menu.AddItem(neverWantedCheckbox);
            menu.AddItem(setWantedLevel);
            menu.AddItem(everyoneIgnoresPlayerCheckbox);
            menu.AddItem(playerOptions);
            menu.AddItem(playerFrozenCheckbox);
            menu.AddItem(playerScenarios);
            menu.AddItem(stopScenario);

            // Handle all checkbox change events.
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
                    SetRunSprintMultiplierForPlayer(PlayerId(), (_checked ? 1.49f : 1f));
                }
                // Fast swim toggled.
                else if (item == fastSwimCheckbox)
                {
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

            #region When lists are selected, handle the functions here.
            menu.OnListSelect += (sender, listItem, index) =>
            {
                // Set wanted Level
                if (listItem == setWantedLevel)
                {
                    SetPlayerWantedLevel(PlayerId(), index, false);
                    SetPlayerWantedLevelNow(PlayerId(), false);
                }
                // Player options (healing, cleaning, armor, dry/wet, etc)
                else if (listItem == playerOptions)
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
                                MainMenu.cf.DriveToWp();
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
                                MainMenu.cf.DriveWander();
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
                    MainMenu.cf.PlayScenario(PedScenarios.ScenarioNames[PedScenarios.Scenarios[index]]);
                }
            };
            #endregion

            #region On Menu Button Press
            menu.OnItemSelect += (sender, item, index) =>
            {
                // Force Stop Scenario button
                if (item == stopScenario)
                {
                    // Play a new scenario named "forcestop" (this scenario doesn't exist, but the "Play" function checks
                    // for the string "forcestop", if that's provided as th scenario name then it will forcefully clear the player task.
                    MainMenu.cf.PlayScenario("forcestop");
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
