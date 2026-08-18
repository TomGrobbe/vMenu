namespace vMenu.Enhanced.Data.Permissions;

/// <summary>
/// Container for permissions plugins bring at runtime. Each registered plugin gets its own
/// <c>vMenu.Enhanced.Plugins.&lt;Id&gt;.All</c> container under this one, with its declared
/// permissions below that.
/// </summary>
[PermissionCategory(Prefix = Prefix)]
public static class Plugins
{
    public const string Prefix = "vMenu.Enhanced.Plugins";

    /// <summary>Grants every permission of every plugin.</summary>
    public const string All = Prefix + PermissionPath.AllSuffix;

    public static string AllFor(string pluginId) =>
        Prefix + PermissionPath.Separator + pluginId + PermissionPath.AllSuffix;

    public static string For(string pluginId, string shortName) =>
        Prefix + PermissionPath.Separator + pluginId + PermissionPath.Separator + shortName;
}
