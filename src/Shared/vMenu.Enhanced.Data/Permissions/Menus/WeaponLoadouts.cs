namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the weapon loadouts menu. A loadout is a named snapshot of the weapons a player
/// is carrying, so equipping one is held apart from merely keeping them.
/// </summary>
[PermissionCategory]
public static class WeaponLoadouts
{
    public const string All = "vMenu.Enhanced.Menus.WeaponLoadouts.All";

    public const string Menu = "vMenu.Enhanced.Menus.WeaponLoadouts.Menu";

    public const string Save = "vMenu.Enhanced.Menus.WeaponLoadouts.Save";

    public const string Manage = "vMenu.Enhanced.Menus.WeaponLoadouts.Manage";

    public const string Equip = "vMenu.Enhanced.Menus.WeaponLoadouts.Equip";

    public const string EquipOnRespawn = "vMenu.Enhanced.Menus.WeaponLoadouts.EquipOnRespawn";

    public const string KeepOnPedChange = "vMenu.Enhanced.Menus.WeaponLoadouts.KeepOnPedChange";
}
