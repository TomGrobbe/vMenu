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
        private int previousVehicle = -1;
        private StorageManager sm = new StorageManager();

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        public CommonFunctions()
        {
            Tick += OnTick;
            Tick += ManageVehicleOptionsMenu;
        }
        #endregion

        #region OnTick Vehicle Options
        private async Task ManageVehicleOptionsMenu()
        {
            if (MainMenu.VehicleOptionsMenu == null)
            {
                await Delay(0);
            }
            else
            {
                // check to see if the vehicle options menu exists but the player is not inside a vehicle.
                if (MainMenu.VehicleOptionsMenu.vehicleModMenu != null && !IsPedInAnyVehicle(PlayerPedId(), false))
                {
                    // If the vehicle mod submenu is open, close it.
                    if (MainMenu.VehicleOptionsMenu.vehicleModMenu.Visible)
                    {
                        MainMenu.VehicleOptionsMenu.vehicleModMenu.GoBack();
                        Notify.Error("You must be inside a vehicle to use this menu.");
                    }
                    // If the vehicle liveries submenu is open, close it.
                    if (MainMenu.VehicleOptionsMenu.vehicleLiveries.Visible)
                    {
                        MainMenu.VehicleOptionsMenu.vehicleLiveries.GoBack();
                        Notify.Error("You must be inside a vehicle to use this menu.");
                    }
                    // If the vehicle colors submenu is open, close it.
                    if (MainMenu.VehicleOptionsMenu.vehicleColors.Visible)
                    {
                        MainMenu.VehicleOptionsMenu.vehicleColors.GoBack();
                        Notify.Error("You must be inside a vehicle to use this menu.");
                    }
                    // If the vehicle doors submenu is open, close it.
                    if (MainMenu.VehicleOptionsMenu.vehicleDoorsMenu.Visible)
                    {
                        MainMenu.VehicleOptionsMenu.vehicleDoorsMenu.GoBack();
                        Notify.Error("You must be inside a vehicle to use this menu.");
                    }
                    // If the vehicle windows submenu is open, close it.
                    if (MainMenu.VehicleOptionsMenu.vehicleWindowsMenu.Visible)
                    {
                        MainMenu.VehicleOptionsMenu.vehicleWindowsMenu.GoBack();
                        Notify.Error("You must be inside a vehicle to use this menu.");
                    }
                    // If the vehicle extras submenu is open, close it.
                    if (MainMenu.VehicleOptionsMenu.vehicleComponents.Visible)
                    {
                        MainMenu.VehicleOptionsMenu.vehicleComponents.GoBack();
                        Notify.Error("You must be inside a vehicle to use this menu.");
                    }
                }
            }
        }
        #endregion

        #region OnTick for spectate handling
        /// <summary>
        /// OnTick runs every game tick.
        /// Used here for the spectating feature.
        /// </summary>
        /// <returns></returns>
        private async Task OnTick()
        {
            // When the player dies while spectating, cancel the spectating to prevent an infinite black loading screen.
            if (GetEntityHealth(PlayerPedId()) < 1 && NetworkIsInSpectatorMode())
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
                //SetPedCoordsKeepVehicle(PlayerPedId(), playerPos.X, playerPos.Y, playerPos.Z + 2.0f);
                await TeleportToCoords(playerPos);

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
        #endregion

        #region Teleport To Coords
        /// <summary>
        /// Teleport the player to a specific location.
        /// </summary>
        /// <param name="targetCoords"></param>
        public async Task TeleportToCoords(Vector3 targetCoords)
        {
            var pos = targetCoords;
            pos.Z = 150.0f;
            SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, pos.Z);
            await Delay(50);
            GetGroundZFor_3dCoord(pos.X, pos.Y, 800f, ref pos.Z, true);
            await Delay(50);
            SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, pos.Z + 2f);
        }

        /// <summary>
        /// 
        /// </summary>
        public async void TeleportToWp()
        {
            if (Game.IsWaypointActive)
            {
                var pos = World.WaypointPosition;
                pos.Z = 150.0f;
                SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, pos.Z);
                await Delay(50);
                GetGroundZFor_3dCoord(pos.X, pos.Y, 800f, ref pos.Z, true);
                await Delay(50);
                SetPedCoordsKeepVehicle(PlayerPedId(), pos.X, pos.Y, pos.Z + 2f);
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
                var userInput = await GetUserInputAsync("Enter Kick Message", "", 100);
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
                Notify.Info("Spectating ~r~" + GetPlayerName(playerId), false, false);
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
                string result = await GetUserInputAsync("Enter Vehicle Name", "adder");
                // If the result was not invalid.
                if (result != "NULL")
                {
                    // Convert it into a model hash.
                    uint model = (uint)GetHashKey(result);
                    //await LoadModel(model);
                    SpawnVehicle(vehicleHash: model, spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false);

                }
                // Result was invalid.
                else
                {
                    Notify.Alert("You cancelled the input or the input was invalid.");
                }
            }
            // Spawn the specified vehicle.
            else
            {
                SpawnVehicle(vehicleHash: (uint)GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious, skipLoad: false);
            }
        }



        /// <summary>
        /// Spawn a vehicle by providing the vehicle hash.
        /// </summary>
        /// <param name="vehicleHash">Hash of the vehicle model to spawn.</param>
        /// <param name="skipLoad">If true, this will not load or verify the model, it will instantly spawn the vehicle.</param>
        public async void SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious, bool skipLoad = false)
        {
            if (!skipLoad)
            {
                bool successFull = await LoadModel(vehicleHash);
                if (!successFull)
                {
                    // Vehicle model is invalid.
                    Notify.Error("This is not a valid model.");
                    return;
                }
            }

            // Get the heading & position for where the vehicle should be spawned.
            Vector3 pos = (spawnInside) ? GetEntityCoords(PlayerPedId(), true) : GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0f, 8f, 0f);
            var heading = GetEntityHeading(PlayerPedId()) + (spawnInside ? 0f : 90f);

            // Create the vehicle.
            var veh = CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z + 1f, heading, true, true);

            // Create a new vehicle object for this vehicle and remove the need to hotwire the car.
            Vehicle vehicle = new Vehicle(veh)
            {
                NeedsToBeHotwired = false
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
                    SetVehicleOnGroundProperly(vehicle.Handle);
                }
            }

            // If the previous vehicle exists...
            if (DoesEntityExist(previousVehicle))
            {
                // And it's actually a vehicle (rather than another random entity type)
                if (IsEntityAVehicle(previousVehicle))
                {
                    // If the previous vehicle should be deleted:
                    if (replacePrevious)
                    {
                        // Delete it.
                        SetEntityAsMissionEntity(previousVehicle, false, false);
                        DeleteVehicle(ref previousVehicle);
                    }
                    // Otherwise
                    else
                    {
                        // Set the vehicle to be no longer needed. This will make the game engine decide when it should be removed (when all players get too far away).
                        SetEntityAsMissionEntity(previousVehicle, false, false);
                        SetVehicleAsNoLongerNeeded(ref previousVehicle);
                    }
                }
            }

            // Set the previous vehicle to the new vehicle.
            previousVehicle = vehicle.Handle;
        }
        #endregion

        #region Save Vehicle
        public async void SaveVehicle()
        {
            if (IsPedInAnyVehicle(PlayerPedId(), false))
            {
                var veh = GetVehicle();
                if (DoesEntityExist(veh) && (IsEntityAVehicle(veh)))
                {
                    //var model = GetVehicleModel(veh);
                    var model = (uint)GetEntityModel(veh);
                    var name = GetLabelText(GetDisplayNameFromVehicleModel(model));
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("name", name);
                    dict.Add("model", model.ToString() ?? "");

                    var saveName = await GetUserInputAsync("Enter a save name", "", 15);
                    if (saveName != "NULL")
                    {
                        if (sm.SaveDictionary("veh_" + saveName, dict, false))
                        {
                            Notify.Success($"Vehicle {saveName} saved.");
                        }
                        else
                        {
                            Notify.Error("Saving failed because this save name is already in use.");
                        }
                    }
                    else
                    {
                        Notify.Error("Saving failed because you did not enter a valid save name.");
                    }
                }
                else
                {
                    Notify.Error("You need to be in a vehicle.");
                }
            }
            else
            {
                Notify.Error("You need to be in a vehicle.");
            }
        }
        #endregion

        #region Loading Saved Vehicle
        public Dictionary<string, string> GetSavedVehicleData(string saveName)
        {
            var dict = sm.GetSavedDictionary(saveName);
            return dict;
        }

        public Dictionary<string, Dictionary<string, string>> GetSavedVehicleList()
        {
            var savedVehicleNames = new List<string>();
            var findHandle = StartFindKvp("veh_");
            while (true)
            {
                var vehString = FindKvp(findHandle);
                if (vehString != "" && vehString != null && vehString != "NULL")
                {
                    savedVehicleNames.Add(vehString);
                }
                else
                {
                    break;
                }
            }

            var vehiclesList = new Dictionary<string, Dictionary<string, string>>();
            foreach (var saveName in savedVehicleNames)
            {
                vehiclesList.Add(saveName, sm.GetSavedDictionary(saveName));
            }
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
        public async Task<string> GetUserInputAsync(string windowTitle = null, string defaultText = null, int maxInputLength = 20)
        {

            MainMenu.DontOpenMenus = true;
            UIMenu openMenu = null;

            // Check for any open menus, then go through all of them and save the state if they're open so we can reopen them later.
            if (MainMenu.Mp.IsAnyMenuOpen())
            {
                if (MainMenu.VehicleOptionsMenu.GetMenu().Visible)
                {
                    openMenu = MainMenu.VehicleOptionsMenu.GetMenu();
                }
                else if (MainMenu.OnlinePlayersMenu.GetMenu().Visible)
                {
                    openMenu = MainMenu.OnlinePlayersMenu.GetMenu();
                }
                else if (MainMenu.PlayerOptionsMenu.GetMenu().Visible)
                {
                    openMenu = MainMenu.PlayerOptionsMenu.GetMenu();
                }
                else if (MainMenu.VehicleSpawnerMenu.GetMenu().Visible)
                {
                    openMenu = MainMenu.VehicleSpawnerMenu.GetMenu();
                }
                else if (MainMenu.SavedVehiclesMenu.GetMenu().Visible)
                {
                    openMenu = MainMenu.SavedVehiclesMenu.GetMenu();
                }

                // Then close all menus.
                MainMenu.Mp.CloseAllMenus();
            }

            // Create the window title string.
            AddTextEntry("FMMC_KEY_TIP1", $"{windowTitle ?? "Enter"}:   (MAX {maxInputLength.ToString()} CHARACTERS)");
            // Display the input box.
            DisplayOnscreenKeyboard(1, "FMMC_KEY_TIP1", "", defaultText ?? "", "", "", "", maxInputLength);
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
                // Reopen any menus if they were open.
                if (openMenu != null)
                {
                    openMenu.Visible = true;
                }
                // Allow menus to be opened again.
                MainMenu.DontOpenMenus = false;
                // Return result.
                return result.ToString();
            }
            else
            {
                // Reopen any menus if they were open.
                if (openMenu != null)
                {
                    openMenu.Visible = true;
                }
                // Allow menus to be opened again.
                MainMenu.DontOpenMenus = false;
                // Return result.
                return "NULL";
            }
        }
        #endregion

        #region Set License Plate Text
        public async void SetLicensePlateTextAsync()
        {
            var text = await GetUserInputAsync("Enter License Plate", maxInputLength: 8);
            if (text != "NULL")
            {
                var veh = GetVehicle();
                if (DoesEntityExist(veh))
                {
                    SetVehicleNumberPlateText(veh, text);
                }
                else
                {
                    Notify.Error("You're not inside a vehicle!");
                }
            }
            else
            {
                Notify.Alert("You did not enter a valid license plate text!");
            }

        }
        #endregion

        #region ToProperString()
        /// <summary>
        /// Converts a PascalCaseString to a Propper Case String.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
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
        /// <returns>True = allowed. False = not allowed.</returns>
        public bool IsAllowed(string permission)
        {
            // Get the permissions.
            var permissions = MainMenu.Permissions;

            // If the player has ALL permissions, then return true.
            if (permissions.ContainsKey("vMenu_everything"))
            {
                if (permissions["vMenu_everything"])
                {
                    return true;
                }
            }

            // If the requested permission exists, then return true/false depending on the value of the permission.
            if (permissions.ContainsKey(permission))
            {
                return permissions[permission];
            }
            // If the permission does not exist, then return false.
            else
            {
                return false;
            }
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
                if (IsPedInAnyVehicle(PlayerPedId(), true))
                {
                    Notify.Alert("You can't start a scenario when you are inside a vehicle.", true, false);
                    canPlay = false;
                }
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
                //    Notify.Custom($"Key: ~r~{t.Key}");
                //    Notify.Custom($"value: ~g~{t.Value}");
                //}
            }

            return dict;
        }
        #endregion
    }

}
