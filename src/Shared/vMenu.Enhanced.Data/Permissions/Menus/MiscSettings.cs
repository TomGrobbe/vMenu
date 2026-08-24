namespace vMenu.Enhanced.Data.Permissions.Menus;

// There is no Menu entry: the menu itself is deliberately ungated, because everything on it changes
// how vMenu presents itself to one player. Only the tools reached from it are grantable.
[PermissionCategory]
public static class MiscSettings
{
    public const string All = "vMenu.Enhanced.Menus.MiscSettings.All";

    public const string PlayerBlips = "vMenu.Enhanced.Menus.MiscSettings.PlayerBlips";

    public const string OverheadNames = "vMenu.Enhanced.Menus.MiscSettings.OverheadNames";

    [StaffOnly]
    public const string ClearArea = "vMenu.Enhanced.Menus.MiscSettings.ClearArea";

    [StaffOnly]
    public const string SeeNoClipPlayers = "vMenu.Enhanced.Menus.MiscSettings.SeeNoClipPlayers";

    [StaffOnly]
    public const string SeeLeaveReasons = "vMenu.Enhanced.Menus.MiscSettings.SeeLeaveReasons";
}
