namespace vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

// Permissions for individual whitelisted weapons. The per weapon permissions are registered at
// runtime from the whitelist file, so only All is known at compile time.
[PermissionCategory(AdditionalParents = new[] { Menus.WeaponOptions.All })]
public static class Weapons
{
    // Not a permission itself; it is not deeper than the category prefix.
    public const string Prefix = "vMenu.Enhanced.SupplementalPermissions.Weapons";

    public const string All = "vMenu.Enhanced.SupplementalPermissions.Weapons.All";

    // Feed it the weapon's spawn name, so the permission reads the same as the config file it came from
    // rather than some shortened form of it.
    public static string ForModel(string modelName) =>
        $"{Prefix}{PermissionPath.Separator}{modelName.ToLowerInvariant()}";
}
