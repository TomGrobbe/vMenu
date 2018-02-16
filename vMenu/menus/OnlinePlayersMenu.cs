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
    class OnlinePlayersMenu : BaseScript
    {
        private UIMenu menu;
        private CommonFunctions cf = new CommonFunctions();
        private Notification Notify = new Notification();

        /// <summary>
        /// Constructor.
        /// Creates the menu.
        /// </summary>
        public OnlinePlayersMenu()
        {
            menu = new UIMenu("Online Players", "Currently connected players.")
            {
                ControlDisablingEnabled = false,
                ScaleWithSafezone = true
            };
            RefreshMenu();
            UIMenuItem onlinePlayersMenuBtn = new UIMenuItem("Online Players", "Online players in this server.");
            MainMenu.menu.AddItem(onlinePlayersMenuBtn);
            MainMenu.menu.BindMenuToItem(menu, onlinePlayersMenuBtn);

            // If the online players menu closes, reopen the main menu.
            menu.OnMenuClose += (sender2) =>
            {
                menu.Visible = false;
                MainMenu.menu.Visible = true;
            };

            MainMenu._mp.Add(menu);
        }

        /// <summary>
        /// Public getter for the menu.
        /// </summary>
        public UIMenu Menu
        {
            get
            {
                return menu;
            }
        }

        /// <summary>
        /// Updates the menu (updated playerlist).
        /// </summary>
        public void RefreshMenu()
        {
            // Remove all current players.
            for (var i = 0; i < menu.MenuItems.Count; i++)
            {
                menu.RemoveItemAt(i);
            }

            // Loop through all online players, and add them to the list if they exist.
            // Made it 64 so it's already future proof ;^)
            for (var i = 0; i < 64; i++)
            {
                // If the player exists.
                if (NetworkIsPlayerActive(i))
                {
                    // Create a new button for that player.
                    UIMenuItem playerItem = new UIMenuItem(GetPlayerServerId(i) + " " + GetPlayerName(i), "Open the player options for " + GetPlayerName(i));
                    // Add the button to the menu.
                    menu.AddItem(playerItem);

                    // Handle button selected event.
                    menu.OnItemSelect += (sender, item, index) =>
                    {
                        // If the button is the player button, then...
                        if (item == playerItem)
                        {
                            var playerIndex = i;
                            string playerName = GetPlayerName(playerIndex);
                            if (playerIndex == PlayerId())
                            {
                                playerName += " (me)";
                            }

                            // ...Create a new menu for that player.
                            UIMenu playerMenu = new UIMenu("[" + GetPlayerServerId(playerIndex).ToString() + "] " + playerName, "Server ID: " + GetPlayerServerId(playerIndex));
                            // Create all player options buttons.
                            UIMenuItem teleportBtn = new UIMenuItem("Teleport to Player", "Teleport to this player.");
                            UIMenuItem teleportInVehBtn = new UIMenuItem("Teleport into Vehicle", "Telepor into the player's vehicle.");
                            UIMenuItem setWaypointBtn = new UIMenuItem("Set waypoint", "Set a waypoint to this player.");
                            UIMenuItem spectateBtn = new UIMenuItem("Spectate Player", "Spectate this player.");
                            UIMenuItem summonBtn = new UIMenuItem("Summon Player", "Teleport the player in front of you.");
                            UIMenuItem killBtn = new UIMenuItem("Kill Player", "Kill the other player!");
                            UIMenuItem kickPlayerBtn = new UIMenuItem("Kick Player", "Kick the player from the server.");
                            kickPlayerBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);

                            // Add all buttons to the player options submenu.
                            playerMenu.AddItem(teleportBtn);
                            playerMenu.AddItem(teleportInVehBtn);
                            playerMenu.AddItem(setWaypointBtn);
                            playerMenu.AddItem(spectateBtn);
                            playerMenu.AddItem(summonBtn);
                            playerMenu.AddItem(killBtn);
                            playerMenu.AddItem(kickPlayerBtn);

                            // Set the player options submenu visible, and the player list menu hidden.
                            playerMenu.Visible = true;
                            menu.Visible = false;

                            // If a button is pressed in the player's options menu.
                            playerMenu.OnItemSelect += (sender2, item2, index2) =>
                            {
                                // Teleport button is pressed.
                                if (item2 == teleportBtn)
                                {
                                    //Vector3 playerPos = GetEntityCoords(GetPlayerPed(playerIndex), true);
                                    //SetPedCoordsKeepVehicle(PlayerPedId(), playerPos.X, playerPos.Y, playerPos.Z + 2.0f);
                                    cf.TeleportToPlayerAsync(playerIndex, false);
                                }
                                // Teleport in vehicle button is pressed.
                                else if (item2 == teleportInVehBtn)
                                {
                                    cf.TeleportToPlayerAsync(playerIndex, true);
                                }
                                // Set waypoint button is pressed.
                                else if (item2 == setWaypointBtn)
                                {
                                    CitizenFX.Core.World.WaypointPosition = GetEntityCoords(GetPlayerPed(playerIndex), true);
                                    Notify.Info("A new waypoint has been set to " + playerName, false, false);
                                }
                                // Spectate player button is pressed.
                                else if (item2 == spectateBtn)
                                {
                                    if (playerIndex == PlayerId())
                                    {
                                        Notify.Error("You can ~h~not ~w~spectate yourself!");
                                    }
                                    else
                                    {
                                        cf.Spectate(playerIndex);
                                    }
                                }
                                // Summon player button is pressed.
                                else if (item2 == summonBtn)
                                {
                                    TriggerServerEvent("vMenu:TeleportPlayer", GetPlayerServerId(playerIndex));
                                }
                                // Kill player button is pressed.
                                else if (item2 == killBtn)
                                {
                                    TriggerServerEvent("vMenu:KillPlayer", GetPlayerServerId(playerIndex));
                                }
                                // Kick player button is pressed.
                                else if (item2 == kickPlayerBtn)
                                {
                                    TriggerServerEvent("vMenu:KickPlayer", GetPlayerServerId(playerIndex));
                                    playerMenu.Visible = false;
                                    RefreshMenu();
                                    menu.RefreshIndex();
                                    menu.Visible = true;
                                }
                            };

                            // If the player options menu closes, reopen the playerlist menu.
                            playerMenu.OnMenuClose += (sender3) =>
                            {
                                playerMenu.Visible = false;
                                menu.Visible = true;
                            };

                            // Re-enable controls & refresh index.
                            playerMenu.ControlDisablingEnabled = false;
                            playerMenu.RefreshIndex();

                            // Add the menu to the Menu Pool.
                            MainMenu._mp.Add(playerMenu);
                        }
                    };

                    menu.RefreshIndex();
                }
            }
        }

    }
}
