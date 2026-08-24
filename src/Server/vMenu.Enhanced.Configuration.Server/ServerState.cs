using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Configuration.Server;

// The weather and time overrides, held here and mirrored to clients as convars. Not persisted across
// restarts: coming back up on the deterministic schedule is the right default.
public static class ServerState
{
    private const string DumpCommand = "vmenu_state";

    private static WeatherType? _weather;

    private static int _timeOffsetSeconds;

    private static double? _frozenAtUnix;

    private static BlackoutMode _blackout;

    private static SnowMode _snow;

    public static WeatherType? Weather => _weather;

    public static int TimeOffsetSeconds => _timeOffsetSeconds;

    public static double? FrozenAtUnix => _frozenAtUnix;

    public static BlackoutMode Blackout => _blackout;

    public static SnowMode Snow => _snow;

    public static void Initialize()
    {
        PublishWeather();
        PublishTime();
        PublishBlackout();
        PublishSnow();

        SharedAPI.Commands.RegisterCommand(DumpCommand, true, DebugCommands.Gate(Dump));
    }

    public static void SetWeather(WeatherType? type)
    {
        _weather = type;

        PublishWeather();
    }

    public static void SetTimeOffset(int seconds)
    {
        _timeOffsetSeconds = WorldStateConvars.NormaliseOffset(seconds);

        PublishTime();
    }

    public static void SetTimeOffsetRunning(int seconds)
    {
        _timeOffsetSeconds = WorldStateConvars.NormaliseOffset(seconds);
        _frozenAtUnix = null;

        PublishTime();
    }

    public static void SetTimeFrozen(bool frozen)
    {
        if (frozen == _frozenAtUnix.HasValue)
        {
            return;
        }

        _frozenAtUnix = frozen ? ServerClock.Now() : null;

        PublishTime();
    }

    public static void SetBlackout(BlackoutMode mode)
    {
        _blackout = mode;

        PublishBlackout();
    }

    public static void SetSnow(SnowMode mode)
    {
        _snow = mode;

        PublishSnow();
    }

    public static void Dump()
    {
        Log.Info(
            $"[State] weather: {(_weather is { } type ? WeatherTypes.NameOf(type) : "dynamic")}, " +
            $"time offset: {_timeOffsetSeconds}s, " +
            $"clock: {(_frozenAtUnix is { } pinned ? $"frozen at unix {pinned:0.000}" : "running")}");
        Log.Info(
            $"[State] blackout: {BlackoutModes.NameOf(_blackout)}, snow: {SnowModes.NameOf(_snow)}");
    }

    private static void PublishWeather() =>
        Native.SetConvarReplicated(
            WorldStateConvars.Weather,
            _weather is { } type ? WeatherTypes.NameOf(type) : WorldStateConvars.Dynamic);

    private static void PublishTime() =>
        Native.SetConvarReplicated(
            WorldStateConvars.TimeOffset,
            WorldStateConvars.FormatTime(_timeOffsetSeconds, _frozenAtUnix));

    private static void PublishBlackout() =>
        Native.SetConvarReplicated(WorldStateConvars.Blackout, BlackoutModes.NameOf(_blackout));

    private static void PublishSnow() =>
        Native.SetConvarReplicated(WorldStateConvars.Snow, SnowModes.NameOf(_snow));
}
