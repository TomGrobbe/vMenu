using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.VehicleData;

using vMenu.Legacy.Hosting;

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

        Script.EventHandlers[VehicleEvents.Spawned] += new Action<Player, int>(OnSpawned);

        Script.EventHandlers[DroppedEvent] += new Action<string, string?>(OnPlayerDropped);

        VehicleOrphanMode.Register();
    }

    private static void OnSpawned([FromSource] Player source, int networkId)
    {
        SpawnedVehicleRegistry.RecordSpawn(source.ServerId(), networkId);

        VehicleOrphanMode.Queue(source.ServerId(), networkId);
    }

    private static void OnPlayerDropped([FromSource] string sourceString, string? reason = null)
    {
        if (Script.IsFromPlayer(sourceString))
        {
            return;
        }

        var source = Script.SourceId(sourceString);

        if (source > 0)
        {
            SpawnedVehicleRegistry.Drop(source);
        }
    }
}
