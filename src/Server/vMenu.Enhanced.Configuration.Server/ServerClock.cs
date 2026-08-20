using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Logging;
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

    public static void Initialize()
    {
        // onStarted rather than here, so nothing is published while both sync features are off.
        var tick = ServerTickRegistry.Register(
            "Clock.Publish",
            Publish,
            TickRate.Every(PublishIntervalMs),
            condition: IsNeeded,
            onStarted: Publish);

        ServerConfig.AddEventListenerFor(
            [WeatherOptionsSettings.Enabled, TimeOptionsSettings.Enabled],
            tick.Reevaluate);

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
            Log.Warning(
                "[Clock] Time sync is off, so vMenu is not driving the clock and there is nothing " +
                $"to reset. Turn on {TimeOptionsSettings.Enabled.Name} first.");

            return;
        }

        var offset = RealTimeOffset();

        ServerState.SetTimeOffset(offset);

        Log.Debug(
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
            Log.Info("[Clock] Both weather and time sync are off, so no world time is published.");

            return;
        }

        if (!TryPublishedUnixSeconds(out var now))
        {
            Log.Warning($"[Clock] Nothing has been published to {WorldStateConvars.Utc} yet.");

            return;
        }

        var speed = Speed();
        var offset = Offset();
        var utcSecondOfDay = (int)(now % 86400L);
        var secondOfDay = GameClock.Mod(GameClock.SecondOfDay(now, speed) + offset, GameClock.SecondsPerGameDay);
        var cycle = GameClock.CycleGameHours(now, speed);
        var resolved = WeatherCycle.Resolve(cycle);

        Log.Info(
            $"[Clock] UTC {now} ({utcSecondOfDay / 3600:00}:{utcSecondOfDay % 3600 / 60:00}:{utcSecondOfDay % 60:00})");
        Log.Info(
            $"[Clock]   in-game {(int)(secondOfDay / 3600):00}:{(int)(secondOfDay % 3600 / 60):00} " +
            $"(offset {offset}s), " +
            $"cycle {cycle.ToString("0.##", CultureInfo.InvariantCulture)} of {GameClock.GameHoursPerCycle} hours, " +
            $"at {speed.ToString("0.###", CultureInfo.InvariantCulture)}x speed");
        Log.Info(
            $"[Clock]   weather {WeatherTypes.NameOf(resolved.Current)}, then " +
            $"{WeatherTypes.NameOf(resolved.Next)} in " +
            $"{(resolved.GameHoursUntilNext * GameClock.RealSecondsPerGameHourAt(speed) / 60.0).ToString("0.#", CultureInfo.InvariantCulture)} real minutes");

        DumpDate(now, speed, offset, secondOfDay);
    }


    private static void DumpDate(long now, double speed, int offset, double secondOfDay)
    {
        var day = (long)GameClock.Mod(GameClock.GameDay(now, offset, speed), MoonCycle.PeriodDays);
        var cycleDays = day + (secondOfDay / GameClock.SecondsPerGameDay);

        CivilTime.FromDays(MoonCycle.EpochDay + day, out var year, out var month, out var dayOfMonth);

        Log.Info(
            $"[Clock]   day {day} of the {MoonCycle.PeriodDays} day loop, " +
            $"{MoonCycle.WeekdayOf(MoonCycle.EpochDay + day)} {dayOfMonth:00}/{month:00}/{year}");
        Log.Info(
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

    // os.time()'s replacement now that the runtime's clock works: UTC seconds since the Unix epoch,
    // the same number in every timezone, which is what the client reads back. SetConvarReplicated
    // carries it to every client.
    private static void Publish() =>
        Native.SetConvarReplicated(
            WorldStateConvars.Utc,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
}
