using System.Globalization;

using CitizenFX.FiveM.Server;

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
    // Read straight from the game rather than from API.Players.All. That is a cache the runtime fills on
    // demand and never prunes, so it keeps handing back players who have already left: they come through
    // with an empty name, they fail every action, and nothing ever notices they are gone.
    public static List<ConnectedPlayer> All()
    {
        var count = Native.GetNumPlayerIndices();
        var players = new List<ConnectedPlayer>(count);

        for (var index = 0; index < count; index++)
        {
            var handle = Native.GetPlayerFromIndex(index);

            if (string.IsNullOrEmpty(handle)
                || !int.TryParse(handle, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId)
                || !Native.DoesPlayerExist(handle))
            {
                continue;
            }

            var name = Native.GetPlayerName(handle);

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
            if (Native.GetPlayerPed(player.ServerId.ToString(CultureInfo.InvariantCulture)) == entity)
            {
                return player;
            }
        }

        return null;
    }
}
