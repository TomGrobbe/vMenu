using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Players.Server;

// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class ConnectedPlayer(int serverId, string name)
{
    public int ServerId { get; } = serverId;

    public string Name { get; } = name;
}

public static class ConnectedPlayers
{
    // Read straight from the game rather than from a cached player list, so a player who has already left
    // is never handed back.
    public static List<ConnectedPlayer> All()
    {
        var count = API.GetNumPlayerIndices();
        var players = new List<ConnectedPlayer>(count);

        for (var index = 0; index < count; index++)
        {
            var handle = API.GetPlayerFromIndex(index);

            if (string.IsNullOrEmpty(handle)
                || !int.TryParse(handle, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId)
                || !API.DoesPlayerExist(handle))
            {
                continue;
            }

            var name = API.GetPlayerName(handle);

            // Falls back to the server id so a row is never blank. Somebody you cannot see is somebody you
            // cannot pick.
            players.Add(new ConnectedPlayer(
                serverId,
                string.IsNullOrWhiteSpace(name) ? $"#{serverId}" : name));
        }

        return players;
    }

    // A scan rather than NetworkGetEntityOwner, which answers which client is simulating the entity. The
    // question here is whose character this is rather than whose machine is running it.
    public static ConnectedPlayer? Owning(IReadOnlyList<ConnectedPlayer> players, int entity)
    {
        if (entity == 0)
        {
            return null;
        }

        foreach (var player in players)
        {
            if (API.GetPlayerPed(player.ServerId.ToString(CultureInfo.InvariantCulture)) == entity)
            {
                return player;
            }
        }

        return null;
    }
}
