using System.Globalization;

namespace vMenu.Enhanced.Data.World;

public sealed class CustomCycleFile
{
    public bool SnowPass { get; set; }

    public List<CustomCycleBlock>? Cycle { get; set; }
}

public sealed class CustomCycleBlock
{
    public double Hours { get; set; }

    public string? Weather { get; set; }

    public string? Blackout { get; set; }
}

public sealed class CustomCycle
{
    private CustomCycle(CycleEntry[] entries, double lengthGameHours, bool snowPass, CustomCycleFile cleaned)
    {
        Entries = entries;
        LengthGameHours = lengthGameHours;
        SnowPass = snowPass;
        Cleaned = cleaned;
        HasBlackouts = entries.Any(entry => entry.Blackout != BlackoutMode.Off);
    }

    public CycleEntry[] Entries { get; }

    public double LengthGameHours { get; }

    public bool SnowPass { get; }

    public bool HasBlackouts { get; }

    // Only the entries that survived, with names normalised, so the clients never see a bad one.
    public CustomCycleFile Cleaned { get; }

    // Turns "this many hours of this weather" into the running start hours the schedule is built on.
    public static CustomCycle? Build(CustomCycleFile? file, Action<string> warn)
    {
        if (file?.Cycle is not { Count: > 0 } blocks)
        {
            warn("it has no cycle entries.");

            return null;
        }

        var entries = new List<CycleEntry>(blocks.Count);
        var kept = new List<CustomCycleBlock>(blocks.Count);
        var start = 0.0;

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var number = (i + 1).ToString(CultureInfo.InvariantCulture);

            if (block is null || !(block.Hours > 0.0) || double.IsInfinity(block.Hours))
            {
                warn($"entry {number} is skipped, its hours must be a number above 0.");

                continue;
            }

            if (!WeatherTypes.TryParse(block.Weather, out var type))
            {
                warn($"entry {number} is skipped, '{block.Weather}' is not a known weather type.");

                continue;
            }

            var blackout = BlackoutMode.Off;

            if (!string.IsNullOrWhiteSpace(block.Blackout)
                && (!BlackoutModes.TryParse(block.Blackout, out blackout) || blackout == BlackoutMode.Dynamic))
            {
                warn($"entry {number} has blackout '{block.Blackout}', which is not off, city or all, so it gets no blackout.");

                blackout = BlackoutMode.Off;
            }

            entries.Add(new CycleEntry(start, type, blackout));
            kept.Add(new CustomCycleBlock
            {
                Hours = block.Hours,
                Weather = WeatherTypes.NameOf(type),
                Blackout = blackout == BlackoutMode.Off ? null : BlackoutModes.NameOf(blackout),
            });

            start += block.Hours;
        }

        if (entries.Count == 0)
        {
            warn("none of its cycle entries are usable.");

            return null;
        }

        return new CustomCycle(
            [.. entries],
            start,
            file.SnowPass,
            new CustomCycleFile { SnowPass = file.SnowPass, Cycle = kept });
    }
}
