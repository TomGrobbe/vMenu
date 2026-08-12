using vMenu.Enhanced.Data.Permissions.Menus;
using vMenu.Enhanced.Data.Permissions.SupplementalPermissions;

namespace vMenu.Enhanced.Permissions;

/// <summary>
/// Weapon checks for the menus. The whitelist is needed here too, otherwise a category grant would
/// light up weapons the server holds back.
/// </summary>
public static class ClientWeaponPermissions
{
    private static readonly HashSet<string> WhitelistedWeapons = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Apply before the matching permission set.</summary>
    public static void ApplyWhitelistedWeapons(string[] weapons)
    {
        WhitelistedWeapons.Clear();

        foreach (var weapon in weapons)
        {
            WhitelistedWeapons.Add(weapon);
        }
    }

    public static bool IsWhitelisted(string spawnName) =>
        WhitelistedWeapons.Contains(spawnName);

    /// <summary>
    /// Whether a whole category submenu should open. Whitelisted weapons inside an allowed category
    /// still have to pass <see cref="CanUseWeapon(string, string)"/>.
    /// </summary>
    public static bool CanUseCategory(string categoryName) =>
        ClientPermissions.IsAllowed(
            WeaponCategories.ForCategory(CategoryName.ToPermissionSegment(categoryName)));

    public static bool CanUseWeapon(string spawnName, string categoryName) =>
        IsWhitelisted(spawnName)
            ? ClientPermissions.IsAllowed(Weapons.ForModel(spawnName))
            : CanUseCategory(categoryName);
}
