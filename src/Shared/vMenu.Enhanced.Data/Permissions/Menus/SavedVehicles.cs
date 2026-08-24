namespace vMenu.Enhanced.Data.Permissions.Menus;

// The saves themselves live on the player's own machine, so these decide what this server lets them
// do with that collection, not who owns it.
[PermissionCategory]
public static class SavedVehicles
{
    public const string All = "vMenu.Enhanced.Menus.SavedVehicles.All";

    public const string Menu = "vMenu.Enhanced.Menus.SavedVehicles.Menu";

    // Storing the vehicle the player is driving.
    public const string Save = "vMenu.Enhanced.Menus.SavedVehicles.Save";

    // Bringing a saved vehicle back. Separate from Save so a server can let players build up a
    // collection here and drive it somewhere else. The model still has to pass the vehicle spawner's own
    // whitelist, so this does not become a way around a restricted vehicle list.
    public const string Spawn = "vMenu.Enhanced.Menus.SavedVehicles.Spawn";

    // Renaming, recategorising, replacing and deleting saves, and managing categories.
    public const string Manage = "vMenu.Enhanced.Menus.SavedVehicles.Manage";
}
