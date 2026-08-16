using System.Globalization;

namespace vMenu.Enhanced.Data.StaffAlerts;

public static class AlertRow
{
    // ASCII 31, "unit separator".
    public const char Separator = '\u001F';

    public static string Format(int id, int secondsLeft, string player, string description)
    {
        var idText = id.ToString(CultureInfo.InvariantCulture);
        var secondsText = secondsLeft.ToString(CultureInfo.InvariantCulture);

        return $"{idText}{Separator}{secondsText}{Separator}{player}{Separator}{description}";
    }

    public static bool TryParse(string? row, out int id, out int secondsLeft, out string player, out string description)
    {
        id = 0;
        secondsLeft = 0;
        player = string.Empty;
        description = string.Empty;

        if (string.IsNullOrEmpty(row))
        {
            return false;
        }

        var parts = row.Split(Separator, 4);

        if (parts.Length < 4
            || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out id)
            || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out secondsLeft))
        {
            return false;
        }

        player = parts[2];
        description = parts[3];

        return true;
    }
}
