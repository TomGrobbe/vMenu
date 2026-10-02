using System.Text;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class PermissionsExampleFile
{
    public static void Write()
    {
        var resource = Native.GetCurrentResourceName();
        var path = PermissionsExample.ResourcePath;

        var bytes = Encoding.UTF8.GetBytes(Render());

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
}
