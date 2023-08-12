using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CitizenFX.Core;

using MenuAPI;

using Newtonsoft.Json;

using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient.menus
{
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new();
        public Dictionary<int, int> PlayerCoordWaypoints = new();

        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        readonly Menu playerMenu = new("Online Players", "Player:");
        IPlayer currentPlayer = new NativePlayer(Game.Player);


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Online Players")
            {
                CounterPreText = "Players: "
            };

            MenuController.AddSubmenu(menu, playerMenu);

            var sendMessage = new MenuItem("Send Private Message", "Sends a private message to this player. ~r~Note: staff may be able to see all PM's.");
            var teleport = new MenuItem("Teleport To Player", "Teleport to this player.");
            var teleportVeh = new MenuItem("Teleport Into Player Vehicle", "Teleport into the vehicle of the player.");
            var summon = new MenuItem("Summon Player", "Teleport the player to you.");
            var toggleGPS = new MenuItem("Toggle GPS", "Enables or disables the GPS route on your radar to this player.");
            var spectate = new MenuItem("Spectate Player", "Spectate this player. Click this button again to stop spectating.");
            var printIdentifiers = new MenuItem("Print Identifiers", "This will print the player's identifiers to the client console (F8). And also save it to the CitizenFX.log file.");
            var kill = new MenuItem("~r~Kill Player", "Kill this player, note they will receive a notification saying that you killed them. It will also be logged in the Staff Actions log.");
            var kick = new MenuItem("~r~Kick Player", "Kick the player from the server.");
            var ban = new MenuItem("~r~Ban Player Permanently", "Ban this player permanently from the server. Are you sure you want to do this? You can specify the ban reason after clicking this button.");
            var tempban = new MenuItem("~r~Ban Player Temporarily", "Give this player a tempban of up to 30 days (max). You can specify duration and ban reason after clicking this button.");

            // always allowed
            playerMenu.AddMenuItem(sendMessage);
            // permissions specific
            if (IsAllowed(Permission.OPTeleport))
            {
                playerMenu.AddMenuItem(teleport);
                playerMenu.AddMenuItem(teleportVeh);
            }
            if (IsAllowed(Permission.OPSummon))
            {
                playerMenu.AddMenuItem(summon);
            }
            if (IsAllowed(Permission.OPSpectate))
            {
                playerMenu.AddMenuItem(spectate);
            }
            if (IsAllowed(Permission.OPWaypoint))
            {
                playerMenu.AddMenuItem(toggleGPS);
            }
            if (IsAllowed(Permission.OPIdentifiers))
            {
                playerMenu.AddMenuItem(printIdentifiers);
            }
            if (IsAllowed(Permission.OPKill))
            {
                playerMenu.AddMenuItem(kill);
            }
            if (IsAllowed(Permission.OPKick))
            {
                playerMenu.AddMenuItem(kick);
            }
            if (IsAllowed(Permission.OPTempBan))
            {
                playerMenu.AddMenuItem(tempban);
            }
            if (IsAllowed(Permission.OPPermBan))
            {
                playerMenu.AddMenuItem(ban);
                ban.LeftIcon = MenuItem.Icon.WARNING;
            }

            playerMenu.OnMenuClose += (sender) =>
            {
                playerMenu.RefreshIndex();
                ban.Label = "";
            };

            playerMenu.OnIndexChange += (sender, oldItem, newItem, oldIndex, newIndex) =>
            {
                ban.Label = "";
            };

            // handle button presses for the specific player's menu.
            playerMenu.OnItemSelect += async (sender, item, index) =>
            {
                // send message
                if (item == sendMessage)
                {
                    if (MainMenu.MiscSettingsMenu != null && !MainMenu.MiscSettingsMenu.MiscDisablePrivateMessages)
                    {
                        var message = await GetUserInput($"Private Message To {currentPlayer.Name}", 200);
                        if (string.IsNullOrEmpty(message))
                        {
                            Notify.Error(CommonErrors.InvalidInput);
                        }
                        else
                        {
                            TriggerServerEvent("vMenu:SendMessageToPlayer", currentPlayer.ServerId, message);
                            PrivateMessage(currentPlayer.ServerId.ToString(), message, true);
                        }
                    }
                    else
                    {
                        Notify.Error("You can't send a private message if you have private messages disabled yourself. Enable them in the Misc Settings menu and try again.");
                    }

                }
                // teleport (in vehicle) button
                else if (item == teleport || item == teleportVeh)
                {
                    if (!currentPlayer.IsLocal)
                    {
                        _ = TeleportToPlayer(currentPlayer, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
                    }
                    else
                    {
                        Notify.Error("You can not teleport to yourself!");
                    }
                }
                // summon button
                else if (item == summon)
                {
                    if (Game.Player.Handle != currentPlayer.Handle)
                    {
                        SummonPlayer(currentPlayer);
                    }
                    else
                    {
                        Notify.Error("You can't summon yourself.");
                    }
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
                    var selectedPedRouteAlreadyActive = false;
                    if (PlayersWaypointList.Count > 0)
                    {
                        if (PlayersWaypointList.Contains(currentPlayer.ServerId))
                        {
                            selectedPedRouteAlreadyActive = true;
                        }
                        foreach (var serverId in PlayersWaypointList)
                        {
                            // remove any coord blip
                            if (PlayerCoordWaypoints.TryGetValue(serverId, out var wp))
                            {
                                SetBlipRoute(wp, false);
                                RemoveBlip(ref wp);

                                PlayerCoordWaypoints.Remove(serverId);
                            }

                            // remove any entity blip
                            var playerId = GetPlayerFromServerId(serverId);

                            if (playerId < 0)
                            {
                                continue;
                            }

                            var playerPed = GetPlayerPed(playerId);
                            if (DoesEntityExist(playerPed) && DoesBlipExist(GetBlipFromEntity(playerPed)))
                            {
                                var oldBlip = GetBlipFromEntity(playerPed);
                                SetBlipRoute(oldBlip, false);
                                RemoveBlip(ref oldBlip);
                                Notify.Custom($"~g~GPS route to ~s~<C>{GetSafePlayerName(currentPlayer.Name)}</C>~g~ is now disabled.");
                            }
                        }
                        PlayersWaypointList.Clear();
                    }

                    if (!selectedPedRouteAlreadyActive)
                    {
                        if (currentPlayer.ServerId != Game.Player.ServerId)
                        {
                            int blip;

                            if (currentPlayer.IsActive && currentPlayer.Character != null)
                            {
                                var ped = GetPlayerPed(currentPlayer.Handle);
                                blip = GetBlipFromEntity(ped);
                                if (!DoesBlipExist(blip))
                                {
                                    blip = AddBlipForEntity(ped);
                                }
                            }
                            else
                            {
                                if (!PlayerCoordWaypoints.TryGetValue(currentPlayer.ServerId, out blip))
                                {
                                    var coords = await MainMenu.RequestPlayerCoordinates(currentPlayer.ServerId);
                                    blip = AddBlipForCoord(coords.X, coords.Y, coords.Z);
                                    PlayerCoordWaypoints[currentPlayer.ServerId] = blip;
                                }
                            }

                            SetBlipColour(blip, 58);
                            SetBlipRouteColour(blip, 58);
                            SetBlipRoute(blip, true);

                            PlayersWaypointList.Add(currentPlayer.ServerId);
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
                        var ids = "~s~";
                        foreach (var s in JsonConvert.DeserializeObject<string[]>(data))
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
                    {
                        KickPlayer(currentPlayer, true);
                    }
                    else
                    {
                        Notify.Error("You cannot kick yourself!");
                    }
                }
                // temp ban
                else if (item == tempban)
                {
                    BanPlayer(currentPlayer, false);
                }
                // perm ban
                else if (item == ban)
                {
                    if (ban.Label == "Are you sure?")
                    {
                        ban.Label = "";
                        _ = UpdatePlayerlist();
                        playerMenu.GoBack();
                        BanPlayer(currentPlayer, true);
                    }
                    else
                    {
                        ban.Label = "Are you sure?";
                    }
                }
            };

            // handle button presses in the player list.
            menu.OnItemSelect += (sender, item, index) =>
                {
                    var baseId = int.Parse(item.Label.Replace(" →→→", "").Replace("Server #", ""));
                    var player = MainMenu.PlayersList.FirstOrDefault(p => p.ServerId == baseId);

                    if (player != null)
                    {
                        currentPlayer = player;
                        playerMenu.MenuSubtitle = $"~s~Player: ~y~{GetSafePlayerName(currentPlayer.Name)}";
                        playerMenu.CounterPreText = $"[Server ID: ~y~{currentPlayer.ServerId}~s~] ";
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
        public async Task UpdatePlayerlist()
        {
            void UpdateStuff()
            {
                menu.ClearMenuItems();

                foreach (var p in MainMenu.PlayersList.OrderBy(a => a.Name))
                {
                    var pItem = new MenuItem($"{GetSafePlayerName(p.Name)}", $"Click to view the options for this player. Server ID: {p.ServerId}. Local ID: {p.Handle}.")
                    {
                        Label = $"Server #{p.ServerId} →→→"
                    };
                    menu.AddMenuItem(pItem);
                    MenuController.BindMenuItem(menu, playerMenu, pItem);
                }

                menu.RefreshIndex();
                //menu.UpdateScaleform();
                playerMenu.RefreshIndex();
                //playerMenu.UpdateScaleform();
            }

            // First, update *before* waiting - so we get all local players.
            UpdateStuff();
            await MainMenu.PlayersList.WaitRequested();

            // Update after waiting too so we have all remote players.
            UpdateStuff();
        }

        /// <summary>
        /// Checks if the menu exists, if not then it creates it first.
        /// Then returns the menu.
        /// </summary>
        /// <returns>The Online Players Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
                return menu;
            }
            else
            {
                _ = UpdatePlayerlist();
                return menu;
            }
        }
    }
}
