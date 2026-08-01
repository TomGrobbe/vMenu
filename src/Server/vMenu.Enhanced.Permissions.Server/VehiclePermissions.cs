using System.Globalization;

using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// Authoritative vehicle spawn checks.
/// </summary>
public static class VehiclePermissions
{
    /// <summary>
    /// A whitelisted model answers only to its own permission; the class permission is not
    /// consulted. Broader grants are picked up by the inheritance chain.
    /// </summary>
    public static bool CanSpawnVehicleModel(string source, string modelName, int vehicleClass) =>
        ModelWhitelist.IsWhitelistedVehicle(modelName)
            ? ServerPermissions.IsPlayerAllowed(source, VehicleModels.ForModel(modelName))
            : ServerPermissions.IsPlayerAllowed(source, VehicleSpawnerClasses.FromClassId(vehicleClass));

    /// <inheritdoc cref="CanSpawnVehicleModel(string, string, int)"/>
    public static bool CanSpawnVehicleModel(Player player, string modelName, int vehicleClass) =>
        CanSpawnVehicleModel(player.Handle.ToString(CultureInfo.InvariantCulture), modelName, vehicleClass);

    public static string[] GetWhitelistedVehicleModels() =>
        ModelWhitelist.GetModels(SupplementalModelKind.Vehicle);
}
