namespace vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

// Permissions for individual whitelisted vehicle models, registered at runtime from the whitelist
// file, so only All is known at compile time.
[PermissionCategory(AdditionalParents = new[] { Menus.VehicleSpawner.All })]
public static class VehicleModels
{
    // Not a permission itself; it is not deeper than the category prefix.
    public const string Prefix = "vMenu.Enhanced.SupplementalPermissions.VehicleModels";

    public const string All = "vMenu.Enhanced.SupplementalPermissions.VehicleModels.All";

    public static string ForModel(string modelName) =>
        $"{Prefix}{PermissionPath.Separator}{modelName.ToLowerInvariant()}";
}
