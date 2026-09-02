using vMenu.Enhanced.Data.World;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Configuration.Server;

public sealed class WorldSyncState
{
    public required bool Weather { get; init; }

    public required bool Time { get; init; }
}

public sealed class WorldClockState
{
    public required double Speed { get; init; }

    public required bool Frozen { get; init; }

    public required double? FrozenAtUnix { get; init; }

    public required int OffsetSeconds { get; init; }

    public required double SecondOfDay { get; init; }

    public required int Hour { get; init; }

    public required int Minute { get; init; }

    public required int Second { get; init; }

    public required double RealSecondsPerGameHour { get; init; }
}

public sealed class WorldDateState
{
    public required long DayOfLoop { get; init; }

    public required long LoopDays { get; init; }

    public required int Year { get; init; }

    public required int Month { get; init; }

    public required int Day { get; init; }

    public required string Weekday { get; init; }
}

public sealed class WorldMoonState
{
    public required double DayOfCycle { get; init; }

    public required double CycleDays { get; init; }

    public required string Phase { get; init; }

    public required double Illumination { get; init; }

    public required double AngleDegrees { get; init; }
}

public sealed class WorldWeatherState
{
    public required string? Override { get; init; }

    public required string Scheduled { get; init; }

    public required string Current { get; init; }

    public required string Next { get; init; }

    public required double GameHoursUntilNext { get; init; }

    public required double RealSecondsUntilNext { get; init; }

    public required double CycleGameHours { get; init; }

    public required double CycleLengthGameHours { get; init; }

    public required string Blackout { get; init; }

    public required string Snow { get; init; }

    public required bool SnowFalling { get; init; }
}

public sealed class WorldForecastEntry
{
    public required string Type { get; init; }

    public required double GameHoursUntilStart { get; init; }

    public required double RealSecondsUntilStart { get; init; }

    public required double GameHoursLong { get; init; }

    public required double RealSecondsLong { get; init; }
}

// One place, so the console dump and the HTTP endpoint can never disagree about the world.
public sealed class WorldSnapshot
{
    public required long Utc { get; init; }

    public required WorldSyncState Sync { get; init; }

    public required WorldClockState Clock { get; init; }

    public required WorldDateState Date { get; init; }

    public required WorldMoonState Moon { get; init; }

    public required WorldWeatherState Weather { get; init; }

    public required IReadOnlyList<WorldForecastEntry> Forecast { get; init; }

    public static WorldSnapshot Capture(int forecastCount)
    {
        var now = ServerClock.Now();
        var speed = ServerClock.Speed();
        var realSecondsPerGameHour = GameClock.RealSecondsPerGameHourAt(speed);
        var offset = ServerState.TimeOffsetSeconds;
        var secondOfDay = ServerClock.InGameSecondOfDay();

        // The live clock, not the pinned one: freezing the time of day does not stop the schedule.
        var cycleGameHours = GameClock.CycleGameHours(now, speed);
        var resolved = WeatherCycle.Resolve(cycleGameHours);
        var effective = ServerState.Weather ?? resolved.Current;

        return new WorldSnapshot
        {
            Utc = (long)now,
            Sync = new WorldSyncState
            {
                Weather = ServerConfig.Value(WeatherOptionsSettings.Enabled),
                Time = ServerConfig.Value(TimeOptionsSettings.Enabled),
            },
            Clock = new WorldClockState
            {
                Speed = speed,
                Frozen = ServerState.FrozenAtUnix.HasValue,
                FrozenAtUnix = ServerState.FrozenAtUnix,
                OffsetSeconds = offset,
                SecondOfDay = secondOfDay,
                Hour = (int)(secondOfDay / 3600.0),
                Minute = (int)(secondOfDay % 3600.0 / 60.0),
                Second = (int)(secondOfDay % 60.0),
                RealSecondsPerGameHour = realSecondsPerGameHour,
            },
            Date = CaptureDate(now, offset, speed, secondOfDay, out var cycleDays),
            Moon = new WorldMoonState
            {
                DayOfCycle = MoonCycle.DayOfCycle(cycleDays),
                CycleDays = MoonCycle.CycleDays,
                Phase = MoonCycle.NameOf(cycleDays),
                Illumination = MoonCycle.Illumination(cycleDays),
                AngleDegrees = MoonCycle.Degrees(cycleDays),
            },
            Weather = new WorldWeatherState
            {
                Override = ServerState.Weather is { } forced ? WeatherTypes.NameOf(forced) : null,
                Scheduled = WeatherTypes.NameOf(resolved.Current),
                Current = WeatherTypes.NameOf(effective),
                Next = WeatherTypes.NameOf(resolved.Next),
                GameHoursUntilNext = resolved.GameHoursUntilNext,
                RealSecondsUntilNext = resolved.GameHoursUntilNext * realSecondsPerGameHour,
                CycleGameHours = cycleGameHours,
                CycleLengthGameHours = GameClock.GameHoursPerCycle,
                Blackout = BlackoutModes.NameOf(ServerState.Blackout),
                Snow = SnowModes.NameOf(ServerState.Snow),
                SnowFalling = SnowModes.Resolve(ServerState.Snow, effective),
            },
            Forecast = CaptureForecast(cycleGameHours, forecastCount, realSecondsPerGameHour),
        };
    }

    private static WorldDateState CaptureDate(
        double now,
        int offset,
        double speed,
        double secondOfDay,
        out double cycleDays)
    {
        // The pinned clock, so a frozen server does not roll over to tomorrow.
        var clock = ServerState.FrozenAtUnix ?? now;
        var day = (long)GameClock.Mod(GameClock.GameDay(clock, offset, speed), MoonCycle.PeriodDays);

        cycleDays = day + (secondOfDay / GameClock.SecondsPerGameDay);

        CivilTime.FromDays(MoonCycle.EpochDay + day, out var year, out var month, out var dayOfMonth);

        return new WorldDateState
        {
            DayOfLoop = day,
            LoopDays = MoonCycle.PeriodDays,
            Year = year,
            Month = month,
            Day = dayOfMonth,
            Weekday = MoonCycle.WeekdayOf(MoonCycle.EpochDay + day),
        };
    }

    private static List<WorldForecastEntry> CaptureForecast(
        double cycleGameHours,
        int count,
        double realSecondsPerGameHour)
    {
        var entries = new List<WorldForecastEntry>();

        if (count <= 0)
        {
            return entries;
        }

        foreach (var entry in WeatherCycle.Forecast(cycleGameHours, count))
        {
            entries.Add(new WorldForecastEntry
            {
                Type = WeatherTypes.NameOf(entry.Type),
                GameHoursUntilStart = entry.GameHoursUntilStart,
                RealSecondsUntilStart = entry.GameHoursUntilStart * realSecondsPerGameHour,
                GameHoursLong = entry.GameHoursLong,
                RealSecondsLong = entry.GameHoursLong * realSecondsPerGameHour,
            });
        }

        return entries;
    }
}
