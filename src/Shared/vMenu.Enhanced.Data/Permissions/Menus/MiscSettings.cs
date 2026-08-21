namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the tools that hang off misc settings.
/// </summary>
/// <remarks>
/// There is no <c>Menu</c> entry: the menu itself is deliberately ungated, because everything on it
/// changes how vMenu presents itself to one player. Only the tools reached from it are grantable.
/// </remarks>
[PermissionCategory]
public static class MiscSettings
{
    public const string All = "vMenu.Enhanced.Menus.MiscSettings.All";

    public const string NoClip = "vMenu.Enhanced.Menus.MiscSettings.NoClip";

    public const string PlayerBlips = "vMenu.Enhanced.Menus.MiscSettings.PlayerBlips";

    public const string OverheadNames = "vMenu.Enhanced.Menus.MiscSettings.OverheadNames";

    public const string ShowLocation = "vMenu.Enhanced.Menus.MiscSettings.ShowLocation";

    public const string ShowCoordinates = "vMenu.Enhanced.Menus.MiscSettings.ShowCoordinates";

    [StaffOnly]
    public const string SeeNoClipPlayers = "vMenu.Enhanced.Menus.MiscSettings.SeeNoClipPlayers";

    [StaffOnly]
    public const string SeeLeaveReasons = "vMenu.Enhanced.Menus.MiscSettings.SeeLeaveReasons";
}
