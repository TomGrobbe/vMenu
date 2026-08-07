using CitizenFX.FiveM.Server;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Permissions;

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
            .Select(static entry => new PermissionExampleEntry(
                entry.Node.Name,
                entry.Depth,
                entry.Node.Source,
                entry.Node.IsStaffOnly,
                entry.Node.ExtraParents));

        if (NativeFixer.SaveResourceFile(resource, path, PermissionsExample.Render(entries)))
        {
            API.Log.Info($"[Permissions] Wrote {path}, describing {PermissionRegistry.Count} permission(s).");
            return;
        }

        API.Log.Error(
            $"[Permissions] Could not write {path}. Add "
            + $"'add_filesystem_permission {resource} write {resource}' to your server.cfg, above the "
            + $"line that starts {resource}.");
    }
}
