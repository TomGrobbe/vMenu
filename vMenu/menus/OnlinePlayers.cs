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
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new List<int>();

        // Menu variable, will be defined in CreateMenu()
        private UIMenu menu;
        private CommonFunctions cf = MainMenu.Cf;

        UIMenu playerMenu = new UIMenu("Online Players", "Player:", true) { AlwaysShowMenuItemCounter = true };
        Player currentPlayer = new Player(Game.Player.Handle);


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(GetPlayerName(Game.Player.Handle), "Online Players", true) { AlwaysShowMenuItemCounter = true };
            menu.CounterPretext = "Players: ";

            MainMenu.Mp.Add(playerMenu);

            UIMenuItem teleport = new UIMenuItem("Teleport To Player", "Teleport to this player.");
            UIMenuItem teleportVeh = new UIMenuItem("Teleport Into Player Vehicle", "Teleport into the vehicle of the player.");
            UIMenuItem summon = new UIMenuItem("Summon Player", "Teleport the player to you.");
            UIMenuItem toggleGPS = new UIMenuItem("Toggle GPS", "Enables or disables the GPS route on your radar to this player.");
            UIMenuItem spectate = new UIMenuItem("Spectate Player", "Spectate this player. Click this button again to stop spectating.");
            UIMenuItem kill = new UIMenuItem("~r~Kill Player", "Kill this player, note they will receive a notification saying that you killed them. It will also be logged in the Staff Actions log.");
            UIMenuItem kick = new UIMenuItem("~r~Kick Player", "Kick the player from the server.");
            UIMenuItem ban = new UIMenuItem("~r~Ban Player Permanently", "Ban this player permanently from the server. Are you sure you want to do this? You can specify the ban reason after clicking this button.");
            UIMenuItem tempban = new UIMenuItem("~r~Ban Player Temporarily", "Give this player a tempban of up to 30 days (max). You can specify duration and ban reason after clicking this button.");

            if (cf.IsAllowed(Permission.OPTeleport))
            {
                playerMenu.AddItem(teleport);
                playerMenu.AddItem(teleportVeh);
            }
            if (cf.IsAllowed(Permission.OPSummon))
            {
                playerMenu.AddItem(summon);
            }
            if (cf.IsAllowed(Permission.OPSpectate))
            {
                playerMenu.AddItem(spectate);
            }
            if (cf.IsAllowed(Permission.OPWaypoint))
            {
                playerMenu.AddItem(toggleGPS);
            }
            if (cf.IsAllowed(Permission.OPKill))
            {
                playerMenu.AddItem(kill);
            }
            if (cf.IsAllowed(Permission.OPKick))
            {
                playerMenu.AddItem(kick);
            }
            if (cf.IsAllowed(Permission.OPTempBan))
            {
                playerMenu.AddItem(tempban);
            }
            if (cf.IsAllowed(Permission.OPPermBan))
            {
                playerMenu.AddItem(ban);
                ban.SetLeftBadge(UIMenuItem.BadgeStyle.Alert);
            }

            playerMenu.OnMenuClose += (sender) =>
            {
                playerMenu.RefreshIndex();
                playerMenu.UpdateScaleform();
                ban.SetRightLabel("");
            };

            playerMenu.OnIndexChange += (sender, index) =>
            {
                ban.SetRightLabel("");
            };

            // handle button presses for the specific player's menu.
            playerMenu.OnItemSelect += (sender, item, index) =>
            {
                // teleport (in vehicle) button
                if (item == teleport || item == teleportVeh)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                        cf.TeleportToPlayerAsync(currentPlayer.Handle, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
                    else
                        Notify.Error("You can not teleport to yourself! Silly you.");
                }
                // summon button
                else if (item == summon)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                        cf.SummonPlayer(currentPlayer);
                    else
                        Notify.Error("You can't summon yourself. You silly.");
                }
                // spectating
                else if (item == spectate)
                {
                    cf.SpectatePlayer(currentPlayer);
                }
                // kill button
                else if (item == kill)
                {
                    cf.KillPlayer(currentPlayer);
                }
                // manage the gps route being clicked.
                else if (item == toggleGPS)
                {
                    bool selectedPedRouteAlreadyActive = false;
                    if (PlayersWaypointList.Count > 0)
                    {
                        if (PlayersWaypointList.Contains(currentPlayer.Handle))
                        {
                            selectedPedRouteAlreadyActive = true;
                        }
                        foreach (int playerId in PlayersWaypointList)
                        {
                            int playerPed = GetPlayerPed(playerId);
                            if (DoesEntityExist(playerPed) && DoesBlipExist(GetBlipFromEntity(playerPed)))
                            {
                                int oldBlip = GetBlipFromEntity(playerPed);
                                SetBlipRoute(oldBlip, false);
                                RemoveBlip(ref oldBlip);
                                Notify.Custom($"~g~GPS route to ~s~<C>{cf.GetSafePlayerName(currentPlayer.Name)}</C>~g~ is now disabled.");
                            }
                        }
                        PlayersWaypointList.Clear();
                    }

                    if (!selectedPedRouteAlreadyActive)
                    {
                        if (currentPlayer.Handle != Game.Player.Handle)
                        {
                            int ped = GetPlayerPed(currentPlayer.Handle);
                            int blip = GetBlipFromEntity(ped);
                            if (DoesBlipExist(blip))
                            {
                                SetBlipColour(blip, 58);
                                SetBlipRouteColour(blip, 58);
                                SetBlipRoute(blip, true);
                            }
                            else
                            {
                                blip = AddBlipForEntity(ped);
                                SetBlipColour(blip, 58);
                                SetBlipRouteColour(blip, 58);
                                SetBlipRoute(blip, true);
                            }
                            PlayersWaypointList.Add(currentPlayer.Handle);
                            Notify.Custom($"~g~GPS route to ~s~<C>{cf.GetSafePlayerName(currentPlayer.Name)}</C>~g~ is now active, press the ~s~Toggle GPS Route~g~ button again to disable the route.");
                        }
                        else
                        {
                            Notify.Error("You can not set a waypoint to yourself.");
                        }
                    }
                }
                // kick button
                else if (item == kick)
                {
                    if (currentPlayer.Handle != Game.Player.Handle)
                        cf.KickPlayer(currentPlayer, true);
                    else
                        Notify.Error("You cannot kick yourself!");
                }
                // temp ban
                else if (item == tempban)
                {
                    cf.BanPlayer(currentPlayer, false);
                }
                // perm ban
                else if (item == ban)
                {
                    if (ban.RightLabel == "Are you sure?")
                    {
                        ban.SetRightLabel("");
                        UpdatePlayerlist();
                        playerMenu.GoBack();
                        cf.BanPlayer(currentPlayer, true);
                    }
                    else
                    {
                        ban.SetRightLabel("Are you sure?");
                    }
                }
            };

            // handle button presses in the player list.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (MainMenu.PlayersList.ToList().Any(p => p.ServerId.ToString() == item.RightLabel.Replace(" →→→", "").Replace("Server #", "")))
                {
                    currentPlayer = MainMenu.PlayersList.ToList().Find(p => p.ServerId.ToString() == item.RightLabel.Replace(" →→→", "").Replace("Server #", ""));
                }
                else
                {
                    playerMenu.GoBack();
                }
            };
        }

        /// <summary>
        /// Updates the player items.
        /// </summary>
        public void UpdatePlayerlist()
        {
            menu.Clear();

            foreach (Player p in MainMenu.PlayersList)
            {
                UIMenuItem pItem = new UIMenuItem($"{p.Name}", $"Click to view the options for this player. Server ID: {p.ServerId}. Local ID: {p.Handle}.");
                pItem.SetRightLabel($"Server #{p.ServerId} →→→");
                menu.AddItem(pItem);
                menu.BindMenuToItem(playerMenu, pItem);
            }

            menu.RefreshIndex();
            menu.UpdateScaleform();
            playerMenu.RefreshIndex();
            playerMenu.UpdateScaleform();


            /*
            // Remove leftover menu items if they exist.
            if (menu.MenuItems.Count > 0)
            {
                menu.MenuItems.Clear();
            }

            // Create a new player list.
            PlayerList pl = MainMenu.PlayersList;

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
                        UIMenu PlayerMenu = new UIMenu(player.Name, "[" + (player.Handle < 10 ? "0" : "") + player.Handle + "] " + player.Name +
                            " (Server ID: " + player.ServerId + ")", true)
                        {
                            ScaleWithSafezone = false,
                            MouseControlsEnabled = false,
                            MouseEdgeEnabled = false,
                            ControlDisablingEnabled = false
                        };

                        // Create all player options buttons.
                        UIMenuItem teleportBtn = new UIMenuItem("Teleport To Player", "Teleport to this player.");
                        UIMenuItem teleportInVehBtn = new UIMenuItem("Teleport Into Vehicle", "Teleport into the player's vehicle.");
                        UIMenuItem setWaypointBtn = new UIMenuItem("Toggle GPS Route", "Enables or disables drawing a GPS route to this player.");
                        UIMenuItem spectateBtn = new UIMenuItem("Spectate Player", "Spectate this player.");
                        UIMenuItem summonBtn = new UIMenuItem("Summon Player", "Bring this player to your location.");
                        summonBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                        UIMenuItem killBtn = new UIMenuItem("Kill Player", "Kill the selected player! Why are you so cruel :(");
                        killBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                        UIMenuItem kickPlayerBtn = new UIMenuItem("~r~Kick Player", "~r~Kick~s~ this player from the server, you need to specify a reason " +
                            "otherwise the kick will be cancelled.");
                        kickPlayerBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                        UIMenuItem permBanBtn = new UIMenuItem("~r~Ban Player", "Ban the player from the server forever.");
                        permBanBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                        UIMenuItem tempBanBtn = new UIMenuItem("~r~Tempban Player", "Ban the player from the server for the specified amount of hours. " +
                            "The player will be able to rejoin after the ban expires.");
                        tempBanBtn.SetRightBadge(UIMenuItem.BadgeStyle.Alert);

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
                        if (cf.IsAllowed(Permission.OPTempBan))
                        {
                            PlayerMenu.AddItem(tempBanBtn);
                        }
                        if (cf.IsAllowed(Permission.OPPermBan))
                        {
                            PlayerMenu.AddItem(permBanBtn);
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
                                bool selectedPedRouteAlreadyActive = false;
                                if (PlayersWaypointList.Count > 0)
                                {
                                    if (PlayersWaypointList.Contains(player.Handle))
                                    {
                                        selectedPedRouteAlreadyActive = true;
                                    }
                                    foreach (int playerId in PlayersWaypointList)
                                    {
                                        int playerPed = GetPlayerPed(playerId);
                                        if (DoesEntityExist(playerPed) && DoesBlipExist(GetBlipFromEntity(playerPed)))
                                        {
                                            int oldBlip = GetBlipFromEntity(playerPed);
                                            SetBlipRoute(oldBlip, false);
                                            RemoveBlip(ref oldBlip);
                                            Notify.Custom($"~g~GPS route to ~s~<C>{GetPlayerName(playerId)}</C>~g~ is now disabled.");
                                        }
                                    }
                                    PlayersWaypointList.Clear();
                                }

                                if (!selectedPedRouteAlreadyActive)
                                {
                                    if (player.Handle != Game.Player.Handle)
                                    {
                                        int ped = GetPlayerPed(player.Handle);
                                        int blip = GetBlipFromEntity(ped);
                                        if (DoesBlipExist(blip))
                                        {
                                            SetBlipColour(blip, 58);
                                            SetBlipRouteColour(blip, 58);
                                            SetBlipRoute(blip, true);
                                        }
                                        else
                                        {
                                            blip = AddBlipForEntity(ped);
                                            SetBlipColour(blip, 58);
                                            SetBlipRouteColour(blip, 58);
                                            SetBlipRoute(blip, true);
                                        }
                                        PlayersWaypointList.Add(player.Handle);
                                        Notify.Custom($"~g~GPS route to ~s~<C>{player.Name}</C>~g~ is now active, press the ~s~Toggle GPS Route~g~ button again to disable the route.");
                                    }
                                    else
                                    {
                                        Notify.Error("You can not set a waypoint to yourself.");
                                    }
                                }

                            }
                            // Spectate player button is pressed.
                            else if (item2 == spectateBtn)
                            {
                                if (player.Handle == Game.Player.Handle)
                                {
                                    Notify.Error("Sorry, you can ~r~~h~not~h~ ~s~spectate yourself!", true, true);
                                }
                                else
                                {
                                    cf.SpectateAsync(player.Handle);
                                }
                            }
                            // Summon player button is pressed.
                            else if (item2 == summonBtn)
                            {
                                if (player.Handle == Game.Player.Handle)
                                {
                                    Notify.Error("Sorry, you can ~r~~h~not~h~ ~s~summon yourself!", true, true);
                                }
                                else
                                {
                                    cf.SummonPlayer(player);
                                }
                            }
                            // Kill player button is pressed.
                            else if (item2 == killBtn)
                            {
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
                            else if (item2 == tempBanBtn)
                            {
                                // Close the menu.
                                PlayerMenu.GoBack();

                                // ban player
                                cf.BanPlayer(player: player, forever: false);

                                // Update the player list.
                                UpdatePlayerlist();

                                // Refresh the index & update scaleform.
                                menu.RefreshIndex();
                                menu.UpdateScaleform();
                            }
                            else if (item2 == permBanBtn)
                            {
                                // Close the menu.
                                PlayerMenu.GoBack();

                                // ban player
                                cf.BanPlayer(player: player, forever: true);

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

                        PlayerMenu.RefreshIndex();
                        PlayerMenu.UpdateScaleform();
                    }
                };
            };
            */
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
