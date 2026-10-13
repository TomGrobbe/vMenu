
using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Props;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Props;

public static class PropSpawning
{
    private const int ModelTimeoutMs = 10_000;

    public static bool IsSpawnable(uint hash) =>
        API.IsModelValid(hash)
        && API.IsModelInCdimage(hash)
        && !API.IsModelAVehicle(hash)
        && !API.IsModelAPed(hash);

    public static async Task<Prop?> SpawnAsync(uint hash, Vector3 position, bool networked, bool frozen)
    {
        if (!IsSpawnable(hash))
        {
            return null;
        }

        API.RequestModel(hash);

        var deadline = API.GetGameTimer() + ModelTimeoutMs;

        while (!API.HasModelLoaded(hash))
        {
            if (API.GetGameTimer() > deadline)
            {
                API.SetModelAsNoLongerNeeded(hash);

                return null;
            }

            await BaseScript.Delay(0);
        }

        var handle = NativeFixer.CreateObjectNoOffset(hash, position.X, position.Y, position.Z, networked, true, true, false);

        API.SetModelAsNoLongerNeeded(hash);

        if (handle <= 0)
        {
            return null;
        }

        var prop = new Prop(handle);

        SpawnedProps.Track(prop.Handle);

        Report(prop.Handle);

        Settle(prop.Handle, frozen, snapToGround: false);

        return prop;
    }

    // Only a networked prop has a network id, or anything the server could know about.
    private static void Report(int entity)
    {
        if (entity == 0 || !API.NetworkGetEntityIsNetworked(entity))
        {
            return;
        }

        BaseScript.TriggerServerEvent(PropEvents.Spawned, API.NetworkGetNetworkIdFromEntity(entity));
    }

    public static void Settle(int entity, bool frozen, bool snapToGround)
    {
        if (entity == 0 || !API.DoesEntityExist(entity))
        {
            return;
        }

        if (snapToGround)
        {
            API.PlaceObjectOnGroundProperly(entity);
        }

        NativeFixer.ResetEntityAlpha(entity);
        API.SetEntityCollision(entity, true, true);
        NativeFixer.SetEntityInvincible(entity, false, false);

        if (frozen)
        {
            API.FreezeEntityPosition(entity, true);

            return;
        }

        API.FreezeEntityPosition(entity, false);
        API.SetEntityHasGravity(entity, true);

        API.ActivatePhysics(entity);
    }
}
