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
    public class OnlinePlayers
    {
        // Menu variable, will be defined in CreateMenu()
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.Cf;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Online Players", true)
            {
                ScaleWithSafezone = false,
                MouseControlsEnabled = false,
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };

            UpdatePlayerlist();
        }

        /// <summary>
        /// Updates the player items.
        /// </summary>
        public void UpdatePlayerlist()
        {
            // Remove leftover menu items if they exist.
            if (menu.MenuItems.Count > 0)
            {
                menu.MenuItems.Clear();
            }

            // Create a new player list.
            PlayerList pl = new PlayerList();

            // Loop through the playerlist.
            foreach (Player p in pl)
            {
                // Create a button for this player and add it to the menu.
                UIMenuItem playerItem = new UIMenuItem(p.Name, "[" + (p.Handle < 10 ? "0" : "") + p.Handle + "] " + p.Name + " (Server ID: " + p.ServerId + ")");
                playerItem.SetRightLabel("→→→");
                menu.AddItem(playerItem);

                // Handle button presses.
                menu.OnItemSelect += (sender, item, index) =>
                {
                    // If the player item is pressed.
                    if (item == playerItem)
                    {
                        // Create the player object.
                        Player player = new Player(int.Parse(item.Description.Substring(1, 2).ToString()));

                        // Create the menu for the player & set the width offset.
                        UIMenu PlayerMenu = new UIMenu(player.Name, "[" + (player.Handle < 10 ? "0" : "") + player.Handle + "] " + player.Name + " (Server ID: " + player.ServerId + ")", true)
                        {
                            ScaleWithSafezone = false,
                            MouseControlsEnabled = false,
                            MouseEdgeEnabled = false,
                            ControlDisablingEnabled = false
                        };
                        PlayerMenu.SetMenuWidthOffset(50);
                        PlayerMenu.RefreshIndex();
                        PlayerMenu.UpdateScaleform();


                        // Create all player options buttons.
                        UIMenuItem teleportBtn = new UIMenuItem("Teleport To Player", "Teleport to this player.");
                        UIMenuItem teleportInVehBtn = new UIMenuItem("Teleport Into Vehicle", "Teleport into the player's vehicle.");
                        UIMenuItem setWaypointBtn = new UIMenuItem("Set Waypoint", "Set a waypoint to this player.");
                        UIMenuItem spectateBtn = new UIMenuItem("Spectate Player", "Spectate this player.");
                        UIMenuItem summonBtn = new UIMenuItem("Summon Player", "Bring this player to your location.");
                        summonBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                        UIMenuItem killBtn = new UIMenuItem("Kill Player", "Kill the selected player! Why are you so cruel :(");
                        killBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                        UIMenuItem kickPlayerBtn = new UIMenuItem("~r~Kick Player", "~r~Kick~s~ this player from the server, you need to specify a reason otherwise the kick will be cancelled.");
                        kickPlayerBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);


                        // Add all buttons to the player options submenu. Keeping permissions in mind.
                        if (cf.IsAllowed(Permission.OPTeleport))
                        {
                            PlayerMenu.AddItem(teleportBtn);
                            PlayerMenu.AddItem(teleportInVehBtn);
                        }
                        if (cf.IsAllowed(Permission.OPWaypoint))
                        {
                            PlayerMenu.AddItem(setWaypointBtn);
                        }
                        if (cf.IsAllowed(Permission.OPSpectate))
                        {
                            PlayerMenu.AddItem(spectateBtn);
                        }
                        if (cf.IsAllowed(Permission.OPSummon))
                        {
                            PlayerMenu.AddItem(summonBtn);
                        }
                        if (cf.IsAllowed(Permission.OPKill))
                        {
                            PlayerMenu.AddItem(killBtn);
                        }
                        if (cf.IsAllowed(Permission.OPKick))
                        {
                            PlayerMenu.AddItem(kickPlayerBtn);
                        }


                        // Add the player menu to the menu pool.
                        MainMenu.Mp.Add(PlayerMenu);

                        // Set the menu invisible.
                        menu.Visible = false;
                        // Set the player menu visible.
                        PlayerMenu.Visible = true;


                        // If a button is pressed in the player's options menu.
                        PlayerMenu.OnItemSelect += (sender2, item2, index2) =>
                        {
                            // Teleport button is pressed.
                            if (item2 == teleportBtn)
                            {
                                cf.TeleportToPlayerAsync(player.Handle, false);
                            }
                            // Teleport in vehicle button is pressed.
                            else if (item2 == teleportInVehBtn)
                            {
                                cf.TeleportToPlayerAsync(player.Handle, true);
                            }
                            // Set waypoint button is pressed.
                            else if (item2 == setWaypointBtn)
                            {
                                World.WaypointPosition = GetEntityCoords(GetPlayerPed(player.Handle), true);
                                //Subtitle.Info($"A new waypoint has been set to ~y~{player.Name}~z~.", prefix: "Info:");
                            }
                            // Spectate player button is pressed.
                            else if (item2 == spectateBtn)
                            {
                                if (player.Handle == PlayerId())
                                {
                                    //Subtitle.Error("You can ~h~not~h~ spectate yourself!", prefix: "Error:");
                                    Notify.Error("Sorry, you can ~r~~h~not~h~ ~s~spectate yourself!");
                                }
                                else
                                {
                                    cf.SpectateAsync(player.Handle);
                                }
                            }
                            // Summon player button is pressed.
                            else if (item2 == summonBtn)
                            {
                                if (player.Handle == PlayerId())
                                {
                                    Notify.Error("Sorry, you can ~r~~h~not~h~ ~s~summon yourself!");
                                }
                                else
                                {
                                    cf.SummonPlayer(player);
                                }
                            }
                            // Kill player button is pressed.
                            else if (item2 == killBtn)
                            {
                                //Subtitle.Info($"~y~{player.Name} ~z~has been killed.", prefix: "Info:");
                                Notify.Success($"Player ~y~<C>{player.Name}</C> ~s~has been killed.");
                                cf.KillPlayer(player);
                            }
                            // Kick player button is pressed.
                            else if (item2 == kickPlayerBtn)
                            {
                                // Close the menu.
                                PlayerMenu.GoBack();

                                // Kick the player.
                                cf.KickPlayer(player, true);

                                // Update the player list.
                                UpdatePlayerlist();

                                // Refresh the index & update scaleform.
                                menu.RefreshIndex();
                                menu.UpdateScaleform();
                            }
                        };

                        // Reopen the playerlist menu when a player specific menu is closed.
                        PlayerMenu.OnMenuClose += (sender2) =>
                        {
                            menu.Visible = true;
                        };
                    }
                };
            };
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Online Players Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
                return menu;
            }
            else
            {
                UpdatePlayerlist();
                return menu;
            }
        }
    }
}
