using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Webhooks.Server;

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

        API.OnNetEvent(PlayerStateEvents.ReportFreecam, new Action<Player, bool>(OnReported), false);
        API.OnNetEvent(PlayerStateEvents.FreecamVehicle, new Action<Player, int, int>(OnVehicle), false);
        API.OnNetEvent(PlayerStateEvents.FreecamDone, new Action<Player>(OnDone), false);
        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);
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
        if (!Active.Contains(source.Handle) || networkId == 0)
        {
            return;
        }

        var entity = Native.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0
            || !Native.DoesEntityExist(entity)
            || Native.GetEntityType(entity) != VehicleType
            || Native.NetworkGetEntityOwner(entity) != source.Handle)
        {
            return;
        }

        Vehicles[source.Handle] = (networkId, lockStatus > 0 ? lockStatus : Unlocked);
    }

    private static void OnDone([FromSource] Player source) => UnlockVehicle(source.Handle);

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
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

        var entity = Native.NetworkGetEntityFromNetworkId(vehicle.NetworkId);

        if (entity != 0 && Native.DoesEntityExist(entity) && Native.GetEntityType(entity) == VehicleType)
        {
            Native.SetVehicleDoorsLocked(entity, vehicle.LockStatus);
        }
    }

    // A named method so the binder finds FromSourceAttribute on it.
    private static void OnReported([FromSource] Player source, bool active)
    {
        if (active && !ServerPermissions.IsPlayerAllowed(source, Global.Freecam))
        {
            active = false;
        }

        var changed = active ? Active.Add(source.Handle) : Active.Remove(source.Handle);

        if (!changed)
        {
            return;
        }

        ServerStateBags.SetPlayer(source.Handle, PlayerStateKeys.Freecam, active);

        WebhookLog.Action(WebhookActor.For(source), active ? "turned the freecam on." : "turned the freecam off.");

        Log.Debug($"[Freecam] {source.Name} ({source.Handle}) is {(active ? "now" : "no longer")} in the freecam.");
    }
}
