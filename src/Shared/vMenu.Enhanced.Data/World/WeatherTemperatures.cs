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

    private static readonly double[] Smog = [11.5, 11, 12.5, 13.5, 20, 26.5, 28.5, 27, 22.5, 20, 13.5, 12, 12];

    private static readonly double[] Clouds = [13, 11.5, 11, 14, 16.5, 20, 24.5, 24.5, 18.5, 16, 12.5, 12.5, 11];

    private static readonly double[] Rain = [12, 13, 11.5, 13, 15.5, 19.5, 24, 21.5, 19, 14, 14, 13, 11.5];

    private static readonly double[] Thunder = [12.5, 12, 12, 14.5, 16, 19, 23, 17, 16, 14.5, 13, 14.5, 12.5];

    private static readonly double[] Neutral = [13.5, 10.5, 9.5, 10, 9.5, 10, 10.5, 10, 10.5, 10, 10, 11, 13];

    private static readonly double[] Clearing = [12.1, 12.1, 12.1, 12.7, 13.7, 16.2, 21.1, 26, 24.6, 20.4, 16.2, 13.7, 12.7];

    private static readonly double[] Foggy = [12.2, 12.2, 12.2, 12.4, 13.2, 14.4, 15.6, 16, 15.2, 14.1, 13.1, 12.4, 12.1];

    private static readonly double[] Halloween = [14, 13.5, 13.5, 15, 17, 28.5, 30.5, 30.5, 16.5, 16.5, 11.5, 11.5, 13.5];

    private static readonly double[] ExtraSunny = [15, 14.5, 14.5, 17, 21.5, 30, 34.5, 34, 27, 22, 17, 15, 15.5];

    private static readonly double[] Clear = [14.5, 14, 14, 16, 19.5, 27, 31, 31.5, 23, 19, 15, 14, 14.5];
  
    private static readonly double[] Blizzard = [-35, -18.8, -15.6, -13.4, -10.2, -7.9, -7.9, -10.2, -14.5, -17.7, -25.3, -31.8, -35];

    private static readonly double[] Snow = [-23, -12.2, -11.1, -6.8, -5.7, -4.6, -3.5, -4.6, -6.8, -10, -14.3, -17.6, -23];

    private static readonly double[] SnowLight = [-5, -6, -3, -2, -1.5, -1.5, -1.5, -1.5, -1, -1.5, -2.5, -3.5, -4.5];

    private static readonly double[] SnowyClearing = [-13, -13, -13, -12.4, -11.7, -9.7, -5.8, -1.9, -3.1, -6.4, -9.7, -11.7, -12.4];

    private static readonly double[] SnowyFoggy = [-9.9, -9.9, -10, -9.6, -8.8, -7.6, -6.4, -6, -6.6, -7.6, -9, -9.6, -10];

    private static readonly double[] SnowyExtraSunny = [-12, -11.9, -11.9, -10.6, -8.2, -3.4, -0.9, -3.4, -6.3, -8.2, -10.6, -11.5, -12];

    private static readonly double[] SnowyClear = [-14, -14, -14, -12.2, -9.1, -4.8, -3, -2.9, -6.7, -9.1, -12.2, -13.4, -14];

    private static readonly double[] SnowySmog = [-11, -11, -11, -9.8, -7.9, -5.2, -4, -5.2, -6.2, -7.9, -9.8, -10.6, -11];

    private static readonly double[] SnowyClouds = [-9, -9, -8.9, -8.6, -7.8, -5.9, -3.9, -4.8, -5.9, -7.8, -8.6, -8.6, -8.9];

    private static readonly double[] SnowyRain = [-4, -4, -4, -3.7, -2.9, -1.3, 0, -0.3, -1.3, -2.9, -3.7, -3.7, -4];

    private static readonly double[] SnowyThunder = [-8, -8, -8, -7.6, -6.9, -5.1, -3.8, -6.2, -6.9, -7.3, -7.6, -7.6, -8];

    private static readonly double[] SnowyNeutral = [-6, -7.9, -7.9, -8, -8, -8, -8, -8, -8, -7.7, -8, -6.9, -6];

    private static readonly double[] SnowyHalloween = [-16, -16, -16, -13.7, -13.7, -2, -1.9, -2, -13.7, -13.7, -16, -16, -16];

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
        WeatherCycle.SnowPass ? SnowyRowOf(type) : NormalRowOf(type);

    private static double[] NormalRowOf(WeatherType type) => type switch
    {
        WeatherType.ExtraSunny => ExtraSunny,
        WeatherType.Clear => Clear,
        WeatherType.Smog => Smog,
        WeatherType.Clouds => Clouds,
        WeatherType.Overcast => Clouds,
        WeatherType.Foggy => Foggy,
        WeatherType.Clearing => Clearing,
        WeatherType.Rain => Rain,
        WeatherType.Thunder => Thunder,
        WeatherType.Neutral => Neutral,
        WeatherType.Snow => Snow,
        WeatherType.Blizzard => Blizzard,
        WeatherType.SnowHalloween => SnowLight,
        WeatherType.SnowLight => SnowLight,
        WeatherType.Xmas => Snow,
        WeatherType.Halloween => Halloween,
        WeatherType.RainHalloween => Halloween,
        _ => Clear,
    };
    private static double[] SnowyRowOf(WeatherType type) => type switch
    {
        WeatherType.ExtraSunny => SnowyExtraSunny,
        WeatherType.Clear => SnowyClear,
        WeatherType.Smog => SnowySmog,
        WeatherType.Clouds => SnowyClouds,
        WeatherType.Overcast => SnowyClouds,
        WeatherType.Foggy => SnowyFoggy,
        WeatherType.Clearing => SnowyClearing,
        WeatherType.Rain => SnowyRain,
        WeatherType.Thunder => SnowyThunder,
        WeatherType.Neutral => SnowyNeutral,
        WeatherType.Snow => Snow,
        WeatherType.Blizzard => Blizzard,
        WeatherType.SnowHalloween => SnowLight,
        WeatherType.SnowLight => SnowLight,
        WeatherType.Xmas => Snow,
        WeatherType.Halloween => SnowyHalloween,
        WeatherType.RainHalloween => SnowyHalloween,
        _ => SnowyClear,
    };
}
