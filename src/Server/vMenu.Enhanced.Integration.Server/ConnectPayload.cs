using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Integration.Server;

public sealed class ConnectPayload
{
    public required string Resource { get; init; }

    public required string Version { get; init; }

    public required string Name { get; init; }

    public required int MaxPlayers { get; init; }

    public required int Count { get; init; }

    // Reads natives, so must be built on the tick thread.
    public static ConnectPayload Current()
    {
        var status = IntegrationStatus.Current();

        return new ConnectPayload
        {
            Resource = WebhookIdentity.Resource,
            Version = WebhookIdentity.Version,
            Name = status.Name,
            MaxPlayers = status.MaxPlayers,
            Count = status.PlayerCount,
        };
    }
}
