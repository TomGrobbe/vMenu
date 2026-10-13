using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Actions.Server;

public static class PersonalVehicleRegistry
{
    private static readonly Dictionary<int, int> MarkedByPlayer = [];

    private static readonly Dictionary<int, (int NetworkId, int Model)> ElectrifiedByPlayer = [];

    public static int Marked(int serverId) =>
        MarkedByPlayer.TryGetValue(serverId, out var networkId) ? networkId : 0;

    public static void SetMarked(int serverId, int networkId)
    {
        Unelectrify(serverId);

        MarkedByPlayer[serverId] = networkId;
    }

    public static void ClearMarked(int serverId)
    {
        Unelectrify(serverId);

        MarkedByPlayer.Remove(serverId);
    }

    public static void Drop(int serverId) => ClearMarked(serverId);

    public static void ForgetAll()
    {
        MarkedByPlayer.Clear();
        ElectrifiedByPlayer.Clear();

        PublishElectrified();
    }

    public static void SetElectrified(int serverId, bool on)
    {
        Unelectrify(serverId);

        var networkId = Marked(serverId);
        var entity = networkId == 0 ? 0 : Native.NetworkGetEntityFromNetworkId(networkId);

        if (!on || entity == 0 || !Native.DoesEntityExist(entity))
        {
            return;
        }

        ElectrifiedByPlayer[serverId] = (networkId, (int)Native.GetEntityModel(entity));

        PublishElectrified();
    }

    public static void PublishElectrified() =>
        ServerStateBags.Set(
            BullyEvents.GlobalBag,
            BullyEvents.ElectrifiedKey,
            ElectrifiedByPlayer.SelectMany(pair => new[] { pair.Value.NetworkId, pair.Key, pair.Value.Model }).ToArray());

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

    private static void Unelectrify(int serverId)
    {
        if (ElectrifiedByPlayer.Remove(serverId))
        {
            PublishElectrified();
        }
    }
}
