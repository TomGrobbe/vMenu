namespace vMenu.Enhanced.Data.StaffAlerts;

public static class AlertSelector
{
    public const string Oldest = "oldest";

    public const string Latest = "latest";

    public static bool IsOldest(string value) => Matches(value, Oldest);

    public static bool IsLatest(string value) => Matches(value, Latest);

    private static bool Matches(string value, string selector) =>
        string.Equals(value, selector, StringComparison.OrdinalIgnoreCase);
}
