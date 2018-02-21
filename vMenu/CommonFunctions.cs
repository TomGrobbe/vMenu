using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace vMenuClient
{
    public class CommonFunctions : BaseScript
    {
        // Variables
        private Notification Notify = new Notification();

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
            if (GetEntityHealth(PlayerPedId()) < 1)
            {
                DoScreenFadeOut(50);
                await Delay(50);
                NetworkSetInSpectatorMode(true, PlayerPedId());
                NetworkSetInSpectatorMode(false, PlayerPedId());

                await Delay(50);
                DoScreenFadeIn(50);
                while (GetEntityHealth(PlayerPedId()) < 1)
                {
                    await Delay(0);
                }
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
                        Notify.Custom("Triggered 1");
                        // Get the vehicle of the specified player.
                        int vehicle = GetVehicle(player: playerId);

                        int totalVehicleSeats = GetVehicleModelNumberOfSeats(GetVehicleModel(vehicle: vehicle));

                        // Does the vehicle exist? Is it NOT dead/broken? Are there enough vehicle seats empty?
                        if (DoesEntityExist(vehicle) && !IsEntityDead(vehicle) && IsAnyVehicleSeatEmpty(vehicle))
                        {
                            TaskWarpPedIntoVehicle(PlayerPedId(), vehicle, (int)VehicleSeat.Any);
                            Notify.Success("Teleported into ~g~" + GetPlayerName(playerId) + "'s ~w~vehicle.");
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

        /// <summary>
        /// Kick player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reason"></param>
        public void KickPlayer(Player player, string reason = "You have been kicked.")
        {
            TriggerServerEvent("vMenu:KickPlayer", player.ServerId, reason);
        }

        /// <summary>
        /// Kill player
        /// </summary>
        /// <param name="player"></param>
        public void KillPlayer(Player player)
        {
            TriggerServerEvent("vMenu:KillPlayer", player.ServerId);
        }

        /// <summary>
        /// Summon player.
        /// </summary>
        /// <param name="player"></param>
        public void SummonPlayer(Player player)
        {
            TriggerServerEvent("vMenu:SummonPlayer", player.ServerId);
        }
        #endregion

        #region Spectate function
        /// <summary>
        /// Toggle spectating for the specified player Id. Leave the player ID empty (or -1) to disable spectating.
        /// </summary>
        /// <param name="playerId"></param>
        public async void SpectateAsync(int playerId = -1)
        {
            // Stop spectating.
            if (NetworkIsInSpectatorMode() || playerId == -1)
            {
                //spectating = false;
                DoScreenFadeOut(100);
                await Delay(100);
                Notify.Info("Stopped spectating.", false, false);
                NetworkSetInSpectatorMode(false, PlayerPedId());
                DoScreenFadeIn(100);
                await Delay(100);
            }
            // Start spectating for the first time.
            else
            {
                //spectating = true;
                DoScreenFadeOut(100);
                await Delay(100);
                Notify.Info("Spectating ~r~" + GetPlayerName(playerId), false, false);
                NetworkSetInSpectatorMode(true, GetPlayerPed(playerId));
                DoScreenFadeIn(100);
                await Delay(100);
            }
        }

        /// <summary>
        /// Cycle to the next available seat.
        /// </summary>
        public void CycleThroughSeats()
        {

            // Create a new vehicle.
            Vehicle vehicle = new Vehicle(GetVehicle());
            if (IsVehicleSeatFree(vehicle.Handle, 2))
            {
                RequestModel((uint)PedHash.Abigail);
                vehicle.CreatePedOnSeat(VehicleSeat.RightRear, new Model(PedHash.Abigail));
                var pos = GetEntityCoords(PlayerPedId(), true);
                var ped = CreatePed(5, (uint)PedHash.Abigail, pos.X, pos.Y, pos.Z + 2f, 0f, true, false);
                TaskWarpPedIntoVehicle(ped, vehicle.Handle, 2);
            }


            // If there are enough empty seats, continue.
            if (AreAnyVehicleSeatsFree(vehicle.Handle))
            {
                // Get the total seats for this vehicle.
                var maxSeats = GetVehicleModelNumberOfSeats((uint)GetEntityModel(vehicle.Handle));

                // If the player is currently in the "last" seat, start from the driver's position and loop through the seats.
                if (GetPedInVehicleSeat(vehicle.Handle, maxSeats - 2) == PlayerPedId())
                {
                    // Loop through all seats.
                    for (var seat = -1; seat < maxSeats - 2; seat++)
                    {
                        // If the seat is free, get in it and stop the loop.
                        if (vehicle.IsSeatFree((VehicleSeat)seat))
                        {
                            TaskWarpPedIntoVehicle(PlayerPedId(), vehicle.Handle, seat);
                            break;
                        }
                    }
                }
                // If the player is not in the "last" seat, loop through all the seats starrting from the driver's position.
                else
                {
                    var switchedPlace = false;
                    var passedCurrentSeat = false;
                    // Loop through all the seats.
                    for (var seat = -1; seat < maxSeats - 1; seat++)
                    {
                        // If this seat is the one the player is sitting on, set passedCurrentSeat to true.
                        // This way we won't just keep placing the ped in the 1st available seat, but actually the first "next" available seat.
                        if (!passedCurrentSeat && GetPedInVehicleSeat(vehicle.Handle, seat) == PlayerPedId())
                        {
                            passedCurrentSeat = true;
                        }

                        // Only if the current seat has been passed, check if the seat is empty and if so teleport into it and stop the loop.
                        if (passedCurrentSeat && IsVehicleSeatFree(vehicle.Handle, seat))
                        {
                            switchedPlace = true;
                            TaskWarpPedIntoVehicle(PlayerPedId(), vehicle.Handle, seat);
                            break;
                        }
                    }
                    // If the player was not switched, then that means there are not enough empty vehicle seats "after" the player, and the player was not sitting in the "last" seat.
                    // To fix this, loop through the entire vehicle again and place them in the first available seat.
                    if (!switchedPlace)
                    {
                        // Loop through all seats, starting at the drivers seat (-1), then moving up.
                        for (var seat = -1; seat < maxSeats - 1; seat++)
                        {
                            // If the seat is free, take it and break the loop.
                            if (IsVehicleSeatFree(vehicle.Handle, seat))
                            {
                                TaskWarpPedIntoVehicle(PlayerPedId(), vehicle.Handle, seat);
                                break;
                            }
                        }
                    }
                }


            }
            else
            {
                Notify.Alert("There are no more available seats to cycle through.");
            }

        }
        #endregion
    }



}
