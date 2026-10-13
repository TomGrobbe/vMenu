using System.Globalization;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.World;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

namespace vMenu.Enhanced.Menus.World;

public static class WeatherTemperature
{
    private const int CacheIntervalMs = 500;

    private const int ChangingIntervalMs = 250;

    public const int UnitCelsius = 0;

    public const int UnitFahrenheit = 1;

    private static double? _seaLevel;

    private static double? _local;

    // Exports run synchronously inside the caller's invoke, where a native would reset its call context and
    // break its result, so they only ever read what this tick cached.
    public static void Initialize()
    {
        TickRegistry.Register(
            "World.Temperature",
            Refresh,
            TickRate.Varying(() => TickRate.Every(Changing ? ChangingIntervalMs : CacheIntervalMs)));

        API.Exports.Local.Set("GetTemperature", new Func<double?>(() => _local));
        API.Exports.Local.Set("GetTemperatureAtHeight", new Func<object, double?>(AtHeight));
    }

    // A blend or a fast clock moves the temperature by a degree in well under a second.
    public static bool Changing =>
        WorldWeather.IsBlending || WorldTime.IsTransitioning || WorldState.TimeSpeed > GameClock.NormalSpeed;

    public static double? Celsius()
    {
        Refresh();

        return _local;
    }

    private static void Refresh()
    {
        _seaLevel = SeaLevelCelsius();
        _local = _seaLevel is { } seaLevel
            ? WeatherTemperatures.AtHeight(seaLevel, Native.GetEntityCoords(Native.PlayerPedId(), false).Z)
            : null;
    }

    private static double? SeaLevelCelsius()
    {
        Native.GetWeatherTypeTransition(out var prev, out var next, out var percent);

        if (!WorldWeather.TryTypeOfHash((uint)prev, out var from) || !WorldWeather.TryTypeOfHash((uint)next, out var to))
        {
            return null;
        }

        return WeatherTemperatures.Blended(from, to, percent, HourOfDay());
    }

    // Lua hands whole numbers over as integers, so the height arrives as whatever number type it was sent as.
    private static double? AtHeight(object height)
    {
        if (_seaLevel is not { } seaLevel || height is not IConvertible convertible)
        {
            return null;
        }

        try
        {
            return WeatherTemperatures.AtHeight(seaLevel, convertible.ToDouble(CultureInfo.InvariantCulture));
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            return null;
        }
    }

    public static double HourOfDay() =>
        Native.GetClockHours() + (Native.GetClockMinutes() / 60.0) + (Native.GetClockSeconds() / 3600.0);

    public static int Unit =>
        UserDefaults.DisplayTemperatureUnit.Value == UnitFahrenheit ? UnitFahrenheit : UnitCelsius;

    public static bool UseMetric => Unit == UnitCelsius;

    public static int Rounded(double celsius, bool metric) =>
        (int)Math.Round(metric ? celsius : WeatherTemperatures.ToFahrenheit(celsius));

    public static string Format(double celsius)
    {
        var metric = UseMetric;

        return $"{Rounded(celsius, metric)}°{(metric ? "C" : "F")}";
    }
}
