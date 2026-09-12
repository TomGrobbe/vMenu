using System.Globalization;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Webhooks.Server;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

public sealed class IntegrationStatus
{
    public required string Resource { get; init; }

    public required string Version { get; init; }

    public required string Status { get; init; }

    public required string Name { get; init; }

    public required int PlayerCount { get; init; }

    public required int MaxPlayers { get; init; }

    public required bool ActionsEnabled { get; init; }

    public static IntegrationStatus Current() => new()
    {
        Resource = WebhookIdentity.Resource,
        Version = WebhookIdentity.Version,
        Status = "ok",
        Name = Native.GetConvar("sv_projectName", string.Empty),
        PlayerCount = ConnectedPlayers.All().Count,
        MaxPlayers = MaxClients(),
        ActionsEnabled = ServerConfig.Value(IntegrationSettings.AllowActions),
    };

    private static int MaxClients() =>
        int.TryParse(
            Native.GetConvar("sv_maxClients", "0"),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var max)
            ? max
            : 0;
}
