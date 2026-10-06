namespace vMenu.Enhanced.Data.World;

public enum WeatherCycleType
{
    Default,
    Snowy,
    Custom,
}

public static class WeatherCycles
{
    public static IReadOnlyList<WeatherCycleType> Selectable { get; } = Enum.GetValues<WeatherCycleType>();

    public static string NameOf(WeatherCycleType cycle) => cycle switch
    {
        WeatherCycleType.Snowy => "snowy",
        WeatherCycleType.Custom => "custom",
        _ => "default",
    };

    public static bool TryParse(string? name, out WeatherCycleType cycle)
    {
        cycle = WeatherCycleType.Default;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        foreach (var candidate in Selectable)
        {
            if (!string.Equals(NameOf(candidate), name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            cycle = candidate;

            return true;
        }

        return false;
    }
}
