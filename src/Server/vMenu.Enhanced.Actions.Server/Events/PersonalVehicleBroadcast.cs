using System.Globalization;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Ticks.Server;

namespace vMenu.Enhanced.Actions.Server.Events;

public static class PersonalVehicleBroadcast
{
    private const long TickMs = 1000;

    private static readonly List<int> Owners = [];

    private static readonly Dictionary<int, List<string>> Occupants = [];

    private static readonly List<string> Nobody = [];

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        ServerTickRegistry.Register("PersonalVehicle.Broadcast", Broadcast, TickRate.Every(TickMs));
    }

    private static void Broadcast()
    {
        Owners.Clear();
        PersonalVehicleRegistry.CollectOwners(Owners);

        if (Owners.Count == 0)
        {
            return;
        }

        var players = ConnectedPlayers.All();

        BuildOccupants(players);

        foreach (var owner in Owners)
        {
            var networkId = PersonalVehicleRegistry.Marked(owner);

            if (networkId == 0)
            {
                continue;
            }

            var entity = PersonalVehicleActions.Resolve(networkId);

            if (entity == 0)
            {
                PersonalVehicleRegistry.ClearMarked(owner);

                API.EmitClient(owner, PersonalVehicleEvents.Lost);

                continue;
            }

            SendTo(owner, networkId, entity);
        }

        Occupants.Clear();
    }

    private static void BuildOccupants(List<ConnectedPlayer> players)
    {
        Occupants.Clear();

        foreach (var owner in Owners)
        {
            var entity = PersonalVehicleActions.Resolve(PersonalVehicleRegistry.Marked(owner));

            if (entity != 0 && !Occupants.ContainsKey(entity))
            {
                Occupants[entity] = [];
            }
        }

        if (Occupants.Count == 0)
        {
            return;
        }

        foreach (var player in players)
        {
            var ped = Native.GetPlayerPed(player.ServerId.ToString(CultureInfo.InvariantCulture));

            if (ped == 0 || !Native.DoesEntityExist(ped))
            {
                continue;
            }

            var vehicle = Native.GetVehiclePedIsIn(ped, false);

            if (vehicle != 0 && Occupants.TryGetValue(vehicle, out var names))
            {
                names.Add(player.Name);
            }
        }
    }

    private static void SendTo(int owner, int networkId, int entity)
    {
        var handle = owner.ToString(CultureInfo.InvariantCulture);

        var inRange = Native.GetPlayerRoutingBucket(handle) == Native.GetEntityRoutingBucket(entity);

        var position = Native.GetEntityCoords(entity);

        var names = Occupants.TryGetValue(entity, out var found) ? found : Nobody;

        var row = PersonalVehicleRow.Format(
            networkId,
            position.X,
            position.Y,
            position.Z,
            (int)Native.GetEntityHeading(entity),
            unchecked((uint)Native.GetEntityModel(entity)),
            inRange,
            names);

        API.EmitClient(owner, PersonalVehicleEvents.Update, row);
    }
}
