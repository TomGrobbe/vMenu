namespace vMenu.Enhanced.Data.Permissions.Menus;

// Every category comes from config/weapons.json and is registered at runtime, so nothing but the
// container itself is known at compile time. A category permission covers every weapon in it except
// models on the server whitelist, which answer to SupplementalPermissions.Weapons instead.
[PermissionCategory(Prefix = Prefix)]
public static class WeaponCategories
{
    // Not a permission itself; it is not deeper than the category prefix.
    public const string Prefix = "vMenu.Enhanced.Menus.WeaponOptions.Categories";

    public const string All = "vMenu.Enhanced.Menus.WeaponOptions.Categories.All";

    // Feed it a segment from CategoryName.ToPermissionSegment, never a raw name.
    public static string ForCategory(string segment) =>
        $"{Prefix}{PermissionPath.Separator}{segment}";
}
