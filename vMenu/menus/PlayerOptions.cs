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
        private static Notification Notify = new Notification();
        private static Subtitles Subtitle = new Subtitles();

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
            menu = new UIMenu(GetPlayerName(PlayerId()), "Player Options", MainMenu.MenuPosition);

            // Create all checkboxes.
            UIMenuCheckboxItem playerGodModeCheckbox = new UIMenuCheckboxItem("God Mode", PlayerGodMode, "If you turn this on, you won't take any damage.");
            UIMenuCheckboxItem invisibleCheckbox = new UIMenuCheckboxItem("Invisibility", PlayerInvisible, "If you turn this on, you will become invisible.");
            UIMenuCheckboxItem unlimitedStaminaCheckbox = new UIMenuCheckboxItem("Unlimited Stamina", PlayerStamina, "If you disable this then you won't be able to run for more than 5 seconds. So it's recommended to keep this on at all times.");
            UIMenuCheckboxItem fastRunCheckbox = new UIMenuCheckboxItem("Fast Running", false, "Super Snail! I'm fast as f*ck boi!!!");
            UIMenuCheckboxItem fastSwimCheckbox = new UIMenuCheckboxItem("Fast Swimming", false, "Super Sail 2.0! Swim like a real snail!");
            UIMenuCheckboxItem superJumpCheckbox = new UIMenuCheckboxItem("Super Jump", PlayerSuperJump, "Super Snail 3.0! You can't beat a snail's jumping skills!");
            UIMenuCheckboxItem noRagdollCheckbox = new UIMenuCheckboxItem("No Ragdoll", PlayerNoRagdoll, "Don't fall over.");
            UIMenuCheckboxItem neverWantedCheckbox = new UIMenuCheckboxItem("Never Wanted", PlayerNeverWanted, "Nobody has time for annoying cops! Bribe them or deal with it.");
            UIMenuCheckboxItem everyoneIgnoresPlayerCheckbox = new UIMenuCheckboxItem("Everyone Ignores You", PlayerIsIgnored, "Annoying hillbillies trying to kill you? Turn this off and the Snail will make them shit their pants!");
            UIMenuCheckboxItem playerFrozenCheckbox = new UIMenuCheckboxItem("Freeze Yourself", PlayerFrozen, "Why would you do this...?");

            // Wanted level options
            List<dynamic> wantedLevelList = new List<dynamic> { "No Cops", 1, 2, 3, 4 };
            UIMenuListItem setWantedLevel = new UIMenuListItem("Set Wanted Level", wantedLevelList, GetPlayerWantedLevel(PlayerId()), "Set the wanted level by selecting a value, and pressing enter.");

            // Player options
            List<dynamic> playerOptionsList = new List<dynamic> { "Heal Player", "Apply Max Armor", "Clean Player", "Dry Player", "Soack Player" };
            UIMenuListItem playerOptions = new UIMenuListItem("Player Options", playerOptionsList, 0, "Select an option and press enter to execute it.");

            // Actions
            List<dynamic> playerActionsList = new List<dynamic> { "Commit Suicide", "Drive To Waypoint", "Drive Wander" };
            UIMenuListItem playerActions = new UIMenuListItem("Player Actions", playerActionsList, 0, "Select an action and press enter to run it, use the cancel button below to stop the driving actions.");
            UIMenuItem cancelActions = new UIMenuItem("Cancel Player Actions", "Click this to cancel any of the driving player actions from the list above.");

            // Scenarios (list can be found in the PedScenarios class)
            UIMenuListItem playerScenarios = new UIMenuListItem("Scenarios", PedScenarios.Scenarios, 0, "Select a scenario and hit enter to start it. Press it again to cancel it. Selecting another scenario and hitting enter will override the current scenario. Pressing enter again will then stop the scenario.");

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
            menu.AddItem(playerActions);
            menu.AddItem(playerFrozenCheckbox);
            menu.AddItem(playerScenarios);

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
                        case 0:
                            SetEntityHealth(PlayerPedId(), GetEntityMaxHealth(PlayerPedId()));
                            Subtitle.Success("Player Healed Successfully.");
                            break;
                        case 1:
                            SetPedArmour(PlayerPedId(), GetPlayerMaxArmour(PlayerId()));
                            Subtitle.Success("Max Armor Applied Successfully.");
                            break;
                        case 2:
                            ClearPedBloodDamage(PlayerPedId());
                            Subtitle.Success("Player Successfully Cleaned.");
                            break;
                        case 3:
                            ClearPedWetness(PlayerPedId());
                            Subtitle.Success("Player Successfully Dried.");
                            break;
                        case 4:
                            SetPedWetnessHeight(PlayerPedId(), 2f);
                            SetPedWetnessEnabledThisFrame(PlayerPedId());
                            Subtitle.Success("Player Successfully Soacked.");
                            break;
                        default:
                            break;
                    }
                }

                // Player actions (suicide, driving tasks, etc)
                else if (listItem == playerActions)
                {
                    switch (index)
                    {
                        case 0:
                            SetEntityHealth(PlayerPedId(), 0);
                            Notify.Success("You killed yourself!");
                            break;
                        case 1:
                            // Todo create drive to wp task.
                            break;
                        case 2:
                            break;
                        // Todo create drive wander task.
                        default:
                            break;
                    }
                }

                //// Player Scenarios 
                //else if (listItem == playerScenarios)
                //{
                //    // If they are currently in a scenario, and they select the same scenario, then cancel it.
                //    if (IsPlayingScenario == index)
                //    {
                //        IsPlayingScenario = -1;
                //    }
                //    // Otherwise, start a new scenario.
                //    else
                //    {
                //        // If the player can start a scenario.
                //        if (CanPlayScenarios())
                //        {
                //            // check if they are starting a new scenario for the first time.
                //            if (IsPlayingScenario == -1)
                //            {
                //                // Clear the tasks (any scenarios).
                //                ClearPedTasks(PlayerPedId());
                //                // Start the new scenario.
                //                TaskStartScenarioInPlace(PlayerPedId(), PedScenarios.ScenarioNames[PedScenarios.Scenarios[IsPlayingScenario]], 0, true);
                //            }
                //            IsPlayingScenario = index;
                //        }
                //        else
                //        {
                //            Notify.Alert("You can only start a scenario if you're standing still, and you're not inside any vehicle.");
                //        }
                //    }
                //}
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
