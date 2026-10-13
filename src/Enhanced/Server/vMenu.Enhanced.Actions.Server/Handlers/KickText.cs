namespace vMenu.Enhanced.Actions.Server.Handlers;

internal static class KickText
{
    public static string For(string by, string? reason)
    {
        var trimmed = reason?.Trim() ?? string.Empty;

        return trimmed.Length > 0
            ? $"You have been kicked from the server by {by}, reason: {trimmed}"
            : $"You have been kicked from the server by {by}.";
    }
}
