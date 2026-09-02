using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Actions.Server;

// An allowance shared by a group of actions: so many of them per player, per stretch of time.
public sealed class ActionRateLimit(string name, IntSetting allowance, IntSetting windowSeconds)
{
    private readonly Dictionary<int, Recent> _byPlayer = [];

    public string Name { get; } = name;

    public int Allowance => ServerConfig.Value(allowance);

    public bool TryTake(Player source, out int retryAfterSeconds)
    {
        retryAfterSeconds = 0;

        var limit = ServerConfig.Value(allowance);
        var window = ServerConfig.Value(windowSeconds) * 1000;

        if (limit <= 0 || window <= 0)
        {
            if (_byPlayer.Count > 0)
            {
                _byPlayer.Clear();
            }

            return true;
        }

        var now = Native.GetGameTimer();

        Sweep(now, window);

        if (!_byPlayer.TryGetValue(source.Handle, out var recent))
        {
            recent = new Recent();

            _byPlayer[source.Handle] = recent;
        }

        if (recent.Stamps.Count < limit)
        {
            recent.Stamps.Add(now);

            return true;
        }

        retryAfterSeconds = Math.Max(1, (int)Math.Ceiling((window - (now - recent.Stamps[0])) / 1000f));

        ReportOnce(source, recent, now, limit, window, retryAfterSeconds);

        return false;
    }

    public void Forget(int serverId) => _byPlayer.Remove(serverId);

    private void ReportOnce(Player source, Recent recent, int now, int limit, int window, int retryAfterSeconds)
    {
        if (recent.Reported && now - recent.ReportedAt < window)
        {
            return;
        }

        recent.Reported = true;
        recent.ReportedAt = now;

        Log.Info(
            $"[Actions] {source.Name} ({source.Handle}) is over the {Name} limit of {limit} action(s) "
            + $"per {window / 1000}s. Refusing for another {retryAfterSeconds}s.");
    }

    private void Sweep(int now, int window)
    {
        List<int>? finished = null;

        foreach (var pair in _byPlayer)
        {
            var stamps = pair.Value.Stamps;

            while (stamps.Count > 0 && now - stamps[0] >= window)
            {
                stamps.RemoveAt(0);
            }

            if (stamps.Count == 0)
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
            _byPlayer.Remove(serverId);
        }
    }

    private sealed class Recent
    {
        public List<int> Stamps { get; } = [];

        public bool Reported { get; set; }

        public int ReportedAt { get; set; }
    }
}
