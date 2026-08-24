namespace vMenu.Enhanced.Data.OnlinePlayers;

// An action answers with a string[], so a player has to fit in one string. Both halves of the
// conversation live here so they cannot drift apart. Deliberately only a server id and a name:
// identifiers stay on the server, which is also why the search runs there.
public static class PlayerRow
{
    // ASCII 31, "unit separator". Chosen because a player name cannot contain it, so no amount of
    // creative naming can break the split.
    public const char Separator = '\u001F';

    public static string Format(int serverId, string name) => $"{serverId}{Separator}{name}";

    // Returns false for anything malformed rather than throwing.
    public static bool TryParse(string? row, out int serverId, out string name)
    {
        serverId = 0;
        name = string.Empty;

        if (string.IsNullOrEmpty(row))
        {
            return false;
        }

        var split = row.IndexOf(Separator);

        if (split < 1)
        {
            return false;
        }

        if (!int.TryParse(row[..split], out serverId))
        {
            return false;
        }

        name = row[(split + 1)..];

        return true;
    }
}
