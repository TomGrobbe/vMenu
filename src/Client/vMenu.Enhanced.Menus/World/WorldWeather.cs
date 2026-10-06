using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Ticks;

using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

// SetWeatherTypeTransition sets an absolute interpolation rather than starting an animation, so
// re-asserting it every tick is the point: two players who joined hours apart land on the same
// value, and anything else nudging the weather is corrected within a tick.
public static class WorldWeather
{
    // A blend is 45s by default, so this still lands about 180 steps across it, half a percent each.
    // The other job, correcting anything else that nudges the weather, is well inside a quarter second.
    private const int IntervalMs = 250;

    private static readonly uint[] Hashes = BuildHashes();

    private static WeatherType _from;
    private static WeatherType _to;
    private static int _changedAtMs;
    private static bool _started;
    private static bool _settled;
    private static bool _wasForced;
    private static bool _blending;

    public static bool IsBlending => _blending;

    // The convar and nothing else. Not a MenuGate, and never a permission: the weather is one value
    // shared by everyone connected, so a player who may not change it still has to see the same sky as
    // the player who can.
    private static bool IsEnabled() => ClientConfig.Value(WeatherOptionsSettings.Enabled);

    public static void Initialize()
    {
        WeatherSet();

        var tick = TickRegistry.Register(
            "World.Weather",
            Apply,
            TickRate.Every(IntervalMs),
            IsEnabled,
            onStarted: () =>
            {
                _started = false;

                // Without this the network keeps ownership and overwrites every weather call we make, so the whole
                // tick runs and changes nothing at all.
                Native.SetWeatherOwnedByNetwork(false);

                Native.ClearOverrideWeather();
                Native.ClearWeatherTypePersist();
            },
            onStopped: () =>
            {
                _blending = false;

                Native.ClearWeatherTypePersist();

                WorldClouds.Release(TransitionSeconds());

                // Handed back, so switching the feature off leaves the weather where it was found.
                Native.SetWeatherOwnedByNetwork(true);
            });

        ClientConfig.AddEventListenerFor([WeatherOptionsSettings.Enabled], tick.Reevaluate);

        ClientConfig.AddEventListenerFor([WeatherOptionsSettings.WeatherCycle], WeatherSet);

        WorldState.Changed += TickRegistry.Reevaluate;
    }

    public static void WeatherSet()
    {
        WeatherCycle.CurrentCycle = (ClientConfig.Value(WeatherOptionsSettings.WeatherCycle)) switch
        {
            0 => WeatherCycleType.Normal,
            1 => WeatherCycleType.Snowy,
            2 => WeatherCycleType.Custom,
            _ => WeatherCycleType.Normal
        };
    }


    // What the game itself reports, which is the only proof the weather calls are landing.
    public static string Describe()
    {
        Native.GetWeatherTypeTransition(out var prev, out var next, out var percent);

        // The native hands the hashes back signed, so the cast is only reinterpreting the same bits.
        return $"{NameOfHash((uint)prev)} to {NameOfHash((uint)next)} at {percent * 100.0f:0}%";
    }

    // An unknown hash also means the game gained a weather type that WeatherType does not have yet.
    private static string NameOfHash(uint hash) =>
        TryTypeOfHash(hash, out var type) ? WeatherTypes.NameOf(type) : $"unknown ({hash})";

    public static bool TryTypeOfHash(uint hash, out WeatherType type)
    {
        for (var i = 0; i < Hashes.Length; i++)
        {
            if (Hashes[i] == hash)
            {
                type = (WeatherType)i;

                return true;
            }
        }

        type = default;

        return false;
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
        var first = !_started;

        if (first)
        {
            _started = true;
            _from = desired;
            _to = desired;
            _settled = true;
        }
        else if (desired != _to)
        {
            // A scheduled change has already been blended to by the boundary window below, so restarting a
            // source blend for it would visibly rewind the sky.
            _settled = forced is null && !_wasForced;
            _from = _to;
            _to = desired;
            _changedAtMs = Native.GetGameTimer();
        }

        _wasForced = forced is not null;

        // A joining player gets the sky it should already be under, so no fade on the first pass.
        if (WorldState.SyncClouds)
        {
            WorldClouds.Apply(CloudTarget(forced, schedule), first ? 0.0f : TransitionSeconds());
        }
        else
        {
            WorldClouds.Release(TransitionSeconds());
        }

        if (!_settled)
        {
            var seconds = TransitionSeconds();
            var progress = seconds <= 0
                ? 1.0
                : Math.Clamp((Native.GetGameTimer() - _changedAtMs) / 1000.0 / seconds, 0.0, 1.0);

            if (progress < 1.0)
            {
                Set(_from, _to, WeatherCycle.Smooth(progress));

                return;
            }

            _settled = true;
        }

        if (forced is null && schedule.GameHoursUntilNext < WeatherCycle.BoundaryWindowGameHours)
        {
            Set(schedule.Current, schedule.Next, WeatherCycle.Smooth(1.0 - (schedule.GameHoursUntilNext / WeatherCycle.BoundaryWindowGameHours)));

            return;
        }

        Set(_to, _to, 0.0);
    }

    private static void Set(WeatherType from, WeatherType to, double percent)
    {
        _blending = from != to && percent > 0.0 && percent < 1.0;

        Native.SetWeatherTypeTransition(Hashes[(int)from], Hashes[(int)to], (float)percent);
    }

    private static float TransitionSeconds() =>
        Math.Max(0, WorldState.WeatherTransitionSeconds);

    // Swaps at the moment the sky starts moving rather than when the schedule flips, so the clouds and
    // the weather arrive together instead of the clouds lagging a boundary window behind.
    private static WeatherType CloudTarget(WeatherType? forced, CycleResolution schedule) =>
        forced ?? (schedule.GameHoursUntilNext < WeatherCycle.BoundaryWindowGameHours ? schedule.Next : schedule.Current);

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
