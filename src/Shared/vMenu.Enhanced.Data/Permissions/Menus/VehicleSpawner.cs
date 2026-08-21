namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the vehicle spawner menu. Per category permissions live one container deeper in
/// <see cref="VehicleSpawnerCategories"/>.
/// </summary>
[PermissionCategory]
public static class VehicleSpawner
{
    public const string All = "vMenu.Enhanced.Menus.VehicleSpawner.All";

    public const string Menu = "vMenu.Enhanced.Menus.VehicleSpawner.Menu";

    public const string SpawnByName = "vMenu.Enhanced.Menus.VehicleSpawner.SpawnByName";

    public const string AllowKeepPreviousVehicle = "vMenu.Enhanced.Menus.VehicleSpawner.AllowKeepPreviousVehicle";

    public const string SpawnLimitTier1 = "vMenu.Enhanced.Menus.VehicleSpawner.SpawnLimitTier1";

    public const string SpawnLimitTier2 = "vMenu.Enhanced.Menus.VehicleSpawner.SpawnLimitTier2";

    public const string SpawnLimitTier3 = "vMenu.Enhanced.Menus.VehicleSpawner.SpawnLimitTier3";
}
