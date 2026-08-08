namespace vMenu.Enhanced.Data.Permissions.Menus;

[PermissionCategory]
public static class TeleportMenu
{
    public const string All = "vMenu.Enhanced.Menus.TeleportMenu.All";

    public const string Menu = "vMenu.Enhanced.Menus.TeleportMenu.Menu";

    /// <summary>Teleporting to the waypoint the player put on their map.</summary>
    public const string Waypoint = "vMenu.Enhanced.Menus.TeleportMenu.Waypoint";

    /// <summary>Teleporting to coordinates the player types in by hand.</summary>
    public const string Coords = "vMenu.Enhanced.Menus.TeleportMenu.Coords";

    /// <summary>Receiving the locations a server owner set up, and teleporting to them.</summary>
    public const string Category = "vMenu.Enhanced.Menus.TeleportMenu.Category";

    /// <summary>Adding a category or a location, which writes the config file for everybody.</summary>
    public const string Manage = "vMenu.Enhanced.Menus.TeleportMenu.Manage";
}
