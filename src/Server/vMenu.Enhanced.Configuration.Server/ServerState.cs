using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Configuration.Server;

/// <summary>The weather and time overrides, held here and mirrored to clients as convars.</summary>
// Not persisted across restarts: coming back up on the deterministic schedule is the right default.
public static class ServerState
{
    private const string DumpCommand = "vmenu_state";

    private static WeatherType? _weather;

    private static int _timeOffsetSeconds;

    public static WeatherType? Weather => _weather;

    public static int TimeOffsetSeconds => _timeOffsetSeconds;

    public static void Initialize()
    {
        PublishWeather();
        PublishTimeOffset();

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

        PublishTimeOffset();
    }

    public static void Dump()
    {
        Log.Info(
            $"[State] weather: {(_weather is { } type ? WeatherTypes.NameOf(type) : "dynamic")}, " +
            $"time offset: {_timeOffsetSeconds}s");
    }

    private static void PublishWeather() =>
        Native.SetConvarReplicated(
            WorldStateConvars.Weather,
            _weather is { } type ? WeatherTypes.NameOf(type) : WorldStateConvars.Dynamic);

    private static void PublishTimeOffset() =>
        Native.SetConvarReplicated(
            WorldStateConvars.TimeOffset,
            WorldStateConvars.FormatOffset(_timeOffsetSeconds));
}
