namespace vMenu.Enhanced.Data.World;

// A struct rather than a record: generated equality routes through EqualityComparer<T>.Default,
// which the client sandbox refuses to load.
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

public readonly struct ForecastEntry(WeatherType type, double gameHoursUntilStart, double gameHoursLong)
{
    public WeatherType Type { get; } = type;

    public double GameHoursUntilStart { get; } = gameHoursUntilStart;

    public double GameHoursLong { get; } = gameHoursLong;
}

// GTA Online's weather schedule: 173 blocks over 384 in-game hours, anchored to the Unix epoch. Taken
// straight from the game's own weather.xml cycle table. Each entry there carries a TimeMult, which
// CWeather multiplies by msPerCycle (120 real seconds, exactly one in-game hour) to get the entry's
// length. Those multipliers are stored here as running start hours instead.
public static class WeatherCycle
{
    private const double EarliestRainHour = 6.0;

    private const double LatestRainHour = 18.0;

    private const int ExpectedCount = 173;

    private const double ShortestBlockGameHours = 1.0;

    // TimeMult is a byte in the game, but nothing in this table goes above five.
    private const double LongestBlockGameHours = 5.0;

    private static readonly CycleEntry[] Entries =
    [
        new(0, WeatherType.ExtraSunny),
        new(2, WeatherType.Clear),
        new(3, WeatherType.Smog),
        new(7, WeatherType.ExtraSunny),
        new(8, WeatherType.Smog),
        new(11, WeatherType.ExtraSunny),
        new(13, WeatherType.ExtraSunny),
        new(14, WeatherType.Overcast),
        new(15, WeatherType.ExtraSunny),
        new(17, WeatherType.Smog),
        new(20, WeatherType.Clouds),
        new(22, WeatherType.Clear),
        new(27, WeatherType.ExtraSunny),
        new(28, WeatherType.Smog),
        new(31, WeatherType.ExtraSunny),
        new(33, WeatherType.ExtraSunny),
        new(34, WeatherType.Clouds),
        new(39, WeatherType.Clear),
        new(41, WeatherType.Smog),
        new(43, WeatherType.Clouds),
        new(45, WeatherType.Clear),
        new(46, WeatherType.Smog),
        new(48, WeatherType.Overcast),
        new(50, WeatherType.Smog),
        new(52, WeatherType.Clear),
        new(53, WeatherType.ExtraSunny),
        new(54, WeatherType.ExtraSunny),
        new(56, WeatherType.Clear),
        new(58, WeatherType.ExtraSunny),
        new(60, WeatherType.Smog),
        new(63, WeatherType.Foggy),
        new(64, WeatherType.Smog),
        new(66, WeatherType.Clouds),
        new(70, WeatherType.Smog),
        new(71, WeatherType.ExtraSunny),
        new(73, WeatherType.ExtraSunny),
        new(74, WeatherType.Smog),
        new(76, WeatherType.Overcast),
        new(77, WeatherType.Clouds),
        new(78, WeatherType.ExtraSunny),
        new(79, WeatherType.Foggy),
        new(80, WeatherType.Clear),
        new(83, WeatherType.Clear),
        new(87, WeatherType.Clear),
        new(92, WeatherType.ExtraSunny),
        new(94, WeatherType.ExtraSunny),
        new(96, WeatherType.Smog),
        new(99, WeatherType.Overcast),
        new(100, WeatherType.Smog),
        new(103, WeatherType.Overcast),
        new(106, WeatherType.Thunder),
        new(107, WeatherType.Smog),
        new(108, WeatherType.Clear),
        new(109, WeatherType.ExtraSunny),
        new(110, WeatherType.ExtraSunny),
        new(112, WeatherType.Clear),
        new(116, WeatherType.Clouds),
        new(118, WeatherType.Clear),
        new(121, WeatherType.ExtraSunny),
        new(122, WeatherType.Clear),
        new(123, WeatherType.Clear),
        new(128, WeatherType.Smog),
        new(129, WeatherType.Clouds),
        new(131, WeatherType.Rain),
        new(132, WeatherType.Smog),
        new(137, WeatherType.Clouds),
        new(139, WeatherType.ExtraSunny),
        new(140, WeatherType.Smog),
        new(142, WeatherType.ExtraSunny),
        new(143, WeatherType.ExtraSunny),
        new(145, WeatherType.Clear),
        new(148, WeatherType.Clear),
        new(153, WeatherType.Smog),
        new(156, WeatherType.Foggy),
        new(158, WeatherType.Clear),
        new(159, WeatherType.ExtraSunny),
        new(160, WeatherType.ExtraSunny),
        new(163, WeatherType.Smog),
        new(165, WeatherType.Overcast),
        new(168, WeatherType.Smog),
        new(169, WeatherType.ExtraSunny),
        new(170, WeatherType.ExtraSunny),
        new(171, WeatherType.Smog),
        new(174, WeatherType.ExtraSunny),
        new(176, WeatherType.ExtraSunny),
        new(177, WeatherType.Clear),
        new(180, WeatherType.Clear),
        new(185, WeatherType.Smog),
        new(188, WeatherType.ExtraSunny),
        new(190, WeatherType.Smog),
        new(191, WeatherType.Overcast),
        new(193, WeatherType.Smog),
        new(196, WeatherType.Foggy),
        new(197, WeatherType.Foggy),
        new(199, WeatherType.Foggy),
        new(200, WeatherType.Smog),
        new(201, WeatherType.ExtraSunny),
        new(203, WeatherType.ExtraSunny),
        new(204, WeatherType.ExtraSunny),
        new(207, WeatherType.ExtraSunny),
        new(208, WeatherType.Clear),
        new(213, WeatherType.Smog),
        new(215, WeatherType.Clear),
        new(220, WeatherType.ExtraSunny),
        new(222, WeatherType.ExtraSunny),
        new(223, WeatherType.Clear),
        new(228, WeatherType.Smog),
        new(229, WeatherType.Clear),
        new(234, WeatherType.ExtraSunny),
        new(235, WeatherType.Clear),
        new(238, WeatherType.Clear),
        new(240, WeatherType.Smog),
        new(242, WeatherType.Smog),
        new(246, WeatherType.Overcast),
        new(247, WeatherType.Thunder),
        new(249, WeatherType.Clearing),
        new(250, WeatherType.ExtraSunny),
        new(252, WeatherType.Clear),
        new(254, WeatherType.ExtraSunny),
        new(255, WeatherType.ExtraSunny),
        new(257, WeatherType.Clear),
        new(262, WeatherType.Smog),
        new(265, WeatherType.Overcast),
        new(266, WeatherType.Smog),
        new(268, WeatherType.Overcast),
        new(271, WeatherType.Smog),
        new(272, WeatherType.Clear),
        new(277, WeatherType.ExtraSunny),
        new(278, WeatherType.ExtraSunny),
        new(279, WeatherType.Smog),
        new(282, WeatherType.ExtraSunny),
        new(284, WeatherType.ExtraSunny),
        new(285, WeatherType.Clear),
        new(288, WeatherType.Clear),
        new(293, WeatherType.Smog),
        new(296, WeatherType.Overcast),
        new(298, WeatherType.Smog),
        new(300, WeatherType.Clear),
        new(304, WeatherType.ExtraSunny),
        new(306, WeatherType.ExtraSunny),
        new(307, WeatherType.ExtraSunny),
        new(310, WeatherType.ExtraSunny),
        new(311, WeatherType.Smog),
        new(312, WeatherType.Smog),
        new(316, WeatherType.ExtraSunny),
        new(317, WeatherType.ExtraSunny),
        new(318, WeatherType.Smog),
        new(321, WeatherType.ExtraSunny),
        new(323, WeatherType.ExtraSunny),
        new(324, WeatherType.Clouds),
        new(329, WeatherType.Clear),
        new(331, WeatherType.Smog),
        new(333, WeatherType.Clear),
        new(338, WeatherType.ExtraSunny),
        new(340, WeatherType.ExtraSunny),
        new(341, WeatherType.Clear),
        new(344, WeatherType.ExtraSunny),
        new(345, WeatherType.Clear),
        new(350, WeatherType.Smog),
        new(353, WeatherType.Overcast),
        new(355, WeatherType.Smog),
        new(357, WeatherType.Clear),
        new(361, WeatherType.ExtraSunny),
        new(363, WeatherType.Smog),
        new(366, WeatherType.Overcast),
        new(367, WeatherType.Clouds),
        new(369, WeatherType.Rain),
        new(370, WeatherType.Thunder),
        new(373, WeatherType.Rain),
        new(374, WeatherType.Clearing),
        new(376, WeatherType.Clouds),
        new(377, WeatherType.ExtraSunny),
        new(381, WeatherType.Smog),
    ];

