namespace vMenu.Enhanced.Data.World;

public static class GameClock
{
    public const double RealSecondsPerGameHour = 120.0;

    public const double GameHoursPerCycle = 384.0;

    public const int SecondsPerGameDay = 86400;

    public const double NormalSpeed = 1.0;

    // A whole in-game day every four real days, which is as good as stopped.
    public const double SlowestSpeed = 0.01;

    // A whole in-game day every three real seconds, which is already unusable.
    public const double FastestSpeed = 1000.0;

    private const double RealSecondsPerGameDay = 24.0 * RealSecondsPerGameHour;

    // Keeps a zero, a negative or a nonsense speed from dividing the clock into infinity.
    public static double ClampSpeed(double speed) =>
        double.IsNaN(speed) ? NormalSpeed : Math.Clamp(speed, SlowestSpeed, FastestSpeed);

    public static double RealSecondsPerGameHourAt(double speed) => RealSecondsPerGameHour / ClampSpeed(speed);

    // Modulo before the multiply, so a 1.8e9 timestamp never goes through a double.
    public static double SecondOfDay(double unixSeconds, double speed)
    {
        var realSecondsPerGameDay = RealSecondsPerGameDay / ClampSpeed(speed);

        return Mod(unixSeconds, realSecondsPerGameDay) * (SecondsPerGameDay / realSecondsPerGameDay);
    }

    public static double CycleGameHours(double unixSeconds, double speed)
    {
        var realSecondsPerGameHour = RealSecondsPerGameHourAt(speed);

        return Mod(unixSeconds, GameHoursPerCycle * realSecondsPerGameHour) / realSecondsPerGameHour;
    }

    // The offset that puts a sped up clock back on the time the normal speed would be showing right now.
    // Zero while the speed is 1, which is what lets one reset button cover every speed, and true only at
    // the instant it is worked out: a faster clock starts pulling away from real time again immediately.
    public static double RealTimeOffset(double unixSeconds, double speed) =>
        Mod(SecondOfDay(unixSeconds, NormalSpeed) - SecondOfDay(unixSeconds, speed), SecondsPerGameDay);

    // Whole in-game days elapsed, which is the number the moon phase counts. Includes the offset so the
    // date turns over exactly when the displayed clock passes midnight.
    public static long GameDay(double unixSeconds, double offsetSeconds, double speed) =>
        (long)Math.Floor((unixSeconds * ClampSpeed(speed) / RealSecondsPerGameDay) + (offsetSeconds / SecondsPerGameDay));

    public static double Mod(double value, double modulus)
    {
        var result = value % modulus;

        return result < 0.0 ? result + modulus : result;
    }
}
