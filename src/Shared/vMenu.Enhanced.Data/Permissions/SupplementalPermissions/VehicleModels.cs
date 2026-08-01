namespace vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

/// <summary>
/// Permissions for individual whitelisted vehicle models. The per model permissions are registered
/// at runtime from the whitelist file, so only <see cref="All"/> is known at compile time.
/// </summary>
[PermissionCategory(AdditionalParents = new[] { Menus.VehicleSpawner.All })]
public static class VehicleModels
{
    /// <summary>Not a permission itself; it is not deeper than the category prefix.</summary>
    public const string Prefix = "vMenu.Enhanced.SupplementalPermissions.VehicleModels";

    public const string All = "vMenu.Enhanced.SupplementalPermissions.VehicleModels.All";

    public static string ForModel(string modelName) =>
        $"{Prefix}{PermissionPath.Separator}{modelName.ToLowerInvariant()}";
}
