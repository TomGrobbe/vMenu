using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Ticks;

using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

// The natives are sticky global state, so they are only touched when the answer flips. The tick is
// here for the automatic mode alone: it follows the weather, and the schedule moves the weather
// without announcing it.
public static class WorldSnow
{
    private const string Asset = "core_snow";

    private const int IntervalMs = 1000;

    private static bool _applied;

    private static bool _requested;

    public static bool Wanted => ClientConfig.Value(WeatherOptionsSettings.Enabled) && WorldState.SnowWanted;

    public static void Initialize()
    {
        var tick = TickRegistry.Register(
            "World.Snow",
            Apply,
            TickRate.Every(IntervalMs),
            () => ClientConfig.Value(WeatherOptionsSettings.Enabled),
            onStopped: Release);

        ClientConfig.AddEventListenerFor([WeatherOptionsSettings.Enabled], tick.Reevaluate);

        WorldState.Changed += Apply;
    }

    public static string Describe() =>
        $"wanted: {Wanted}, applied: {_applied}, game reports level {Native.GetSnowLevel():0.00}, " +
        $"{Asset} loaded: {Native.HasNamedPtfxAssetLoaded(Asset)}";

    // Note: ForceSnowPass (Cfx native) is VERY broken. If you use it, you lose complete control over the weather in the game.
    // Can't set types, can't override it, can't transition anymore.
    // Completely broken. Game also starts to report wrong previous or next weather types.
    // Luckily for us, there's a new native: ForceGlobalSnowFx that's just baked into the game by R*, not Cfx's old one.
    // That one works flawlessly, along with the other ones found below. They are updated natives of older
    // forced vehicle trails and ped footsteps.
    private static void SetSnowEffectsEnabled(bool wanted)
    {
        Native.ForceGlobalSnowFx(wanted);
        Native.UseSnowFootVfxWhenUnsheltered(wanted);
        Native.UseSnowWheelVfxWhenUnsheltered(wanted);
        Native.FootVfxSetOverrideIceWithSnow(wanted);
    }

    private static void Apply()
    {
        var wanted = Wanted;

        if (wanted == _applied)
        {
            return;
        }

        _applied = wanted;

        SetSnowEffectsEnabled(wanted);

        if (wanted)
        {
            Request();
        }
        else
        {
            Drop();
        }
    }

    private static void Release()
    {
        _applied = false;

        SetSnowEffectsEnabled(false);

        Drop();
    }

    private static void Request()
    {
        if (_requested)
        {
            return;
        }

        _requested = true;

        Native.RequestNamedPtfxAsset(Asset);
    }

    private static void Drop()
    {
        if (!_requested)
        {
            return;
        }

        _requested = false;

        Native.RemoveNamedPtfxAsset(Asset);
    }
}
