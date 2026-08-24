using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

namespace vMenu.Enhanced.Permissions;

// Ped spawn checks for the menus. The whitelist is needed here too, otherwise a category grant would
// light up models the server holds back.
public static class ClientPedPermissions
{
    private static readonly HashSet<string> WhitelistedPeds = new(StringComparer.OrdinalIgnoreCase);

    // Apply before the matching permission set.
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

    // Whether a whole category submenu should open. Whitelisted models inside an allowed category still
    // have to pass CanSpawnPed.
    public static bool CanSpawnCategory(string categoryName) =>
        ClientPermissions.IsAllowed(
            PedModelCategories.ForCategory(CategoryName.ToPermissionSegment(categoryName)));

    public static bool CanSpawnPed(string modelName, string categoryName) =>
        IsWhitelisted(modelName)
            ? ClientPermissions.IsAllowed(Peds.ForModel(modelName))
            : CanSpawnCategory(categoryName);
}
