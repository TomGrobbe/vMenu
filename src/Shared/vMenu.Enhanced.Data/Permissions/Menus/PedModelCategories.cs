namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Per category ped spawn permissions. Every category comes from
/// <c>config/ped-models.json</c> and is registered at runtime, so nothing but the container itself
/// is known at compile time. A category permission covers every ped in it except models on the
/// server whitelist, which answer to <see cref="SupplementalPermissions.Peds"/> instead.
/// </summary>
[PermissionCategory(Prefix = Prefix)]
public static class PedModelCategories
{
    /// <summary>Not a permission itself; it is not deeper than the category prefix.</summary>
    public const string Prefix = "vMenu.Enhanced.Menus.PedModels.Categories";

    public const string All = "vMenu.Enhanced.Menus.PedModels.Categories.All";

    /// <summary>
    /// The permission for a category a server owner defined. Feed it a segment from
    /// <see cref="CategoryName.ToPermissionSegment"/>, never a raw name.
    /// </summary>
    public static string ForCategory(string segment) =>
        $"{Prefix}{PermissionPath.Separator}{segment}";
}
