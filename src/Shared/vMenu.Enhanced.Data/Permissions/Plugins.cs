namespace vMenu.Enhanced.Data.Permissions;

// Container for permissions plugins bring at runtime. Each registered plugin gets its own
// vMenu.Enhanced.Plugins.<Id>.All container under this one, with its declared permissions below that.
[PermissionCategory(Prefix = Prefix)]
public static class Plugins
{
    public const string Prefix = "vMenu.Enhanced.Plugins";

    // Grants every permission of every plugin.
    public const string All = Prefix + PermissionPath.AllSuffix;

    public static string AllFor(string pluginId) =>
        Prefix + PermissionPath.Separator + pluginId + PermissionPath.AllSuffix;

    public static string For(string pluginId, string shortName) =>
        Prefix + PermissionPath.Separator + pluginId + PermissionPath.Separator + shortName;
}