    public static CycleResolution Resolve(double cycleGameHours)
    {
        var position = GameClock.Mod(cycleGameHours, GameClock.GameHoursPerCycle);

        var index = IndexAt(position);

        var current = Entries[index].Type;

        // Neighbouring entries often repeat a type, so index + 1 would announce a change that never happens
        // on screen. Skipping to the next different type spans the whole run instead.
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

    public static IReadOnlyList<ForecastEntry> Forecast(double cycleGameHours, int count)
    {
        if (count <= 0)
        {
            return [];
        }

        var position = GameClock.Mod(cycleGameHours, GameClock.GameHoursPerCycle);
        var index = IndexAt(position);
        var previous = Entries[index].Type;
        var boundaries = new List<ForecastEntry>(count + 1);

        for (var step = 1; step <= Entries.Length * (count + 1) && boundaries.Count <= count; step++)
        {
            var entry = Entries[(index + step) % Entries.Length];

            if (entry.Type == previous)
            {
                continue;
            }

            previous = entry.Type;

            var wraps = (index + step) / Entries.Length;

            boundaries.Add(new ForecastEntry(
                entry.Type,
                entry.GameHour + (wraps * GameClock.GameHoursPerCycle) - position,
                0.0));
        }

        var forecast = new List<ForecastEntry>(count);

        for (var i = 0; i + 1 < boundaries.Count && forecast.Count < count; i++)
        {
            forecast.Add(new ForecastEntry(
                boundaries[i].Type,
                boundaries[i].GameHoursUntilStart,
                boundaries[i + 1].GameHoursUntilStart - boundaries[i].GameHoursUntilStart));
        }

        return forecast;
    }

    private static int IndexAt(double position)
    {
        for (var i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].GameHour > position)
            {
                return i - 1;
            }
        }

        return Entries.Length - 1;
    }

    // Checks the table against every structural property the schedule is known to have. Returns messages
    // rather than logging, since this assembly has no runtime to log to. The night rain check is the
    // sensitive one: a mistyped offset usually drags rain into darkness.
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
