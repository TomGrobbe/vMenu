using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace vMenu.Enhanced.BrokenNatives.Server;

public static partial class NativeFixer
{
    public static bool DropPlayer(string client, string reason)
    {
        API.DropPlayer(client, reason);
        return true;
    }

    public static int GetGameTimer()
        => unchecked((int)API.GetGameTimer());

    public static bool SetEntityOrphanMode(int entityID, int orphanModeInt)
    {
        API.SetEntityOrphanMode(entityID, orphanModeInt);
        return true;
    }

    public static bool SetPlayerRoutingBucket(string clientIDStr, int routingBucketID)
    {
        API.SetPlayerRoutingBucket(clientIDStr, routingBucketID);
        return true;
    }

    public static bool SetRoutingBucketPopulationEnabled(int routingBucketID, bool enabled)
    {
        API.SetRoutingBucketPopulationEnabled(routingBucketID, enabled);
        return true;
    }

    public static bool DeleteEntity(int entityID)
    {
        API.DeleteEntity(entityID);
        return true;
    }
}
