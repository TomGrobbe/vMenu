namespace vMenu.Enhanced.Data.World;

public enum BlackoutMode
{
    Off,
    City,
    CityAndVehicles,
}

public static class BlackoutModes
{
    public static IReadOnlyList<BlackoutMode> Selectable { get; } = Enum.GetValues<BlackoutMode>();

    public static string NameOf(BlackoutMode mode) => mode switch
    {
        BlackoutMode.City => "city",
        BlackoutMode.CityAndVehicles => "all",
        _ => "off",
    };

    public static bool TryParse(string? name, out BlackoutMode mode)
    {
        mode = BlackoutMode.Off;

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
}
