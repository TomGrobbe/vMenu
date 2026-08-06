using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

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

    public static void Initialize()
    {
        // onStarted rather than here, so nothing is published while both sync features are off.
        ServerTickRegistry.Register(
            "Clock.Publish",
            Publish,
            TickRate.Every(PublishIntervalMs),
            condition: IsNeeded,
            onStarted: Publish);

        SharedAPI.Commands.RegisterCommand(DumpCommand, true, new Action(Dump));
    }

    public static void Dump()
    {
        if (!IsNeeded())
        {
            API.Log.Info("[Clock] Both weather and time sync are off, so no world time is published.");

            return;
        }

        var now = CurrentUnixSeconds();
        var secondOfDay = GameClock.SecondOfDay(now);
        var cycle = GameClock.CycleGameHours(now);
        var resolved = WeatherCycle.Resolve(cycle);

        API.Log.Info($"[Clock] UTC {now} ({DateTimeOffset.FromUnixTimeSeconds(now).UtcDateTime:HH:mm:ss})");
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

    private static long CurrentUnixSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private static void Publish() =>
        Native.SetConvarReplicated(WorldStateConvars.Utc, WorldStateConvars.FormatUnix(CurrentUnixSeconds()));
}
