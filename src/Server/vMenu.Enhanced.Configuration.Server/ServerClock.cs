using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Ticks.Server;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Configuration.Server;

/// <summary>
/// Sends the server's UTC time to every client via convars to stay in sync for time and weather.
/// </summary>
public static class ServerClock
{
    private const string DumpCommand = "vmenu_clock";

    private const int PublishIntervalMs = 1000;

    /// <summary>Handled by server/host_clock.lua, which writes the convar named in the argument.</summary>
    // DateTime.UtcNow throws in the C# server runtime, which has no implementation for the Windows
    // call .NET uses to detect leap second support, so the write has to happen in Lua.
    private const string PublishEvent = "vMenu.Enhanced:Clock:Publish";

    // The tick publishes once from onStarted and again on its first iteration with nothing in
    // between, so the same second legitimately comes back twice while the server is coming up.
    private const int StaleChecksBeforeWarning = 5;

    private static bool _emitted;

    private static long _lastPublished;

    private static int _staleChecks;

    private static bool _stalled;

    public static void Initialize()
    {
        // onStarted rather than here, so nothing is published while both sync features are off.
        ServerTickRegistry.Register(
            "Clock.Publish",
            Publish,
            TickRate.Every(PublishIntervalMs),
            condition: IsNeeded,
            onStarted: Publish,
            onStopped: Reset);

        SharedAPI.Commands.RegisterCommand(DumpCommand, true, DebugCommands.Gate(Dump));
    }

    public static void Dump()
    {
        if (!IsNeeded())
        {
            API.Log.Info("[Clock] Both weather and time sync are off, so no world time is published.");

            return;
        }

        if (!TryPublishedUnixSeconds(out var now))
        {
            API.Log.Warn($"[Clock] Nothing has been published to {WorldStateConvars.Utc} yet.");

            return;
        }

        var utcSecondOfDay = (int)(now % 86400L);
        var secondOfDay = GameClock.SecondOfDay(now);
        var cycle = GameClock.CycleGameHours(now);
        var resolved = WeatherCycle.Resolve(cycle);

        API.Log.Info(
            $"[Clock] UTC {now} ({utcSecondOfDay / 3600:00}:{utcSecondOfDay % 3600 / 60:00}:{utcSecondOfDay % 60:00})");
        API.Log.Info(
            $"[Clock]   in-game {(int)(secondOfDay / 3600):00}:{(int)(secondOfDay % 3600 / 60):00}, " +
            $"cycle {cycle.ToString("0.##", CultureInfo.InvariantCulture)} of {GameClock.GameHoursPerCycle} hours");
        API.Log.Info(
            $"[Clock]   weather {WeatherTypes.NameOf(resolved.Current)}, then " +
            $"{WeatherTypes.NameOf(resolved.Next)} in " +
            $"{(resolved.GameHoursUntilNext * GameClock.RealSecondsPerGameHour / 60.0).ToString("0.#", CultureInfo.InvariantCulture)} real minutes");
    }

    /// <summary>The clock feeds both features, so it runs while either one wants it.</summary>
    private static bool IsNeeded() =>
        ServerConfig.Value(WeatherOptionsSettings.Enabled) || ServerConfig.Value(TimeOptionsSettings.Enabled);

    private static bool TryPublishedUnixSeconds(out long unixSeconds) =>
        WorldStateConvars.TryParseUnix(Native.GetConvar(WorldStateConvars.Utc, string.Empty), out unixSeconds);

    private static void Publish()
    {
        VerifyPreviousPublish();

        API.EmitLocal(PublishEvent, WorldStateConvars.Utc);

        _emitted = true;
    }

    // The event is fire and forget, so the convar moving on is the only sign the handler ran. Checked
    // one publish late on purpose: the handler may not have run by the time the emit call returns.
    private static void VerifyPreviousPublish()
    {
        if (!_emitted)
        {
            return;
        }

        if (TryPublishedUnixSeconds(out var published) && published > _lastPublished)
        {
            _lastPublished = published;
            _staleChecks = 0;

            if (_stalled)
            {
                _stalled = false;

                API.Log.Info("[Clock] The server clock is being published again.");
            }

            return;
        }

        if (++_staleChecks >= StaleChecksBeforeWarning)
        {
            ReportStalled(published);
        }
    }

    // Once per outage rather than once per second, so a broken clock does not bury the console.
    private static void ReportStalled(long published)
    {
        if (_stalled)
        {
            return;
        }

        _stalled = true;

        API.Log.Error(
            "[Clock] The world time is not being published, so weather and time are not synced: " +
            (published > 0L
                ? $"{WorldStateConvars.Utc} has been stuck at {published} for {_staleChecks} seconds."
                : $"{WorldStateConvars.Utc} has never been set."));
        API.Log.Error(
            $"[Clock] Nothing is handling '{PublishEvent}'. Check that server/host_clock.lua exists in " +
            "the resource and is listed in fxmanifest.lua.");
    }

    // Otherwise a restart would compare against a value from before the stop and read as stuck.
    private static void Reset()
    {
        _emitted = false;
        _lastPublished = 0L;
        _staleChecks = 0;
    }
}
