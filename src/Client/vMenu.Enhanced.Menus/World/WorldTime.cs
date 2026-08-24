using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Ticks;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;

namespace vMenu.Enhanced.Menus.World;

public static class WorldTime
{
    // NetworkOverrideClockTime takes whole seconds and the clock runs at 30x, so a clock that is only
    // keeping up with the server has nothing new to send more often than this.
    private const int SteadyIntervalMs = 100;

    private const int NormalMsPerGameMinute = 2000;

    private static long _shownDay = long.MinValue;

    private static double _shownOffset;
    private static double _rampFrom;
    private static double _rampTo;
    private static int _rampStartMs;
    private static bool _started;
    private static bool _ramping;
    private static bool _wasFrozen;
    private static bool _catchingUp;
    private static double _catchUpFrom;
    private static double _frozenAnchor;
    private static int _catchUpStartMs;

    // See the matching note in WorldWeather: the convar decides, never a permission.
    private static bool IsEnabled() => ClientConfig.Value(TimeOptionsSettings.Enabled);

    public static void Initialize()
    {
        var tick = TickRegistry.Register(
            "World.Time",
            Apply,
            // A manual time change sweeps hours in a couple of seconds, and at the steady rate that sweep is
            // visibly steppy, so the ramp gets a frame each instead.
            TickRate.Varying(() => _ramping || _catchingUp ? TickRate.PerFrame : TickRate.Every(SteadyIntervalMs)),
            IsEnabled,
            onStarted: () =>
            {
                _started = false;
                _ramping = false;
                _catchingUp = false;
                _wasFrozen = false;
                _shownDay = long.MinValue;
            },
            onStopped: () =>
            {
                Native.NetworkClearClockTimeOverride();

                // Otherwise switching time sync off while frozen leaves the game's own clock stopped.
                Native.PauseClock(false);
                Native.SetMillisecondsPerGameMinute(NormalMsPerGameMinute);
            });

        ClientConfig.AddEventListenerFor([TimeOptionsSettings.Enabled], tick.Reevaluate);

        WorldState.Changed += TickRegistry.Reevaluate;
    }

    // What the game itself reports, which is the only proof the clock calls are landing.
    public static string Describe() =>
        $"{Native.GetClockHours():00}:{Native.GetClockMinutes():00}:{Native.GetClockSeconds():00} on " +
        $"{MoonCycle.WeekdayOf(GameDayNow())} " +
        $"{Native.GetClockDayOfMonth():00}/{Native.GetClockMonth() + 1:00}/{Native.GetClockYear()}";

    // The day vMenu handed to the game, which is what the whole date and moon rest on.
    public static string DescribeDate() =>
        _shownDay == long.MinValue
            ? "no date has been set yet"
            : $"vMenu set day {_shownDay} of the {MoonCycle.PeriodDays} day loop, " +
              $"game reports day {GameDayNow() - MoonCycle.EpochDay}";

    public static double MoonCycleDays => GameDayNow() - MoonCycle.EpochDay + FractionOfDayNow();

    // Read back out of the natives rather than from _shownDay, for the same reason Describe is: this is
    // the number the sky is actually being drawn from, not the one vMenu believes it sent.
    public static string DescribeMoon()
    {
        var cycleDays = MoonCycleDays;

        return $"{MoonCycle.DayOfCycle(cycleDays).ToString("0.##", CultureInfo.InvariantCulture)} of " +
            $"{MoonCycle.CycleDays.ToString("0", CultureInfo.InvariantCulture)} days through the cycle, " +
            $"{MoonCycle.NameOf(cycleDays)}, " +
            $"{(MoonCycle.Illumination(cycleDays) * 100.0).ToString("0", CultureInfo.InvariantCulture)}% lit, " +
            $"angle {MoonCycle.Degrees(cycleDays).ToString("0.#", CultureInfo.InvariantCulture)} degrees";
    }

