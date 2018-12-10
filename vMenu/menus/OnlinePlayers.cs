using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient
{
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new List<int>();

        // Menu variable, will be defined in CreateMenu()
        private UIMenu menu;

        UIMenu playerMenu = new UIMenu("Online Players", "Player:", RightAlignMenus()) { AlwaysShowMenuItemCounter = true };
        Player currentPlayer = new Player(Game.Player.Handle);


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new UIMenu(Game.Player.Name, "Online Players", RightAlignMenus()) { AlwaysShowMenuItemCounter = true };
            menu.CounterPretext = "Players: ";

            MainMenu.Mp.Add(playerMenu);

            UIMenuItem teleport = new UIMenuItem("Teleport To Player", "Teleport to this player.");
            UIMenuItem teleportVeh = new UIMenuItem("Teleport Into Player Vehicle", "Teleport into the vehicle of the player.");
            UIMenuItem summon = new UIMenuItem("Summon Player", "Teleport the player to you.");
            UIMenuItem toggleGPS = new UIMenuItem("Toggle GPS", "Enables or disables the GPS route on your radar to this player.");
            UIMenuItem spectate = new UIMenuItem("Spectate Player", "Spectate this player. Click this button again to stop spectating.");
            UIMenuItem printIdentifiers = new UIMenuItem("Print Identifiers", "This will print the player's identifiers to the client console (F8). And also save it to the CitizenFX.log file.");
            UIMenuItem kill = new UIMenuItem("~r~Kill Player", "Kill this player, note they will receive a notification saying that you killed them. It will also be logged in the Staff Actions log.");
            UIMenuItem kick = new UIMenuItem("~r~Kick Player", "Kick the player from the server.");
            UIMenuItem ban = new UIMenuItem("~r~Ban Player Permanently", "Ban this player permanently from the server. Are you sure you want to do this? You can specify the ban reason after clicking this button.");
            UIMenuItem tempban = new UIMenuItem("~r~Ban Player Temporarily", "Give this player a tempban of up to 30 days (max). You can specify duration and ban reason after clicking this button.");

            if (IsAllowed(Permission.OPTeleport))
            {
                playerMenu.AddItem(teleport);
                playerMenu.AddItem(teleportVeh);
            }
            if (IsAllowed(Permission.OPSummon))
            {
                playerMenu.AddItem(summon);
            }
            if (IsAllowed(Permission.OPSpectate))
            {
                playerMenu.AddItem(spectate);
            }
            if (IsAllowed(Permission.OPWaypoint))
            {
                playerMenu.AddItem(toggleGPS);
            }
            if (IsAllowed(Permission.OPIdentifiers))
            {
                playerMenu.AddItem(printIdentifiers);
            }
            if (IsAllowed(Permission.OPKill))
            {
                playerMenu.AddItem(kill);
            }
            if (IsAllowed(Permission.OPKick))
            {
                playerMenu.AddItem(kick);
            }
            if (IsAllowed(Permission.OPTempBan))
            {
                playerMenu.AddItem(tempban);
            }
            if (IsAllowed(Permission.OPPermBan))
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
                        TeleportToPlayer(currentPlayer.Handle, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
                    else
                        Notify.Error("You can not teleport to yourself!");
                }
                // summon button
                else if (item == summon)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                        SummonPlayer(currentPlayer);
                    else
                        Notify.Error("You can't summon yourself.");
                }
                // spectating
                else if (item == spectate)
                {
                    SpectatePlayer(currentPlayer);
                }
                // kill button
                else if (item == kill)
                {
                    KillPlayer(currentPlayer);
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
                                Notify.Custom($"~g~GPS route to ~s~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~ is now disabled.");
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
                            Notify.Custom($"~g~GPS route to ~s~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~ is now active, press the ~s~Toggle GPS Route~g~ button again to disable the route.");
                        }
                        else
                        {
                            Notify.Error("You can not set a waypoint to yourself.");
                        }
                    }
                }
                else if (item == printIdentifiers)
                {
                    Func<string, string> CallbackFunction = (data) =>
                    {
                        Debug.WriteLine(data);
                        string ids = "~s~";
                        foreach (string s in JsonConvert.DeserializeObject<string[]>(data))
                        {
                            ids += "~n~" + s;
                        }
                        Notify.Custom($"~y~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~'s Identifiers: {ids}", false);
                        return data;
                    };
                    BaseScript.TriggerServerEvent("vMenu:GetPlayerIdentifiers", currentPlayer.ServerId, CallbackFunction);
                }
                // kick button
                else if (item == kick)
                {
                    if (currentPlayer.Handle != Game.Player.Handle)
                        KickPlayer(currentPlayer, true);
                    else
                        Notify.Error("You cannot kick yourself!");
                }
                // temp ban
                else if (item == tempban)
                {
                    BanPlayer(currentPlayer, false);
                }
                // perm ban
                else if (item == ban)
                {
                    if (ban.RightLabel == "Are you sure?")
                    {
                        ban.SetRightLabel("");
                        UpdatePlayerlist();
                        playerMenu.GoBack();
                        BanPlayer(currentPlayer, true);
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
                        playerMenu.Subtitle.Caption = $"~s~Player: ~y~{GetSafePlayerName(currentPlayer.Name)}";
                        playerMenu.CounterPretext = $"[Server ID: ~y~{currentPlayer.ServerId}~s~] ";
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
                UIMenuItem pItem = new UIMenuItem($"{GetSafePlayerName(p.Name)}", $"Click to view the options for this player. Server ID: {p.ServerId}. Local ID: {p.Handle}.");
                pItem.SetRightLabel($"Server #{p.ServerId} →→→");
                menu.AddItem(pItem);
                menu.BindMenuToItem(playerMenu, pItem);
            }

            menu.RefreshIndex();
            menu.UpdateScaleform();
            playerMenu.RefreshIndex();
            playerMenu.UpdateScaleform();
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
