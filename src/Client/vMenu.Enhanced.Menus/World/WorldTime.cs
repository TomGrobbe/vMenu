using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Ticks;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;

namespace vMenu.Enhanced.Menus.World;

/// <summary>Drives the in-game clock from the server's time.</summary>
public static class WorldTime
{
    // NetworkOverrideClockTime takes whole seconds and the clock runs at 30x, so the value it is
    // given can only change once every 33ms. Running per frame just re-sent a number that had not
    // moved. At this rate the sun advances a fiftieth of a degree per step, which cannot be seen.
    // A speed multiplier makes each step that much bigger, which is the point of raising it.
    private const int IntervalMs = 50;

    private static long _shownDay = long.MinValue;

    private static double _shownOffset;
    private static double _rampFrom;
    private static double _rampTo;
    private static int _rampStartMs;
    private static bool _started;

    /// <summary>Whether the server wants a shared clock at all.</summary>
    // See the matching note in WorldWeather: the convar decides, never a permission.
    private static bool IsEnabled() => ClientConfig.Value(TimeOptionsSettings.Enabled);

    public static void Initialize()
    {
        TickRegistry.Register(
            "World.Time",
            Apply,
            TickRate.Every(IntervalMs),
            IsEnabled,
            onStarted: () =>
            {
                _started = false;
                _shownDay = long.MinValue;
            },
            onStopped: Native.NetworkClearClockTimeOverride);

        WorldState.Changed += TickRegistry.Reevaluate;
    }

    /// <summary>What the game itself reports, which is the only proof the clock calls are landing.</summary>
    public static string Describe() =>
        $"{Native.GetClockHours():00}:{Native.GetClockMinutes():00}:{Native.GetClockSeconds():00} on " +
        $"{MoonCycle.WeekdayOf(GameDayNow())} " +
        $"{Native.GetClockDayOfMonth():00}/{Native.GetClockMonth() + 1:00}/{Native.GetClockYear()}";

    /// <summary>The day vMenu handed to the game, which is what the whole date and moon rest on.</summary>
    public static string DescribeDate() =>
        _shownDay == long.MinValue
            ? "no date has been set yet"
            : $"vMenu set day {_shownDay} of the {MoonCycle.PeriodDays} day loop, " +
              $"game reports day {GameDayNow() - MoonCycle.EpochDay}";

    /// <summary>Where the moon is, worked out from the date the game is holding right now.</summary>
    // Read back out of the natives rather than from _shownDay, for the same reason Describe is: this
    // is the number the sky is actually being drawn from, not the one vMenu believes it sent.
    public static string DescribeMoon()
    {
        var cycleDays = GameDayNow() - MoonCycle.EpochDay + FractionOfDayNow();

        return $"{MoonCycle.DayOfCycle(cycleDays).ToString("0.##", CultureInfo.InvariantCulture)} of " +
            $"{MoonCycle.CycleDays.ToString("0", CultureInfo.InvariantCulture)} days through the cycle, " +
            $"{MoonCycle.NameOf(cycleDays)}, " +
            $"{(MoonCycle.Illumination(cycleDays) * 100.0).ToString("0", CultureInfo.InvariantCulture)}% lit, " +
            $"angle {MoonCycle.Degrees(cycleDays).ToString("0.#", CultureInfo.InvariantCulture)} degrees";
    }

    /// <summary>The date the game holds, as days since the Unix epoch.</summary>
    private static long GameDayNow() =>
        CivilTime.ToUnixSeconds(
            Native.GetClockYear(),
            Native.GetClockMonth() + 1,
            Native.GetClockDayOfMonth(),
            0,
            0,
            0) / GameClock.SecondsPerGameDay;

    private static double FractionOfDayNow() =>
        ((Native.GetClockHours() * 3600.0) + (Native.GetClockMinutes() * 60.0) + Native.GetClockSeconds())
        / GameClock.SecondsPerGameDay;

    private static void Apply()
    {
        if (!WorldState.HasClock)
        {
            return;
        }

        var offset = Ramp();

        // Real seconds, not zero. Forcing them to zero is what made the legacy clock visibly step.
        var total = GameClock.Mod(
            GameClock.SecondOfDay(WorldState.UnixSeconds, WorldState.TimeSpeed) + offset,
            GameClock.SecondsPerGameDay);

        Native.NetworkOverrideClockTime((int)(total / 3600), (int)(total % 3600 / 60), (int)(total % 60));

        ApplyDate(offset);
    }

    /// <summary>
    /// Sets the in-game date, which nothing else does and which the moon's position depends on.
    /// </summary>
    private static void ApplyDate(double offset)
    {
        var day = (long)GameClock.Mod(
            GameClock.GameDay(WorldState.UnixSeconds, offset, WorldState.TimeSpeed),
            MoonCycle.PeriodDays);

        if (day == _shownDay)
        {
            return;
        }

        _shownDay = day;

        CivilTime.FromDays(MoonCycle.EpochDay + day, out var year, out var month, out var dayOfMonth);

        Native.SetClockDate(dayOfMonth, month - 1, year);
    }

    /// <summary>Eases the displayed offset toward the server's, so a jump reads as a time lapse.</summary>
    private static double Ramp()
    {
        var target = WorldState.TimeOffsetSeconds;

        if (!_started)
        {
            _started = true;
            _shownOffset = target;
            _rampFrom = target;
            _rampTo = target;

            return _shownOffset;
        }

        if (Math.Abs(target - _rampTo) > 0.5)
        {
            _rampFrom = _shownOffset;
            _rampTo = target;
            _rampStartMs = Native.GetGameTimer();
        }

        var seconds = Math.Max(0, ClientConfig.Value(TimeOptionsSettings.TransitionSeconds));

        if (seconds <= 0)
        {
            _shownOffset = _rampTo;

            return _shownOffset;
        }

        var progress = unchecked(Native.GetGameTimer() - _rampStartMs) / 1000.0 / seconds;

        _shownOffset = progress >= 1.0
            ? _rampTo
            : _rampFrom + (Shortest(_rampTo - _rampFrom) * Smooth(progress));

        return _shownOffset;
    }

    /// <summary>Nine hours back sweeps back, not fifteen forward.</summary>
    private static double Shortest(double delta)
    {
        var wrapped = GameClock.Mod(delta, GameClock.SecondsPerGameDay);

        return wrapped > GameClock.SecondsPerGameDay / 2.0
            ? wrapped - GameClock.SecondsPerGameDay
            : wrapped;
    }

    private static double Smooth(double t) => t * t * (3.0 - (2.0 * t));
}
