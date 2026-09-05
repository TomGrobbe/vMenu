using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles.Personal;

using VehicleSpawnerSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleSpawner;

namespace vMenu.Enhanced.Menus.Vehicles;

// Neither caller checks permissions here. Each one has its own wording for a refusal, so the check
// stays with them and this only does the work.
public static class VehicleSpawning
{
    private const int DriverSeat = -1;

    private static readonly TextInfo TitleCase = new CultureInfo("en-US", false).TextInfo;

    private static int _previousVehicle;

    public static Task<Vehicle?> SpawnAsync(string modelName) => SpawnAsync(API.Hash(modelName));

    public static async Task<Vehicle?> SpawnAsync(uint hash)
    {
        // Checked and requested by hand because API.Vehicles.RequestAndCreate uses DateTime, which is
        // currently broken and crashes the game.
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

        var removingCurrent = WillRemoveCurrent(ped.Handle, currentVehicle?.Handle ?? 0);

        var position = SpawnPosition(ped, currentVehicle, hash, spawnInside, removingCurrent);

        var heading = spawnInside ? ped.Heading : ped.Heading + 90f;

        await RemovePreviousAsync(ped.Handle, currentVehicle?.Handle ?? 0, removingCurrent);

        var orphanMode = VehicleSpawnerSettings.NormaliseOrphanMode(ClientConfig.Value(VehicleSpawnerSettings.OrphanMode));

        var scriptHost = orphanMode != VehicleSpawnerSettings.DeleteWhenNotRelevant;

        var newVehicle = await API.Vehicles.RequestAndCreate(hash, position, (int)heading, true, scriptHost, true);

        Native.SetModelAsNoLongerNeeded(hash);

        if (newVehicle is null)
        {
            return null;
        }

        _previousVehicle = newVehicle.Handle;

        ReportSpawn(newVehicle.Handle);

        MenuAudit.ReportAction(AuditActions.VehicleSpawned, DisplayName(hash));

        if (!spawnInside)
        {
            Native.SetVehicleOnGroundProperly(newVehicle.Handle, 5f);

            return newVehicle;
        }

        Native.SetVehicleEngineOn(VehicleIndex: newVehicle.Handle, EngineOnFlag: true, bNoDelay: true, bOnlyStartWithPlayerInput: false);

        if (Native.IsThisModelAHeli(hash) || Native.IsThisModelAPlane(hash))
        {
            newVehicle.HeliBladesSpeed = 1f;

            VehicleTurbulence.Write(newVehicle.Handle, hash);
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

    private static void ReportSpawn(int entity)
    {
        if (entity == 0 || !Native.NetworkGetEntityIsNetworked(entity))
        {
            return;
        }

        API.EmitServer(VehicleEvents.Spawned, Native.NetworkGetNetworkIdFromEntity(entity));
    }

    private static Vector3 SpawnPosition(Ped ped, Vehicle? currentVehicle, uint hash, bool spawnInside, bool removingCurrent)
    {
        if (spawnInside && currentVehicle is null)
        {
            return ped.Position;
        }

        Native.GetModelDimensions(hash, out var spawnedMin, out var spawnedMax);

        var clearance = (Math.Abs((spawnedMin - spawnedMax).Y) / 2) + 1f;

        if (currentVehicle is not null && removingCurrent)
        {
            return currentVehicle.Position;
        }

        if (currentVehicle is null)
        {
            return Native.GetOffsetFromEntityInWorldCoords(ped.Handle, 0f, clearance + 2f, 0f);
        }

        Native.GetModelDimensions(currentVehicle.Model, out var currentMin, out var currentMax);

        clearance += Math.Abs((currentMin - currentMax).Y) / 2;

        return Native.GetOffsetFromEntityInWorldCoords(currentVehicle.Handle, 0f, clearance, 0f);
    }

    private static async Task RemovePreviousAsync(int ped, int currentVehicle, bool removingCurrent)
    {
        var replace = VehicleSpawnOptions.ReplacePrevious;

        if (MayRemove(_previousVehicle, ped))
        {
            if (replace)
            {
                await RemoveAsync(_previousVehicle, notify: false);
            }
            else if (!ClientConfig.Value(VehicleSpawnerSettings.KeepSpawnedVehiclesPersistent))
            {
                var handle = _previousVehicle;
                Native.SetEntityAsNoLongerNeeded(ref handle);
            }

            _previousVehicle = 0;
        }

        if (!removingCurrent)
        {
            return;
        }

        _previousVehicle = _previousVehicle == currentVehicle ? 0 : _previousVehicle;

        await RemoveAsync(currentVehicle, notify: true);
    }

    private static async Task RemoveAsync(int vehicle, bool notify)
    {
        if (vehicle == 0 || !Native.DoesEntityExist(vehicle))
        {
            return;
        }

        var personal = PersonalVehicle.Owns(vehicle);

        if (personal)
        {
            await PersonalVehicle.SurrenderAsync();
        }

        VehicleDeletion.DeleteLocally(vehicle);

        if (personal)
        {
            Notifications.Info(MenuText.Key(Loc.VehicleSpawner.PersonalVehicleReplaced));

            return;
        }

        if (notify)
        {
            Notifications.Info(MenuText.Key(Loc.VehicleSpawner.OldVehicleRemoved));
        }
    }

    private static bool WillRemoveCurrent(int ped, int currentVehicle)
    {
        if (!VehicleSpawnOptions.ReplacePrevious
            || currentVehicle == 0
            || !Native.DoesEntityExist(currentVehicle))
        {
            return false;
        }

        if (PersonalVehicle.Owns(currentVehicle) && !VehicleSpawnOptions.ForcedReplace)
        {
            return false;
        }

        return Native.GetPedInVehicleSeat(currentVehicle, DriverSeat, false) == ped;
    }

    private static bool MayRemove(int vehicle, int ped)
    {
        if (vehicle == 0 || !Native.DoesEntityExist(vehicle) || !Native.IsEntityAVehicle(vehicle))
        {
            return false;
        }

        if (PersonalVehicle.Owns(vehicle) && !VehicleSpawnOptions.ForcedReplace)
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
