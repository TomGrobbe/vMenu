namespace vMenu.Enhanced.Data.World;

public enum BlackoutMode
{
    Dynamic,
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
        BlackoutMode.Dynamic => "dynamic",
        _ => "off",
    };

    public static bool TryParse(string? name, out BlackoutMode mode)
    {
        mode = BlackoutMode.Dynamic;

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

    // An admin pick always wins. A forced weather pauses the schedule too,
    // so nothing changes when something is manually set to something else.
    public static BlackoutMode Resolve(BlackoutMode mode, bool weatherForced, bool weatherEnabled, double cycleGameHours)
    {
        if (mode != BlackoutMode.Dynamic)
        {
            return mode;
        }

        if (!weatherEnabled || weatherForced || !WeatherCycle.HasScheduledBlackouts)
        {
            return BlackoutMode.Off;
        }

        return WeatherCycle.BlackoutAt(cycleGameHours);
    }
}
