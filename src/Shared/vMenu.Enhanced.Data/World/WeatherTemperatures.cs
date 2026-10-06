namespace vMenu.Enhanced.Data.World;

public readonly struct TemperatureOutlookEntry(double gameHoursAhead, double hourOfDay, WeatherType type, double celsius)
{
    public double GameHoursAhead { get; } = gameHoursAhead;

    public double HourOfDay { get; } = hourOfDay;

    public WeatherType Type { get; } = type;

    public double Celsius { get; } = celsius;
}

public static class WeatherTemperatures
{
    private static readonly double[] SampleHours = [0, 5, 6, 7, 10, 12, 16, 17, 18, 19, 20, 21, 22];

    private static readonly double[] SampleHolds = [4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    private const double FadeStartHeight = 200.0;

    private const double FadeEndHeight = 1000.0;

    private const double DropAtFadeEnd = 15.0;

    private const double LatestHour = 23.999;

    private static readonly double[] ExtraSunny = [12, 12, 12, 15, 20, 30, 35, 30, 24, 20, 15, 13, 12];

    private static readonly double[] Clear = [12, 12, 12, 15, 20, 27, 30, 30, 24, 20, 15, 13, 12];

    private static readonly double[] Smog = [12, 12, 12, 15, 20, 27, 30, 27, 24, 20, 15, 13, 12];

    private static readonly double[] Clouds = [12, 12, 12, 13, 15, 20, 25, 23, 20, 15, 13, 13, 12];

    private static readonly double[] Rain = [12, 12, 12, 13, 15, 20, 24, 23, 20, 15, 13, 13, 12];

    private static readonly double[] Thunder = [12, 12, 12, 13, 15, 20, 23, 17, 15, 14, 13, 13, 12];

    private static readonly double[] Neutral = [12, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 11, 12];

    private static readonly double[] Blizzard = [-30, -15, -12, -10, -7, -5, -5, -7, -11, -14, -21, -27, -30];

    private static readonly double[] Snow = [-20, -10, -9, -5, -4, -2, -1, -2, -5, -8, -12, -15, -20];

    private static readonly double[] SnowLight = [-10, -12, -6, -4, -3, -1, -1, -1, -2, -3, -5, -7, -9];

    private static readonly double[] Halloween = [12, 12, 12, 15, 15, 30, 30, 30, 15, 15, 12, 12, 12];

    public static IReadOnlyList<double> Hours => SampleHours;

    public static IReadOnlyList<double> Holds => SampleHolds;

    public static IReadOnlyList<double> Row(WeatherType type) => SamplesOf(type);

    public static double AtSeaLevel(WeatherType type, double hourOfDay)
    {
        var samples = SamplesOf(type);
        var hour = Math.Clamp(hourOfDay, 0.0, LatestHour);
        var index = SampleHours.Length - 1;

        while (SampleHours[index] > hour)
        {
            index--;
        }

        var nextIndex = (index + 1) % SampleHours.Length;
        var nextHour = nextIndex == 0 ? SampleHours[0] + 24.0 : SampleHours[nextIndex];
        var start = SampleHours[index] + SampleHolds[index];
        var t = Math.Clamp((hour - start) / (nextHour - start), 0.0, 1.0);

        return samples[index] + ((samples[nextIndex] - samples[index]) * t);
    }

    public static double Blended(WeatherType from, WeatherType to, double percent, double hourOfDay)
    {
        var start = AtSeaLevel(from, hourOfDay);

        return start + ((AtSeaLevel(to, hourOfDay) - start) * Math.Clamp(percent, 0.0, 1.0));
    }

    public static double AtHeight(double seaLevelCelsius, double height) =>
        seaLevelCelsius
        - (Math.Clamp((height - FadeStartHeight) / (FadeEndHeight - FadeStartHeight), 0.0, 1.0) * DropAtFadeEnd);

    public static double ToFahrenheit(double celsius) => (celsius * 9.0 / 5.0) + 32.0;

    public static IReadOnlyList<TemperatureOutlookEntry> Outlook(
        double cycleGameHours,
        double secondOfDay,
        bool clockFrozen,
        WeatherType? forced,
        int stepHours,
        int count)
    {
        var entries = new List<TemperatureOutlookEntry>();

        if (stepHours <= 0 || count <= 0)
        {
            return entries;
        }

        var hourNow = secondOfDay / 3600.0;
        var first = clockFrozen ? stepHours : stepHours - GameClock.Mod(hourNow, stepHours);

        for (var i = 0; i < count; i++)
        {
            var ahead = first + (i * stepHours);
            var type = forced ?? WeatherCycle.Resolve(cycleGameHours + ahead).Current;
            var hourOfDay = clockFrozen ? hourNow : Math.Round(GameClock.Mod(hourNow + ahead, 24.0)) % 24.0;

            entries.Add(new TemperatureOutlookEntry(ahead, hourOfDay, type, AtSeaLevel(type, hourOfDay)));
        }

        return entries;
    }

    private static double[] SamplesOf(WeatherType type) =>
        WeatherCycle.SnowPass && !WeatherTypes.IsSnowy(type) ? SnowLight : NormalRowOf(type);

    private static double[] NormalRowOf(WeatherType type) => type switch
    {
        WeatherType.ExtraSunny => ExtraSunny,
        WeatherType.Clear => Clear,
        WeatherType.Smog => Smog,
        WeatherType.Clouds => Clouds,
        WeatherType.Overcast => Clouds,
        WeatherType.Foggy => Clouds,
        WeatherType.Clearing => Clouds,
        WeatherType.Rain => Rain,
        WeatherType.Thunder => Thunder,
        WeatherType.Neutral => Neutral,
        WeatherType.Snow => Snow,
        WeatherType.Blizzard => Blizzard,
        WeatherType.SnowHalloween => SnowLight,
        WeatherType.SnowLight => SnowLight,
        WeatherType.Xmas => SnowLight,
        WeatherType.Halloween => Halloween,
        WeatherType.RainHalloween => Halloween,
        _ => Clear,
    };
}
