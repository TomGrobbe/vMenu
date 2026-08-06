namespace vMenu.Enhanced.Data.World;

/// <summary>Turns a Unix timestamp into GTA's in-game clock and weather cycle position.</summary>
public static class GameClock
{
    public const double RealSecondsPerGameHour = 120.0;

    public const double GameHoursPerCycle = 384.0;

    public const int SecondsPerGameDay = 86400;

    private const double RealSecondsPerGameDay = 24.0 * RealSecondsPerGameHour;

    private const double RealSecondsPerWeatherCycle = GameHoursPerCycle * RealSecondsPerGameHour;

    // Modulo before the multiply, so a 1.8e9 timestamp never goes through a double.
    public static double SecondOfDay(double unixSeconds) =>
        Mod(unixSeconds, RealSecondsPerGameDay) * 30.0;

    public static double CycleGameHours(double unixSeconds) =>
        Mod(unixSeconds, RealSecondsPerWeatherCycle) / RealSecondsPerGameHour;

    public static double Mod(double value, double modulus)
    {
        var result = value % modulus;

        return result < 0.0 ? result + modulus : result;
    }
}
