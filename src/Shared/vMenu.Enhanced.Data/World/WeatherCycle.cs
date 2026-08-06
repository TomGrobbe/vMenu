namespace vMenu.Enhanced.Data.World;

// A plain struct rather than a record: generated record equality routes through
// EqualityComparer<T>.Default, which the client sandbox refuses to load.
public readonly struct CycleEntry(double gameHour, WeatherType type)
{
    public double GameHour { get; } = gameHour;

    public WeatherType Type { get; } = type;
}

public readonly struct CycleResolution(WeatherType current, WeatherType next, double gameHoursUntilNext)
{
    public WeatherType Current { get; } = current;

    public WeatherType Next { get; } = next;

    public double GameHoursUntilNext { get; } = gameHoursUntilNext;
}

/// <summary>
/// GTA Online's weather schedule: 55 blocks over 384 in-game hours, anchored to the Unix epoch.
/// </summary>
// Transcribed from adam10603/GTAWeather, which observed it in forecast vocabulary: clear is
// ExtraSunny, mostly clear is Clear, partly cloudy is Clouds, mostly cloudy is Clearing, cloudy is
// Overcast, mist is Neutral, fog is Foggy, haze is Smog, drizzle is Rain, rain is Thunder. The storm
// pair is inferred, so Clearing, Overcast and Neutral are the likeliest to need correcting.
public static class WeatherCycle
{
    private const double EarliestRainHour = 6.0;

    private const double LatestRainHour = 18.0;

    private const int ExpectedCount = 55;

    private const double ShortestBlockGameHours = 1.0;

    private const double LongestBlockGameHours = 30.0;

    private static readonly CycleEntry[] Entries =
    [
        new(0, WeatherType.Clouds),
        new(4, WeatherType.Neutral),
        new(7, WeatherType.Clearing),
        new(11, WeatherType.ExtraSunny),
        new(14, WeatherType.Neutral),
        new(16, WeatherType.ExtraSunny),
        new(28, WeatherType.Neutral),
        new(31, WeatherType.ExtraSunny),
        new(41, WeatherType.Smog),
        new(45, WeatherType.Clouds),
        new(52, WeatherType.Neutral),
        new(55, WeatherType.Overcast),
        new(62, WeatherType.Foggy),
        new(66, WeatherType.Overcast),
        new(72, WeatherType.Clouds),
        new(78, WeatherType.Foggy),
        new(82, WeatherType.Overcast),
        new(92, WeatherType.Clear),
        new(104, WeatherType.Clouds),
        new(105, WeatherType.Rain),
        new(108, WeatherType.Clouds),
        new(125, WeatherType.Neutral),
        new(128, WeatherType.Clouds),
        new(131, WeatherType.Thunder),
        new(134, WeatherType.Rain),
        new(137, WeatherType.Overcast),
        new(148, WeatherType.Neutral),
        new(151, WeatherType.Clearing),
        new(155, WeatherType.Foggy),
        new(159, WeatherType.ExtraSunny),
        new(176, WeatherType.Clear),
        new(196, WeatherType.Foggy),
        new(201, WeatherType.Clouds),
        new(220, WeatherType.Neutral),
        new(222, WeatherType.Clear),
        new(244, WeatherType.Neutral),
        new(246, WeatherType.Clear),
        new(247, WeatherType.Thunder),
        new(250, WeatherType.Rain),
        new(252, WeatherType.Clouds),
        new(268, WeatherType.Neutral),
        new(270, WeatherType.Clouds),
        new(272, WeatherType.Overcast),
        new(277, WeatherType.Clouds),
        new(292, WeatherType.Neutral),
        new(295, WeatherType.Clouds),
        new(300, WeatherType.Clearing),
        new(306, WeatherType.Clouds),
        new(318, WeatherType.Clearing),
        new(330, WeatherType.Clouds),
        new(337, WeatherType.ExtraSunny),
        new(367, WeatherType.Clouds),
        new(369, WeatherType.Thunder),
        new(376, WeatherType.Rain),
        new(377, WeatherType.Clouds),
    ];

    public static CycleResolution Resolve(double cycleGameHours)
    {
        var position = GameClock.Mod(cycleGameHours, GameClock.GameHoursPerCycle);

        var index = Entries.Length - 1;

        for (var i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].GameHour > position)
            {
                index = i - 1;

                break;
            }
        }

        var current = Entries[index].Type;

        // The first and last entries share a type, so the wrap is one continuous block and index + 1
        // would announce a change that never happens on screen.
        for (var step = 1; step <= Entries.Length; step++)
        {
            var next = Entries[(index + step) % Entries.Length];

            if (next.Type == current)
            {
                continue;
            }

            var until = next.GameHour - position;

            if (until <= 0.0)
            {
                until += GameClock.GameHoursPerCycle;
            }

            return new CycleResolution(current, next.Type, until);
        }

        return new CycleResolution(current, current, GameClock.GameHoursPerCycle);
    }

    /// <summary>Checks the table against every structural property the schedule is known to have.</summary>
    // Returns messages rather than logging, since this assembly has no runtime to log to. The night
    // rain check is the sensitive one: a mistyped offset usually drags rain into darkness.
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        if (Entries.Length != ExpectedCount)
        {
            problems.Add($"Expected {ExpectedCount} entries, found {Entries.Length}.");
        }

        if (Entries.Length == 0)
        {
            return problems;
        }

        if (Entries[0].GameHour != 0.0)
        {
            problems.Add($"First entry starts at {Entries[0].GameHour}, expected 0.");
        }

        if (Entries[^1].GameHour >= GameClock.GameHoursPerCycle)
        {
            problems.Add($"Last entry starts at {Entries[^1].GameHour}, which is outside the cycle.");
        }

        if (Entries[0].Type != Entries[^1].Type)
        {
            problems.Add(
                $"Cycle starts on {Entries[0].Type} but ends on {Entries[^1].Type}; " +
                "the schedule is meant to wrap seamlessly.");
        }

        for (var i = 1; i < Entries.Length; i++)
        {
            if (Entries[i].GameHour <= Entries[i - 1].GameHour)
            {
                problems.Add($"Entry {i} at {Entries[i].GameHour} does not come after entry {i - 1}.");
            }
        }

        var total = 0.0;

        for (var i = 0; i < Entries.Length; i++)
        {
            var start = Entries[i].GameHour;
            var end = i + 1 < Entries.Length ? Entries[i + 1].GameHour : GameClock.GameHoursPerCycle;
            var duration = end - start;

            total += duration;

            if (duration < ShortestBlockGameHours || duration > LongestBlockGameHours)
            {
                problems.Add(
                    $"Entry {i} ({Entries[i].Type}) lasts {duration} in-game hours, outside the " +
                    $"known {ShortestBlockGameHours} to {LongestBlockGameHours} range.");
            }

            if (Entries[i].Type is not (WeatherType.Rain or WeatherType.Thunder))
            {
                continue;
            }

            var startHour = start % 24.0;

            if (startHour < EarliestRainHour || startHour + duration > LatestRainHour)
            {
                problems.Add(
                    $"Entry {i} ({Entries[i].Type}) runs {startHour:0.#} to {startHour + duration:0.#} " +
                    "in-game hours, but precipitation never falls at night.");
            }
        }

        if (total != GameClock.GameHoursPerCycle)
        {
            problems.Add($"Durations sum to {total} in-game hours, expected {GameClock.GameHoursPerCycle}.");
        }

        return problems;
    }
}
