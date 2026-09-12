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
}
