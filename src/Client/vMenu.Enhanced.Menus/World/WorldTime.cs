using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Ticks;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;

namespace vMenu.Enhanced.Menus.World;

/// <summary>Drives the in-game clock from the server's time.</summary>
public static class WorldTime
{
    // NetworkOverrideClockTime takes whole seconds and the clock runs at 30x, so the value it is
    // given can only change once every 33ms. Running per frame just re-sent a number that had not
    // moved. At this rate the sun advances a fiftieth of a degree per step, which cannot be seen.
    private const int IntervalMs = 50;

    private static readonly MenuGate Condition = MenuGate.Setting(TimeOptionsSettings.Enabled);

    private static double _shownOffset;
    private static double _rampFrom;
    private static double _rampTo;
    private static int _rampStartMs;
    private static bool _started;

    public static void Initialize()
    {
        TickRegistry.Register(
            "World.Time",
            Apply,
            TickRate.Every(IntervalMs),
            Condition.Evaluate,
            onStarted: () => _started = false,
            onStopped: Native.NetworkClearClockTimeOverride);

        WorldState.Changed += TickRegistry.Reevaluate;
    }

    private static void Apply()
    {
        if (!WorldState.HasClock)
        {
            return;
        }

        // Real seconds, not zero. Forcing them to zero is what made the legacy clock visibly step.
        var total = GameClock.Mod(
            GameClock.SecondOfDay(WorldState.UnixSeconds) + Ramp(),
            GameClock.SecondsPerGameDay);

        Native.NetworkOverrideClockTime((int)(total / 3600), (int)(total % 3600 / 60), (int)(total % 60));
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
