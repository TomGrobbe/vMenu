using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.VehicleData;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class VehicleSpawnActions
{
    private const string DroppedEvent = "playerDropped";

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(VehicleEvents.Spawned, new Action<Player, int>(OnSpawned), false);

        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);

        VehicleOrphanMode.Register();
    }

    private static void OnSpawned([FromSource] Player source, int networkId)
    {
        SpawnedVehicleRegistry.RecordSpawn(source.Handle, networkId);

        VehicleOrphanMode.Queue(source.Handle, networkId);
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source > 0)
        {
            SpawnedVehicleRegistry.Drop(source);
        }
    }
}
