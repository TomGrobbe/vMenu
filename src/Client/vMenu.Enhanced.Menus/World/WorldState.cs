using System.Globalization;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

/// <summary>The server's clock and overrides, read from the replicated convars.</summary>
public static class WorldState
{
    private const string DumpCommand = "vmenu_world";

    // Only ever runs before the server's first value arrives
    private const int FallbackIntervalMs = 500;

    /// <summary>Past this the server restarted or its clock jumped, so snap instead of easing.</summary>
    private const double HardResyncSeconds = 30.0;

    private const double DriftCorrection = 0.1;

    private static float _speedMultiplier;
    private static double _anchorUnix;
    private static int _anchorTimerMs;
    private static bool _anchored;
    private static bool _warnedAboutFallback;
    private static bool _heardFromServer;

    private static TickHandle? _fallback;

    public static WeatherType? WeatherOverride { get; private set; }

    public static int TimeOffsetSeconds { get; private set; }

    #region convars
    public static int TimeTransitionSeconds { get; private set; }

    public static int WeatherTransitionSeconds { get; private set; }

    public static bool SyncClouds { get; private set; }
    #endregion

    public static bool HasClock => _anchored;

    public static event Action? Changed;

    public static double UnixSeconds =>
        _anchored ? _anchorUnix + ((Native.GetGameTimer() - _anchorTimerMs) / 1000.0) : 0.0;

    /// <summary>How fast the clock runs, which the weather schedule follows as well.</summary>
    public static double TimeSpeed =>
        GameClock.ClampSpeed(_speedMultiplier);

    /// <summary>The clock with the server's offset applied, as an in-game second of day.</summary>
    public static double SecondOfDay =>
        GameClock.Mod(GameClock.SecondOfDay(UnixSeconds, TimeSpeed) + TimeOffsetSeconds, GameClock.SecondsPerGameDay);

    /// <summary>What the schedule says right now, ignoring any override.</summary>
    public static CycleResolution Schedule => WeatherCycle.Resolve(GameClock.CycleGameHours(UnixSeconds, TimeSpeed));

    /// <summary>The type actually in force.</summary>
    public static WeatherType Weather => WeatherOverride ?? Schedule.Current;

    /// <summary>Whether either sync feature wants the clock. The same condition the server publishes on.</summary>
    public static bool IsNeeded() =>
        ClientConfig.Value(WeatherOptionsSettings.Enabled) || ClientConfig.Value(TimeOptionsSettings.Enabled);

    public static void Initialize()
    {
        ClientConfig.Track(WorldStateConvars.All);

        ReadSettings();

        // Get these values once, because listeners only trigger when values change
        // after registering a listener for it.
        ReadClock();
        ReadOverrides();

        ClientConfig.AddEventListenerFor(
            [
                TimeOptionsSettings.SpeedMultiplier,
                TimeOptionsSettings.TransitionSeconds,
                WeatherOptionsSettings.TransitionSeconds,
                WeatherOptionsSettings.SyncClouds,
            ],
            ReadSettings);

        ClientConfig.AddEventListenerFor([WorldStateConvars.Utc], ReadClock);
        ClientConfig.AddEventListenerFor([WorldStateConvars.Weather, WorldStateConvars.TimeOffset], ReadOverrides);

        // Gated to match the server, which publishes nothing while both features are off.
        _fallback = TickRegistry.Register(
            "World.Clock.Fallback",
            AnchorFromLocalClock,
            TickRate.Every(FallbackIntervalMs),
            () => IsNeeded() && !_anchored);

        ClientConfig.AddEventListenerFor(
            [WeatherOptionsSettings.Enabled, TimeOptionsSettings.Enabled],
            _fallback.Reevaluate);

        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));
    }

    public static void Dump()
    {
        if (!IsNeeded())
        {
            Log.Info("[World] Both weather and time sync are off on this server, so nothing is being synced.");
        }

        Log.Info($"[World] {WorldStateConvars.Utc} = '{Native.GetConvar(WorldStateConvars.Utc, string.Empty)}'");
        Log.Info($"[World] {WorldStateConvars.Weather} = '{Native.GetConvar(WorldStateConvars.Weather, string.Empty)}'");
        Log.Info(
            $"[World] {WorldStateConvars.TimeOffset} = '{Native.GetConvar(WorldStateConvars.TimeOffset, string.Empty)}'");

        Log.Info(
            "[World] clock: " + (_anchored
                ? _heardFromServer ? "anchored to the server" : "anchored to THIS MACHINE, no server time has arrived"
                : "not anchored") +
            $", running at {TimeSpeed.ToString("0.###", CultureInfo.InvariantCulture)}x speed");
        Log.Info(
            $"[World] override: {(WeatherOverride is { } forced ? WeatherTypes.NameOf(forced) : "none")}, " +
            $"schedule: {WeatherTypes.NameOf(Schedule.Current)}, " +
            $"in force: {WeatherTypes.NameOf(Weather)}, time offset: {TimeOffsetSeconds}s");

        // Everything above is what vMenu believes. This is what the game actually has.
        Log.Info($"[World] game reports: {WorldWeather.Describe()}");
        Log.Info($"[World] clouds: {WorldClouds.Describe()}");
        Log.Info($"[World] game clock: {WorldTime.Describe()}");
        Log.Info($"[World] date: {WorldTime.DescribeDate()}");
        Log.Info($"[World] moon: {WorldTime.DescribeMoon()}");
    }

    private static void ReadSettings()
    {
        _speedMultiplier = ClientConfig.Value(TimeOptionsSettings.SpeedMultiplier);
        TimeTransitionSeconds = ClientConfig.Value(TimeOptionsSettings.TransitionSeconds);
        WeatherTransitionSeconds = ClientConfig.Value(WeatherOptionsSettings.TransitionSeconds);
        SyncClouds = ClientConfig.Value(WeatherOptionsSettings.SyncClouds);
    }

    private static void ReadClock()
    {
        if (!WorldStateConvars.TryParseUnix(Native.GetConvar(WorldStateConvars.Utc, string.Empty), out var published))
        {
            return;
        }

        _heardFromServer = true;

        Anchor(published);

        // Whatever the machine clock had is now beaten by a real value, so the fallback is done.
        _fallback?.Reevaluate();
    }

    // No comparison against what was read last, because the module only calls this when the convar
    // actually moved.
    private static void ReadOverrides()
    {
        var weather = Native.GetConvar(WorldStateConvars.Weather, WorldStateConvars.Dynamic);
        var offset = Native.GetConvar(WorldStateConvars.TimeOffset, "0");

        WeatherOverride = WeatherTypes.TryParse(weather, out var type) ? type : null;
        TimeOffsetSeconds = WorldStateConvars.TryParseOffset(offset, out var seconds) ? seconds : 0;

        Changed?.Invoke();
    }

    // Corrected by a fraction of the drift rather than snapped, because a snap on a server whose
    // clock wobbles by a second would step the sky by thirty in-game seconds each time.
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

            Log.Warning("[World] No server time yet, falling back to this machine's clock.");
        }

        _anchorUnix = CivilTime.ToUnixSeconds(year, month, day, hour, minute, second);
        _anchorTimerMs = Native.GetGameTimer();
        _anchored = true;

        // The loop only checks whether it is running, not its condition, so it has to be told.
        _fallback?.Reevaluate();
    }
}
