using System.Globalization;

namespace vMenu.Enhanced.Data.World;

// The replicated convars the server publishes world state through. Not Settings, and not in
// ConfigCatalog: that catalog is owner authored config and drives the generated example file, so
// listing these would invite editing state the server overwrites.
public static class WorldStateConvars
{
    public const string Utc = "vMenu.Enhanced.State.Utc";

    public const string Weather = "vMenu.Enhanced.State.Weather";

    public const string TimeOffset = "vMenu.Enhanced.State.TimeOffset";

    public const string Blackout = "vMenu.Enhanced.State.Blackout";

    public const string Snow = "vMenu.Enhanced.State.Snow";

    public static readonly string[] All = [Utc, Weather, TimeOffset, Blackout, Snow];

    public const string Dynamic = "dynamic";

    private const char FrozenSeparator = '@';

    public static int NormaliseOffset(int seconds)
    {
        var wrapped = seconds % GameClock.SecondsPerGameDay;

        return wrapped < 0 ? wrapped + GameClock.SecondsPerGameDay : wrapped;
    }

    public static bool TryParseOffset(string? value, out int seconds)
    {
        seconds = 0;

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return false;
        }

        seconds = NormaliseOffset(parsed);

        return true;
    }

    public static bool TryParseUnix(string? value, out long unixSeconds) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out unixSeconds)
        && unixSeconds > 0L;

    public static string FormatOffset(int seconds) =>
        NormaliseOffset(seconds).ToString(CultureInfo.InvariantCulture);

    public static string FormatTime(int offsetSeconds, double? frozenAtUnix) =>
        frozenAtUnix is { } pinned
            ? FormatOffset(offsetSeconds) + FrozenSeparator + pinned.ToString("F3", CultureInfo.InvariantCulture)
            : FormatOffset(offsetSeconds);

    public static bool TryParseTime(string? value, out int offsetSeconds, out double? frozenAtUnix)
    {
        offsetSeconds = 0;
        frozenAtUnix = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var separator = value.IndexOf(FrozenSeparator);

        if (separator < 0)
        {
            return TryParseOffset(value, out offsetSeconds);
        }

        if (!TryParseOffset(value[..separator], out offsetSeconds))
        {
            return false;
        }

        if (double.TryParse(
                value[(separator + 1)..],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var pinned)
            && pinned > 0.0)
        {
            frozenAtUnix = pinned;
        }

        return true;
    }

    public static string FormatUnix(long unixSeconds) =>
        unixSeconds.ToString(CultureInfo.InvariantCulture);
}
