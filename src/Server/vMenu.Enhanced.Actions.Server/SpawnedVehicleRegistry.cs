namespace vMenu.Enhanced.Actions.Server;

public static class SpawnedVehicleRegistry
{
    private const int MaxRemembered = 32;

    private static readonly Dictionary<int, List<int>> ByPlayer = [];

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

    public static bool WasSpawnedBy(int serverId, int networkId) =>
        ByPlayer.TryGetValue(serverId, out var spawned) && spawned.Contains(networkId);

    public static void PruneSpawned(int serverId, Predicate<int> stillExists)
    {
        if (ByPlayer.TryGetValue(serverId, out var spawned))
        {
            spawned.RemoveAll(networkId => !stillExists(networkId));
        }
    }

    public static void Drop(int serverId) => ByPlayer.Remove(serverId);

    public static void ForgetAll() => ByPlayer.Clear();
}
