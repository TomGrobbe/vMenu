using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Webhooks.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Actions.Server.Events;

public static class PlayerFreecamState
{
    private const string DroppedEvent = "playerDropped";

    private const int VehicleType = 2;

    private const int Unlocked = 1;

    private static readonly HashSet<int> Active = [];

    private static readonly Dictionary<int, (int NetworkId, int LockStatus)> Vehicles = [];

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Script.EventHandlers[PlayerStateEvents.ReportFreecam] += new Action<Player, bool>(OnReported);
        Script.EventHandlers[PlayerStateEvents.FreecamVehicle] += new Action<Player, int, int>(OnVehicle);
        Script.EventHandlers[PlayerStateEvents.FreecamDone] += new Action<Player>(OnDone);
        Script.EventHandlers[DroppedEvent] += new Action<string, string?>(OnPlayerDropped);
    }

    public static bool IsActive(int serverId) => Active.Contains(serverId);

    public static bool HasAny => Active.Count > 0 || Vehicles.Count > 0;

    public static void Prune(HashSet<int> connected)
    {
        if (Active.Count > 0)
        {
            Active.RemoveWhere(serverId => !connected.Contains(serverId));
        }

        foreach (var serverId in Vehicles.Keys.Where(serverId => !connected.Contains(serverId)).ToList())
        {
            UnlockVehicle(serverId);
        }
    }

    // Only a vehicle the player owns, so nobody can have the server unlock somebody else's car.
    private static void OnVehicle([FromSource] Player source, int networkId, int lockStatus)
    {
        if (!Active.Contains(source.ServerId()) || networkId == 0)
        {
            return;
        }

        var entity = API.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0
            || !API.DoesEntityExist(entity)
            || API.GetEntityType(entity) != VehicleType
            || API.NetworkGetEntityOwner(entity) != source.ServerId())
        {
            return;
        }

        Vehicles[source.ServerId()] = (networkId, lockStatus > 0 ? lockStatus : Unlocked);
    }

    private static void OnDone([FromSource] Player source) => UnlockVehicle(source.ServerId());

    private static void OnPlayerDropped([FromSource] string sourceString, string? reason = null)
    {
        if (Script.IsFromPlayer(sourceString))
        {
            return;
        }

        var source = Script.SourceId(sourceString);

        if (source <= 0)
        {
            return;
        }

        Active.Remove(source);

        UnlockVehicle(source);
    }

    private static void UnlockVehicle(int serverId)
    {
        if (!Vehicles.Remove(serverId, out var vehicle))
        {
            return;
        }

        var entity = API.NetworkGetEntityFromNetworkId(vehicle.NetworkId);

        if (entity != 0 && API.DoesEntityExist(entity) && API.GetEntityType(entity) == VehicleType)
        {
            API.SetVehicleDoorsLocked(entity, vehicle.LockStatus);
        }
    }

    // A named method so the binder finds FromSourceAttribute on it.
    private static void OnReported([FromSource] Player source, bool active)
    {
        if (active && !ServerPermissions.IsPlayerAllowed(source, Global.Freecam))
        {
            active = false;
        }

        var changed = active ? Active.Add(source.ServerId()) : Active.Remove(source.ServerId());

        if (!changed)
        {
            return;
        }

        ServerStateBags.SetPlayer(source.ServerId(), PlayerStateKeys.Freecam, active);

        WebhookLog.Action(WebhookActor.For(source), active ? "turned the freecam on." : "turned the freecam off.");

        Log.Debug($"[Freecam] {source.Name} ({source.ServerId()}) is {(active ? "now" : "no longer")} in the freecam.");
    }
}
