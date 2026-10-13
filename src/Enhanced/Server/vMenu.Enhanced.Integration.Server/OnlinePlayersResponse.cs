using System.Globalization;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Players.Server;

namespace vMenu.Enhanced.Integration.Server;

// Exposes only these fields; never the IP or any other identifier.
public sealed class OnlinePlayer
{
    public required int ServerId { get; init; }

    public required string Name { get; init; }

    public required int Ping { get; init; }

    public string? Discord { get; init; }
}

public sealed class OnlinePlayersResponse
{
    public required int Count { get; init; }

    public required IReadOnlyList<OnlinePlayer> Players { get; init; }

    public static OnlinePlayersResponse Current()
    {
        var connected = ConnectedPlayers.All();
        var players = new List<OnlinePlayer>(connected.Count);

        foreach (var player in connected)
        {
            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);

            players.Add(new OnlinePlayer
            {
                ServerId = player.ServerId,
                Name = player.Name,
                Ping = Native.GetPlayerPing(handle),
                Discord = PlayerIdentifiers.Discord(handle),
            });
        }

        return new OnlinePlayersResponse
        {
            Count = players.Count,
            Players = players,
        };
    }
}
