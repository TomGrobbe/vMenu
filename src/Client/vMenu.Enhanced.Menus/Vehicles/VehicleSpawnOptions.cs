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
        UserDefaults.VehicleSpawnerReplacePrevious.Value || !CanKeepPrevious;

    public static bool CanKeepPrevious =>
        ClientPermissions.IsAllowed(VehicleSpawnerPermissions.AllowKeepPreviousVehicle);

    public static void SetReplacePrevious(bool replace)
    {
        if (!replace && !CanKeepPrevious)
        {
            return;
        }

        UserDefaults.VehicleSpawnerReplacePrevious.Value = replace;
    }
}
