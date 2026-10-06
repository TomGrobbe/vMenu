using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Ticks;

using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

public static class WorldBlackout
{
    private const int IntervalMs = 1000;

    private static BlackoutMode _applied = BlackoutMode.Off;

    private static TickHandle? _tick;

    public static void Initialize()
    {
        _tick = TickRegistry.Register(
            "World.Blackout",
            Apply,
            TickRate.Every(IntervalMs),
            static () => WorldState.Blackout == BlackoutMode.Dynamic
                && WeatherCycle.HasScheduledBlackouts
                && ClientConfig.Value(WeatherOptionsSettings.Enabled));

        ClientConfig.AddEventListenerFor([WeatherOptionsSettings.Enabled], OnChanged);

        WorldState.Changed += OnChanged;

        Apply();
    }

    public static string Describe() => $"applied: {BlackoutModes.NameOf(_applied)}";

    private static void OnChanged()
    {
        _tick?.Reevaluate();

        Apply();
    }

    private static void Apply()
    {
        var mode = WorldState.EffectiveBlackout;

        if (mode == _applied)
        {
            return;
        }

        _applied = mode;

        Native.SetArtificialLightsState(mode != BlackoutMode.Off);
        Native.SetArtificialLightsStateAffectsVehicles(mode == BlackoutMode.CityAndVehicles);
    }
}
