using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

using FxEvents;

using Newtonsoft.Json;

using ScaleformUI;
using ScaleformUI.Menu;

using vMenu.Client.Menus;
using vMenu.Client.Settings;
using vMenu.Shared.Objects;
using vMenu.Client.Events;

using vMenu.Shared.Enums;

using static vMenu.Client.Functions.MenuFunctions;

using static CitizenFX.Core.Native.API;

namespace vMenu.Client.Functions
{
    public class EntitySpawner
    {
        private static Vehicle _previousVehicle;
        
        public static async Task<int> SpawnVehicle(string vehicleName = "custom", bool spawnInside = false, bool replacePrevious = false)
        {
        	if (vehicleName == "custom")
        	{
                // Get the result.
                var result = await GetUserInput(windowTitle: "Enter Vehicle Name");
                // If the result was not invalid.
                if (!string.IsNullOrEmpty(result))
                {
                    // Convert it into a model hash.
                    var model = (uint)GetHashKey(result);
                    return await SpawnVehicle(model, spawnInside: spawnInside, replacePrevious: replacePrevious);
                }
                // Result was invalid.
                else
                {
                    Notify.Error("invalid");
                    return 0;
                }
            }


            return await SpawnVehicle((uint)GetHashKey(vehicleName), spawnInside: spawnInside, replacePrevious: replacePrevious);
        }
        public static async Task<int> SpawnVehicle(uint vehicleHash, bool spawnInside, bool replacePrevious)
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                var tmpOldVehicle = GetVehicle();
            }
            var modelClass = GetVehicleClassFromName(vehicleHash);
            if (!VehicleOptionsMenu.allowedCategories[modelClass])
            {
                Notify.Alert("You are not allowed to spawn this vehicle, because it belongs to a category which is restricted by the server owner.");
                return 0;
            }

            var successFull = await LoadModel(vehicleHash);
            if (!successFull || !IsModelAVehicle(vehicleHash))
            {
                // Vehicle model is invalid.
                Notify.Error("Invalid Model");
                return 0;
            }

            var pos = new CitizenFX.Core.Vector3(0, 0, 0);
            if (pos.IsZero)
            {
                pos = spawnInside ? GetEntityCoords(Game.PlayerPed.Handle, true) : GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, 8f, 0f);
                pos += new CitizenFX.Core.Vector3(0f, 0f, 1f);
            }
            float heading = 0;
            heading = heading == -1 ? GetEntityHeading(Game.PlayerPed.Handle) + (spawnInside ? 0f : 90f) : heading;
            if (_previousVehicle != null)
            {
                // And it's actually a vehicle (rather than another random entity type)
                if (_previousVehicle.Exists() && _previousVehicle.PreviouslyOwnedByPlayer &&
                    (_previousVehicle.Occupants.Count() == 0 || _previousVehicle.Driver.Handle == Game.PlayerPed.Handle))
                {
                    // If the previous vehicle should be deleted:
                    if (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious))
                    {
                        // Delete it.
                        _previousVehicle.PreviouslyOwnedByPlayer = false;
                        SetEntityAsMissionEntity(_previousVehicle.Handle, true, true);
                        _previousVehicle.Delete();
                    }
                    // Otherwise
                    else
                    {
                        if (!true)
                        {
                            SetEntityAsMissionEntity(_previousVehicle.Handle, false, false);
                        }
                    }
                    _previousVehicle = null;
                }
            }

            if (Game.PlayerPed.IsInVehicle() && (replacePrevious || !IsAllowed(Permission.VSDisableReplacePrevious)))
            {
                if (GetVehicle().Driver == Game.PlayerPed)
                {
                    var tmpveh = GetVehicle();
                    SetVehicleHasBeenOwnedByPlayer(tmpveh.Handle, false);
                    SetEntityAsMissionEntity(tmpveh.Handle, true, true);

                    if (_previousVehicle != null)
                    {
                        if (_previousVehicle.Handle == tmpveh.Handle)
                        {
                            _previousVehicle = null;
                        }
                    }
                    tmpveh.Delete();
                    Notify.Info("Your old car was removed to prevent your new car from glitching inside it. Next time, get out of your vehicle before spawning a new one if you want to keep your old one.");
                }
            }

            if (_previousVehicle != null)
            {
                _previousVehicle.PreviouslyOwnedByPlayer = false;
            }
            var vehicle = new Vehicle(CreateVehicle(vehicleHash, pos.X, pos.Y, pos.Z, heading, true, false))
            {
                NeedsToBeHotwired = false,
                PreviouslyOwnedByPlayer = true,
                IsPersistent = true,
                IsStolen = false,
                IsWanted = false
            };
            // If spawnInside is true
            if (spawnInside)
            {
                // Set the vehicle's engine to be running.
                vehicle.IsEngineRunning = true;
                new Ped(Game.PlayerPed.Handle).SetIntoVehicle(vehicle, VehicleSeat.Driver);

            }
            SetModelAsNoLongerNeeded(vehicleHash);
            return vehicle.Handle;
        }
        #region Load Model
        /// <summary>
        /// Check and load a model.
        /// </summary>
        /// <param name="modelHash"></param>
        /// <returns>True if model is valid & loaded, false if model is invalid.</returns>
        private static async Task<bool> LoadModel(uint modelHash)
        {
            // Check if the model exists in the game.
            if (IsModelInCdimage(modelHash))
            {
                // Load the model.
                RequestModel(modelHash);
                // Wait until it's loaded.
                while (!HasModelLoaded(modelHash))
                {
                    await BaseScript.Delay(0);
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
        public static Vehicle GetVehicle(bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return Game.PlayerPed.LastVehicle;
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    return Game.PlayerPed.CurrentVehicle;
                }
            }
            return null;
        }
    }
}