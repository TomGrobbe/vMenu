namespace vMenu.Enhanced.Data.World;

public enum SnowMode
{
    Automatic,
    On,
    Off,
}

public static class SnowModes
{
    public static IReadOnlyList<SnowMode> Selectable { get; } = Enum.GetValues<SnowMode>();

    public static string NameOf(SnowMode mode) => mode switch
    {
        SnowMode.On => "on",
        SnowMode.Off => "off",
        _ => "auto",
    };

    public static bool TryParse(string? name, out SnowMode mode)
    {
        mode = SnowMode.Automatic;

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

            mode = candidate;

            return true;
        }

        return false;
    }

    public static bool Resolve(SnowMode mode, WeatherType weather) => mode switch
    {
        SnowMode.On => true,
        SnowMode.Off => false,
        _ => WeatherTypes.IsSnowy(weather),
    };
}
