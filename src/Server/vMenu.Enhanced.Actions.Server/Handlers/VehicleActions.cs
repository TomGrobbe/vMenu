using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Serialization.Server;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;
using VehicleOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

/// <summary>
/// Actions on a vehicle that already exists.
/// </summary>
public static class VehicleActions
{
    /// <summary>There is no <c>IsEntityAVehicle</c> server side, so the entity type is the check.</summary>
    private const int VehicleEntityType = 2;

    private const int DriverSeat = -1;

    /// <summary>
    /// Added to the configured reach. The client picked its target a round trip ago, against its own
    /// copy of the world.
    /// </summary>
    private const float RangeSlack = 10f;

    public static void Register()
    {
        ActionRegistry.Register(
            ActionIds.VehicleOptions.DeleteVehicle,
            VehicleOptionsPermissions.DeleteVehicle,
            DeleteVehicle);
        ActionRegistry.Register(
            ActionIds.VehicleOptions.SpawnVehicle,
            VehicleOptionsPermissions.SpawnVehicle,
            SpawnVehicle);
        
    }

    public class Vector3Veh()
    {
        public float X;
        public float Y;
        public float Z;
    }
    public class VehicleData
    {
        public uint Hash;
        public Vector3Veh Position = new();
        public int Heading;
        public string ModelType = string.Empty;
    }

    private static ActionResponse SpawnVehicle(Player source, string[] args)
    {
        API.Log.Info(ServerJson.Serialize(args));
        if (ServerJson.TryDeserialize(args[0], out VehicleData? vehicle, out var error))
        {
            if (vehicle != null)
            {
                API.Log.Debug($"spawning vehicle {vehicle.Hash}");
                var spawnedVehicle = Native.CreateVehicleServerSetter(vehicle.Hash, vehicle.ModelType, vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z, vehicle.Heading);
                
                return new ActionResponse(ActionStatus.Ok, [Native.NetworkGetNetworkIdFromEntity(spawnedVehicle).ToString()]);
            }
        }

        return ActionResponse.Failed();
    }
    /// <summary>
    /// Deletes a vehicle the client picked out, once it is really a vehicle and the player is either
    /// driving it or standing near enough to it. Without those two checks this is a delete-anything
    /// primitive.
    /// </summary>
    private static ActionResponse DeleteVehicle(Player source, string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var networkId))
        {
            return ActionResponse.InvalidRequest();
        }

        var entity = Native.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return ActionResponse.NotFound();
        }

        if (Native.GetEntityType(entity) != VehicleEntityType)
        {
            return ActionResponse.InvalidRequest();
        }

        var ped = source.PedIndex;

        if (ped <= 0 || !Native.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        // Checked before distance, which would otherwise measure the cockpit of a cargo plane against
        // the plane's own origin and refuse the player flying it.
        if (Native.GetVehiclePedIsIn(ped, false) == entity)
        {
            return Native.GetPedInVehicleSeat(entity, DriverSeat) == ped
                ? Delete(entity)
                : ActionResponse.Refused();
        }

        var reach = ServerConfig.Value(VehicleOptionsSettings.DeleteVehicleDistance) + RangeSlack;

        if (Vector3.DistanceSquared(Native.GetEntityCoords(ped), Native.GetEntityCoords(entity)) > reach * reach)
        {
            API.Log.Warn($"[Actions] {source} asked to delete a vehicle further than {reach}m away. Refused.");

            return ActionResponse.TooFar();
        }

        return Delete(entity);
    }

    /// <remarks>
    /// The outcome is deliberately not verified. The removal only lands on the next server tick, so
    /// <c>DoesEntityExist</c> still reports the vehicle right here and every successful delete would
    /// be answered as a failure. A handler runs to completion on the event's thread and replies from
    /// it, so there is no point at which the server could look again before answering.
    /// </remarks>
    private static ActionResponse Delete(int entity)
    {
        Native.DeleteEntity(entity);

        return ActionResponse.Ok();
    }
}
