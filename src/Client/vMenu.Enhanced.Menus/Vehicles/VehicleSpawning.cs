using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Personal;

using VehicleSpawnerSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleSpawner;

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
    private const int DriverSeat = -1;

    private static readonly TextInfo TitleCase = new CultureInfo("en-US", false).TextInfo;

    private static int _previousVehicle;

    /// <inheritdoc cref="SpawnAsync(uint)"/>
    public static Task<Vehicle?> SpawnAsync(string modelName) => SpawnAsync(API.Hash(modelName));


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
        var spawnInside = VehicleSpawnOptions.SpawnInside;
        var currentVehicle = ped.IsPedInAnyVehicle() ? ped.Vehicle! : null;

        Vector3? velocity = null;
        var rpm = 0f;
        var speed = 0f;

        if (spawnInside && currentVehicle is not null)
        {
            velocity = currentVehicle.Velocity;
            speed = Native.GetEntitySpeedVector(currentVehicle.Handle, true).Y;
            rpm = Native.GetVehicleCurrentRpm(currentVehicle.Handle);
        }

        var position = SpawnPosition(ped, currentVehicle, hash, spawnInside);

        var heading = spawnInside ? ped.Heading : ped.Heading + 90f;

        RemovePrevious(ped.Handle, currentVehicle?.Handle ?? 0);

        var newVehicle = await API.Vehicles.RequestAndCreate(hash, position, (int)heading, true, true, true);

        Native.SetModelAsNoLongerNeeded(hash);

        if (newVehicle is null)
        {
            return null;
        }

        _previousVehicle = newVehicle.Handle;

        PersonalVehicle.ReportSpawned(newVehicle.Handle);

        if (!spawnInside)
        {
            Native.SetVehicleOnGroundProperly(newVehicle.Handle, 5f);

            return newVehicle;
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

        if (rpm > 0.2f)
        {
            Native.SetVehicleCurrentRpm(newVehicle.Handle, rpm);
        }

        ped.SetPedIntoVehicle(newVehicle.Handle, DriverSeat);

        return newVehicle;
    }

    private static Vector3 SpawnPosition(Ped ped, Vehicle? currentVehicle, uint hash, bool spawnInside)
    {
        if (spawnInside && currentVehicle is null)
        {
            return ped.Position;
        }

        Native.GetModelDimensions(hash, out var spawnedMin, out var spawnedMax);

        var clearance = (Math.Abs((spawnedMin - spawnedMax).Y) / 2) + 1f;

        if (currentVehicle is null)
        {
            return Native.GetOffsetFromEntityInWorldCoords(ped.Handle, 0f, clearance + 2f, 0f);
        }

        Native.GetModelDimensions(currentVehicle.Model, out var currentMin, out var currentMax);

        clearance += Math.Abs((currentMin - currentMax).Y) / 2;

        return Native.GetOffsetFromEntityInWorldCoords(currentVehicle.Handle, 0f, clearance, 0f);
    }

    private static void RemovePrevious(int ped, int currentVehicle)
    {
        var replace = VehicleSpawnOptions.ReplacePrevious;

        if (MayRemove(_previousVehicle, ped))
        {
            if (replace)
            {
                VehicleDeletion.DeleteLocally(_previousVehicle);
            }
            else if (!ClientConfig.Value(VehicleSpawnerSettings.KeepSpawnedVehiclesPersistent))
            {
                var handle = _previousVehicle;
                Native.SetEntityAsNoLongerNeeded(ref handle);
            }

            _previousVehicle = 0;
        }

        if (!replace || currentVehicle == 0 || !Native.DoesEntityExist(currentVehicle))
        {
            return;
        }

        if (PersonalVehicle.Owns(currentVehicle))
        {
            return;
        }

        if (Native.GetPedInVehicleSeat(currentVehicle, DriverSeat, false) != ped)
        {
            return;
        }

        _previousVehicle = _previousVehicle == currentVehicle ? 0 : _previousVehicle;

        VehicleDeletion.DeleteLocally(currentVehicle);

        Notifications.Info(MenuText.Key(Loc.VehicleSpawner.OldVehicleRemoved));
    }

    private static bool MayRemove(int vehicle, int ped)
    {
        if (vehicle == 0 || !Native.DoesEntityExist(vehicle) || !Native.IsEntityAVehicle(vehicle))
        {
            return false;
        }

        if (PersonalVehicle.Owns(vehicle))
        {
            return false;
        }

        var driver = Native.GetPedInVehicleSeat(vehicle, DriverSeat, false);

        return driver == 0 || driver == ped;
    }

    public static string DisplayName(uint hash)
    {
        var displayName = Native.GetDisplayNameFromVehicleModel(hash);
        var labelText = Native.GetLabelText(displayName);

        return TitleCase.ToTitleCase(labelText == "NULL" ? displayName : labelText);
    }
}
