namespace vMenu.Enhanced.Data.World;

public static class WeatherTypes
{
    public static IReadOnlyList<WeatherType> Selectable { get; } = Enum.GetValues<WeatherType>();

    public static string NameOf(WeatherType type) => type switch
    {
        WeatherType.Clear => "CLEAR",
        WeatherType.ExtraSunny => "EXTRASUNNY",
        WeatherType.Clouds => "CLOUDS",
        WeatherType.Overcast => "OVERCAST",
        WeatherType.Rain => "RAIN",
        WeatherType.Clearing => "CLEARING",
        WeatherType.Thunder => "THUNDER",
        WeatherType.Smog => "SMOG",
        WeatherType.Foggy => "FOGGY",
        WeatherType.Xmas => "XMAS",
        WeatherType.Snow => "SNOW",
        WeatherType.SnowLight => "SNOWLIGHT",
        WeatherType.Blizzard => "BLIZZARD",
        WeatherType.Halloween => "HALLOWEEN",
        WeatherType.Neutral => "NEUTRAL",
        WeatherType.RainHalloween => "RAIN_HALLOWEEN",
        WeatherType.SnowHalloween => "SNOW_HALLOWEEN",
        _ => "CLEAR",
    };

    public static bool TryParse(string? name, out WeatherType type)
    {
        type = WeatherType.Clear;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        foreach (var candidate in Selectable)
        {
            if (!string.Equals(NameOf(candidate), name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            type = candidate;

            return true;
        }

        return false;
    }

    public static bool IsKnown(string? name) => TryParse(name, out _);
}
