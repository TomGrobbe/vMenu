using System.Globalization;

namespace vMenu.Enhanced.Data.World;

/// <summary>The replicated convars the server publishes world state through.</summary>
// Not Settings, and not in ConfigCatalog: that catalog is owner authored config and drives the
// generated example file, so listing these would invite editing state the server overwrites.
public static class WorldStateConvars
{
    public const string Utc = "vMenu.Enhanced.State.Utc";

    public const string Weather = "vMenu.Enhanced.State.Weather";

    public const string TimeOffset = "vMenu.Enhanced.State.TimeOffset";

    /// <summary>All three, for handing to the configuration module in one go.</summary>
    public static readonly string[] All = [Utc, Weather, TimeOffset];

    /// <summary>Weather is following the schedule.</summary>
    public const string Dynamic = "dynamic";

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

    public static string FormatUnix(long unixSeconds) =>
        unixSeconds.ToString(CultureInfo.InvariantCulture);
}
