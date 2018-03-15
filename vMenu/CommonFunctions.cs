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
    public class CommonFunctions : BaseScript
    {
        #region Variables
        // Variables
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private string currentScenario = "";
        private Vehicle previousVehicle;
        private StorageManager sm = new StorageManager();
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public CommonFunctions() { }

        #region Get Localized Label Text
        /// <summary>
        /// Get the localized name from a text label (for classes that don't have BaseScript)
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string GetLocalizedName(string label)
        {
            return GetLabelText(label);
        }
        #endregion

        #region Get Localized Vehicle Display Name
        /// <summary>
        /// Get the localized model name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetVehDisplayNameFromModel(string name)
        {
            return GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(name)));
        }
        #endregion

        #region GetHashKey for other classes
        /// <summary>
        /// Get the hash key for the specified string.
        /// </summary>
        /// <param name="input">String to convert into a hash.</param>
        /// <returns>The has value of the input string.</returns>
        public uint GetHash(string input)
        {
            return (uint)GetHashKey(input);
        }
        #endregion

        #region DoesModelExist
        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelName">The model name</param>
        /// <returns></returns>
        public bool DoesModelExist(string modelName)
        {
            return IsModelInCdimage((uint)GetHashKey(modelName));
        }

        /// <summary>
        /// Does this model exist?
        /// </summary>
        /// <param name="modelHash">The model hash</param>
        /// <returns></returns>
        public bool DoesModelExist(uint modelHash)
        {
            return IsModelInCdimage(modelHash);
        }
        #endregion

        #region GetVehicle from specified player id (if not specified, return the vehicle of the current player)
        /// <summary>
        /// Get the vehicle from the specified player. If no player specified, then return the vehicle of the current player.
        /// Optionally specify the 'lastVehicle' (bool) parameter to return the last vehicle the player was in.
        /// </summary>
        /// <param name="ped">Get the vehicle for this player.</param>
        /// <param name="lastVehicle">If true, return the last vehicle, if false (default) return the current vehicle.</param>
        /// <returns>Returns a vehicle (int).</returns>
        public int GetVehicle(int player = -1, bool lastVehicle = false)
        {
            if (player == -1)
            {
                return GetVehiclePedIsIn(PlayerPedId(), lastVehicle);
            }
            return GetVehiclePedIsIn(GetPlayerPed(player), lastVehicle);
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

        #region Drive Tasks (Unimplemented)
        /// <summary>
        /// Todo
        /// </summary>
        public void DriveToWp()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Todo
        /// </summary>
        public void DriveWander()
        {
            throw new NotImplementedException();
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
                    Notify.Error("Sorry, you can ~r~~h~not~h~ ~s~teleport to yourself!");
                    return;
                }

                // Get the coords of the other player.
                Vector3 playerPos = GetEntityCoords(playerPed, true);

                // Then await the proper loading/teleporting.
                await TeleportToCoords(playerPos);

                // If the player should be teleported inside the other player's vehcile.
                if (inVehicle)
                {
                    // Is the other player inside a vehicle?
                    if (IsPedInAnyVehicle(playerPed, false))
                    {
                        // Get the vehicle of the specified player.
                        int vehicle = GetVehicle(player: playerId);

                        int totalVehicleSeats = GetVehicleModelNumberOfSeats(GetVehicleModel(vehicle: vehicle));

                        // Does the vehicle exist? Is it NOT dead/broken? Are there enough vehicle seats empty?
                        if (DoesEntityExist(vehicle) && !IsEntityDead(vehicle) && IsAnyVehicleSeatEmpty(vehicle))
                        {
                            TaskWarpPedIntoVehicle(PlayerPedId(), vehicle, (int)VehicleSeat.Any);
                            Notify.Success("Teleported into ~g~<C>" + GetPlayerName(playerId) + "</C>'s ~s~vehicle.");
                        }
                        // If there are not enough empty vehicle seats or the vehicle doesn't exist/is dead then notify the user.
                        else
                        {
                            // If there's only one seat on this vehicle, tell them that it's a one-seater.
                            if (totalVehicleSeats == 1)
                            {
                                Notify.Error("This vehicle only has room for 1 player!");
                            }
                            // Otherwise, tell them there's not enough empty seats remaining.
                            else
                            {
                                Notify.Error("Not enough empty vehicle seats remaining!");
                            }
                        }
                    }
                }
                // The player is not being teleported into the vehicle, so the teleporting is successfull.
                // Notify the user.
                else
                {
                    Notify.Success("Teleported to ~y~<C>" + GetPlayerName(playerId) + "</C>~s~.");
                }
            }
            // The specified playerId does not exist, notify the user of the error.
            else
            {
                Notify.Error(CommonErrors.PlayerNotFound, placeholderValue: "So the teleport has been cancelled.");
                //Notify.Error("This player does not exist so the teleport has been cancelled.");
                return;
            }
        }
        #endregion

        #region Teleport To Coords
        /// <summary>
        /// Teleport the player to a specific location.
        /// </summary>
        /// <param name="pos">These are the target coordinates to teleport to.</param>
        public async Task TeleportToCoords(Vector3 pos)
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false) && GetPedInVehicleSeat(GetVehicle(), -1) == PlayerPedId())
            {
                SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, pos.Z);
            }
            else
            {
                SetEntityCoords(PlayerPedId(), pos.X, pos.Y, pos.Z, false, false, false, true);
            }

            var timer = 0;
            var failed = false;
            while (!GetGroundZFor_3dCoord(pos.X, pos.Y, 800f, ref pos.Z, true))
            {
                await Delay(0);
                timer++;
                if (timer > 60)
                {
                    failed = true;
                    break;
                }
            }
            if (IsEntityInWater(PlayerPedId()) || IsEntityInAir(PlayerPedId()))
            {
                failed = true;
            }
            if (failed)
            {
                GiveWeaponToPed(PlayerPedId(), (uint)WeaponHash.Parachute, 1, false, true);
                var safePos = pos;
                safePos.Z = 810f;
                var foundSafeSpot = GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, 0, ref safePos, 0, 0, 0);
                if (foundSafeSpot)
                {
                    Notify.Alert("No safe location near waypoint :( going to closest safe location instead.");
                    if (IsPedInAnyVehicle(PlayerPedId(), false) && GetPedInVehicleSeat(GetVehicle(), -1) == PlayerPedId())
                    {
                        SetPedCoordsKeepVehicle(PlayerPedId(), safePos.X, safePos.Y, safePos.Z);
                    }
                    else
                    {
                        SetEntityCoords(PlayerPedId(), safePos.X, safePos.Y, safePos.Z, false, false, false, true);
                    }
                }
                else
                {
                    Notify.Alert("No safe location near you :( open your parachute!");
                    if (IsPedInAnyVehicle(PlayerPedId(), false) && GetPedInVehicleSeat(GetVehicle(), -1) == PlayerPedId())
                    {
                        SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, 810f);
                    }
                    else
                    {
                        SetEntityCoords(PlayerPedId(), pos.X, pos.Y, 810f, false, false, false, true);
                    }
                }
            }
            else
            {
                if (IsPedInAnyVehicle(PlayerPedId(), false) && GetPedInVehicleSeat(GetVehicle(), -1) == PlayerPedId())
                {
                    SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, pos.Z + 2f);
                }
                else
                {
                    SetEntityCoords(PlayerPedId(), pos.X, pos.Y, pos.Z + 2f, false, false, false, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async void TeleportToWp()
        {
            if (Game.IsWaypointActive)
            {
                var pos = World.WaypointPosition;
                pos.Z = 200f;
                await TeleportToCoords(pos);
            }
        }
        #endregion

        #region Kick Player
        /// <summary>
        /// Kick a user with the provided kick reason. Or ask the current player to provide a reason.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="askUserForReason"></param>
        /// <param name="reason"></param>
        public async void KickPlayer(Player player, bool askUserForReason, string providedReason = "You have been kicked.")
        {
            // Default kick reason.
            var defaultReason = "You have been kicked.";
            var cancel = false;
            // If we need to ask for the user's input and the default reason is the same as the provided reason, get the user input..
            if (askUserForReason && providedReason == defaultReason)
            {
                var userInput = await GetUserInput("Enter Kick Message", "", 100);
                // If the input is not invalid, set the kick reason to the user's custom message.
                if (userInput != "NULL")
                {
                    defaultReason += $" Reason: {userInput}";
                }
                else
                {
                    cancel = true;
                    return;
                }
            }
            // If the provided reason is not the same as the default reason, set the kick reason to the provided reason.
            else if (providedReason != defaultReason)
            {
                defaultReason = providedReason;
            }
            // Otherwise, don't change anything.


            // Kick the player using the specified reason.
            if (!cancel)
            {
                TriggerServerEvent("vMenu:KickPlayer", player.ServerId, defaultReason);
            }

        }
        #endregion

        #region Kill Player
        /// <summary>
        /// Kill player
        /// </summary>
        /// <param name="player"></param>
        public void KillPlayer(Player player)
        {
            TriggerServerEvent("vMenu:KillPlayer", player.ServerId);
        }
        #endregion

        #region Summon Player
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
                Notify.Info($"Spectating ~r~{GetPlayerName(playerId)}</C>~s~.", false, false);
                NetworkSetInSpectatorMode(true, GetPlayerPed(playerId));
                DoScreenFadeIn(100);
                await Delay(100);
            }
        }
        #endregion

        #region Cycle Through Vehicle Seats
        /// <summary>
        /// Cycle to the next available seat.
        /// </summary>
        public void CycleThroughSeats()
        {

            // Create a new vehicle.
            Vehicle vehicle = new Vehicle(GetVehicle());

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

        #region Spawn Vehicle
        #region Overload Spawn Vehicle Function
        /// <summary>
        /// Spawn a vehicle by providing a vehicle name.
        /// If no name is specified or "custom" was passed, the user will be asked to input the vehicle name to spawn.
        /// </summary>
        /// <param name="vehicleName">The name of the vehicle to spawn.</param>
        public async void SpawnVehicle(string vehicleName = "custom", bool spawnInside = false, bool replacePrevious = false)
        {
            if (vehicleName == "custom")
            {
                // Get the result.
                string result = await GetUserInput("Enter Vehicle Name", "Adder");
                // If the result was not invalid.
                if (result != "NULL")
                {
                    // Convert it into a model hash.
                    uint model = (uint)GetHashKey(result);
                    int modelClass = GetVehicleClassFromName(model);
                    //var tmpMenu = new VehicleSpawner()
                    if (MainMenu.VehicleSpawnerMenu.allowedCategories[modelClass])
                    {
                        SpawnVehicle(vehicleHash: model, spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false);
                    }
                    else
                    {
                        Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
                    }
                }
                // Result was invalid.
                else
                {
                    Notify.Error(CommonErrors.InvalidInput);
                }
            }
            // Spawn the specified vehicle.
            else
            {
                SpawnVehicle(vehicleHash: (uint)GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false);
            }
        }
        #endregion

        #region Main Spawn Vehicle Function
        /// <summary>
        /// Spawn a vehicle by providing the vehicle hash.
        /// </summary>
        /// <param name="vehicleHash">Hash of the vehicle model to spawn.</param>
        /// <param name="skipLoad">If true, this will not load or verify the model, it will instantly spawn the vehicle.</param>
        public async void SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious, bool skipLoad = false, Dictionary<string, string> vehicleInfo = null)
        {

            if (!skipLoad)
            {
                bool successFull = await LoadModel(vehicleHash);
                if (!successFull || !IsModelAVehicle(vehicleHash))
                {
                    // Vehicle model is invalid.
                    Notify.Error(CommonErrors.InvalidModel);
                    return;
                }
            }
            if (MainMenu.DebugMode)
            {
                Log("Spawning of vehicle is NOT cancelled, if this model is invalid then there's something wrong.");
            }

            // Get the heading & position for where the vehicle should be spawned.
            Vector3 pos = (spawnInside) ? GetEntityCoords(PlayerPedId(), true) : GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0f, 8f, 0f);
            float heading = GetEntityHeading(PlayerPedId()) + (spawnInside ? 0f : 90f);

            // If the previous vehicle exists...
            if (previousVehicle != null)
            {
                ClearPedTasksImmediately(PlayerPedId());
                // And it's actually a vehicle (rather than another random entity type)
                //if (IsEntityAVehicle(previousVehicle) && IsVehiclePreviouslyOwnedByPlayer(previousVehicle))
                if (previousVehicle.Exists() && previousVehicle.PreviouslyOwnedByPlayer &&
                    (previousVehicle.Occupants.Count() == 0 || previousVehicle.Driver.Handle == PlayerPedId()))
                {
                    // If the previous vehicle should be deleted:
                    if (replacePrevious)
                    {
                        // Delete it.
                        previousVehicle.PreviouslyOwnedByPlayer = false;
                        previousVehicle.Delete();
                    }
                    // Otherwise
                    else
                    {
                        // Set the vehicle to be no longer needed. This will make the game engine decide when it should be removed (when all players get too far away).
                        previousVehicle.PreviouslyOwnedByPlayer = false;
                        previousVehicle.MarkAsNoLongerNeeded();
                    }
                    previousVehicle = null;
                }
                else if (IsPedInAnyVehicle(PlayerPedId(), false))
                {
                    if (GetPedInVehicleSeat(GetVehicle(), -1) == PlayerPedId() && IsVehiclePreviouslyOwnedByPlayer(GetVehicle()))
                    {
                        int tmpveh = GetVehicle();
                        SetVehicleHasBeenOwnedByPlayer(tmpveh, false);
                        SetEntityAsMissionEntity(tmpveh, true, true);
                        DeleteVehicle(ref tmpveh);
                    }
                }
            }
            else if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                if (GetPedInVehicleSeat(GetVehicle(), -1) == PlayerPedId() && IsVehiclePreviouslyOwnedByPlayer(GetVehicle()))
                {
                    int tmpveh = GetVehicle();
                    SetVehicleHasBeenOwnedByPlayer(tmpveh, false);
                    SetEntityAsMissionEntity(tmpveh, true, true);
                    DeleteVehicle(ref tmpveh);
                }
            }

            // Create the new vehicle and remove the need to hotwire the car.
            Vehicle vehicle = new Vehicle(CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z + 1f, heading, true, false))
            {
                NeedsToBeHotwired = false,
                PreviouslyOwnedByPlayer = true,
                IsPersistent = true
            };

            // If spawnInside is true
            if (spawnInside)
            {
                // Set the vehicle's engine to be running.
                vehicle.IsEngineRunning = true;

                // Set the ped into the vehicle.
                new Ped(PlayerPedId()).SetIntoVehicle(vehicle, VehicleSeat.Driver);

                // If the vehicle is a helicopter and the player is in the air, set the blades to be full speed.
                if (vehicle.ClassType == VehicleClass.Helicopters && GetEntityHeightAboveGround(PlayerPedId()) > 10.0f)
                {
                    SetHeliBladesFullSpeed(vehicle.Handle);
                }
                // If it's not a helicopter or the player is not in the air, set the vehicle on the ground properly.
                else
                {
                    //SetVehicleOnGroundProperly(vehicle.Handle);
                    vehicle.PlaceOnGround();
                }
            }

            // If mod info about the vehicle was specified, check if it's not null.
            if (vehicleInfo != null)
            {
                // Set the modkit so we can modify the car.
                SetVehicleModKit(vehicle.Handle, 0);

                // Loop through all extra ID's and set it if it was specified in the dictionary mods.
                for (var extraId = 0; extraId < 15; extraId++)
                {
                    if (DoesExtraExist(vehicle.Handle, extraId))
                    {
                        vehicle.ToggleExtra(extraId, (vehicleInfo[$"extra{extraId.ToString()}"] == "true"));
                    }
                }

                // Set all other Toggles.
                int wheelType = int.Parse(vehicleInfo["wheelType"]);
                SetVehicleWheelType(vehicle.Handle, wheelType);
                bool customWheels = vehicleInfo["customWheels"] == "true";
                SetVehicleMod(vehicle.Handle, 23, 0, customWheels);
                if (vehicle.Model.IsBike)
                {
                    SetVehicleMod(vehicle.Handle, 24, 0, customWheels);
                }
                bool turbo = vehicleInfo["turbo"] == "true";
                ToggleVehicleMod(vehicle.Handle, 18, turbo);

                bool tireSmokeEnabled = vehicleInfo["tireSmoke"] == "true";
                int tireR = int.Parse(vehicleInfo["tireSmokeR"]);
                int tireG = int.Parse(vehicleInfo["tireSmokeG"]);
                int tireB = int.Parse(vehicleInfo["tireSmokeB"]);
                SetVehicleTyreSmokeColor(vehicle.Handle, tireR, tireG, tireB);
                ToggleVehicleMod(vehicle.Handle, 20, tireSmokeEnabled);

                bool xenonHeadlights = vehicleInfo["xenonHeadlights"] == "true";
                ToggleVehicleMod(vehicle.Handle, 22, xenonHeadlights);

                int oldLivery = int.Parse(vehicleInfo["oldLivery"]);
                SetVehicleLivery(vehicle.Handle, oldLivery);

                int primaryColor = int.Parse(vehicleInfo["primaryColor"].ToString());
                int secondaryColor = int.Parse(vehicleInfo["secondaryColor"].ToString());
                int pearlescentColor = int.Parse(vehicleInfo["pearlescentColor"].ToString());
                int wheelColor = int.Parse(vehicleInfo["wheelsColor"].ToString());

                int.TryParse(vehicleInfo["interiorColor"], out int interiorcolor);
                int.TryParse(vehicleInfo["dashboardColor"], out int dashboardcolor);

                SetVehicleInteriorColour(vehicle.Handle, interiorcolor);
                SetVehicleDashboardColour(vehicle.Handle, dashboardcolor);

                SetVehicleNumberPlateText(vehicle.Handle, vehicleInfo["plate"]);
                SetVehicleNumberPlateTextIndex(vehicle.Handle, int.Parse(vehicleInfo["plateStyle"]));

                // Declare a variable to indicate how many "iterations" the foreach loop further down should be "skipped",
                // before starting "consider" the "mod strings" as "dynamic" mods.
                int skip = 8 + 24 + 2;

                bool updateSavedVehicleInfo = false;
                if (vehicleInfo.ContainsKey("windowTint"))
                {
                    int.TryParse(vehicleInfo["windowTint"].ToString(), out int tint);
                    SetVehicleWindowTint(vehicle.Handle, tint);
                    skip++; // add one to skip because the tint was found.
                }
                else
                {
                    updateSavedVehicleInfo = true;
                }

                // Loop through all mods.
                foreach (var mod in vehicleInfo)
                {
                    // Ignore the first "skip" amount of mods because those are not dynamic mods.
                    skip--;
                    if (skip < 0)
                    {
                        var key = int.Parse(mod.Key);
                        var val = int.Parse(mod.Value);
                        SetVehicleMod(vehicle.Handle, key, val, customWheels);
                    }
                }

                // Set the vehicle colours last to make sure they don't get overridden by applying vehicle mods.
                SetVehicleColours(vehicle.Handle, primaryColor, secondaryColor);
                SetVehicleExtraColours(vehicle.Handle, pearlescentColor, wheelColor);

                if (updateSavedVehicleInfo)
                {
                    // Because we need to re-save the vehicle with the new modded format, we'll teleport the player into it.
                    TaskWarpPedIntoVehicle(PlayerPedId(), vehicle.Handle, -1);
                    SaveVehicle(vehicleInfo["name"].ToString());
                }
            }

            // Set the previous vehicle to the new vehicle.
            previousVehicle = vehicle;

            // Discard the model.
            SetModelAsNoLongerNeeded(vehicleHash);
        }
        #endregion
        #endregion

        #region Save Vehicle
        /// <summary>
        /// Saves the vehicle the player is currently in to the client's kvp storage.
        /// </summary>
        public async void SaveVehicle(string updateExistingSavedVehicleName = null)
        {
            // Only continue if the player is in a vehicle.
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // Make sure the entity is actually a vehicle and it still exists, and it's not dead.
                if (DoesEntityExist(veh) && (IsEntityAVehicle(veh)) && !IsEntityDead(veh))
                {
                    // Get some info about the car.
                    var model = (uint)GetEntityModel(veh);
                    var name = GetLabelText(GetDisplayNameFromVehicleModel(model));

                    int primaryColor = 0;
                    int secondaryColor = 0;
                    int pearlescentColor = 0;
                    int wheelColor = 0;
                    GetVehicleExtraColours(veh, ref pearlescentColor, ref wheelColor);
                    GetVehicleColours(veh, ref primaryColor, ref secondaryColor);

                    // Store all info into a Dictionary.
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("name", name != "NULL" ? name : "N/A");
                    dict.Add("model", model.ToString() ?? "");

                    // Add all vehicle extras.
                    for (var extraId = 0; extraId < 15; extraId++)
                    {
                        if (DoesExtraExist(veh, extraId))
                        {
                            if (IsVehicleExtraTurnedOn(veh, extraId))
                            {
                                dict.Add($"extra{extraId.ToString()}", "true");
                            }
                            else
                            {
                                dict.Add($"extra{extraId.ToString()}", "false");
                            }
                        }
                        else
                        {
                            dict.Add($"extra{extraId.ToString()}", "false");
                        }
                    }

                    // Add more stuff to the db.
                    dict.Add("customWheels", GetVehicleModVariation(veh, 23) ? "true" : "false");
                    dict.Add("wheelType", GetVehicleWheelType(veh).ToString());
                    dict.Add("turbo", IsToggleModOn(veh, 18) ? "true" : "false");

                    dict.Add("tireSmoke", IsToggleModOn(veh, 20) ? "true" : "false");
                    var tireR = 255;
                    var tireG = 255;
                    var tireB = 255;
                    GetVehicleTyreSmokeColor(veh, ref tireR, ref tireG, ref tireB);

                    dict.Add("tireSmokeR", tireR.ToString());
                    dict.Add("tireSmokeG", tireG.ToString());
                    dict.Add("tireSmokeB", tireB.ToString());

                    dict.Add("xenonHeadlights", IsToggleModOn(veh, 22) ? "true" : "false");
                    dict.Add("oldLivery", GetVehicleLivery(veh).ToString());

                    dict.Add("primaryColor", primaryColor.ToString());
                    dict.Add("secondaryColor", secondaryColor.ToString());
                    dict.Add("wheelsColor", wheelColor.ToString());
                    dict.Add("pearlescentColor", pearlescentColor.ToString());
                    var interiorColor = 0;
                    var dashboardColor = 0;
                    GetVehicleInteriorColour(veh, ref interiorColor);
                    GetVehicleDashboardColour(veh, ref dashboardColor);
                    dict.Add("interiorColor", interiorColor.ToString());
                    dict.Add("dashboardColor", dashboardColor.ToString());

                    dict.Add("plate", GetVehicleNumberPlateText(veh).ToString());
                    dict.Add("plateStyle", GetVehicleNumberPlateTextIndex(veh).ToString());
                    dict.Add("windowTint", GetVehicleWindowTint(veh).ToString());

                    // Now add all vehicle mods that are dynamic (different per vehicle model).
                    Vehicle vehicle = new Vehicle(veh);
                    foreach (VehicleMod mod in vehicle.Mods.GetAllMods())
                    {
                        var modType = ((int)mod.ModType).ToString();
                        var modValue = mod.Index;
                        dict.Add(modType.ToString(), modValue.ToString());
                    }


                    if (updateExistingSavedVehicleName == null)
                    {
                        // Ask the user for a save name (will be displayed to the user and will be used as unique identifier for this vehicle)
                        var saveName = await GetUserInput("Enter a save name", "", 15);
                        // If the name is not invalid.
                        if (saveName != "NULL")
                        {
                            // Save everything from the dictionary into the client's kvp storage.
                            // If the save was successfull:
                            if (sm.SaveDictionary("veh_" + saveName, dict, false))
                            {
                                Notify.Success($"Vehicle {saveName} saved.");
                            }
                            // If the save was not successfull:
                            else
                            {
                                Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: "(" + saveName + ")");
                            }
                        }
                        // The user did not enter a valid name to use as a save name for this vehicle.
                        else
                        {
                            Notify.Error(CommonErrors.InvalidSaveName);
                        }
                    }
                    // We need to update an existing slot.
                    else
                    {
                        sm.SaveDictionary("veh_" + updateExistingSavedVehicleName, dict, true);
                    }

                }
                // The player is not inside a vehicle, or the vehicle is dead/not existing so we won't do anything. Only alert the user.
                else
                {
                    Notify.Error(CommonErrors.NoVehicle, placeholderValue: "to save it");
                }
            }
            // The player is not inside a vehicle.
            else
            {
                Notify.Error(CommonErrors.NoVehicle);
            }
        }
        #endregion

        #region Loading Saved Vehicle
        /// <summary>
        /// Get the data for a specific saved vehicle.
        /// </summary>
        /// <param name="saveName">Name is the key (vehicle name) to load the info for.</param>
        /// <returns></returns>
        public Dictionary<string, string> GetSavedVehicleData(string saveName)
        {
            var dict = sm.GetSavedDictionary(saveName);
            return dict;
        }
        #endregion

        #region Get Saved Vehicles Dictionary
        /// <summary>
        /// Get a dictionary containing all saved vehicle names (keys) and a nested dictionary for all the vehicle modifications
        /// and customization for each specific vehicle.
        /// </summary>
        /// <returns>A dictionary containing all saved vehicle names (keys) and the vehicle info for each vehicle.</returns>
        public Dictionary<string, Dictionary<string, string>> GetSavedVehiclesDictionary()
        {
            // Create a list to store all saved vehicle names in.
            var savedVehicleNames = new List<string>();
            // Start looking for kvps starting with veh_
            var findHandle = StartFindKvp("veh_");
            // Keep looking...
            while (true)
            {
                // Get the kvp string key.
                var vehString = FindKvp(findHandle);

                // If it exists then the key to the list.
                if (vehString != "" && vehString != null && vehString != "NULL")
                {
                    savedVehicleNames.Add(vehString);
                }
                // Otherwise stop.
                else
                {
                    break;
                }
            }
            // Create a Dictionary to store all vehicle information in.
            var vehiclesList = new Dictionary<string, Dictionary<string, string>>();
            // Loop through all save names (keys) from the list above, convert the string into a dictionary 
            // and add it to the dictionary above, with the vehicle save name as the key.
            foreach (var saveName in savedVehicleNames)
            {
                vehiclesList.Add(saveName, sm.GetSavedDictionary(saveName));
            }
            // Return the vehicle dictionary containing all vehicle save names (keys) linked to the correct vehicle
            // including all vehicle mods/customization parts.
            return vehiclesList;
        }
        #endregion

        #region Load Model
        /// <summary>
        /// Check and load a model.
        /// </summary>
        /// <param name="modelHash"></param>
        /// <returns>True if model is valid & loaded, false if model is invalid.</returns>
        private async Task<bool> LoadModel(uint modelHash)
        {
            // Check if the model exists in the game.
            if (IsModelInCdimage(modelHash))
            {
                // Load the model.
                RequestModel(modelHash);
                // Wait until it's loaded.
                while (!HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }
                // Model is loaded, return true.
                return true;
            }
            // Model is not valid or is not loaded correctly.
            else
            {
                // Return false.
                return false;
            }
        }
        #endregion

        #region GetUserInput
        /// <summary>
        /// Gets input from the user.
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="defaultText"></param>
        /// <param name="maxInputLength"></param>
        /// <returns>Reruns the input or "NULL" if cancelled.</returns>
        public async Task<string> GetUserInput(string windowTitle = null, string defaultText = null, int maxInputLength = 20)
        {
            // Create the window title string.
            AddTextEntry("FMMC_KEY_TIP1", $"{windowTitle ?? "Enter"}:   (MAX {maxInputLength.ToString()} CHARACTERS)");

            // Display the input box.
            DisplayOnscreenKeyboard(1, "FMMC_KEY_TIP1", "", defaultText ?? "", "", "", "", maxInputLength);
            await Delay(0);
            // Wait for a result.
            while (true)
            {
                // Cancelled
                if (UpdateOnscreenKeyboard() == 2)
                {
                    break;
                }
                // Finished
                else if (UpdateOnscreenKeyboard() == 1)
                {
                    break;
                }
                // Not displaying keyboard
                else if (UpdateOnscreenKeyboard() == 3)
                {
                    break;
                }
                // Still editing
                else
                {
                    await Delay(0);
                }
                // Just in case something goes wrong, add wait to prevent crashing.
                await Delay(0);
            }
            // Get the result
            int status = UpdateOnscreenKeyboard();
            string result = GetOnscreenKeyboardResult();

            // If the result is not empty or null
            if (result != "" && result != null && status == 1)
            {
                // Return result.
                return result.ToString();
            }
            else
            {
                return "NULL";
            }
        }
        #endregion

        #region Set License Plate Text
        /// <summary>
        /// Set the license plate text using the player's custom input.
        /// </summary>
        public async void SetLicensePlateTextAsync()
        {
            // Get the input.
            var text = await GetUserInput("Enter License Plate", maxInputLength: 8);
            // If the input is valid.
            if (text != "NULL")
            {
                // Get the vehicle.
                var veh = GetVehicle();
                // If it exists.
                if (DoesEntityExist(veh))
                {
                    // Set the license plate.
                    SetVehicleNumberPlateText(veh, text);
                }
                // If it doesn't exist, notify the user.
                else
                {
                    Notify.Error(CommonErrors.NoVehicle);
                    //Notify.Error("You're not inside a vehicle!");
                }
            }
            // No valid text was given.
            else
            {
                Notify.Error(CommonErrors.InvalidInput);
                //Notify.Error($"License plate text ~r~{(text == "NULL" ? "(empty input)" : text)} ~s~can not be used on a license plate!");
            }

        }
        #endregion

        #region ToProperString()
        /// <summary>
        /// Converts a PascalCaseString to a Propper Case String.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Input string converted to a normal sentence.</returns>
        public string ToProperString(string inputString)
        {
            var outputString = "";
            var prevUpper = true;
            foreach (char c in inputString)
            {
                if (char.IsLetter(c) && c != ' ' && c == char.Parse(c.ToString().ToUpper()))
                {
                    if (prevUpper)
                    {
                        outputString += $"{c.ToString()}";
                    }
                    else
                    {
                        outputString += $" {c.ToString()}";
                    }
                    prevUpper = true;
                }
                else
                {
                    prevUpper = false;
                    outputString += c.ToString();
                }
            }
            while (outputString.IndexOf("  ") != -1)
            {
                outputString = outputString.Replace("  ", " ");
            }
            return outputString;
        }

        #endregion

        #region Permissions
        /// <summary>
        /// Checks if the specified permission is granted for this user.
        /// Also checks parent/inherited/wildcard permissions.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool IsAllowed(Permission permission)
        {
            // Get the permissions.
            return PermissionsManager.IsAllowed(permission);
        }
        #endregion

        #region Play Scenarios
        /// <summary>
        /// Play the specified scenario name.
        /// If "forcestop" is specified, any currrently playing scenarios will be forcefully stopped.
        /// </summary>
        /// <param name="scenarioName"></param>
        public void PlayScenario(string scenarioName)
        {
            // If there's currently no scenario playing, or the current scenario is not the same as the new scenario, then..
            if (currentScenario == "" || currentScenario != scenarioName)
            {
                // Set the current scenario.
                currentScenario = scenarioName;
                // Clear all tasks to make sure the player is ready to play the scenario.
                ClearPedTasks(PlayerPedId());

                var canPlay = true;
                // Check if the player CAN play a scenario... 
                //if (IsPedInAnyVehicle(PlayerPedId(), true))
                //{
                //    Notify.Alert("You can't start a scenario when you are inside a vehicle.", true, false);
                //    canPlay = false;
                //}
                if (IsPedRunning(PlayerPedId()))
                {
                    Notify.Alert("You can't start a scenario when you are running.", true, false);
                    canPlay = false;
                }
                if (IsEntityDead(PlayerPedId()))
                {
                    Notify.Alert("You can't start a scenario when you are dead.", true, false);
                    canPlay = false;
                }
                if (IsPlayerInCutscene(PlayerPedId()))
                {
                    Notify.Alert("You can't start a scenario when you are in a cutscene.", true, false);
                    canPlay = false;
                }
                if (IsPedFalling(PlayerPedId()))
                {
                    Notify.Alert("You can't start a scenario when you are falling.", true, false);
                    canPlay = false;
                }
                if (IsPedRagdoll(PlayerPedId()))
                {
                    Notify.Alert("You can't start a scenario when you are currently in a ragdoll state.", true, false);
                    canPlay = false;
                }
                if (!IsPedOnFoot(PlayerPedId()))
                {
                    Notify.Alert("You must be on foot to start a scenario.", true, false);
                    canPlay = false;
                }
                if (NetworkIsInSpectatorMode())
                {
                    Notify.Alert("You can't start a scenario when you are currently spectating.", true, false);
                    canPlay = false;
                }
                if (GetEntitySpeed(PlayerPedId()) > 5.0f)
                {
                    Notify.Alert("You can't start a scenario when you are moving too fast.", true, false);
                    canPlay = false;
                }

                if (canPlay)
                {
                    // If the scenario is a "sit" scenario, then the scenario needs to be played at a specific location.
                    if (PedScenarios.PositionBasedScenarios.Contains(scenarioName))
                    {
                        // Get the offset-position from the player. (0.5m behind the player, and 0.5m below the player seems fine for most scenarios)
                        var pos = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0f, -0.5f, -0.5f);
                        var heading = GetEntityHeading(PlayerPedId());
                        // Play the scenario at the specified location.
                        TaskStartScenarioAtPosition(PlayerPedId(), scenarioName, pos.X, pos.Y, pos.Z, heading, -1, true, false);
                    }
                    // If it's not a sit scenario (or maybe it is, but using the above native causes other
                    // issues for some sit scenarios so those are not registered as "sit" scenarios), then play it at the current player's position.
                    else
                    {
                        TaskStartScenarioInPlace(PlayerPedId(), scenarioName, 0, true);
                    }
                }
            }
            // If the new scenario is the same as the currently playing one, cancel the current scenario.
            else
            {
                currentScenario = "";
                ClearPedTasks(PlayerPedId());
                ClearPedSecondaryTask(PlayerPedId());
            }

            // If the scenario name to play is called "forcestop" then clear the current scenario and force any tasks to be cleared.
            if (scenarioName == "forcestop")
            {
                currentScenario = "";
                ClearPedTasks(PlayerPedId());
                ClearPedTasksImmediately(PlayerPedId());
            }

        }
        #endregion

        #region Data parsing functions
        /// <summary>
        /// Converts a dictionary (string, string) into a json string.
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public string DictionaryToJson(Dictionary<string, string> dict)
        {
            var entries = dict.Select(d =>
                string.Format("\"{0}\": \"{1}\"", d.Key, string.Join(",", d.Value)));
            return "{" + string.Join(",", entries) + "}";
        }

        /// <summary>
        /// Converts a simple json string (only containing (string) key : (string) value).
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public Dictionary<string, string> JsonToDictionary(string json)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var entries = json.Split(',');
            foreach (var entry in entries)
            {
                //Notify.Custom(entry.ToString());
                var items = entry.Split(':');
                var key = "";
                var counter = 1;
                foreach (var item in items)
                {
                    counter++;
                    if (counter % 2 == 0)
                    {
                        key = item.Split('"')[1].ToString();
                    }
                    else
                    {
                        var val = item.Split('"')[1].ToString();
                        dict.Add(key, item.Split('"')[1].ToString());
                        counter = 1;
                    }
                }
                //foreach (KeyValuePair<string, string> t in dict)
                //{
                //    Notify.Custom($"Key: ~r~{t.Key} ~s~value: ~g~{t.Value}");
                //}
            }

            return dict;
        }
        #endregion

        #region Weather Sync
        /// <summary>
        /// Update the server with the new weather type, blackout status and dynamic weather changes enabled status.
        /// </summary>
        /// <param name="newWeather">The new weather type.</param>
        /// <param name="blackout">Manual blackout mode enabled/disabled.</param>
        /// <param name="dynamicChanges">Dynamic weather changes enabled/disabled.</param>
        public void UpdateServerWeather(string newWeather, bool blackout, bool dynamicChanges)
        {
            TriggerServerEvent("vMenu:UpdateServerWeather", newWeather, blackout, dynamicChanges);
        }

        /// <summary>
        /// Modify the clouds for everyone. If removeClouds is true, then remove all clouds. If it's false, then randomize the clouds.
        /// </summary>
        /// <param name="removeClouds">Removes the clouds from the sky if true, otherwise randomizes the clouds type for all players.</param>
        public void ModifyClouds(bool removeClouds)
        {
            TriggerServerEvent("vMenu:UpdateServerWeatherCloudsType", removeClouds);
        }
        #endregion

        #region Time Sync
        /// <summary>
        /// Update the server with the new time. If an invalid time is provided, then the time will be set to midnight (00:00);
        /// </summary>
        /// <param name="hours">Hours (0-23)</param>
        /// <param name="minutes">Minutes (0-59)</param>
        /// <param name="freezeTime">Should the time be frozen?</param>
        public void UpdateServerTime(int hours, int minutes, bool freezeTime)
        {
            var realHours = hours;
            var realMinutes = minutes;
            if (hours > 23 || hours < 0)
            {
                realHours = 0;
            }
            if (minutes > 59 || minutes < 0)
            {
                realMinutes = 0;
            }
            TriggerServerEvent("vMenu:UpdateServerTime", realHours, realMinutes, freezeTime);
        }


        #endregion

        #region StringToStringArray
        /// <summary>
        /// Converts the inputString into 1, 2 or 3 strings in a string[] (array).
        /// Each string in the array is up to 99 characters long at max.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>String[] containing 1, 2 or 3 strings.</returns>
        public string[] StringToArray(string inputString)
        {
            string[] outputString = new string[3];

            var lastSpaceIndex = 0;
            var newStartIndex = 0;
            outputString[0] = inputString;

            if (inputString.Length > 99)
            {
                for (int i = 0; i < inputString.Length; i++)
                {
                    if (inputString.Substring(i, 1) == " ")
                    {
                        lastSpaceIndex = i;
                    }

                    if (inputString.Length > 99 && i >= 98)
                    {
                        if (i == 98)
                        {
                            outputString[0] = inputString.Substring(0, lastSpaceIndex);
                            newStartIndex = lastSpaceIndex + 1;
                        }
                        if (i > 98 && i < 198)
                        {
                            if (i == 197)
                            {
                                outputString[1] = inputString.Substring(newStartIndex, (lastSpaceIndex - (outputString[0].Length - 1)) - (inputString.Length - 1 > 197 ? 1 : -1));
                                newStartIndex = lastSpaceIndex + 1;
                            }
                            else if (i == inputString.Length - 1 && inputString.Length < 198)
                            {
                                outputString[1] = inputString.Substring(newStartIndex, ((inputString.Length - 1) - outputString[0].Length));
                                newStartIndex = lastSpaceIndex + 1;
                            }
                        }
                        if (i > 197)
                        {
                            if (i == inputString.Length - 1 || i == 296)
                            {
                                outputString[2] = inputString.Substring(newStartIndex, ((inputString.Length - 1) - outputString[0].Length) - outputString[1].Length);
                            }
                        }
                    }
                }
            }

            return outputString;
        }
        #endregion

        #region Hud Functions

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition)
        {
            DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);
        }

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size)
        {
            DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);
        }

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification)
        {
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);
        }

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font)
        {
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);
        }

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        /// <param name="disableTextOutline">Disables the default text outline.</param>
        public void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (IsHudPreferenceSwitchedOn() && CitizenFX.Core.UI.Screen.Hud.IsVisible && !MainMenu.MiscSettingsMenu.HideHud)
            {
                SetTextFont(font);
                SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    SetTextWrap(0f, xPosition);
                }
                SetTextJustification((int)justification);
                if (!disableTextOutline) { SetTextOutline(); }
                BeginTextCommandDisplayText("STRING");
                AddTextComponentSubstringPlayerName(text);
                EndTextCommandDisplayText(xPosition, yPosition);
            }
        }
        #endregion

        #region Set Player Skin
        /// <summary>
        /// Sets the player's model to the provided modelName.
        /// </summary>
        /// <param name="modelName">The model name.</param>
        public void SetPlayerSkin(string modelName, Dictionary<string, string> pedCustomizationOptions = null)
        {
            int model = GetHashKey(modelName);
            SetPlayerSkin(model, pedCustomizationOptions);
        }

        /// <summary>
        /// Sets the player's model to the provided modelName.
        /// </summary>
        /// <param name="modelHash">The model hash.</param>
        public async void SetPlayerSkin(int modelHash, Dictionary<string, string> pedCustomizationOptions = null)
        {
            uint model = (uint)modelHash;
            if (IsModelInCdimage(model))
            {
                await SaveWeaponLoadout();
                RequestModel(model);
                while (!HasModelLoaded(model))
                {
                    await Delay(0);
                }
                SetPlayerModel(PlayerId(), model);
                SetPedDefaultComponentVariation(PlayerPedId());

                if (pedCustomizationOptions != null && pedCustomizationOptions.Count > 1)
                {
                    var ped = PlayerPedId();
                    for (var i = 0; i < 21; i++)
                    {
                        int drawable = int.Parse(pedCustomizationOptions[$"drawable_variation_{i.ToString()}"]);
                        int drawableTexture = int.Parse(pedCustomizationOptions[$"drawable_texture_{i.ToString()}"]);
                        SetPedComponentVariation(ped, i, drawable, drawableTexture, 0);
                    }

                    for (var i = 0; i < 21; i++)
                    {
                        int prop = int.Parse(pedCustomizationOptions[$"prop_{i.ToString()}"]);
                        int propTexture = int.Parse(pedCustomizationOptions[$"prop_texture_{i.ToString()}"]);
                        if (prop == -1 || propTexture == -1)
                        {
                            ClearPedProp(ped, i);
                        }
                        else
                        {
                            SetPedPropIndex(ped, i, prop, propTexture, true);
                        }
                    }
                }

                RestoreWeaponLoadout();
            }
            else
            {
                Notify.Error(CommonErrors.InvalidModel);
            }
        }
        #endregion

        #region Save Ped Model + Customizations
        /// <summary>
        /// Saves the current player ped.
        /// </summary>
        public async void SavePed()
        {
            // Get the save name.
            string name = await GetUserInput("Enter a ped save name", maxInputLength: 15);
            // If the save name is not invalid.
            if (name != "" && name != null && name != "NULL")
            {
                // Create a dictionary to store all data in.
                Dictionary<string, string> pedData = new Dictionary<string, string>();

                // Get the ped.
                int ped = PlayerPedId();

                // Get the ped model hash & add it to the dictionary.
                int model = GetEntityModel(ped);
                pedData.Add("modelHash", model.ToString());

                // Loop through all drawable variations.
                for (var i = 0; i < 21; i++)
                {
                    int drawable = GetPedDrawableVariation(ped, i);
                    int textureVariation = GetPedTextureVariation(ped, i);
                    pedData.Add($"drawable_variation_{i.ToString()}", drawable.ToString());
                    pedData.Add($"drawable_texture_{i.ToString()}", textureVariation.ToString());
                }

                // Loop through all prop variations.
                for (var i = 0; i < 21; i++)
                {
                    int prop = GetPedPropIndex(ped, i);
                    int propTexture = GetPedPropTextureIndex(ped, i);
                    pedData.Add($"prop_{i.ToString()}", $"{prop.ToString()}");
                    pedData.Add($"prop_texture_{i.ToString()}", $"{propTexture.ToString()}");
                }

                // Try to save the data, and save the result in a variable.
                bool saveSuccessful = sm.SaveDictionary("ped_" + name, pedData, false);

                // If the save was successfull.
                if (saveSuccessful)
                {
                    Notify.Success("Ped saved.");
                }
                // Save was not successfull.
                else
                {
                    Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: name);
                    //Notify.Error("Could not save this ped because the save name already exists.");
                }
            }
            // User cancelled the saving or they did not enter a valid name.
            else
            {
                Notify.Error(CommonErrors.InvalidSaveName);
            }
        }
        #endregion

        #region Load Saved Ped
        /// <summary>
        /// Load the saved ped and spawn it.
        /// </summary>
        /// <param name="savedName">The ped saved name</param>
        public async void LoadSavedPed(string savedName)
        {
            string savedPedName = savedName ?? await GetUserInput("Enter A Saved Ped Name");
            if (savedPedName == null || savedPedName == "NULL" || savedPedName == "")
            {
                //Notify.Error("Invalid saved ped name.");
                Notify.Error(CommonErrors.InvalidInput);
            }
            else
            {
                Dictionary<string, string> dict = sm.GetSavedDictionary("ped_" + savedPedName);
                int model = int.Parse(dict["modelHash"]);
                if (dict != null && dict.Count > 1)
                {
                    SetPlayerSkin(model, dict);
                }
                else
                {
                    Notify.Error(CommonErrors.CouldNotLoadSave, placeholderValue: "this saved ped");
                    //Notify.Error("Sorry, could not load saved ped. Is your save file corrupt?");
                }
            }
        }
        #endregion

        #region Save and restore weapon loadouts when changing models

        private struct WeaponInfo
        {
            public int Ammo;
            public uint Hash;
            public List<uint> Components;
            public int Tint;
        }
        private List<WeaponInfo> weaponsList = new List<WeaponInfo>();
        /// <summary>
        /// Saves all current weapons and components.
        /// </summary>
        public async Task SaveWeaponLoadout()
        {
            await Delay(1);
            weaponsList.Clear();
            await Delay(1);
            foreach (var vw in ValidWeapons.Weapons)
            {
                if (HasPedGotWeapon(PlayerPedId(), vw.Value, false))
                {
                    List<uint> components = new List<uint>();
                    foreach (var wc in ValidWeapons.weaponComponents)
                    {
                        if (DoesWeaponTakeWeaponComponent(vw.Value, wc.Value))
                        {
                            if (HasPedGotWeaponComponent(PlayerPedId(), vw.Value, wc.Value))
                            {
                                components.Add(wc.Value);
                            }
                        }
                    }
                    weaponsList.Add(new WeaponInfo()
                    {
                        Ammo = GetAmmoInPedWeapon(PlayerPedId(), vw.Value),
                        Hash = vw.Value,
                        Components = components,
                        Tint = GetPedWeaponTintIndex(PlayerPedId(), vw.Value)
                    });
                }
            }
            await Delay(1);
        }

        /// <summary>
        /// Restores all weapons and components
        /// </summary>
        public async void RestoreWeaponLoadout()
        {
            await Delay(0);
            if (weaponsList.Count > 0)
            {
                foreach (WeaponInfo wi in weaponsList)
                {
                    GiveWeaponToPed(PlayerPedId(), wi.Hash, wi.Ammo, false, false);
                    if (wi.Components.Count > 0)
                    {
                        foreach (var wc in wi.Components)
                        {
                            GiveWeaponComponentToPed(PlayerPedId(), wi.Hash, wc);
                        }
                    }
                    SetPedWeaponTintIndex(PlayerPedId(), wi.Hash, wi.Tint);
                }
            }
        }
        #endregion

        #region Get "Header" Menu Item
        /// <summary>
        /// Get a header menu item (text-centered, disabled UIMenuItem)
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public UIMenuItem GetSpacerMenuItem(string title, string description = null)
        {
            string output = "~h~";
            int length = title.Length;
            int totalSize = 90 - int.Parse((length * 3).ToString());

            for (var i = 0; i < totalSize / 2; i++)
            {
                output += " ";
            }
            output += title;
            UIMenuItem item = new UIMenuItem(output, description ?? "")
            {
                Enabled = false
            };
            return item;
        }
        #endregion

        #region Log Function
        public void Log(string data)
        {
            Debug.WriteLine(data, "");
        }
        #endregion

        #region Get Currently Opened Menu
        /// <summary>
        /// Returns the currently opened menu, if no menu is open, it'll return null.
        /// </summary>
        /// <returns></returns>
        public UIMenu GetOpenMenu()
        {
            UIMenu output = null;
            if (MainMenu.Mp.IsAnyMenuOpen())
            {
                foreach (UIMenu m in MainMenu.Mp.ToList())
                {
                    if (m.Visible)
                    {
                        return m;
                    }
                }
            }
            return output;

        }
        #endregion
    }
}
