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
    // NetworkOverrideClockTime takes whole seconds and the clock runs at 30x, so a clock that is only
    // keeping up with the server has nothing new to send more often than this.
    private const int SteadyIntervalMs = 100;

    private static long _shownDay = long.MinValue;

    private static double _shownOffset;
    private static double _rampFrom;
    private static double _rampTo;
    private static int _rampStartMs;
    private static bool _started;
    private static bool _ramping;

    /// <summary>Whether the server wants a shared clock at all.</summary>
    // See the matching note in WorldWeather: the convar decides, never a permission.
    private static bool IsEnabled() => ClientConfig.Value(TimeOptionsSettings.Enabled);

    public static void Initialize()
    {
        var tick = TickRegistry.Register(
            "World.Time",
            Apply,
            // A manual time change sweeps hours in a couple of seconds, and at the steady rate that
            // sweep is visibly steppy, so the ramp gets a frame each instead.
            TickRate.Varying(() => _ramping ? TickRate.PerFrame : TickRate.Every(SteadyIntervalMs)),
            IsEnabled,
            onStarted: () =>
            {
                _started = false;
                _ramping = false;
                _shownDay = long.MinValue;
            },
            onStopped: Native.NetworkClearClockTimeOverride);

        ClientConfig.AddEventListenerFor([TimeOptionsSettings.Enabled], tick.Reevaluate);

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

    public static double MoonCycleDays => GameDayNow() - MoonCycle.EpochDay + FractionOfDayNow();

    /// <summary>Where the moon is, worked out from the date the game is holding right now.</summary>
    // Read back out of the natives rather than from _shownDay, for the same reason Describe is: this
    // is the number the sky is actually being drawn from, not the one vMenu believes it sent.
    public static string DescribeMoon()
    {
        var cycleDays = MoonCycleDays;

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

        Native.SetMillisecondsPerGameMinute((int)(2000 / GameClock.ClampSpeed(WorldState.TimeSpeed)));
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
            _ramping = false;
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
            _ramping = true;
        }

        var seconds = Math.Max(0, WorldState.TimeTransitionSeconds);

        if (seconds <= 0)
        {
            _ramping = false;
            _shownOffset = _rampTo;

            return _shownOffset;
        }

        var progress = (Native.GetGameTimer() - _rampStartMs) / 1000.0 / seconds;

        if (progress >= 1.0)
        {
            _ramping = false;
            _shownOffset = _rampTo;

            return _shownOffset;
        }

        _shownOffset = _rampFrom + (Shortest(_rampTo - _rampFrom) * Smooth(progress));

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
