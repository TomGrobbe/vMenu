using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

namespace vMenu.Enhanced.Permissions;

/// <summary>
/// Ped spawn checks for the menus. The whitelist is needed here too, otherwise a category grant
/// would light up models the server holds back.
/// </summary>
public static class ClientPedPermissions
{
    private static readonly HashSet<string> WhitelistedPeds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Apply before the matching permission set.</summary>
    public static void ApplyWhitelistedPedModels(string[] models)
    {
        WhitelistedPeds.Clear();

        foreach (var model in models)
        {
            WhitelistedPeds.Add(model);
        }
    }

    public static bool IsWhitelisted(string modelName) =>
        WhitelistedPeds.Contains(modelName);

    /// <summary>
    /// Whether a whole category submenu should open. Whitelisted models inside an allowed category
    /// still have to pass <see cref="CanSpawnPed(string, string)"/>.
    /// </summary>
    public static bool CanSpawnCategory(string categoryName) =>
        ClientPermissions.IsAllowed(
            PedModelCategories.ForCategory(CategoryName.ToPermissionSegment(categoryName)));

    public static bool CanSpawnPed(string modelName, string categoryName) =>
        IsWhitelisted(modelName)
            ? ClientPermissions.IsAllowed(Peds.ForModel(modelName))
            : CanSpawnCategory(categoryName);
}
