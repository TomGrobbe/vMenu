namespace vMenu.Enhanced.Actions.Server;

public static class PersonalVehicleRegistry
{
    private static readonly Dictionary<int, int> MarkedByPlayer = [];

    public static int Marked(int serverId) =>
        MarkedByPlayer.TryGetValue(serverId, out var networkId) ? networkId : 0;

    public static void SetMarked(int serverId, int networkId) => MarkedByPlayer[serverId] = networkId;

    public static void ClearMarked(int serverId) => MarkedByPlayer.Remove(serverId);

    public static void Drop(int serverId) => MarkedByPlayer.Remove(serverId);

    public static void ForgetAll() => MarkedByPlayer.Clear();

    public static void CollectOwners(List<int> owners)
    {
        foreach (var pair in MarkedByPlayer)
        {
            if (pair.Value != 0)
            {
                owners.Add(pair.Key);
            }
        }
    }
}
