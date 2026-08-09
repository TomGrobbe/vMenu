namespace vMenu.Enhanced.Data.World;

/// <summary>Reads the preset times of day a server owner lists in a convar.</summary>
// Deliberately stricter than TimeText: this is written once in a config file rather than typed by a
// player under pressure, so a malformed entry is reported instead of guessed at.
public static class TimePresets
{
    public const string Default = "0000,0300,0600,0900,1200,1500,1800,2100";

    /// <param name="rejected">Entries that were not four digit 24 hour times, for the caller to report.</param>
    public static List<int> Parse(string? text, List<string>? rejected = null)
    {
        var presets = new List<int>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return presets;
        }

        foreach (var entry in text.Split(','))
        {
            // A trailing comma is a slip, not a mistake worth naming.
            if (entry.Length == 0)
            {
                continue;
            }

            if (TryParse(entry, out var secondOfDay))
            {
                presets.Add(secondOfDay);

                continue;
            }

            rejected?.Add(entry);
        }

        return presets;
    }

    private static bool TryParse(string entry, out int secondOfDay)
    {
        secondOfDay = 0;

        if (entry.Length != 4)
        {
            return false;
        }

        foreach (var character in entry)
        {
            if (!char.IsAsciiDigit(character))
            {
                return false;
            }
        }

        var hour = ((entry[0] - '0') * 10) + (entry[1] - '0');
        var minute = ((entry[2] - '0') * 10) + (entry[3] - '0');

        if (hour > 23 || minute > 59)
        {
            return false;
        }

        secondOfDay = (hour * 3600) + (minute * 60);

        return true;
    }
}
