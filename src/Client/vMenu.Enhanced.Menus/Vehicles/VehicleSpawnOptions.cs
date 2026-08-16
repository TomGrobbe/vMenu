using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using VehicleSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleSpawner;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleSpawnOptions
{
    public static bool SpawnInside => UserDefaults.VehicleSpawnerSpawnInside.Value;

    public static void SetSpawnInside(bool spawnInside) =>
        UserDefaults.VehicleSpawnerSpawnInside.Value = spawnInside;

    public static bool ReplacePrevious =>
        UserDefaults.VehicleSpawnerReplacePrevious.Value || !IsAllowedToKeep;

    private static bool IsAllowedToKeep =>
        ClientPermissions.IsAllowed(VehicleSpawnerPermissions.AllowKeepPreviousVehicle);

    public static void SetReplacePrevious(bool replace)
    {
        if (!replace && !IsAllowedToKeep)
        {
            return;
        }

        UserDefaults.VehicleSpawnerReplacePrevious.Value = replace;
    }
}
