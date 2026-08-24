namespace vMenu.Enhanced.Data.Permissions.Menus;

// The saves themselves live on the player's own machine, so these decide what this server lets them
// do with that collection, not who owns it.
[PermissionCategory]
public static class SavedPeds
{
    public const string All = "vMenu.Enhanced.Menus.SavedPeds.All";

    public const string Menu = "vMenu.Enhanced.Menus.SavedPeds.Menu";

    public const string Save = "vMenu.Enhanced.Menus.SavedPeds.Save";

    public const string Spawn = "vMenu.Enhanced.Menus.SavedPeds.Spawn";

    public const string Manage = "vMenu.Enhanced.Menus.SavedPeds.Manage";
}
