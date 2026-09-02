namespace vMenu.Enhanced.Data.Permissions.Menus;

[PermissionCategory]
public static class TeleportMenu
{
    public const string All = "vMenu.Enhanced.Menus.TeleportMenu.All";

    public const string Menu = "vMenu.Enhanced.Menus.TeleportMenu.Menu";

    // Teleporting to the waypoint the player put on their map.
    public const string Waypoint = "vMenu.Enhanced.Menus.TeleportMenu.Waypoint";

    // Teleporting to coordinates the player types in by hand.
    public const string Coords = "vMenu.Enhanced.Menus.TeleportMenu.Coords";

    // Receiving the locations a server owner set up, and teleporting to them.
    public const string Category = "vMenu.Enhanced.Menus.TeleportMenu.Category";

    // Adding or removing a category or a location, which writes the config file for everybody.
    [StaffOnly]
    public const string Manage = "vMenu.Enhanced.Menus.TeleportMenu.Manage";
}