    // The date the game holds, as days since the Unix epoch.
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

        var offset = Ramp() + CatchUp();

        // Real seconds, not zero. Forcing them to zero is what made the legacy clock visibly step.
        var total = GameClock.Mod(
            GameClock.SecondOfDay(WorldState.ClockUnixSeconds, WorldState.TimeSpeed) + offset,
            GameClock.SecondsPerGameDay);

        Native.SetMillisecondsPerGameMinute(
            (int)(NormalMsPerGameMinute / GameClock.ClampSpeed(WorldState.TimeSpeed)));

        // NetworkOverrideClockTime sets the clock, it does not stop it, so without this a frozen clock
        // creeps forward between re-asserts and snaps back on every one, which the clouds show up badly.
        Native.PauseClock(WorldState.IsTimeFrozen);

        Native.NetworkOverrideClockTime((int)(total / 3600), (int)(total % 3600 / 60), (int)(total % 60));

        ApplyDate(offset);
    }

    // Sets the in-game date, which nothing else does and which the moon's position depends on.
    private static void ApplyDate(double offset)
    {
        var day = (long)GameClock.Mod(
            GameClock.GameDay(WorldState.ClockUnixSeconds, offset, WorldState.TimeSpeed),
            MoonCycle.PeriodDays);

        if (day == _shownDay)
        {
            return;
        }

        _shownDay = day;

        CivilTime.FromDays(MoonCycle.EpochDay + day, out var year, out var month, out var dayOfMonth);

        Native.SetClockDate(dayOfMonth, month - 1, year);
    }

    // Eases the displayed offset toward the server's, so a jump reads as a time lapse.
    private static double Ramp()
    {
        var target = WorldState.TimeOffsetSeconds;

        if (!_started)
        {
            _started = true;

            return Snap(target);
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

    private static double CatchUp()
    {
        var frozen = WorldState.IsTimeFrozen;

        if (frozen != _wasFrozen)
        {
            _wasFrozen = frozen;

            _catchingUp = false;

            if (!frozen)
            {
                BeginCatchUp();
            }
        }

        if (frozen)
        {
            // Kept for the moment it is let go, when the convar no longer carries it.
            _frozenAnchor = WorldState.FrozenAtUnix!.Value;

            return 0.0;
        }

        if (!_catchingUp)
        {
            return 0.0;
        }

        var seconds = Math.Max(0, WorldState.TimeTransitionSeconds);

        if (seconds <= 0)
        {
            _catchingUp = false;

            return 0.0;
        }

        var progress = (Native.GetGameTimer() - _catchUpStartMs) / 1000.0 / seconds;

        if (progress >= 1.0)
        {
            _catchingUp = false;

            return 0.0;
        }

        return _catchUpFrom * (1.0 - Smooth(progress));
    }

    private static void BeginCatchUp()
    {
        // Mod, not Shortest: this time really passed, so it winds forward however far that is.
        _catchUpFrom = -GameClock.Mod(
            GameClock.SecondOfDay(WorldState.UnixSeconds, WorldState.TimeSpeed)
            - GameClock.SecondOfDay(_frozenAnchor, WorldState.TimeSpeed),
            GameClock.SecondsPerGameDay);

        _catchUpStartMs = Native.GetGameTimer();

        _catchingUp = _catchUpFrom < -0.5;
    }

    private static double Snap(double target)
    {
        _ramping = false;
        _shownOffset = target;
        _rampFrom = target;
        _rampTo = target;

        return _shownOffset;
    }

    // Nine hours back sweeps back, not fifteen forward.
    private static double Shortest(double delta)
    {
        var wrapped = GameClock.Mod(delta, GameClock.SecondsPerGameDay);

        return wrapped > GameClock.SecondsPerGameDay / 2.0
            ? wrapped - GameClock.SecondsPerGameDay
            : wrapped;
    }

    private static double Smooth(double t) => t * t * (3.0 - (2.0 * t));
}
