using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Actions.Server;

// An allowance shared by a group of actions: so many of them per player, per stretch of time.
public sealed class ActionRateLimit(string name, IntSetting allowance, IntSetting windowSeconds)
{
    private readonly PlayerWindows _windows = new();

    public string Name { get; } = name;

    public int Allowance => ServerConfig.Value(allowance);

    public bool TryTake(Player source, out int retryAfterSeconds)
    {
        retryAfterSeconds = 0;

        var limit = ServerConfig.Value(allowance);
        var window = ServerConfig.Value(windowSeconds) * 1000;

        if (limit <= 0 || window <= 0)
        {
            _windows.Clear();

            return true;
        }

        var now = Native.GetGameTimer();
        var recent = _windows.For(source.Handle, now, window);

        if (recent.Count < limit)
        {
            recent.Add(now);

            return true;
        }

        retryAfterSeconds = Math.Max(1, (int)Math.Ceiling((window - (now - recent.Oldest)) / 1000f));

        if (recent.FirstReportIn(now, window))
        {
            Report(source, limit, window, retryAfterSeconds);
        }

        return false;
    }

    public void Forget(int serverId) => _windows.Forget(serverId);

    private void Report(Player source, int limit, int window, int retryAfterSeconds)
    {
        Log.Info(
            $"[Actions] {source.Name} ({source.Handle}) is over the {Name} limit of {limit} action(s) "
            + $"per {window / 1000}s. Refusing for another {retryAfterSeconds}s.");
    }
}
