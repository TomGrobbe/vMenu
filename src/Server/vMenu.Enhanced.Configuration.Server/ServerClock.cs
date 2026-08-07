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

    private const string ResetCommand = "vmenu_resettime";

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

        // Not behind the debug gate: this one changes the world rather than reporting on it, and an
        // owner who has just raised the speed needs it whether or not they are debugging.
        SharedAPI.Commands.RegisterCommand(ResetCommand, true, new Action(ResetToRealTime));
    }

    /// <summary>
    /// The offset that lands the clock back on the time it would be showing at normal speed, which is
    /// what both the menu's reset button and <c>vmenu_resettime</c> ask for.
    /// </summary>
    // Worked out here because the published time and the speed convar both live here, and because a
    // client working it out for itself would use its own slightly older idea of what time it is.
    public static int RealTimeOffset() =>
        TryPublishedUnixSeconds(out var now) ? (int)GameClock.RealTimeOffset(now, Speed()) : 0;

    /// <summary>The console side of the menu's reset button.</summary>
    public static void ResetToRealTime()
    {
        if (!ServerConfig.Value(TimeOptionsSettings.Enabled))
        {
            API.Log.Warn(
                "[Clock] Time sync is off, so vMenu is not driving the clock and there is nothing " +
                $"to reset. Turn on {TimeOptionsSettings.Enabled.Name} first.");

            return;
        }

        var offset = RealTimeOffset();

        ServerState.SetTimeOffset(offset);

        API.Log.Info(
            offset == 0
                ? "[Clock] The clock is back on the server's own time."
                : $"[Clock] The clock is back on the server's own time, which needed a {offset}s offset " +
                  "because it is not running at normal speed. A sped up clock pulls away from real " +
                  "time again from here, so run this whenever it needs lining back up.");
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

        var speed = Speed();
        var offset = Offset();
        var utcSecondOfDay = (int)(now % 86400L);
        var secondOfDay = GameClock.Mod(GameClock.SecondOfDay(now, speed) + offset, GameClock.SecondsPerGameDay);
        var cycle = GameClock.CycleGameHours(now, speed);
        var resolved = WeatherCycle.Resolve(cycle);

        API.Log.Info(
            $"[Clock] UTC {now} ({utcSecondOfDay / 3600:00}:{utcSecondOfDay % 3600 / 60:00}:{utcSecondOfDay % 60:00})");
        API.Log.Info(
            $"[Clock]   in-game {(int)(secondOfDay / 3600):00}:{(int)(secondOfDay % 3600 / 60):00} " +
            $"(offset {offset}s), " +
            $"cycle {cycle.ToString("0.##", CultureInfo.InvariantCulture)} of {GameClock.GameHoursPerCycle} hours, " +
            $"at {speed.ToString("0.###", CultureInfo.InvariantCulture)}x speed");
        API.Log.Info(
            $"[Clock]   weather {WeatherTypes.NameOf(resolved.Current)}, then " +
            $"{WeatherTypes.NameOf(resolved.Next)} in " +
            $"{(resolved.GameHoursUntilNext * GameClock.RealSecondsPerGameHourAt(speed) / 60.0).ToString("0.#", CultureInfo.InvariantCulture)} real minutes");

        DumpDate(now, speed, offset, secondOfDay);
    }

    /// <summary>The date and moon every client should be on, which is the sum of everything above.</summary>
    // Worked out here rather than read back from a player, so a report of "the moon is wrong" can be
    // checked against what the server itself thinks without anybody being in game.
    private static void DumpDate(long now, double speed, int offset, double secondOfDay)
    {
        var day = (long)GameClock.Mod(GameClock.GameDay(now, offset, speed), MoonCycle.PeriodDays);
        var cycleDays = day + (secondOfDay / GameClock.SecondsPerGameDay);

        CivilTime.FromDays(MoonCycle.EpochDay + day, out var year, out var month, out var dayOfMonth);

        API.Log.Info(
            $"[Clock]   day {day} of the {MoonCycle.PeriodDays} day loop, " +
            $"{MoonCycle.WeekdayOf(MoonCycle.EpochDay + day)} {dayOfMonth:00}/{month:00}/{year}");
        API.Log.Info(
            $"[Clock]   moon {MoonCycle.DayOfCycle(cycleDays).ToString("0.##", CultureInfo.InvariantCulture)} of " +
            $"{MoonCycle.CycleDays.ToString("0", CultureInfo.InvariantCulture)} days through the cycle, " +
            $"{MoonCycle.NameOf(cycleDays)}, " +
            $"{(MoonCycle.Illumination(cycleDays) * 100.0).ToString("0", CultureInfo.InvariantCulture)}% lit, " +
            $"angle {MoonCycle.Degrees(cycleDays).ToString("0.#", CultureInfo.InvariantCulture)} degrees");
    }

    private static int Offset() =>
        WorldStateConvars.TryParseOffset(Native.GetConvar(WorldStateConvars.TimeOffset, "0"), out var seconds)
            ? seconds
            : 0;

    private static double Speed() =>
        GameClock.ClampSpeed(ServerConfig.Value(TimeOptionsSettings.SpeedMultiplier));

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
