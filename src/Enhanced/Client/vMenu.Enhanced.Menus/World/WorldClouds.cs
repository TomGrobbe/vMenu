using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.World;

namespace vMenu.Enhanced.Menus.World;

public static class WorldClouds
{
    private static string _current = string.Empty;

    // Whether the dome is currently ours rather than the game's.
    public static bool IsHeld => _current.Length > 0;

    // What the game reports, for the world dump.
    public static string Describe() =>
        IsHeld ? $"{_current} at {Native.GetCloudHatOpacity() * 100.0f:0}%" : "picked by the game";

    // Fades to the dome that suits a weather type, if it is not already up.
    public static void Apply(WeatherType type, float seconds)
    {
        var hat = HatFor(type);

        if (string.Equals(hat, _current, StringComparison.Ordinal))
        {
            return;
        }

        _current = hat;

        Native.SetCloudHatTransition(hat, seconds);
    }

    // Hands the sky back, after which the game resumes picking its own dome.
    public static void Release(float seconds)
    {
        if (!IsHeld)
        {
            return;
        }

        Native.UnloadCloudHat(_current, seconds);

        _current = string.Empty;
    }

    // One dome per weather type rather than a random pick from a set. Every client runs the same
    // schedule off the same clock, so a plain mapping lands them all on the same sky with nothing to
    // sync. A random pick would need the server to choose and publish it.
    private static string HatFor(WeatherType type) => type switch
    {
        WeatherType.ExtraSunny => "Clear 01",
        WeatherType.Clear => "Puffs",
        WeatherType.Neutral => "Horizon",
        WeatherType.Smog => "horizonband1",
        WeatherType.Foggy => "horizonband2",
        WeatherType.Clouds => "Cloudy 01",
        WeatherType.Overcast => "altostratus",
        WeatherType.Clearing => "Wispy",
        WeatherType.Rain => "RAIN",
        WeatherType.RainHalloween => "RAIN",
        WeatherType.Thunder => "Stormy 01",
        WeatherType.Blizzard => "Stormy 01",
        WeatherType.Snow => "Snowy 01",
        WeatherType.SnowLight => "Snowy 01",
        WeatherType.SnowHalloween => "Snowy 01",
        WeatherType.Xmas => "Snowy 01",
        WeatherType.Halloween => "Nimbus",
        _ => "Clear 01",
    };
}
