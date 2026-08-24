using System.Globalization;

namespace vMenu.Enhanced.Data.Configuration;

// Turns a raw convar string into a typed value. Pure so both sides parse identically; reporting an
// unparseable value is the caller's job, because this assembly has no logger.
public static class ConvarValue
{
    private static readonly string[] True = ["true", "1", "yes", "on"];

    private static readonly string[] False = ["false", "0", "no", "off"];

    // Trims, unquotes and maps an empty value to null.
    public static string? Normalise(string? raw)
    {
        if (raw is null)
        {
            return null;
        }

        var value = raw.Trim();

        if (value.Length > 1 && value[0] == '"' && value[^1] == '"')
        {
            value = value[1..^1].Trim();
        }

        return value.Length == 0 ? null : value;
    }

    public static bool? ParseBool(string? raw)
    {
        if (Normalise(raw) is not { } value)
        {
            return null;
        }

        if (Matches(True, value))
        {
            return true;
        }

        return Matches(False, value) ? false : null;
    }

    // Hand rolled rather than candidates.Contains(value, comparer): that binds to
    // MemoryExtensions.Contains rather than LINQ, and the sandbox refuses the implicit string[] to
    // ReadOnlySpan<string> conversion it needs.
    private static bool Matches(string[] candidates, string value)
    {
        foreach (var candidate in candidates)
        {
            if (string.Equals(candidate, value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static int? ParseInt(string? raw) =>
        Normalise(raw) is { } value && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    public static float? ParseFloat(string? raw) =>
        Normalise(raw) is { } value && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
}
