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

        // Not behind the debug gate: this one changes the world rather than reporting on it, and an owner
        // who has just raised the speed needs it whether or not they are debugging.
        SharedAPI.Commands.RegisterCommand(ResetCommand, true, new Action(ResetToRealTime));
    }

    // The offset that lands the clock back on the time it would be showing at normal speed, which is
    // what both the menu's reset button and vmenu_resettime ask for. Worked out here because the
    // published time and the speed convar both live here, and because a client working it out for itself
    // would use its own slightly older idea of what time it is.
    public static int RealTimeOffset() => (int)GameClock.RealTimeOffset(Now(), Speed());

    public static double Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

    // The console side of the menu's reset button.
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

        ServerState.SetTimeOffsetRunning(offset);

        Log.Debug(
            offset == 0
                ? "[Clock] The clock is back on the server's own time."
                : $"[Clock] The clock is back on the server's own time, which needed a {offset}s offset " +
                  "because it is not running at normal speed. A sped up clock pulls away from real " +
                  "time again from here, so run this whenever it needs lining back up.");
    }

    public static double InGameSecondOfDay()
    {
        var speed = Speed();
        var clock = ServerState.FrozenAtUnix ?? Now();

        return GameClock.Mod(GameClock.SecondOfDay(clock, speed) + Offset(), GameClock.SecondsPerGameDay);
    }

    public static void Dump()
    {
        if (!IsNeeded())
        {
            Log.Info("[Clock] Both weather and time sync are off, so no world time is published.");

            return;
        }

        if (!TryPublishedUnixSeconds(out _))
        {
            Log.Warning($"[Clock] Nothing has been published to {WorldStateConvars.Utc} yet.");

            return;
        }

        var world = WorldSnapshot.Capture(0);
        var utcSecondOfDay = (int)(world.Utc % 86400L);

        Log.Info(
            $"[Clock] UTC {world.Utc} ({utcSecondOfDay / 3600:00}:{utcSecondOfDay % 3600 / 60:00}:{utcSecondOfDay % 60:00})"
            + (world.Clock.FrozenAtUnix is { } pinned ? $", FROZEN at unix {pinned:0.000}" : string.Empty));
        Log.Info(
            $"[Clock]   in-game {world.Clock.Hour:00}:{world.Clock.Minute:00} " +
            $"(offset {world.Clock.OffsetSeconds}s), " +
            $"cycle {world.Weather.CycleGameHours.ToString("0.##", CultureInfo.InvariantCulture)} of {world.Weather.CycleLengthGameHours} hours, " +
            $"at {world.Clock.Speed.ToString("0.###", CultureInfo.InvariantCulture)}x speed");
        Log.Info(
            $"[Clock]   weather {world.Weather.Current}, then " +
            $"{world.Weather.Next} in " +
            $"{(world.Weather.RealSecondsUntilNext / 60.0).ToString("0.#", CultureInfo.InvariantCulture)} real minutes");

        DumpDate(world);
    }

    private static void DumpDate(WorldSnapshot world)
    {
        Log.Info(
            $"[Clock]   day {world.Date.DayOfLoop} of the {world.Date.LoopDays} day loop, " +
            $"{world.Date.Weekday} {world.Date.Day:00}/{world.Date.Month:00}/{world.Date.Year}");
        Log.Info(
            $"[Clock]   moon {world.Moon.DayOfCycle.ToString("0.##", CultureInfo.InvariantCulture)} of " +
            $"{world.Moon.CycleDays.ToString("0", CultureInfo.InvariantCulture)} days through the cycle, " +
            $"{world.Moon.Phase}, " +
            $"{(world.Moon.Illumination * 100.0).ToString("0", CultureInfo.InvariantCulture)}% lit, " +
            $"angle {world.Moon.AngleDegrees.ToString("0.#", CultureInfo.InvariantCulture)} degrees");
    }

    private static int Offset() =>
        WorldStateConvars.TryParseTime(Native.GetConvar(WorldStateConvars.TimeOffset, "0"), out var seconds, out _)
            ? seconds
            : 0;

    public static double Speed() =>
        GameClock.ClampSpeed(ServerConfig.Value(TimeOptionsSettings.SpeedMultiplier));

    // The clock feeds both features, so it runs while either one wants it.
    private static bool IsNeeded() =>
        ServerConfig.Value(WeatherOptionsSettings.Enabled) || ServerConfig.Value(TimeOptionsSettings.Enabled);

    private static bool TryPublishedUnixSeconds(out long unixSeconds) =>
        WorldStateConvars.TryParseUnix(Native.GetConvar(WorldStateConvars.Utc, string.Empty), out unixSeconds);

    // os.time()'s replacement now that the runtime's clock works: UTC seconds since the Unix epoch, the
    // same number in every timezone, which is what the client reads back. SetConvarReplicated carries it
    // to every client.
    private static void Publish() =>
        Native.SetConvarReplicated(
            WorldStateConvars.Utc,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
}
