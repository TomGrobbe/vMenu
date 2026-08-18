using CitizenFX.FiveM.Server;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

using PluginPermissions = vMenu.Enhanced.Data.Permissions.Plugins;

namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// Writes the whole permission tree to <c>config/permissions.cfg.example</c> on every start, so the
/// reference can never drift from what the registry actually knows.
/// </summary>
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

        if (NativeFixer.SaveResourceFile(resource, path, PermissionsExample.Render(entries)))
        {
            Log.Debug($"[Permissions] Wrote {path}, describing {PermissionRegistry.Count} permission(s).");
            return;
        }

        Log.Error(
            $"[Permissions] Could not write {path}. Add "
            + $"'add_filesystem_permission {resource} write {resource}' to your server.cfg, above the "
            + $"line that starts {resource}.");
    }

    /// <summary>
    /// Whether a permission was brought by a plugin, which gets a template of its own instead.
    /// </summary>
    // The container above them all stays: it is vMenu's own permission, and it is what lets an owner
    // grant every plugin at once without opening a single per plugin file.
    private static bool BelongsToAPlugin(string permission) =>
        permission.StartsWith(PluginPermissions.Prefix + PermissionPath.Separator, StringComparison.OrdinalIgnoreCase)
        && !permission.Equals(PluginPermissions.All, StringComparison.OrdinalIgnoreCase);
}
