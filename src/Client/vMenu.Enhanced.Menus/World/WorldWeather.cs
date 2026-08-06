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
    // A blend is 45s by default, so this still lands about 180 steps across it, half a percent each.
    // The other job, correcting anything else that nudges the weather, is well inside a quarter second.
    private const int IntervalMs = 250;

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

                // Without this the network keeps ownership and overwrites every weather call we make,
                // so the whole tick runs and changes nothing at all.
                Native.SetWeatherOwnedByNetwork(false);

                Native.ClearOverrideWeather();
                Native.ClearWeatherTypePersist();
            },
            onStopped: () =>
            {
                Native.ClearWeatherTypePersist();

                // Handed back, so switching the feature off leaves the weather where it was found.
                Native.SetWeatherOwnedByNetwork(true);
            });

        WorldState.Changed += TickRegistry.Reevaluate;
    }

    /// <summary>What the game itself reports, which is the only proof the weather calls are landing.</summary>
    public static string Describe()
    {
        Native.GetWeatherTypeTransition(out var prev, out var next, out var percent);

        // The native hands the hashes back signed, so the cast is only reinterpreting the same bits.
        return $"{NameOfHash(unchecked((uint)prev))} to {NameOfHash(unchecked((uint)next))} at {percent * 100.0f:0}%";
    }

    // An unknown hash also means the game gained a weather type that WeatherType does not have yet.
    private static string NameOfHash(uint hash)
    {
        for (var i = 0; i < Hashes.Length; i++)
        {
            if (Hashes[i] == hash)
            {
                return WeatherTypes.NameOf((WeatherType)i);
            }
        }

        return $"unknown ({hash})";
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
