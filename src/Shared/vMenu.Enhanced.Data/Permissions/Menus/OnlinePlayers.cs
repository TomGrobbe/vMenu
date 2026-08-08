namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the online players menu and the things you can do to a player from it.
/// </summary>
/// <remarks>
/// <see cref="Menu"/> is what gets somebody the player list at all, so it is also the permission the
/// search runs under. Everything else is granted separately, because being allowed to see who is
/// online says nothing about being allowed to kick them.
/// </remarks>
[PermissionCategory]
public static class OnlinePlayers
{
    public const string All = "vMenu.Enhanced.Menus.OnlinePlayers.All";

    public const string Menu = "vMenu.Enhanced.Menus.OnlinePlayers.Menu";

    public const string Kick = "vMenu.Enhanced.Menus.OnlinePlayers.Kick";

    public const string Kill = "vMenu.Enhanced.Menus.OnlinePlayers.Kill";

    public const string TeleportTo = "vMenu.Enhanced.Menus.OnlinePlayers.TeleportTo";

    public const string Summon = "vMenu.Enhanced.Menus.OnlinePlayers.Summon";

    public const string SendMessage = "vMenu.Enhanced.Menus.OnlinePlayers.SendMessage";

    public const string Waypoint = "vMenu.Enhanced.Menus.OnlinePlayers.Waypoint";

    public const string Identifiers = "vMenu.Enhanced.Menus.OnlinePlayers.Identifiers";

    public const string TxAdmin = "vMenu.Enhanced.Menus.OnlinePlayers.TxAdmin";
}
