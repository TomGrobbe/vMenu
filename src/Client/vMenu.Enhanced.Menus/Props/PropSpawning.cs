using System.Numerics;

using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;

using vMenu.Enhanced.Data.Props;

namespace vMenu.Enhanced.Menus.Props;

public static class PropSpawning
{
    private const int ModelTimeoutMs = 10_000;

    public static bool IsSpawnable(uint hash) =>
        Native.IsModelValid(hash)
        && Native.IsModelInCdimage(hash)
        && !Native.IsModelAVehicle(hash)
        && !Native.IsModelAPed(hash);

    public static async Task<Prop?> SpawnAsync(uint hash, Vector3 position, bool networked, bool frozen)
    {
        if (!IsSpawnable(hash))
        {
            return null;
        }

        Native.RequestModel(hash);

        var deadline = Native.GetGameTimer() + ModelTimeoutMs;

        while (!Native.HasModelLoaded(hash))
        {
            if (Native.GetGameTimer() > deadline)
            {
                Native.SetModelAsNoLongerNeeded(hash);

                return null;
            }

            await API.Delay(0);
        }

        var prop = networked
            ? await API.Props.RequestAndCreate(hash, position, false, true, true, true, false)
            : API.Props.Create(hash, position, false, false, true, true, false);

        Native.SetModelAsNoLongerNeeded(hash);

        if (prop is null)
        {
            return null;
        }

        SpawnedProps.Track(prop.Handle);

        Report(prop.Handle);

        Settle(prop.Handle, frozen, snapToGround: false);

        return prop;
    }

    // Only a networked prop has a network id, or anything the server could know about.
    private static void Report(int entity)
    {
        if (entity == 0 || !Native.NetworkGetEntityIsNetworked(entity))
        {
            return;
        }

        API.EmitServer(PropEvents.Spawned, Native.NetworkGetNetworkIdFromEntity(entity));
    }

    public static void Settle(int entity, bool frozen, bool snapToGround)
    {
        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return;
        }

        if (snapToGround)
        {
            Native.PlaceObjectOnGroundProperly(entity);
        }

        Native.ResetEntityAlpha(entity);
        Native.SetEntityCollision(entity, true, true);
        Native.SetEntityInvincible(entity, false, false);

        if (frozen)
        {
            Native.FreezeEntityPosition(entity, true);

            return;
        }

        Native.FreezeEntityPosition(entity, false);
        Native.SetEntityHasGravity(entity, true);

        Native.ActivatePhysics(entity);
    }
}
