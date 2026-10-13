using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;

namespace vMenu.Enhanced.Menus.Bully;

public static class Electrified
{
    public static int OwnerOf(int vehicle)
    {
        if (vehicle == 0 || !Native.NetworkGetEntityIsNetworked(vehicle))
        {
            return 0;
        }

        var networkId = Native.NetworkGetNetworkIdFromEntity(vehicle);
        var model = (int)Native.GetEntityModel(vehicle);
        var all = StateBags.Get<int[]>(BullyEvents.GlobalBag, BullyEvents.ElectrifiedKey) ?? [];

        for (var index = 0; index + 2 < all.Length; index += 3)
        {
            if (all[index] == networkId && all[index + 2] == model)
            {
                return all[index + 1];
            }
        }

        return 0;
    }

    public static bool IsOwnedBy(int networkId, int owner)
    {
        var all = StateBags.Get<int[]>(BullyEvents.GlobalBag, BullyEvents.ElectrifiedKey) ?? [];

        for (var index = 0; index + 2 < all.Length; index += 3)
        {
            if (all[index] == networkId && all[index + 1] == owner)
            {
                return true;
            }
        }

        return false;
    }
}
