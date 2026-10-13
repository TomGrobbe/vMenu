using CitizenFX.FiveM.Server;

namespace vMenu.Enhanced.Players.Server;

public static class PlayerIdentifiers
{
    private const string DiscordPrefix = "discord:";

    // The only identifier the integration may expose; IP and the rest stay server-side, as in the player list.
    public static string? Discord(string handle)
    {
        var count = Native.GetNumPlayerIdentifiers(handle);

        for (var index = 0; index < count; index++)
        {
            var identifier = Native.GetPlayerIdentifier(handle, index);

            if (!string.IsNullOrEmpty(identifier)
                && identifier.StartsWith(DiscordPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return identifier[DiscordPrefix.Length..];
            }
        }

        return null;
    }

    public static string? Find(string handle, string identifier)
    {
        var count = Native.GetNumPlayerIdentifiers(handle);

        for (var index = 0; index < count; index++)
        {
            var own = Native.GetPlayerIdentifier(handle, index);

            if (!string.IsNullOrEmpty(own) && own.Equals(identifier, StringComparison.OrdinalIgnoreCase))
            {
                return own;
            }
        }

        return null;
    }

    public static Dictionary<string, string> Collect(string handle, IReadOnlyCollection<string> types)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (types.Count == 0)
        {
            return result;
        }

        var count = Native.GetNumPlayerIdentifiers(handle);

        for (var index = 0; index < count; index++)
        {
            var identifier = Native.GetPlayerIdentifier(handle, index);
            if (string.IsNullOrEmpty(identifier))
            {
                continue;
            }

            var colon = identifier.IndexOf(':');
            if (colon <= 0)
            {
                continue;
            }

            var type = identifier[..colon];
            if (result.ContainsKey(type))
            {
                continue;
            }

            foreach (var wanted in types)
            {
                if (type.Equals(wanted, StringComparison.OrdinalIgnoreCase))
                {
                    result[type] = identifier[(colon + 1)..];
                    break;
                }
            }
        }

        return result;
    }
}
