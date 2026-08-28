using System.Globalization;

namespace vMenu.Enhanced.Data.Admin;

public static class AnnouncementRow
{
    public const char Separator = '\u001F';

    public static string Format(int index, string name, string text, int everyMinutes, string at, string clock)
    {
        var indexText = index.ToString(CultureInfo.InvariantCulture);
        var minutesText = everyMinutes.ToString(CultureInfo.InvariantCulture);

        return $"{indexText}{Separator}{name}{Separator}{text}{Separator}{minutesText}{Separator}{at}{Separator}{clock}";
    }

    public static bool TryParse(
        string? row,
        out int index,
        out string name,
        out string text,
        out int everyMinutes,
        out string at,
        out string clock)
    {
        index = 0;
        name = string.Empty;
        text = string.Empty;
        everyMinutes = 0;
        at = string.Empty;
        clock = string.Empty;

        if (string.IsNullOrEmpty(row))
        {
            return false;
        }

        var parts = row.Split(Separator, 6);

        if (parts.Length < 6
            || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
            || !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out everyMinutes))
        {
            return false;
        }

        name = parts[1];
        text = parts[2];
        at = parts[4];
        clock = parts[5];

        return true;
    }
}
