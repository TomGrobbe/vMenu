using System.Globalization;
using System.Text;

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
    private CustomCycle(CycleEntry[] entries, double lengthGameHours, bool snowPass)
    {
        Entries = entries;
        LengthGameHours = lengthGameHours;
        SnowPass = snowPass;
        HasBlackouts = entries.Any(entry => entry.Blackout != BlackoutMode.Off);
    }

    public CycleEntry[] Entries { get; }

    public double LengthGameHours { get; }

    public bool SnowPass { get; }

    public bool HasBlackouts { get; }

    // Not JSON, because FiveM wants a replicated state bag value under 1024 bytes.
    public string Pack()
    {
        var packed = new StringBuilder(SnowPass ? "1|" : "0|");

        for (var i = 0; i < Entries.Length; i++)
        {
            var entry = Entries[i];
            var end = i + 1 < Entries.Length ? Entries[i + 1].GameHour : LengthGameHours;

            packed.Append((end - entry.GameHour).ToString("0.######", CultureInfo.InvariantCulture));
            packed.Append((char)('A' + (int)entry.Type));

            if (entry.Blackout == BlackoutMode.City)
            {
                packed.Append('c');
            }
            else if (entry.Blackout == BlackoutMode.CityAndVehicles)
            {
                packed.Append('a');
            }
        }

        return packed.ToString();
    }

    public static CustomCycleFile? Unpack(string packed)
    {
        if (packed.Length < 2 || packed[0] is not ('0' or '1') || packed[1] != '|')
        {
            return null;
        }

        var cycle = new List<CustomCycleBlock>();
        var number = 2;

        for (var i = 2; i < packed.Length; i++)
        {
            var letter = packed[i];

            if (letter is (>= '0' and <= '9') or '.')
            {
                continue;
            }

            if (letter is < 'A' or > 'Z'
                || !double.TryParse(
                    packed.Substring(number, i - number),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var hours))
            {
                return null;
            }

            var weather = letter - 'A';
            var blackout = i + 1 < packed.Length ? packed[i + 1] : ' ';

            if (blackout is 'c' or 'a')
            {
                i++;
            }

            cycle.Add(new CustomCycleBlock
            {
                Hours = hours,
                Weather = weather < WeatherTypes.Selectable.Count ? WeatherTypes.NameOf((WeatherType)weather) : null,
                Blackout = blackout switch
                {
                    'c' => BlackoutModes.NameOf(BlackoutMode.City),
                    'a' => BlackoutModes.NameOf(BlackoutMode.CityAndVehicles),
                    _ => null,
                },
            });

            number = i + 1;
        }

        return number == packed.Length ? new CustomCycleFile { SnowPass = packed[0] == '1', Cycle = cycle } : null;
    }

    // Turns "this many hours of this weather" into the running start hours the schedule is built on.
    public static CustomCycle? Build(CustomCycleFile? file, Action<string> warn)
    {
        if (file?.Cycle is not { Count: > 0 } blocks)
        {
            warn("it has no cycle entries.");

            return null;
        }

        var entries = new List<CycleEntry>(blocks.Count);
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
            file.SnowPass);
    }
}
