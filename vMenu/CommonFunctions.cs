using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    class CommonFunctions : BaseScript
    {
        // Variables
        private Notification Notify = new Notification();
        private int spectatePlayer = -1;
        private bool spectating = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CommonFunctions()
        {
            Tick += OnTick;
        }

        #region OnTick for spectate handling
        /// <summary>
        /// OnTick runs every game tick.
        /// Used here for the spectating feature.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // If the player is not spectating yet, but "spectating" is true, enable it.
            if (spectatePlayer != -1 && spectating && NetworkIsPlayerActive(spectatePlayer))
            {
                DoScreenFadeOut(200);
                await Delay(200);
                NetworkSetInSpectatorMode(true, GetPlayerPed(spectatePlayer));
                DoScreenFadeIn(200);
                await Delay(200);
                spectating = true;


                // Wait until spectating is cancelled.
                // Either by the user itself, or if the other player disconencts, or if the current player dies.
                while (spectating && spectatePlayer != -1 && NetworkIsPlayerActive(spectatePlayer) && !IsPlayerDead(PlayerId()))
                {
                    await Delay(0);
                }
                DoScreenFadeOut(200);
                await Delay(200);
                NetworkSetInSpectatorMode(false, PlayerPedId());
                DoScreenFadeIn(200);
                await Delay(200);
            }
        }
        #endregion

        #region GetVehicle from specified player id (if not specified, return the vehicle of the current player)
        /// <summary>
        /// Get the vehicle from the specified player. If no player specified, then return the vehicle of the current player.
        /// Optionally specify <see cref="=bool last"/> to return the last vehicle the player was in.
        /// </summary>
        /// <param name="ped">Get the vehicle for this player.</param>
        /// <param name="last">If true, return the last vehicle, if false (default) return the current vehicle.</param>
        /// <returns>Returns a vehicle (int).</returns>
        public int GetVehicle(int player = -1, bool last = false)
        {
            if (player == -1)
            {
                return GetVehiclePedIsIn(PlayerPedId(), last);
            }
            return GetVehiclePedIsIn(GetPlayerPed(player), last);
        }
        #endregion
        #region GetVehicleModel (uint)(hash) from Entity/Vehicle (int)
        /// <summary>
        /// Get the vehicle model hash (as uint) from the specified (int) entity/vehicle.
        /// </summary>
        /// <param name="vehicle">Entity/vehicle.</param>
        /// <returns>Returns the (uint) model hash from a (vehicle) entity.</returns>
        public uint GetVehicleModel(int vehicle)
        {
            return (uint)GetHashKey(GetEntityModel(vehicle).ToString());
        }
        #endregion
        #region Teleport to player (or teleport into the player's vehicle)
        /// <summary>
        /// Teleport to the specified player id. (Optionally teleport into their vehicle).
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inVehicle"></param>
        public async void TeleportToPlayerAsync(int playerId, bool inVehicle = false)
        {
            // If the player exists.
            if (NetworkIsPlayerActive(playerId))
            {
                int playerPed = GetPlayerPed(playerId);
                if (PlayerPedId() == playerPed)
                {
                    Notify.Alert("Sorry, you can ~h~not ~w~teleport to yourself!");
                    return;
                }

                // Get the coords of the other player.
                Vector3 playerPos = GetEntityCoords(playerPed, true);
                // Teleport to the other player (2.0 meters above the other player).
                SetPedCoordsKeepVehicle(PlayerPedId(), playerPos.X, playerPos.Y, playerPos.Z + 2.0f);

                // If the player should be teleported inside the other player's vehcile.
                if (inVehicle)
                {
                    // Allow the world to load around the player first.
                    await Delay(5);

                    // Is the other player inside a vehicle?
                    if (IsPedInAnyVehicle(playerPed, false))
                    {
                        // Get the vehicle of the specified player.
                        int vehicle = GetVehicle(player: playerId);

                        int totalVehicleSeats = GetVehicleModelNumberOfSeats(GetVehicleModel(vehicle: vehicle));

                        // Does the vehicle exist? Is it NOT dead/broken? Are there enough vehicle seats empty?
                        if (DoesEntityExist(vehicle) && !IsEntityDead(vehicle) && IsAnyVehicleSeatEmpty(vehicle))
                        {
                            // Loop through all the vehicle seats to find an empty seat.
                            for (var seat = 0; seat < totalVehicleSeats; seat++)
                            {
                                // If this vehicle seat is free, set the ped into it.
                                if (IsVehicleSeatFree(vehicle, seat))
                                {
                                    TaskWarpPedIntoVehicle(PlayerPedId(), vehicle, seat);
                                    Notify.Success("Teleported into ~g~" + GetPlayerName(playerId) + "'s ~w~vehicle.");
                                    break; // Stop the loop.
                                }
                            }
                        }
                        // If there are not enough empty vehicle seats or the vehicle doesn't exist/is dead then notify the user.
                        else
                        {
                            // If there's only one seat on this vehicle, tell them that it's a one-seater.
                            if (totalVehicleSeats == 1)
                            {
                                Notify.Alert("This vehicle only has room for 1 player!");
                            }
                            // Otherwise, tell them there's not enough empty seats remaining.
                            else
                            {
                                Notify.Alert("Not enough empty vehicle seats remaining!");
                            }
                        }
                    }
                }
                // The player is not being teleported into the vehicle, so the teleporting is successfull.
                // Notify the user.
                else
                {
                    Notify.Success("Teleported to " + GetPlayerName(playerId) + ".");
                }
            }
            // The specified playerId does not exist, notify the user of the error.
            else
            {
                Notify.Error("This player does not exist so the teleport has been cancelled.");
                return;
            }
        }

        public void KillPlayer(Player player)
        {
            TriggerServerEvent("vMenu:KillPlayer", player.Handle);
        }

        public void SummonPlayer(Player player)
        {
            TriggerServerEvent("vMenu:SummonPlayer", player.Handle);
        }
        #endregion

        #region Spectate function
        /// <summary>
        /// Toggle spectating for the specified player Id. Leave the player ID empty (or -1) to disable spectating.
        /// </summary>
        /// <param name="playerId"></param>
        public void Spectate(int playerId = -1)
        {
            if (spectating || playerId == -1)
            {
                spectating = false;
                spectatePlayer = -1;
                Notify.Info("Stopped spectating.", false, false);
            }
            else if (spectating && playerId != -1 && NetworkIsPlayerActive(playerId))
            {
                spectating = false;
                spectatePlayer = -1;
                Notify.Info("Switching to player " + GetPlayerName(playerId), false, false);
                spectating = true;
                spectatePlayer = playerId;
            }
            else
            {
                spectatePlayer = playerId;
                spectating = true;
                Notify.Info("Currently spectating " + GetPlayerName(playerId) + ".", false, false);
            }
        }
        #endregion
    }



}
