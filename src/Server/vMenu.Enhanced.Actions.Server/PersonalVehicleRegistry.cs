namespace vMenu.Enhanced.Actions.Server;

public static class PersonalVehicleRegistry
{
    private const int MaxRemembered = 32;

    private static readonly Dictionary<int, Tracked> ByPlayer = [];

    public static void RecordSpawn(int serverId, int networkId)
    {
        if (networkId == 0)
        {
            return;
        }

        if (!ByPlayer.TryGetValue(serverId, out var tracked))
        {
            tracked = new Tracked();

            ByPlayer[serverId] = tracked;
        }

        tracked.Spawned.Remove(networkId);
        tracked.Spawned.Add(networkId);

        while (tracked.Spawned.Count > MaxRemembered)
        {
            tracked.Spawned.RemoveAt(0);
        }
    }

    public static bool WasSpawnedBy(int serverId, int networkId) =>
        ByPlayer.TryGetValue(serverId, out var tracked) && tracked.Spawned.Contains(networkId);

    public static void PruneSpawned(int serverId, Predicate<int> stillExists)
    {
        if (!ByPlayer.TryGetValue(serverId, out var tracked))
        {
            return;
        }

        tracked.Spawned.RemoveAll(networkId => !stillExists(networkId));
    }

    public static int Marked(int serverId) =>
        ByPlayer.TryGetValue(serverId, out var tracked) ? tracked.Marked : 0;

    public static void SetMarked(int serverId, int networkId)
    {
        if (!ByPlayer.TryGetValue(serverId, out var tracked))
        {
            tracked = new Tracked();

            ByPlayer[serverId] = tracked;
        }

        tracked.Marked = networkId;
    }

    public static void ClearMarked(int serverId)
    {
        if (ByPlayer.TryGetValue(serverId, out var tracked))
        {
            tracked.Marked = 0;
        }
    }

    public static void Drop(int serverId) => ByPlayer.Remove(serverId);

    public static void CollectOwners(List<int> owners)
    {
        foreach (var pair in ByPlayer)
        {
            if (pair.Value.Marked != 0)
            {
                owners.Add(pair.Key);
            }
        }
    }

    private sealed class Tracked
    {
        public List<int> Spawned { get; } = [];

        public int Marked { get; set; }
    }
}
