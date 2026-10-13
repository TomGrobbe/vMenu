using vMenu.Enhanced.Data.Permissions;

using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Permissions.Server;

// Writes the whole permission tree to config/permissions.cfg.example on every start, so the
// reference can never drift from what the registry actually knows.
public static partial class PermissionsExampleFile
{
    public static string Render(bool ready = false)
    {
        var entries = PermissionRegistry.EnumerateTree()
            .Where(static entry => !BelongsToAPlugin(entry.Node.Name))
            .Select(static entry => new PermissionExampleEntry(
                entry.Node.Name,
                entry.Depth,
                entry.Node.Source,
                entry.Node.IsStaffOnly,
                entry.Node.ExtraParents));

        return PermissionsExample.Render(entries, ready);
    }

    // Whether a permission was brought by a plugin, which gets a template of its own instead. The
    // container above them all stays: it is vMenu's own permission, and it is what lets an owner grant
    // every plugin at once without opening a single per plugin file.
    private static bool BelongsToAPlugin(string permission) =>
        permission.StartsWith(PluginPermissions.Prefix + PermissionPath.Separator, StringComparison.OrdinalIgnoreCase)
        && !permission.Equals(PluginPermissions.All, StringComparison.OrdinalIgnoreCase);
}
