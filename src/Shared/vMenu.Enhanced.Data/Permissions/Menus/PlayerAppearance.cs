namespace vMenu.Enhanced.Data.Permissions.Menus;

// Changing the clothes and props on the ped the player is already wearing. Changing which ped that
// is lives in PedModels.
[PermissionCategory]
public static class PlayerAppearance
{
    public const string All = "vMenu.Enhanced.Menus.PlayerAppearance.All";

    public const string Menu = "vMenu.Enhanced.Menus.PlayerAppearance.Menu";

    public const string Customize = "vMenu.Enhanced.Menus.PlayerAppearance.Customize";

    public const string WalkingStyle = "vMenu.Enhanced.Menus.PlayerAppearance.WalkingStyle";

    public const string IlluminatedClothing = "vMenu.Enhanced.Menus.PlayerAppearance.IlluminatedClothing";

    // Flipping a helmet visor deliberately has no permission of its own. It changes nothing but the
    // player's own hat, so it is open to anybody this server gave any vMenu access to at all. See
    // ClientPermissions.HasAnyPermission.
}
