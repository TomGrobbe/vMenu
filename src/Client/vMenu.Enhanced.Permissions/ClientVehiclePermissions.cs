using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

namespace vMenu.Enhanced.Permissions;

/// <summary>
/// Vehicle spawn checks for the menus, mirroring what the server will decide. The whitelist is
/// needed here too, otherwise a class grant would light up models the server holds back.
/// </summary>
public static class ClientVehiclePermissions
{
    private static readonly HashSet<string> WhitelistedVehicles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Apply before the matching permission set.</summary>
    public static void ApplyWhitelistedVehicleModels(string[] models)
    {
        WhitelistedVehicles.Clear();

        foreach (var model in models)
        {
            WhitelistedVehicles.Add(model);
        }
    }

    public static bool IsWhitelisted(string modelName) =>
        WhitelistedVehicles.Contains(modelName);

    public static bool CanSpawnVehicle(string modelName, int vehicleClass) =>
        IsWhitelisted(modelName)
            ? ClientPermissions.IsAllowed(VehicleModels.ForModel(modelName))
            : ClientPermissions.IsAllowed(VehicleSpawnerClasses.FromClassId(vehicleClass));

    /// <summary>
    /// Whether a whole class submenu should open. Whitelisted models inside an allowed class still
    /// have to pass <see cref="CanSpawnVehicle(string, int)"/>.
    /// </summary>
    public static bool CanSpawnVehicleClass(int vehicleClass) =>
        ClientPermissions.IsAllowed(VehicleSpawnerClasses.FromClassId(vehicleClass));
}
