using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.World;

/// <summary>The server's clock and overrides, read from the replicated convars.</summary>
// Polled rather than listened to: the clock convar changes every second, so a change listener would
// fire on every client forever to service a value only used when re-anchoring.
public static class WorldState
{
    private const int PollIntervalMs = 250;

    /// <summary>Past this the server restarted or its clock jumped, so snap instead of easing.</summary>
    private const double HardResyncSeconds = 30.0;

    private const double DriftCorrection = 0.1;

    private static double _anchorUnix;
    private static int _anchorTimerMs;
    private static bool _anchored;
    private static bool _warnedAboutFallback;

    private static string _lastUtc = string.Empty;
    private static string _lastWeather = string.Empty;
    private static string _lastOffset = string.Empty;

    public static WeatherType? WeatherOverride { get; private set; }

    public static int TimeOffsetSeconds { get; private set; }

    public static bool HasClock => _anchored;

    public static event Action? Changed;

    public static double UnixSeconds =>
        _anchored ? _anchorUnix + (unchecked(Native.GetGameTimer() - _anchorTimerMs) / 1000.0) : 0.0;

    /// <summary>The clock with the server's offset applied, as an in-game second of day.</summary>
    public static double SecondOfDay =>
        GameClock.Mod(GameClock.SecondOfDay(UnixSeconds) + TimeOffsetSeconds, GameClock.SecondsPerGameDay);

    /// <summary>What the schedule says right now, ignoring any override.</summary>
    public static CycleResolution Schedule => WeatherCycle.Resolve(GameClock.CycleGameHours(UnixSeconds));

    /// <summary>The type actually in force.</summary>
    public static WeatherType Weather => WeatherOverride ?? Schedule.Current;

    public static void Initialize()
    {
        Poll();

        TickRegistry.Register("World.State", Poll, TickRate.Every(PollIntervalMs));
    }

    private static void Poll()
    {
        var changed = false;

        var utc = Native.GetConvar(WorldStateConvars.Utc, string.Empty);

        if (!string.Equals(utc, _lastUtc, StringComparison.Ordinal))
        {
            _lastUtc = utc;

            if (WorldStateConvars.TryParseUnix(utc, out var published))
            {
                Anchor(published);
            }
        }

        if (!_anchored)
        {
            AnchorFromLocalClock();
        }

        var weather = Native.GetConvar(WorldStateConvars.Weather, WorldStateConvars.Dynamic);

        if (!string.Equals(weather, _lastWeather, StringComparison.Ordinal))
        {
            _lastWeather = weather;
            WeatherOverride = WeatherTypes.TryParse(weather, out var type) ? type : null;
            changed = true;
        }

        var offset = Native.GetConvar(WorldStateConvars.TimeOffset, "0");

        if (!string.Equals(offset, _lastOffset, StringComparison.Ordinal))
        {
            _lastOffset = offset;
            TimeOffsetSeconds = WorldStateConvars.TryParseOffset(offset, out var seconds) ? seconds : 0;
            changed = true;
        }

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    // Corrected by a fraction of the drift rather than snapped: the server publishes once a second
    // and this polls four times, so a snap would step the sky by up to thirty in-game seconds.
    private static void Anchor(double published)
    {
        if (!_anchored)
        {
            _anchorUnix = published;
            _anchorTimerMs = Native.GetGameTimer();
            _anchored = true;

            return;
        }

        var drift = published - UnixSeconds;

        if (Math.Abs(drift) > HardResyncSeconds)
        {
            _anchorUnix = published;
            _anchorTimerMs = Native.GetGameTimer();

            return;
        }

        _anchorUnix += drift * DriftCorrection;
    }

    /// <summary>
    /// Used only until the server's first value arrives. A wrong machine clock is why the server
    /// publishes the time at all, so this is a degraded mode, not a supported one.
    /// </summary>
    private static void AnchorFromLocalClock()
    {
        Native.GetUtcTime(out var year, out var month, out var day, out var hour, out var minute, out var second);

        if (year < 2000)
        {
            return;
        }

        if (!_warnedAboutFallback)
        {
            _warnedAboutFallback = true;

            API.Log.Warn("[World] No server time yet, falling back to this machine's clock.");
        }

        _anchorUnix = CivilTime.ToUnixSeconds(year, month, day, hour, minute, second);
        _anchorTimerMs = Native.GetGameTimer();
        _anchored = true;
    }
}
