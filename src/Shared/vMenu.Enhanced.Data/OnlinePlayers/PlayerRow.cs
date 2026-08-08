namespace vMenu.Enhanced.Data.OnlinePlayers;

/// <summary>
/// One player in the list the server sends back, as it travels over the wire.
/// </summary>
/// <remarks>
/// An action answers with a <c>string[]</c>, so a player has to fit in one string. Both halves of the
/// conversation live here so they cannot drift apart.
/// <para>
/// Deliberately only a server id and a name. Identifiers stay on the server, which is also why the
/// search runs there.
/// </para>
/// </remarks>
public static class PlayerRow
{
    /// <summary>
    /// ASCII 31, "unit separator". Chosen because a player name cannot contain it, so no amount of
    /// creative naming can break the split.
    /// </summary>
    public const char Separator = '\u001F';

    public static string Format(int serverId, string name) => $"{serverId}{Separator}{name}";

    /// <summary>Reads a row back. Returns false for anything malformed rather than throwing.</summary>
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
