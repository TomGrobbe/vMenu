namespace vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

// Permissions for individual whitelisted ped models, registered at runtime from the whitelist file,
// so only All is known at compile time.
[PermissionCategory(AdditionalParents = new[] { Menus.PedModels.All })]
public static class Peds
{
    // Not a permission itself; it is not deeper than the category prefix.
    public const string Prefix = "vMenu.Enhanced.SupplementalPermissions.Peds";

    public const string All = "vMenu.Enhanced.SupplementalPermissions.Peds.All";

    public static string ForModel(string modelName) =>
        $"{Prefix}{PermissionPath.Separator}{modelName.ToLowerInvariant()}";
}
