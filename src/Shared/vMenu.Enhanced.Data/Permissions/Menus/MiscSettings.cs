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
}
