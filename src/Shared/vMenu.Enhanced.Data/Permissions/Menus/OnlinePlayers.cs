namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the online players menu and the things you can do to a player from it.
/// </summary>
[PermissionCategory]
public static class OnlinePlayers
{
    public const string All = "vMenu.Enhanced.Menus.OnlinePlayers.All";

    public const string Menu = "vMenu.Enhanced.Menus.OnlinePlayers.Menu";

    [StaffOnly]
    public const string Kick = "vMenu.Enhanced.Menus.OnlinePlayers.Kick";

    [StaffOnly]
    public const string Kill = "vMenu.Enhanced.Menus.OnlinePlayers.Kill";

    [StaffOnly]
    public const string TeleportTo = "vMenu.Enhanced.Menus.OnlinePlayers.TeleportTo";

    [StaffOnly]
    public const string Summon = "vMenu.Enhanced.Menus.OnlinePlayers.Summon";

    public const string SendMessage = "vMenu.Enhanced.Menus.OnlinePlayers.SendMessage";

    [StaffOnly]
    public const string Waypoint = "vMenu.Enhanced.Menus.OnlinePlayers.Waypoint";

    [StaffOnly]
    public const string Identifiers = "vMenu.Enhanced.Menus.OnlinePlayers.Identifiers";

    [StaffOnly]
    public const string TxAdmin = "vMenu.Enhanced.Menus.OnlinePlayers.TxAdmin";
}
