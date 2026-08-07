using System.Globalization;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Ticks;

using TimeOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.TimeOptions;
using WeatherOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.WeatherOptions;

namespace vMenu.Enhanced.Menus.World;

/// <summary>The server's clock and overrides, read from the replicated convars.</summary>
// Polled rather than listened to: the clock convar changes every second, so a change listener would
// fire on every client forever to service a value only used when re-anchoring.
public static class WorldState
{
    private const string DumpCommand = "vmenu_world";

    // Twice the server's publish rate, which is the slowest that still sees every value it sends.
    // Slower than this and reads start landing on the same second twice, so the clock re-anchors less
    // often than it could.
    private const int PollIntervalMs = 500;

    /// <summary>Past this the server restarted or its clock jumped, so snap instead of easing.</summary>
    private const double HardResyncSeconds = 30.0;

    private const double DriftCorrection = 0.1;

    private static double _anchorUnix;
    private static int _anchorTimerMs;
    private static bool _anchored;
    private static bool _warnedAboutFallback;
    private static bool _heardFromServer;

    private static string _lastUtc = string.Empty;
    private static string _lastWeather = string.Empty;
    private static string _lastOffset = string.Empty;

    public static WeatherType? WeatherOverride { get; private set; }

    public static int TimeOffsetSeconds { get; private set; }

    public static bool HasClock => _anchored;

    public static event Action? Changed;

    public static double UnixSeconds =>
        _anchored ? _anchorUnix + (unchecked(Native.GetGameTimer() - _anchorTimerMs) / 1000.0) : 0.0;

    /// <summary>How fast the clock runs, which the weather schedule follows as well.</summary>
    // Read live rather than cached, so raising it takes effect without a restart. The same convar
    // reaches every client, so nobody's sky runs at a different speed from anybody else's.
    public static double TimeSpeed =>
        GameClock.ClampSpeed(ClientConfig.Value(TimeOptionsSettings.SpeedMultiplier));

    /// <summary>The clock with the server's offset applied, as an in-game second of day.</summary>
    public static double SecondOfDay =>
        GameClock.Mod(GameClock.SecondOfDay(UnixSeconds, TimeSpeed) + TimeOffsetSeconds, GameClock.SecondsPerGameDay);

    /// <summary>What the schedule says right now, ignoring any override.</summary>
    public static CycleResolution Schedule => WeatherCycle.Resolve(GameClock.CycleGameHours(UnixSeconds, TimeSpeed));

    /// <summary>The type actually in force.</summary>
    public static WeatherType Weather => WeatherOverride ?? Schedule.Current;

    /// <summary>Whether either sync feature wants the clock. The same condition the server publishes on.</summary>
    // Convars only, and deliberately no permission: the sky and the clock are the same for everybody
    // on the server, so they cannot depend on what any one player is allowed to change.
    public static bool IsNeeded() =>
        ClientConfig.Value(WeatherOptionsSettings.Enabled) || ClientConfig.Value(TimeOptionsSettings.Enabled);

    public static void Initialize()
    {
        // Gated to match the server, which publishes nothing while both features are off. Without
        // this every client on such a server falls back to its own machine clock and says so.
        TickRegistry.Register(
            "World.State",
            Poll,
            TickRate.Every(PollIntervalMs),
            IsNeeded,
            onStarted: Poll);

        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));
    }

    public static void Dump()
    {
        if (!IsNeeded())
        {
            API.Log.Info("[World] Both weather and time sync are off on this server, so nothing is being synced.");
        }

        API.Log.Info($"[World] {WorldStateConvars.Utc} = '{Native.GetConvar(WorldStateConvars.Utc, string.Empty)}'");
        API.Log.Info($"[World] {WorldStateConvars.Weather} = '{Native.GetConvar(WorldStateConvars.Weather, string.Empty)}'");
        API.Log.Info(
            $"[World] {WorldStateConvars.TimeOffset} = '{Native.GetConvar(WorldStateConvars.TimeOffset, string.Empty)}'");

        API.Log.Info(
            "[World] clock: " + (_anchored
                ? _heardFromServer ? "anchored to the server" : "anchored to THIS MACHINE, no server time has arrived"
                : "not anchored") +
            $", running at {TimeSpeed.ToString("0.###", CultureInfo.InvariantCulture)}x speed");
        API.Log.Info(
            $"[World] override: {(WeatherOverride is { } forced ? WeatherTypes.NameOf(forced) : "none")}, " +
            $"schedule: {WeatherTypes.NameOf(Schedule.Current)}, " +
            $"in force: {WeatherTypes.NameOf(Weather)}, time offset: {TimeOffsetSeconds}s");

        // Everything above is what vMenu believes. This is what the game actually has.
        API.Log.Info($"[World] game reports: {WorldWeather.Describe()}");
        API.Log.Info($"[World] clouds: {WorldClouds.Describe()}");
        API.Log.Info($"[World] game clock: {WorldTime.Describe()}");
        API.Log.Info($"[World] date: {WorldTime.DescribeDate()}");
        API.Log.Info($"[World] moon: {WorldTime.DescribeMoon()}");
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
                _heardFromServer = true;

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
