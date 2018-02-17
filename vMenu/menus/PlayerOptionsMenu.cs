using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient.menus
{
    class PlayerOptionsMenu : BaseScript
    {
        // Variables
        private UIMenu menu;
        private CommonFunctions cf = new CommonFunctions();
        private Notification Notify = new Notification();
        private Subtitles Subtitle = new Subtitles();

        private bool firstTick = true;

        private bool godMode = false;
        private bool invisible = false;
        private bool unlimitedStamina = true;
        private bool superJump = false;
        private bool noRagdoll = false;
        private bool neverWanted = false;
        private bool everyoneIgnoresPlayer = false;
        private bool playerFrozen = false;
        // will be implemented later: private bool driving = false;
        private int IsPlayingScenario = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerOptionsMenu()
        {
            CreateMenu();

            Tick += OnTick;
            Tick += ScenarioTick;
        }

        #region Public getter for the menu.
        /// <summary>
        /// Public getter for the menu.
        /// </summary>
        //public UIMenu Menu
        //{
        //    get
        //    {
        //        return menu;
        //    }
        //}
        #endregion

        #region OnTick Tasks
        /// <summary>
        /// OnTick used for all basic options that need to be set every tick.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // Just here to stop the annoying build warnings about no "Await in async task".
            if (firstTick)
            {
                firstTick = false;
                await Delay(0);
            }

            // God mode
            SetEntityInvincible(PlayerPedId(), godMode);

            // Invisibility
            SetEntityVisible(PlayerPedId(), !invisible, false);

            // Unlimited Stamina
            if (unlimitedStamina)
            {
                ResetPlayerStamina(PlayerId());
            }

            // Super Jump
            if (superJump)
            {
                SetSuperJumpThisFrame(PlayerId());
            }

            // No Ragdoll
            SetPedCanRagdoll(PlayerPedId(), !noRagdoll);
            SetPedCanRagdollFromPlayerImpact(PlayerPedId(), !noRagdoll);

            // Never Wanted
            if (neverWanted && GetPlayerWantedLevel(PlayerId()) > 0)
            {
                ClearPlayerWantedLevel(PlayerId());
            }

            // Everyone Ignores Player
            SetEveryoneIgnorePlayer(PlayerId(), everyoneIgnoresPlayer);
        }

        /// <summary>
        /// OnTick used for playing/stopping scenarios.
        /// </summary>
        /// <returns></returns>
        private async Task ScenarioTick()
        {
            // Each game tick, check if the player is using a scenario.
            if (IsPlayingScenario != -1)
            {
                // If so, set the current scenario to the playing scenario.
                var currentScenario = IsPlayingScenario;
                // Then loop below and wait until the player is no longer playing a scenario (IsPlayingScenario == -1).
                while (IsPlayingScenario != -1)
                {
                    // Wait to prevent crashing.
                    await Delay(0);
                    // If the scenario is not the "current" scenario and it's not -1 ( = no scenario) then:
                    if (IsPlayingScenario != currentScenario && IsPlayingScenario != -1)
                    {
                        if (CanPlayScenarios())
                        {
                            // Clear the tasks (any scenarios).
                            ClearPedTasks(PlayerPedId());
                            // Start the new scenario.
                            TaskStartScenarioInPlace(PlayerPedId(), PedScenarios.ScenarioNames[PedScenarios.Scenarios[IsPlayingScenario]], 0, true);
                            // Set the current scenario to be the new scenario.
                            currentScenario = IsPlayingScenario;
                        }
                        else
                        {
                            Notify.Alert("You can only start a scenario if you're standing still, and you're not inside any vehicle.");
                            ClearPedTasksImmediately(PlayerPedId());
                            IsPlayingScenario = -1;
                            currentScenario = IsPlayingScenario;
                        }

                    }
                    // Keep looping until the scenario id = -1s
                }
                // When it's -1, clear the player tasks once.
                if (IsPlayingScenario == -1)
                {
                    ClearPedTasks(PlayerPedId());
                }
                // Then go back to the regular OnTick loop and wait for any new scenarios to be selected.
            }
        }
        #endregion

        #region Create the Main Menu.
        /// <summary>
        /// Create the Player Options menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu("Player Options", "Common player options.")
            {
                ControlDisablingEnabled = false,
                ScaleWithSafezone = true
            };

            #region Create Menu Items
            // Checkboxes
            UIMenuCheckboxItem playerGodModeCheckbox = new UIMenuCheckboxItem("God Mode", godMode, "If you turn this on, you won't take any damage.");
            UIMenuCheckboxItem invisibleCheckbox = new UIMenuCheckboxItem("Invisibility", invisible, "If you turn this on, you will become invisible.");
            UIMenuCheckboxItem unlimitedStaminaCheckbox = new UIMenuCheckboxItem("Unlimited Stamina", unlimitedStamina, "If you disable this then you won't be able to run for more than 5 seconds. So it's recommended to keep this on at all times.");
            UIMenuCheckboxItem fastRunCheckbox = new UIMenuCheckboxItem("Fast Running", false, "Super Snail! I'm fast as f*ck boi!!!");
            UIMenuCheckboxItem fastSwimCheckbox = new UIMenuCheckboxItem("Fast Swimming", false, "Super Sail 2.0! Swim like a real snail!");
            UIMenuCheckboxItem superJumpCheckbox = new UIMenuCheckboxItem("Super Jump", superJump, "Super Snail 3.0! You can't beat a snail's jumping skills!");
            UIMenuCheckboxItem noRagdollCheckbox = new UIMenuCheckboxItem("No Ragdoll", noRagdoll, "Don't fall over.");
            UIMenuCheckboxItem neverWantedCheckbox = new UIMenuCheckboxItem("Never Wanted", neverWanted, "Nobody has time for annoying cops! Bribe them or deal with it.");
            UIMenuCheckboxItem everyoneIgnoresPlayerCheckbox = new UIMenuCheckboxItem("Everyone Ignores You", everyoneIgnoresPlayer, "Annoying hillbillies trying to kill you? Turn this off and the Snail will make them shit their pants!");
            UIMenuCheckboxItem playerFrozenCheckbox = new UIMenuCheckboxItem("Freeze Yourself", playerFrozen, "Why would you do this...?");

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
            #endregion

            #region Add items to menu
            // Add all items to the menu.
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
            #endregion

            #region Checkbox Changes
            // Handle all checkbox changes.
            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                // Toggle godmode.
                if (item == playerGodModeCheckbox)
                {
                    godMode = _checked;
                    Subtitle.Info("God mode has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Toggle invisibility.
                else if (item == invisibleCheckbox)
                {
                    invisible = _checked;
                    Subtitle.Info("Invisibility has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Toggle unlimited stamina.
                else if (item == unlimitedStaminaCheckbox)
                {
                    unlimitedStamina = _checked;
                    Subtitle.Info("Unlimited Stamina has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Toggle fast running.
                else if (item == fastRunCheckbox)
                {
                    SetRunSprintMultiplierForPlayer(PlayerId(), (_checked ? 1.49f : 1.0f));
                    Subtitle.Info("Fast Running has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Toggle fast swimming
                else if (item == fastSwimCheckbox)
                {
                    SetSwimMultiplierForPlayer(PlayerId(), (_checked ? 1.49f : 1.0f));
                    Subtitle.Info("Fast Swimming has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Super jump
                else if (item == superJumpCheckbox)
                {
                    superJump = _checked;
                    Subtitle.Info("Superjump has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // No Ragdoll
                else if (item == noRagdollCheckbox)
                {
                    noRagdoll = _checked;
                    Subtitle.Info("No Ragdoll has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Never Wanted
                else if (item == neverWantedCheckbox)
                {
                    neverWanted = _checked;
                    Subtitle.Info("Never Wanted has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Everyone Ignores Player
                else if (item == everyoneIgnoresPlayerCheckbox)
                {
                    everyoneIgnoresPlayer = _checked;
                    Subtitle.Info("Everyone Ignores Player has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
                // Freeze Player
                else if (item == playerFrozenCheckbox)
                {
                    FreezeEntityPosition(PlayerPedId(), playerFrozen);
                    Subtitle.Info("Freeze Player has been " + (_checked ? "~g~enabled" : "~r~disabled") + "~b~.");
                }
            };
            #endregion

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

                // Player Scenarios 
                else if (listItem == playerScenarios)
                {
                    // If they are currently in a scenario, and they select the same scenario, then cancel it.
                    if (IsPlayingScenario == index)
                    {
                        IsPlayingScenario = -1;
                    }
                    // Otherwise, start a new scenario.
                    else
                    {
                        // If the player can start a scenario.
                        if (CanPlayScenarios())
                        {
                            // check if they are starting a new scenario for the first time.
                            if (IsPlayingScenario == -1)
                            {
                                // Clear the tasks (any scenarios).
                                ClearPedTasks(PlayerPedId());
                                // Start the new scenario.
                                TaskStartScenarioInPlace(PlayerPedId(), PedScenarios.ScenarioNames[PedScenarios.Scenarios[IsPlayingScenario]], 0, true);
                            }
                            IsPlayingScenario = index;
                        }
                        else
                        {
                            Notify.Alert("You can only start a scenario if you're standing still, and you're not inside any vehicle.");
                        }
                    }
                }
            };
            #endregion

            // Open the main menu if this submenu closes.
            menu.OnMenuClose += (sender) =>
            {
                menu.Visible = false;
                MainMenu.menu.Visible = true;
            };

            // Add this menu to the menu pool.
            MainMenu._mp.Add(menu);

            // Create a button for this menu.
            UIMenuItem PlayerOptionsButton = new UIMenuItem("Player Options", "Configure common player options in this submenu.");
            // Add the button to the main menu.
            MainMenu.menu.AddItem(PlayerOptionsButton);
            // Bind the button from the main menu to this menu.
            MainMenu.menu.BindMenuToItem(menu, PlayerOptionsButton);

            // Refresh the index on this menu, the main menu and the menu pool.
            menu.RefreshIndex();
            MainMenu.menu.RefreshIndex();
            MainMenu._mp.RefreshIndex();

        }

        #endregion

        #region Check if the player can start a new scenario
        /// <summary>
        /// Returns true if the player can start a scenario, false if they can't.
        /// </summary>
        /// <returns>(bool) True: the player can start a new scenario. False: the player can NOT start a new scenario.</returns>
        private bool CanPlayScenarios()
        {
            return !(IsPedInAnyVehicle(PlayerPedId(), true) || IsPedDeadOrDying(PlayerPedId(), true) || IsPedRagdoll(PlayerPedId()) || IsPedStopped(PlayerPedId()));
        }
        #endregion

    }
}
