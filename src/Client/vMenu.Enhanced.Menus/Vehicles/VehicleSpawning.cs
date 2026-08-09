using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;
using CitizenFX.FiveM.Shared.Data;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// Putting a vehicle into the world, shared by the spawner and the saved vehicles menu.
/// </summary>
/// <remarks>
/// Neither caller checks permissions here. Each one has its own wording for a refusal, so the check
/// stays with them and this only does the work.
/// </remarks>
public static class VehicleSpawning
{
    private static readonly TextInfo TitleCase = new CultureInfo("en-US", false).TextInfo;

    /// <inheritdoc cref="SpawnAsync(uint)"/>
    public static Task<Vehicle?> SpawnAsync(string modelName) => SpawnAsync(API.Hash(modelName));

    /// <summary>Spawns a vehicle beside the one the player is in, and puts them in the driver's seat.</summary>
    /// <returns>Null when the model is not valid or the game refused to create it.</returns>
    public static async Task<Vehicle?> SpawnAsync(uint hash)
    {
        // Checked and requested by hand because API.Vehicles.RequestAndCreate uses DateTime, which
        // is currently broken and crashes the game.
        // https://github.com/citizenfx/rfc/discussions/328
        if (!Native.IsModelValid(hash))
        {
            return null;
        }

        Native.RequestModel(hash);

        while (!Native.HasModelLoaded(hash))
        {
            await API.Delay(0);
        }

        var ped = API.Players.Local.Ped!;

        var position = ped.Position;
        Vector3? velocity = null;
        var rpm = 100f;
        var speed = 0f;

        if (ped.IsPedInAnyVehicle())
        {
            var currentVehicle = ped.Vehicle!;

            NativeFixer.GetModelDimensions(currentVehicle.Model, out var currentMin, out var currentMax);
            NativeFixer.GetModelDimensions(hash, out var spawnedMin, out var spawnedMax);

            var yOffset = (Math.Abs((currentMin - currentMax).Y) / 2) + (Math.Abs((spawnedMin - spawnedMax).Y) / 2) + 1f;
            position = Native.GetOffsetFromEntityInWorldCoords(currentVehicle.Handle, 0f, yOffset, 0f);

            velocity = currentVehicle.Velocity;
            speed = Native.GetEntitySpeedVector(currentVehicle.Handle, true).Y;
            rpm = Native.GetVehicleCurrentRpm(currentVehicle.Handle);

            var handle = currentVehicle.Handle;
            Native.SetEntityAsNoLongerNeeded(new Ref<int>(ref handle));
        }

        var newVehicle = await API.Vehicles.RequestAndCreate(hash, position, (int)ped.Heading, true, true, true);

        Native.SetModelAsNoLongerNeeded(hash);

        if (newVehicle is null)
        {
            return null;
        }

        Native.SetVehicleEngineOn(VehicleIndex: newVehicle.Handle, EngineOnFlag: true, bNoDelay: true, bOnlyStartWithPlayerInput: false);

        if ((Native.IsThisModelAHeli(hash) is bool isHeli && isHeli) || Native.IsThisModelAPlane(hash))
        {
            newVehicle.HeliBladesSpeed = 1f;

            if (isHeli)
            {
                Native.SetHeliTurbulenceScalar(newVehicle.Handle, 0f);
            }
            else
            {
                Native.SetPlaneTurbulenceMultiplier(newVehicle.Handle, 0f);
            }
        }

        Native.SetVehicleForwardSpeed(newVehicle.Handle, speed);

        if (velocity.HasValue)
        {
            newVehicle.Velocity = velocity.Value;
        }

        Native.SetVehicleCurrentRpm(newVehicle.Handle, rpm);

        ped.SetPedIntoVehicle(newVehicle.Handle, -1);

        return newVehicle;
    }

    /// <summary>The game's own name for a model, falling back to the model name itself.</summary>
    public static string DisplayName(uint hash)
    {
        var displayName = Native.GetDisplayNameFromVehicleModel(hash);
        var labelText = Native.GetLabelText(displayName);

        return TitleCase.ToTitleCase(labelText == "NULL" ? displayName : labelText);
    }
}
