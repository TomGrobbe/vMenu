namespace vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

/// <summary>
/// Permissions for individual whitelisted weapons. The per weapon permissions are registered at
/// runtime from the whitelist file, so only <see cref="All"/> is known at compile time.
/// </summary>
[PermissionCategory(AdditionalParents = new[] { Menus.WeaponOptions.All })]
public static class Weapons
{
    /// <summary>Not a permission itself; it is not deeper than the category prefix.</summary>
    public const string Prefix = "vMenu.Enhanced.SupplementalPermissions.Weapons";

    public const string All = "vMenu.Enhanced.SupplementalPermissions.Weapons.All";

    /// <summary>
    /// Feed it the weapon's spawn name, so the permission reads the same as the config file it came
    /// from rather than some shortened form of it.
    /// </summary>
    public static string ForModel(string modelName) =>
        $"{Prefix}{PermissionPath.Separator}{modelName.ToLowerInvariant()}";
}
