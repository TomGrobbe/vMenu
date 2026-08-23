namespace vMenu.Enhanced.Data.Permissions.Menus;

[PermissionCategory]
public static class PropSpawner
{
    public const string All = "vMenu.Enhanced.Menus.PropSpawner.All";

    public const string Menu = "vMenu.Enhanced.Menus.PropSpawner.Menu";

    public const string Spawn = "vMenu.Enhanced.Menus.PropSpawner.Spawn";

    public const string Networked = "vMenu.Enhanced.Menus.PropSpawner.Networked";

    public const string Sets = "vMenu.Enhanced.Menus.PropSpawner.Sets";

    public const string SetsManage = "vMenu.Enhanced.Menus.PropSpawner.SetsManage";

    public const string Delete = "vMenu.Enhanced.Menus.PropSpawner.Delete";

    [StaffOnly]
    public const string Manage = "vMenu.Enhanced.Menus.PropSpawner.Manage";
}
