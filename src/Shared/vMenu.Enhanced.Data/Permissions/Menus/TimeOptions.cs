namespace vMenu.Enhanced.Data.Permissions.Menus;

[PermissionCategory]
public static class TimeOptions
{
    public const string All = "vMenu.Enhanced.Menus.TimeOptions.All";

    // Not [StaffOnly]: reading the clock changes nothing.
    public const string Menu = "vMenu.Enhanced.Menus.TimeOptions.Menu";

    [StaffOnly]
    public const string SetTime = "vMenu.Enhanced.Menus.TimeOptions.SetTime";
}
