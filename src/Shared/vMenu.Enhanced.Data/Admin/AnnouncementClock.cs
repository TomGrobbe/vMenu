namespace vMenu.Enhanced.Data.Admin;

public static class AnnouncementClock
{
    public const string Real = "real";

    public const string Game = "game";

    public static bool IsGame(string? clock) =>
        string.Equals(clock, Game, StringComparison.OrdinalIgnoreCase);

    public static bool IsKnown(string? clock) =>
        string.IsNullOrWhiteSpace(clock)
        || string.Equals(clock, Real, StringComparison.OrdinalIgnoreCase)
        || IsGame(clock);
}
