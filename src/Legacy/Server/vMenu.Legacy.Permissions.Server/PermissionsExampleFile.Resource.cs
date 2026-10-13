using System.Text;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives.Server;

namespace vMenu.Enhanced.Permissions.Server;

public static partial class PermissionsExampleFile
{
    public static void Write()
    {
        var resource = API.GetCurrentResourceName();
        var path = PermissionsExample.ResourcePath;

        var bytes = Encoding.UTF8.GetBytes(Render());

        if (NativeFixer.SaveResourceFile(resource, path, bytes))
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
