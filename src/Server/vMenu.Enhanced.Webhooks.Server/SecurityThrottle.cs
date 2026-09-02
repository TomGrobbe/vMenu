using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Configuration.Server;

using LoggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Logging;

namespace vMenu.Enhanced.Webhooks.Server;

// Lives at the sink rather than at each caller, so every project that reports a security line shares
// one allowance per player.
internal static class SecurityThrottle
{
    private static readonly Dictionary<int, Recent> ByPlayer = [];

    internal static bool TryTake(int serverId)
    {
        var limit = ServerConfig.Value(LoggingSettings.SecurityLimit);
        var window = ServerConfig.Value(LoggingSettings.SecurityLimitSeconds) * 1000;

        if (limit <= 0 || window <= 0)
        {
            if (ByPlayer.Count > 0)
            {
                ByPlayer.Clear();
            }

            return true;
        }

        var now = Native.GetGameTimer();

        Sweep(now, window);

        if (!ByPlayer.TryGetValue(serverId, out var recent))
        {
            recent = new Recent();

            ByPlayer[serverId] = recent;
        }

        if (recent.Stamps.Count >= limit)
        {
            recent.Suppressed++;

            return false;
        }

        recent.Stamps.Add(now);

        return true;
    }

    internal static int TakeSuppressed(int serverId)
    {
        if (!ByPlayer.TryGetValue(serverId, out var recent) || recent.Suppressed == 0)
        {
            return 0;
        }

        var suppressed = recent.Suppressed;

        recent.Suppressed = 0;

        return suppressed;
    }

    internal static void Forget(int serverId) => ByPlayer.Remove(serverId);

    private static void Sweep(int now, int window)
    {
        List<int>? finished = null;

        foreach (var pair in ByPlayer)
        {
            var recent = pair.Value;

            while (recent.Stamps.Count > 0 && now - recent.Stamps[0] >= window)
            {
                recent.Stamps.RemoveAt(0);
            }

            // Kept while anything is still owed a mention, or the count that says how much was left
            // out would go missing along with the entry.
            if (recent.Stamps.Count == 0 && recent.Suppressed == 0)
            {
                finished ??= [];

                finished.Add(pair.Key);
            }
        }

        if (finished is null)
        {
            return;
        }

        foreach (var serverId in finished)
        {
            ByPlayer.Remove(serverId);
        }
    }

    private sealed class Recent
    {
        public List<int> Stamps { get; } = [];

        public int Suppressed { get; set; }
    }
}
