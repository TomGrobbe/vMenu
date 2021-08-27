using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using static vMenuShared.PermissionsManager;

namespace vMenuClient
{
    public class OnlinePlayers
    {
        public List<int> PlayersWaypointList = new List<int>();

        // Menu variable, will be defined in CreateMenu()
        private Menu menu;

        Menu playerMenu = new Menu("Online Players", "Player:");
        IPlayer currentPlayer = new NativePlayer(Game.Player);
        private string currentPlayerName;

        private bool cancelUpdatePlayerTimer = false;


        /// <summary>
        /// Creates the menu.
        /// </summary>
        private void CreateMenu()
        { 
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Online Players") { };
            menu.CounterPreText = "Players: ";

            MenuController.AddSubmenu(menu, playerMenu);

            MenuItem sendMessage = new MenuItem("Send Private Message", "Sends a private message to this player. ~r~Note: staff may be able to see all PM's.");
            MenuItem teleport = new MenuItem("Teleport To Player", "Teleport to this player.");
            MenuItem teleportVeh = new MenuItem("Teleport Into Player Vehicle", "Teleport into the vehicle of the player.");
            MenuItem summon = new MenuItem("Summon Player", "Teleport the player to you.");
            MenuItem toggleGPS = new MenuItem("Toggle GPS", "Enables or disables the GPS route on your radar to this player. ~y~Note for when the server is using OneSync Infinity: this may not work if the player is too far away.");
            MenuItem spectate = new MenuItem("Spectate Player", "Spectate this player. Click this button again to stop spectating. ~y~Note for when the server is using OneSync Infinity: You will be teleported to the player if you're too far away, you might want to go into noclip to become invisible, before using this option!");
            MenuItem printIdentifiers = new MenuItem("Print Identifiers", "This will print the player's identifiers to the client console (F8). And also save it to the CitizenFX.log file.");
            MenuItem kill = new MenuItem("~r~Kill Player", "Kill this player, note they will receive a notification saying that you killed them. It will also be logged in the Staff Actions log.");
            MenuItem kick = new MenuItem("~r~Kick Player", "Kick the player from the server.");
            MenuItem ban = new MenuItem("~r~Ban Player Permanently", "Ban this player permanently from the server. Are you sure you want to do this? You can specify the ban reason after clicking this button.");
            MenuItem tempban = new MenuItem("~r~Ban Player Temporarily", "Give this player a tempban of up to 30 days (max). You can specify duration and ban reason after clicking this button.");

            menu.OnMenuClose += async (sender) =>
            {
                await BaseScript.Delay(100);
                var currentMenu = MenuController.GetCurrentMenu();
                
                if (currentMenu != null && currentMenu.MenuSubtitle == "Main Menu")
                {
                    StopUpdatePlayerTimer();
                }
            };

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

                currentPlayer = null;
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
                        string message = await GetUserInput($"Private Message To {currentPlayer.Name}", 200);
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
                        _ = TeleportToPlayer(currentPlayer, item == teleportVeh); // teleport to the player. optionally in the player's vehicle if that button was pressed.
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
                        currentPlayerName = player.Name;
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
        public async Task UpdatePlayerlist(bool fromTimer = false)
        {
            void UpdateStuff()
            {
                var oldIndex = menu.CurrentIndex;
                
                menu.ClearMenuItems(fromTimer);

                var foundCurrentPlayer = false;

                foreach (IPlayer p in MainMenu.PlayersList.OrderBy(a => a.Name))
                {
                    MenuItem pItem = new MenuItem($"{GetSafePlayerName(p.Name)}", $"Click to view the options for this player. Server ID: {p.ServerId}. Local ID: {p.Handle}.")
                    {
                        Label = $"Server #{p.ServerId} →→→"
                    };
                    menu.AddMenuItem(pItem);
                    MenuController.BindMenuItem(menu, playerMenu, pItem);

                    if (currentPlayer != null && p.ServerId == currentPlayer.ServerId)
                    {
                        foundCurrentPlayer = true;
                    }
                }

                if (currentPlayer != null && !foundCurrentPlayer)
                {
                    Notify.Info($"Player: {currentPlayerName} has left the server.");
                    
                    playerMenu.GoBack();
                    
                    menu.RefreshIndex();
                }

                // We don't want to call RefreshIndex if calling from the timer, as we're on the list currently
                if (fromTimer)
                {
                    if (oldIndex > MainMenu.PlayersList.Count() - 1)
                    {
                        menu.RefreshIndex(MainMenu.PlayersList.Count() - 1);
                    }
                    
                    return;
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

        /// <summary>
        /// Update player list while it's open every 10 seconds
        /// We don't want this to happen onTick because that's excessive, but it's not
        /// abnormal for a staff member to have Online Players open for long periods of time.
        /// </summary>
        /// <returns>void</returns>
        public async Task UpdatePlayerListTimer()
        {
            while (true)
            {
                // Wait 10 seconds
                await BaseScript.Delay(10000);

                if (cancelUpdatePlayerTimer)
                {
                    cancelUpdatePlayerTimer = false;
                    return;
                }
                
                // Request players from server
                MainMenu.PlayersList.RequestPlayerList();
                
                await UpdatePlayerlist(true);
            }
        }

        /// <summary>
        /// Set cancelUpdatePlayerTimer so that we stop the timer if we're not on the OnlinePlayers menu.
        /// </summary>
        /// <returns>OnlinePlayers</returns>
        public OnlinePlayers StopUpdatePlayerTimer()
        {
            cancelUpdatePlayerTimer = true;

            return this;
        }
    }
}
