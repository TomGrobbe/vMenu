using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Integration.Server;

public sealed class PlayersChangedPayload
{
    public required string Resource { get; init; }

    public required string Version { get; init; }

    public required int Count { get; init; }

    public required IReadOnlyList<OnlinePlayer> Players { get; init; }

    public static PlayersChangedPayload Of(OnlinePlayersResponse roster) => new()
    {
        Resource = WebhookIdentity.Resource,
        Version = WebhookIdentity.Version,
        Count = roster.Count,
        Players = roster.Players,
    };
}
