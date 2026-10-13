using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
using vMenu.Enhanced.Menus.Vehicles.Personal;

using VehicleSpawnerSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleSpawner;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles;

// Callers check permissions themselves, each with its own refusal wording; this only does the work.
public static class VehicleSpawning
{
    private const int DriverSeat = -1;

    private static readonly TextInfo TitleCase = new CultureInfo("en-US").TextInfo;

    private static int _previousVehicle;

    public static Task<Vehicle?> SpawnAsync(string modelName) => SpawnAsync(Hashing.Joaat(modelName));

    public static async Task<Vehicle?> SpawnAsync(uint hash)
    {

        if (!API.IsModelValid(hash))
        {
            return null;
        }

        API.RequestModel(hash);

        while (!API.HasModelLoaded(hash))
        {
            await BaseScript.Delay(0);
        }

        var ped = Game.PlayerPed!;
        var spawnInside = VehicleSpawnOptions.SpawnInside;
        var currentVehicle = ped.IsInVehicle() ? ped.CurrentVehicle! : null;

        Vector3? velocity = null;
        var rpm = 0f;
        var speed = 0f;

        if (spawnInside && currentVehicle is not null)
        {
            velocity = currentVehicle.Velocity;
            speed = API.GetEntitySpeedVector(currentVehicle.Handle, true).Y;
            rpm = API.GetVehicleCurrentRpm(currentVehicle.Handle);
        }

        var removingCurrent = WillRemoveCurrent(ped.Handle, currentVehicle?.Handle ?? 0);

        var position = SpawnPosition(ped, currentVehicle, hash, spawnInside, removingCurrent);

        var heading = spawnInside ? ped.Heading : ped.Heading + 90f;

        await RemovePreviousAsync(ped.Handle, currentVehicle?.Handle ?? 0, removingCurrent);

        var orphanMode = VehicleSpawnerSettings.NormaliseOrphanMode(ClientConfig.Value(VehicleSpawnerSettings.OrphanMode));

        var scriptHost = orphanMode != VehicleSpawnerSettings.DeleteWhenNotRelevant;

        var newHandle = NativeFixer.CreateVehicle(hash, position.X, position.Y, position.Z, (int)heading, true, scriptHost, true);

        API.SetModelAsNoLongerNeeded(hash);

        if (newHandle <= 0)
        {
            return null;
        }

        var newVehicle = new Vehicle(newHandle);

        _previousVehicle = newVehicle.Handle;

        ReportSpawn(newVehicle.Handle);

        MenuAudit.ReportAction(AuditActions.VehicleSpawned, VehicleModelNames.Resolve(hash));

        if (!spawnInside)
        {
            NativeFixer.SetVehicleOnGroundProperly(newVehicle.Handle, 5f);

            return newVehicle;
        }

        API.SetVehicleEngineOn(newVehicle.Handle, true, true, false);

        if (API.IsThisModelAHeli(hash) || API.IsThisModelAPlane(hash))
        {
            API.SetHeliBladesSpeed(newVehicle.Handle, 1f);

            VehicleTurbulence.Write(newVehicle.Handle, hash);
        }

        API.SetVehicleForwardSpeed(newVehicle.Handle, speed);

        if (velocity.HasValue)
        {
            newVehicle.Velocity = velocity.Value;
        }

        if (rpm > 0.2f)
        {
            API.SetVehicleCurrentRpm(newVehicle.Handle, rpm);
        }

        API.SetPedIntoVehicle(ped.Handle, newVehicle.Handle, DriverSeat);

        return newVehicle;
    }

    private static void ReportSpawn(int entity)
    {
        if (entity == 0 || !API.NetworkGetEntityIsNetworked(entity))
        {
            return;
        }

        BaseScript.TriggerServerEvent(VehicleEvents.Spawned, API.NetworkGetNetworkIdFromEntity(entity));
    }

    private static Vector3 SpawnPosition(Ped ped, Vehicle? currentVehicle, uint hash, bool spawnInside, bool removingCurrent)
    {
        if (spawnInside && currentVehicle is null)
        {
            return ped.Position;
        }

        NativeFixer.GetModelDimensions(hash, out var spawnedMin, out var spawnedMax);

        var clearance = (Math.Abs((spawnedMin - spawnedMax).Y) / 2) + 1f;

        if (currentVehicle is not null && removingCurrent)
        {
            return currentVehicle.Position;
        }

        if (currentVehicle is null)
        {
            return API.GetOffsetFromEntityInWorldCoords(ped.Handle, 0f, clearance + 2f, 0f);
        }

        NativeFixer.GetModelDimensions(currentVehicle.Model, out var currentMin, out var currentMax);

        clearance += Math.Abs((currentMin - currentMax).Y) / 2;

        return API.GetOffsetFromEntityInWorldCoords(currentVehicle.Handle, 0f, clearance, 0f);
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
                NativeFixer.SetEntityAsNoLongerNeeded(ref handle);
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
        if (vehicle == 0 || !API.DoesEntityExist(vehicle))
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
            || !API.DoesEntityExist(currentVehicle))
        {
            return false;
        }

        if (PersonalVehicle.Owns(currentVehicle) && !VehicleSpawnOptions.ForcedReplace)
        {
            return false;
        }

        return NativeFixer.GetPedInVehicleSeat(currentVehicle, DriverSeat, false) == ped;
    }

    private static bool MayRemove(int vehicle, int ped)
    {
        if (vehicle == 0 || !API.DoesEntityExist(vehicle) || !API.IsEntityAVehicle(vehicle))
        {
            return false;
        }

        if (PersonalVehicle.Owns(vehicle) && !VehicleSpawnOptions.ForcedReplace)
        {
            return false;
        }

        var driver = NativeFixer.GetPedInVehicleSeat(vehicle, DriverSeat, false);

        return driver == 0 || driver == ped;
    }

    public static string DisplayName(uint hash)
    {
        var displayName = API.GetDisplayNameFromVehicleModel(hash);
        var labelText = API.GetLabelText(displayName);

        return TitleCase.ToTitleCase(labelText == "NULL" ? displayName : labelText);
    }
}
