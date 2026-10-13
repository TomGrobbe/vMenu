using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

using VehicleSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleSpawner;
using VehicleSpawnerSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleSpawner;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleSpawnLimit
{
    private static readonly List<int> Stamps = [];

    public static bool TryTakeOrWarn()
    {
        if (TryTake(out var retryAfterSeconds))
        {
            return true;
        }

        Notifications.Warning(MenuText.Key(
            Loc.VehicleSpawner.TooManySpawns,
            ("seconds", MenuText.Literal(retryAfterSeconds.ToString()))));

        return false;
    }

    private static bool TryTake(out int retryAfterSeconds)
    {
        retryAfterSeconds = 0;

        var limit = Allowance();
        var window = ClientConfig.Value(VehicleSpawnerSettings.SpawnLimitSeconds) * 1000;

        if (limit <= 0 || window <= 0)
        {
            Stamps.Clear();

            return true;
        }

        var now = Native.GetGameTimer();

        while (Stamps.Count > 0 && now - Stamps[0] >= window)
        {
            Stamps.RemoveAt(0);
        }

        if (Stamps.Count < limit)
        {
            Stamps.Add(now);

            return true;
        }

        retryAfterSeconds = Math.Max(1, (int)Math.Ceiling((window - (now - Stamps[0])) / 1000f));

        return false;
    }

    private static int Allowance()
    {
        if (ClientPermissions.IsAllowed(VehicleSpawnerPermissions.SpawnLimitTier3))
        {
            return ClientConfig.Value(VehicleSpawnerSettings.SpawnLimitTier3);
        }

        return ClientPermissions.IsAllowed(VehicleSpawnerPermissions.SpawnLimitTier2)
            ? ClientConfig.Value(VehicleSpawnerSettings.SpawnLimitTier2)
            : ClientConfig.Value(VehicleSpawnerSettings.SpawnLimitTier1);
    }
}
