using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles;

internal static class NetworkEntity
{
    private const int StreamTimeoutMs = 3000;

    private const int ControlTimeoutMs = 500;

    public static int Find(int networkId)
    {
        if (networkId == 0 || !API.NetworkDoesNetworkIdExist(networkId))
        {
            return 0;
        }

        var entity = API.NetworkGetEntityFromNetworkId(networkId);

        return entity != 0 && API.DoesEntityExist(entity) ? entity : 0;
    }

    public static async Task<int> ResolveAsync(int networkId)
    {
        var started = API.GetGameTimer();

        while (API.GetGameTimer() - started < StreamTimeoutMs)
        {
            if (Find(networkId) is var entity and not 0)
            {
                return entity;
            }

            await BaseScript.Delay(0);
        }

        return 0;
    }

    public static async Task<bool> TakeControlAsync(int entity)
    {
        if (API.NetworkHasControlOfEntity(entity))
        {
            return true;
        }

        var started = API.GetGameTimer();

        while (API.GetGameTimer() - started < ControlTimeoutMs)
        {
            API.NetworkRequestControlOfEntity(entity);

            if (API.NetworkHasControlOfEntity(entity))
            {
                return true;
            }

            await BaseScript.Delay(0);
        }

        return false;
    }
}
