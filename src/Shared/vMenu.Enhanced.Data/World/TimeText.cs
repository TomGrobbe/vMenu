namespace vMenu.Enhanced.Data.World;

// Reads a time of day the way a player would type it. Whitespace is stripped first, so "12 30 pm"
// and "1230pm" are the same input and the digit only forms fall out of the length. Without a
// meridiem the value is read as a 24 hour clock.
public static class TimeText
{
    public const string Example = "15:25";

    public const int MaxInputLength = 12;

    private static readonly char[] Separators = [':', '.', ','];

    public static bool TryParse(string? input, out int secondOfDay)
    {
        secondOfDay = 0;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var text = Compact(input);
        var meridiem = TakeMeridiem(ref text);

        if (text.Length == 0)
        {
            return false;
        }

        if (!TrySplit(text, out var hour, out var minute))
        {
            return false;
        }

        if (meridiem is not Meridiem.None)
        {
            if (hour is < 1 or > 12)
            {
                return false;
            }

            hour = meridiem is Meridiem.Am
                ? (hour == 12 ? 0 : hour)
                : (hour == 12 ? 12 : hour + 12);
        }

        if (hour > 23 || minute > 59)
        {
            return false;
        }

        secondOfDay = (hour * 3600) + (minute * 60);

        return true;
    }

    public static string Format(int secondOfDay)
    {
        var wrapped = WorldStateConvars.NormaliseOffset(secondOfDay);

        return $"{wrapped / 3600:00}:{wrapped % 3600 / 60:00}";
    }

    private static string Compact(string input)
    {
        var chars = new char[input.Length];
        var length = 0;

        foreach (var character in input)
        {
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            chars[length++] = char.ToUpperInvariant(character);
        }

        return new string(chars, 0, length);
    }

    private static Meridiem TakeMeridiem(ref string text)
    {
        foreach (var (suffix, meridiem) in Suffixes)
        {
            if (!text.EndsWith(suffix, StringComparison.Ordinal))
            {
                continue;
            }

            text = text[..^suffix.Length];

            return meridiem;
        }

        return Meridiem.None;
    }

    private static bool TrySplit(string text, out int hour, out int minute)
    {
        hour = 0;
        minute = 0;

        var separator = text.IndexOfAny(Separators);

        if (separator >= 0)
        {
            var right = text[(separator + 1)..];

            return TryDigits(text[..separator], 1, 2, out hour)
                && TryDigits(right, 1, 2, out minute);
        }

        if (!TryDigits(text, 1, 4, out var value))
        {
            return false;
        }

        // Bare digits: up to two are an hour, three or four carry the minutes as the last two.
        if (text.Length <= 2)
        {
            hour = value;

            return true;
        }

        hour = value / 100;
        minute = value % 100;

        return true;
    }

    private static bool TryDigits(string text, int minLength, int maxLength, out int value)
    {
        value = 0;

        if (text.Length < minLength || text.Length > maxLength)
        {
            return false;
        }

        foreach (var character in text)
        {
            if (!char.IsAsciiDigit(character))
            {
                return false;
            }

            value = (value * 10) + (character - '0');
        }

        return true;
    }

    private static (string Suffix, Meridiem Meridiem)[] Suffixes =>
    [
        ("AM", Meridiem.Am),
        ("PM", Meridiem.Pm),
        ("A", Meridiem.Am),
        ("P", Meridiem.Pm),
    ];

    private enum Meridiem
    {
        None,
        Am,
        Pm,
    }
}
