namespace vMenu.Enhanced.Data.World;

public enum MoonPhase
{
    New,
    WaxingCrescent,
    FirstQuarter,
    WaxingGibbous,
    Full,
    WaningGibbous,
    LastQuarter,
    WaningCrescent,
}

/// <summary>Where the moon sits in its cycle, which follows from the in-game date and nothing else.</summary>
// GTA offsets the moon's angle by PI * ((fmod(cycleTime, 55) - 27) / 27), where cycleTime is whole
// days since 1 January 2000 plus the fraction of the current day. Everything here is that one
// formula in friendlier units, so the angle is the number to trust and the rest is read off it.
public static class MoonCycle
{
    /// <summary>In-game days from one new moon to the next.</summary>
    public const double CycleDays = 55.0;

    /// <summary>The day the angle passes through zero, which is the moon fully lit.</summary>
    public const double FullMoonDay = 27.0;

    /// <summary>The moon's 55 day cycle and the 7 day week, so the date can wrap without either jumping.</summary>
    // Keeping the day count a multiple of 55 and 7 apart from the true one leaves both cycles where
    // they were, which is why the in-game date loops here rather than running away forever.
    public const long PeriodDays = 385;

    /// <summary>1 January 2000 in days since the Unix epoch, the date the cycle measures from.</summary>
    public const long EpochDay = 10957;

    private static readonly string[] Weekdays =
        ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    public static double DayOfCycle(double cycleDays) => GameClock.Mod(cycleDays, CycleDays);

    public static double Radians(double cycleDays) =>
        Math.PI * ((DayOfCycle(cycleDays) - FullMoonDay) / FullMoonDay);

    public static double Degrees(double cycleDays) => Radians(cycleDays) * 180.0 / Math.PI;

    /// <summary>How much of the disc is lit, 0 at new and 1 at full.</summary>
    public static double Illumination(double cycleDays) => (1.0 + Math.Cos(Radians(cycleDays))) / 2.0;

    public static MoonPhase PhaseOf(double cycleDays)
    {
        var waxing = DayOfCycle(cycleDays) < FullMoonDay;

        return Illumination(cycleDays) switch
        {
            < 0.03 => MoonPhase.New,
            < 0.47 => waxing ? MoonPhase.WaxingCrescent : MoonPhase.WaningCrescent,
            <= 0.53 => waxing ? MoonPhase.FirstQuarter : MoonPhase.LastQuarter,
            <= 0.97 => waxing ? MoonPhase.WaxingGibbous : MoonPhase.WaningGibbous,
            _ => MoonPhase.Full,
        };
    }

    public static string NameOf(double cycleDays) => PhaseOf(cycleDays) switch
    {
        MoonPhase.New => "new moon",
        MoonPhase.WaxingCrescent => "waxing crescent",
        MoonPhase.FirstQuarter => "first quarter",
        MoonPhase.WaxingGibbous => "waxing gibbous",
        MoonPhase.Full => "full moon",
        MoonPhase.WaningGibbous => "waning gibbous",
        MoonPhase.LastQuarter => "last quarter",
        _ => "waning crescent",
    };

    /// <summary>1 January 1970 was a Thursday, which is where a week counted in Unix days starts.</summary>
    public static string WeekdayOf(long unixDay) => Weekdays[(int)GameClock.Mod(unixDay + 3, 7)];
}
