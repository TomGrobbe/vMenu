using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Ticks;

using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

/// <summary>Drives the weather from the schedule, or from whatever the server has forced.</summary>
// SetWeatherTypeTransition sets an absolute interpolation rather than starting an animation, so
// re-asserting it every tick is the point: two players who joined hours apart land on the same
// value, and anything else nudging the weather is corrected within a tick.
public static class WorldWeather
{
    private const int IntervalMs = 100;

    /// <summary>How long before a scheduled change the sky starts moving, in in-game hours.</summary>
    private const double BoundaryWindowGameHours = 0.5;

    private static readonly MenuGate Condition = MenuGate.Setting(WeatherOptionsSettings.Enabled);

    private static readonly uint[] Hashes = BuildHashes();

    private static WeatherType _from;
    private static WeatherType _to;
    private static int _changedAtMs;
    private static bool _started;
    private static bool _settled;
    private static bool _wasForced;

    public static void Initialize()
    {
        TickRegistry.Register(
            "World.Weather",
            Apply,
            TickRate.Every(IntervalMs),
            Condition.Evaluate,
            onStarted: () =>
            {
                _started = false;

                Native.ClearOverrideWeather();
                Native.ClearWeatherTypePersist();
            },
            onStopped: Native.ClearWeatherTypePersist);

        WorldState.Changed += TickRegistry.Reevaluate;
    }

    private static void Apply()
    {
        if (!WorldState.HasClock)
        {
            return;
        }

        var forced = WorldState.WeatherOverride;
        var schedule = WorldState.Schedule;
        var desired = forced ?? schedule.Current;

        if (!_started)
        {
            _started = true;
            _from = desired;
            _to = desired;
            _settled = true;
        }
        else if (desired != _to)
        {
            // A scheduled change has already been blended to by the boundary window below, so
            // restarting a source blend for it would visibly rewind the sky.
            _settled = forced is null && !_wasForced;
            _from = _to;
            _to = desired;
            _changedAtMs = Native.GetGameTimer();
        }

        _wasForced = forced is not null;

        if (!_settled)
        {
            var seconds = Math.Max(0, ClientConfig.Value(WeatherOptionsSettings.TransitionSeconds));
            var progress = seconds <= 0
                ? 1.0
                : Math.Clamp(unchecked(Native.GetGameTimer() - _changedAtMs) / 1000.0 / seconds, 0.0, 1.0);

            if (progress < 1.0)
            {
                Set(_from, _to, Smooth(progress));

                return;
            }

            _settled = true;
        }

        if (forced is null && schedule.GameHoursUntilNext < BoundaryWindowGameHours)
        {
            Set(schedule.Current, schedule.Next, Smooth(1.0 - (schedule.GameHoursUntilNext / BoundaryWindowGameHours)));

            return;
        }

        Set(_to, _to, 0.0);
    }

    private static void Set(WeatherType from, WeatherType to, double percent) =>
        Native.SetWeatherTypeTransition(Hashes[(int)from], Hashes[(int)to], (float)percent);

    private static double Smooth(double t) => t * t * (3.0 - (2.0 * t));

    private static uint[] BuildHashes()
    {
        var types = WeatherTypes.Selectable;
        var hashes = new uint[types.Count];

        foreach (var type in types)
        {
            hashes[(int)type] = API.Hash(WeatherTypes.NameOf(type));
        }

        return hashes;
    }
}
