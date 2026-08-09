namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the player options menu: things done to the player's own ped.
/// </summary>
[PermissionCategory]
public static class PlayerOptions
{
    public const string All = "vMenu.Enhanced.Menus.PlayerOptions.All";

    public const string Menu = "vMenu.Enhanced.Menus.PlayerOptions.Menu";

    /// <summary>Also covers being dragged out of, shot inside, and knocked off a vehicle.</summary>
    public const string Godmode = "vMenu.Enhanced.Menus.PlayerOptions.Godmode";
}
