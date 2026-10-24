using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Configuration.Server;

using LoggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Logging;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives.Server;

namespace vMenu.Enhanced.Webhooks.Server;

// Lives at the sink rather than at each caller, so every project that reports a security line shares
// one allowance per player.
internal static class SecurityThrottle
{
    private static readonly PlayerWindows ByPlayer = new();

    internal static bool TryTake(int serverId)
    {
        var limit = ServerConfig.Value(LoggingSettings.SecurityLimit);
        var window = ServerConfig.Value(LoggingSettings.SecurityLimitSeconds) * 1000;

        if (limit <= 0 || window <= 0)
        {
            ByPlayer.Clear();

            return true;
        }

        var now = NativeFixer.GetGameTimer();
        var recent = ByPlayer.For(serverId, now, window);

        if (recent.Count >= limit)
        {
            recent.Suppressed++;

            return false;
        }

        recent.Add(now);

        return true;
    }

    internal static int TakeSuppressed(int serverId)
    {
        if (ByPlayer.Find(serverId) is not { Suppressed: > 0 } recent)
        {
            return 0;
        }

        var suppressed = recent.Suppressed;

        recent.Suppressed = 0;

        return suppressed;
    }

    internal static void Forget(int serverId) => ByPlayer.Forget(serverId);
}
