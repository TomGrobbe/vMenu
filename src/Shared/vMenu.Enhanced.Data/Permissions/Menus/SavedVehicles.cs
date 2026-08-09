namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the saved vehicles menu. The saves themselves live on the player's own machine,
/// so these decide what this server lets them do with that collection, not who owns it.
/// </summary>
[PermissionCategory]
public static class SavedVehicles
{
    public const string All = "vMenu.Enhanced.Menus.SavedVehicles.All";

    public const string Menu = "vMenu.Enhanced.Menus.SavedVehicles.Menu";

    /// <summary>Storing the vehicle the player is driving.</summary>
    public const string Save = "vMenu.Enhanced.Menus.SavedVehicles.Save";

    /// <summary>
    /// Bringing a saved vehicle back. Separate from <see cref="Save"/> so a server can let players
    /// build up a collection here and drive it somewhere else.
    /// </summary>
    // The model still has to pass the vehicle spawner's own whitelist, so this does not become a
    // way around a restricted vehicle list.
    public const string Spawn = "vMenu.Enhanced.Menus.SavedVehicles.Spawn";

    /// <summary>Renaming, recategorising, replacing and deleting saves, and managing categories.</summary>
    public const string Manage = "vMenu.Enhanced.Menus.SavedVehicles.Manage";
}
