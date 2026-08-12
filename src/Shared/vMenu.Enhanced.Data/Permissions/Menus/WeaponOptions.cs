namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the weapon options menu. Per category permissions live one container deeper in
/// <see cref="WeaponCategories"/>, and individual whitelisted weapons answer to
/// <see cref="SupplementalPermissions.Weapons"/>.
/// </summary>
[PermissionCategory]
public static class WeaponOptions
{
    public const string All = "vMenu.Enhanced.Menus.WeaponOptions.All";

    public const string Menu = "vMenu.Enhanced.Menus.WeaponOptions.Menu";

    public const string GetAll = "vMenu.Enhanced.Menus.WeaponOptions.GetAll";

    public const string RemoveAll = "vMenu.Enhanced.Menus.WeaponOptions.RemoveAll";

    public const string UnlimitedAmmo = "vMenu.Enhanced.Menus.WeaponOptions.UnlimitedAmmo";

    public const string NoReload = "vMenu.Enhanced.Menus.WeaponOptions.NoReload";

    public const string SetAllAmmo = "vMenu.Enhanced.Menus.WeaponOptions.SetAllAmmo";

    public const string Spawn = "vMenu.Enhanced.Menus.WeaponOptions.Spawn";

    public const string SpawnByName = "vMenu.Enhanced.Menus.WeaponOptions.SpawnByName";

    public const string Modify = "vMenu.Enhanced.Menus.WeaponOptions.Modify";

    public const string Parachute = "vMenu.Enhanced.Menus.WeaponOptions.Parachute";
}
