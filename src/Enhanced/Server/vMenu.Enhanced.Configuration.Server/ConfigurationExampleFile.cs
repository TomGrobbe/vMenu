using System.Text;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Configuration.Server;

// Writes every known setting to config/configuration.cfg.example on every start, so the reference
// can never drift from what the code actually reads.
public static class ConfigurationExampleFile
{
    public static void Write()
    {
        var resource = Native.GetCurrentResourceName();
        var path = ConfigurationExample.ResourcePath;
        var bytes = Encoding.UTF8.GetBytes(ConfigurationExample.Render());

        if (Native.SaveResourceFile(resource, path, bytes))
        {
            Log.Debug($"[Config] Wrote {path}.");
            return;
        }

        Log.Error(
            $"[Config] Could not write {path}. Add "
            + $"'add_filesystem_permission {resource} write {resource}' to your server.cfg, above the "
            + $"line that starts {resource}.");
    }
}
