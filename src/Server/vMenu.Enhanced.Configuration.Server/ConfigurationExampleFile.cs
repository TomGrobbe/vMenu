using CitizenFX.FiveM.Server;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Data.Configuration;

namespace vMenu.Enhanced.Configuration.Server;

/// <summary>
/// Writes every known setting to <c>config/configuration.cfg.example</c> on every start, so the
/// reference can never drift from what the code actually reads.
/// </summary>
public static class ConfigurationExampleFile
{
    public static void Write()
    {
        var resource = Native.GetCurrentResourceName();
        var path = ConfigurationExample.ResourcePath;

        if (NativeFixer.SaveResourceFile(resource, path, ConfigurationExample.Render()))
        {
            API.Log.Info($"[Config] Wrote {path}.");
            return;
        }

        API.Log.Error(
            $"[Config] Could not write {path}. Add "
            + $"'add_filesystem_permission {resource} write {resource}' to your server.cfg, above the "
            + $"line that starts {resource}.");
    }
}
