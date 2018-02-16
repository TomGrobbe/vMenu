using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenu.menus
{
    class PlayerOptionsMenu : BaseScript
    {
        private UIMenu menu;
        private CommonFunctions cf = new CommonFunctions();
        private Notification Notify = new Notification();

        private bool godMode = false;
        private bool invisible = false;
        private bool unlimitedStamina = true;
        private bool fastRun = false;
        private bool fastSwim = false;
        private bool superJump = false;
        private bool noRagdoll = false;
        private bool neverWanted = false;
        private bool everyoneIgnoresPlayer = false;
        private bool playerFrozen = false;
        private enum Scenarios { NONE, LEANING, SMOKING, SUNBATH, SUNBATH2 };
        private bool driving = false;

        public PlayerOptionsMenu()
        {
            menu = new UIMenu("Player Options", "Common player options.")
            {
                ControlDisablingEnabled = false,
                ScaleWithSafezone = true
            };

            CreateMenu();

            Tick += OnTick;
        }

        private async Task OnTick()
        {
            SetEntityInvincible(PlayerPedId(), godMode);
            SetEntityVisible(PlayerPedId(), !invisible, false);
            if (unlimitedStamina)
            {
                ResetPlayerStamina(PlayerId());
            }
            if (superJump)
            {
                SetSuperJumpThisFrame(PlayerId());
            }
            SetPedCanRagdoll(PlayerPedId(), !noRagdoll);
            SetPedCanRagdollFromPlayerImpact(PlayerPedId(), !noRagdoll);
            if (neverWanted && GetPlayerWantedLevel(PlayerId()) > 0)
            {
                SetPlayerWantedLevel(PlayerId(), 0, false);
                SetPlayerWantedLevelNow(PlayerId(), false);
            }
            SetEveryoneIgnorePlayer(PlayerId(), everyoneIgnoresPlayer);
        }

        private void CreateMenu()
        {
            // Checkboxes
            UIMenuCheckboxItem playerGodModeCheckbox = new UIMenuCheckboxItem("God Mode", godMode, "If you turn this on, you won't take any damage.");
            UIMenuCheckboxItem invisibleCheckbox = new UIMenuCheckboxItem("Invisibility", invisible, "If you turn this on, you will become invisible.");
            UIMenuCheckboxItem unlimitedStaminaCheckbox = new UIMenuCheckboxItem("Unlimited Stamina", unlimitedStamina, "If you disable this then you won't be able to run for more than 5 seconds. So it's recommended to keep this on at all times.");
            UIMenuCheckboxItem fastRunCheckbox = new UIMenuCheckboxItem("Fast Running", fastRun, "Super Snail! I'm fast as f*ck boi!!!");
            UIMenuCheckboxItem fastSwimCheckbox = new UIMenuCheckboxItem("Fast Swimming", fastSwim, "Super Sail 2.0! Swim like a real snail!");
            UIMenuCheckboxItem superJumpCheckbox = new UIMenuCheckboxItem("Super Jump", superJump, "Super Snail 3.0! You can't beat a snail's jumping skills!");
            UIMenuCheckboxItem noRagdollCheckbox = new UIMenuCheckboxItem("No Ragdoll", noRagdoll, "Don't fall over.");
            UIMenuCheckboxItem neverWantedCheckbox = new UIMenuCheckboxItem("Never Wanted", neverWanted, "Nobody has time for annoying cops! Bribe them or deal with it.");
            UIMenuCheckboxItem everyoneIgnoresPlayerCheckbox = new UIMenuCheckboxItem("Everyone Ignores You", everyoneIgnoresPlayer, "Annoying hillbillies trying to kill you? Turn this off and the Snail will make them shit their pants!");
            UIMenuCheckboxItem playerFrozenCheckbox = new UIMenuCheckboxItem("Freeze Yourself", playerFrozen, "Why would you do this...?");

            // Wanted level options
            List<dynamic> wantedLevelList = new List<dynamic> { 0, 1, 2, 3, 4 };
            UIMenuListItem setWantedLevel = new UIMenuListItem("Set Wanted Level", wantedLevelList, GetPlayerWantedLevel(PlayerId()), "Set the wanted level by selecting a value, and pressing enter.");
            // Player options
            List<dynamic> playerOptionsList = new List<dynamic> { "Heal Player", "Apply Max Armor", "Clean Player", "Dry Player", "Soack Player" };
            UIMenuListItem playerOptions = new UIMenuListItem("Player Options", playerOptionsList, 0, "Select an option and press enter to execute it.");
            // Actions
            List<dynamic> playerActionsList = new List<dynamic> { "Commit Suicide", "Drive To Waypoint", "Drive Wander" };
            UIMenuListItem playerActions = new UIMenuListItem("Player Actions", playerActionsList, 0, "Select an action and press enter to run it, use the cancel button below to stop the driving actions.");
            UIMenuItem cancelActions = new UIMenuItem("Cancel Player Actions", "Click this to cancel any of the driving player actions from the list above.");
            /// Scenarios (list is in <cref see="PedScenarios.cs">PedScenarios.cs</cref>
            UIMenuListItem playerScenarios = new UIMenuListItem("Scenarios", PedScenarios.Scenarios, 0, "Select a scenario and hit enter to start it. Press it again to cancel it. Selecting another scenario and hitting enter will override the current scenario. Pressing enter again will then stop the scenario.");

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

            // Handle all checkbox changes.
            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                // Toggle godmode.
                if (item == playerGodModeCheckbox)
                {
                    godMode = _checked;
                }
                // Toggle invisibility.
                if (item == invisibleCheckbox)
                {
                    invisible = _checked;
                }
                if (item == unlimitedStaminaCheckbox)
                {
                    unlimitedStamina = _checked;
                }
                if (item == fastRunCheckbox)
                {
                    if (_checked)
                    {
                        SetRunSprintMultiplierForPlayer(PlayerId(), 1.49f);
                    }
                    else
                    {
                        SetRunSprintMultiplierForPlayer(PlayerId(), 1f);
                    }
                }
                if (item == fastSwimCheckbox)
                {
                    if (_checked)
                    {
                        SetSwimMultiplierForPlayer(PlayerId(), 1.49f);
                    }
                    else
                    {
                        SetSwimMultiplierForPlayer(PlayerId(), 1f);
                    }
                }
                if (item == superJumpCheckbox)
                {
                    superJump = _checked;
                }
                if (item == noRagdollCheckbox)
                {
                    noRagdoll = _checked;
                }
                if (item == neverWantedCheckbox)
                {
                    neverWanted = _checked;
                }
                if (item == everyoneIgnoresPlayerCheckbox)
                {
                    everyoneIgnoresPlayer = _checked;
                }
                if (item == playerFrozenCheckbox)
                {
                    FreezeEntityPosition(PlayerPedId(), playerFrozen);
                }
            };


            //FreezeEntityPosition(PlayerPedId(), playerFrozen);


        }
    }
}
