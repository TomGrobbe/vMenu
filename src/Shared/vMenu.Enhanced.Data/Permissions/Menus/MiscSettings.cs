namespace vMenu.Enhanced.Data.Permissions.Menus;

// There is no Menu entry: the menu itself is deliberately ungated, because everything on it changes
// how vMenu presents itself to one player. All is kept as the container grant plugins hang their own
// permissions under.
[PermissionCategory]
public static class MiscSettings
{
    public const string All = "vMenu.Enhanced.Menus.MiscSettings.All";
}
