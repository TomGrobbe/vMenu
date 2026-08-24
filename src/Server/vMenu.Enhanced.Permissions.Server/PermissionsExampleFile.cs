using System.Text;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Permissions.Server;

// Writes the whole permission tree to config/permissions.cfg.example on every start, so the
// reference can never drift from what the registry actually knows.
public static class PermissionsExampleFile
{
    public static void Write()
    {
        var resource = Native.GetCurrentResourceName();
        var path = PermissionsExample.ResourcePath;

        var entries = PermissionRegistry.EnumerateTree()
            .Where(static entry => !BelongsToAPlugin(entry.Node.Name))
            .Select(static entry => new PermissionExampleEntry(
                entry.Node.Name,
                entry.Depth,
                entry.Node.Source,
                entry.Node.IsStaffOnly,
                entry.Node.ExtraParents));

        var bytes = Encoding.UTF8.GetBytes(PermissionsExample.Render(entries));

        if (Native.SaveResourceFile(resource, path, bytes))
        {
            Log.Debug($"[Permissions] Wrote {path}, describing {PermissionRegistry.Count} permission(s).");
            return;
        }

        Log.Error(
            $"[Permissions] Could not write {path}. Add "
            + $"'add_filesystem_permission {resource} write {resource}' to your server.cfg, above the "
            + $"line that starts {resource}.");
    }

    // Whether a permission was brought by a plugin, which gets a template of its own instead. The
    // container above them all stays: it is vMenu's own permission, and it is what lets an owner grant
    // every plugin at once without opening a single per plugin file.
    private static bool BelongsToAPlugin(string permission) =>
        permission.StartsWith(PluginPermissions.Prefix + PermissionPath.Separator, StringComparison.OrdinalIgnoreCase)
        && !permission.Equals(PluginPermissions.All, StringComparison.OrdinalIgnoreCase);
}
