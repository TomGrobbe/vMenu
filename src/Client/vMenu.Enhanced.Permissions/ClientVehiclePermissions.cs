using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

namespace vMenu.Enhanced.Permissions;

/// <summary>
/// Vehicle spawn checks for the menus, mirroring what the server will decide. The whitelist and the
/// server's own categories are needed here too, otherwise a class grant would light up models the
/// server holds back or moved somewhere else.
/// </summary>
public static class ClientVehiclePermissions
{
    private static readonly HashSet<string> WhitelistedVehicles = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> CategoryByModel = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Apply before the matching permission set.</summary>
    public static void ApplyWhitelistedVehicleModels(string[] models)
    {
        WhitelistedVehicles.Clear();

        foreach (var model in models)
        {
            WhitelistedVehicles.Add(model);
        }
    }

    /// <summary>
    /// The categories a server owner defined, as two index aligned arrays. Apply before the
    /// matching permission set.
    /// </summary>
    public static void ApplyCustomCategories(string[] models, string[] categories)
    {
        CategoryByModel.Clear();

        for (var index = 0; index < models.Length && index < categories.Length; index++)
        {
            CategoryByModel[models[index]] = categories[index];
        }
    }

    public static bool IsWhitelisted(string modelName) =>
        WhitelistedVehicles.Contains(modelName);

    /// <summary>The category a model was moved into, or null when it is still in its game class.</summary>
    public static string? CategoryOfModel(string modelName) =>
        CategoryByModel.TryGetValue(modelName, out var category) ? category : null;

    public static bool CanSpawnVehicle(string modelName, int vehicleClass)
    {
        if (IsWhitelisted(modelName))
        {
            return ClientPermissions.IsAllowed(VehicleModels.ForModel(modelName));
        }

        return CategoryOfModel(modelName) is { } category
            ? CanSpawnCustomCategory(category)
            : ClientPermissions.IsAllowed(VehicleSpawnerCategories.FromClassId(vehicleClass));
    }

    /// <summary>
    /// Whether a whole game class submenu should open. Whitelisted models inside an allowed class
    /// still have to pass <see cref="CanSpawnVehicle(string, int)"/>.
    /// </summary>
    public static bool CanSpawnVehicleClass(int vehicleClass) =>
        ClientPermissions.IsAllowed(VehicleSpawnerCategories.FromClassId(vehicleClass));

    /// <inheritdoc cref="CanSpawnVehicleClass(int)"/>
    public static bool CanSpawnCustomCategory(string categoryName) =>
        ClientPermissions.IsAllowed(
            VehicleSpawnerCategories.ForCustom(CategoryName.ToPermissionSegment(categoryName)));
}
