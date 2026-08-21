using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Vehicles;

internal static class NetworkEntity
{
    private const int StreamTimeoutMs = 3000;

    private const int ControlTimeoutMs = 500;

    public static int Find(int networkId)
    {
        if (networkId == 0 || !Native.NetworkDoesNetworkIdExist(networkId))
        {
            return 0;
        }

        var entity = Native.NetworkGetEntityFromNetworkId(networkId);

        return entity != 0 && Native.DoesEntityExist(entity) ? entity : 0;
    }

    public static async Task<int> ResolveAsync(int networkId)
    {
        var started = Native.GetGameTimer();

        while (Native.GetGameTimer() - started < StreamTimeoutMs)
        {
            if (Find(networkId) is var entity and not 0)
            {
                return entity;
            }

            await API.Delay(0);
        }

        return 0;
    }

    public static async Task<bool> TakeControlAsync(int entity)
    {
        if (Native.NetworkHasControlOfEntity(entity))
        {
            return true;
        }

        var started = Native.GetGameTimer();

        while (Native.GetGameTimer() - started < ControlTimeoutMs)
        {
            Native.NetworkRequestControlOfEntity(entity);

            if (Native.NetworkHasControlOfEntity(entity))
            {
                return true;
            }

            await API.Delay(0);
        }

        return false;
    }
}
