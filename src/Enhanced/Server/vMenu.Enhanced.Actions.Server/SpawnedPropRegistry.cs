namespace vMenu.Enhanced.Actions.Server;

public static class SpawnedPropRegistry
{
    private const int MaxRemembered = 128;

    private static readonly Dictionary<int, List<int>> ByPlayer = [];

    public static int PlayerCount => ByPlayer.Count;

    public static void RecordSpawn(int serverId, int networkId)
    {
        if (networkId == 0)
        {
            return;
        }

        if (!ByPlayer.TryGetValue(serverId, out var spawned))
        {
            spawned = [];

            ByPlayer[serverId] = spawned;
        }

        spawned.Remove(networkId);
        spawned.Add(networkId);

        while (spawned.Count > MaxRemembered)
        {
            spawned.RemoveAt(0);
        }
    }

    public static void RecordRemoval(int serverId, int networkId)
    {
        if (ByPlayer.TryGetValue(serverId, out var spawned))
        {
            spawned.Remove(networkId);
        }
    }

    public static bool WasSpawnedBy(int serverId, int networkId) =>
        ByPlayer.TryGetValue(serverId, out var spawned) && spawned.Contains(networkId);

    public static int? SpawnedBy(int networkId)
    {
        foreach (var player in ByPlayer)
        {
            if (player.Value.Contains(networkId))
            {
                return player.Key;
            }
        }

        return null;
    }

    public static bool IsKnown(int networkId) => SpawnedBy(networkId) is not null;

    public static IReadOnlyList<int> PropsOf(int serverId) =>
        ByPlayer.TryGetValue(serverId, out var spawned) ? spawned : [];

    public static IEnumerable<string> Describe()
    {
        foreach (var player in ByPlayer)
        {
            yield return $"player {player.Key}: {player.Value.Count} prop(s), "
                + $"network id(s) {string.Join(", ", player.Value)}";
        }
    }

    public static void Drop(int serverId) => ByPlayer.Remove(serverId);
}
