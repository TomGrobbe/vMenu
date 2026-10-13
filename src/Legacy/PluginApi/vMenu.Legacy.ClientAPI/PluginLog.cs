using CitizenFX.Core;

namespace vMenu.Enhanced.ClientAPI;

internal static class PluginLog
{
    internal static void Warn(string message) => Debug.WriteLine($"^3[WARN] {message}^7");

    internal static void Error(string message) => Debug.WriteLine($"^1[ERROR] {message}^7");
}
