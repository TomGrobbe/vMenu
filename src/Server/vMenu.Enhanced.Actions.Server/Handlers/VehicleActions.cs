using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Logging;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;
using VehicleOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class VehicleActions
{
    // There is no IsEntityAVehicle server side, so the entity type is the check.
    private const int VehicleEntityType = 2;

    private const int DriverSeat = -1;

    // Added to the configured reach. The client picked its target a round trip ago, against its own copy
    // of the world.
    private const float RangeSlack = 10f;

    public static void Register() =>
        ActionRegistry.Register(
            ActionIds.VehicleOptions.DeleteVehicle,
            VehicleOptionsPermissions.DeleteVehicle,
            DeleteVehicle);

    // Only once it is really a vehicle and the player is either driving it or standing near enough to
    // it. Without those two checks this is a delete-anything primitive.
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

        // Checked before distance, which would otherwise measure the cockpit of a cargo plane against the
        // plane's own origin and refuse the player flying it.
        if (Native.GetVehiclePedIsIn(ped, false) == entity)
        {
            return Native.GetPedInVehicleSeat(entity, DriverSeat) == ped
                ? Delete(entity)
                : ActionResponse.Refused();
        }

        var reach = ServerConfig.Value(VehicleOptionsSettings.DeleteVehicleDistance) + RangeSlack;

        if (Vector3.DistanceSquared(Native.GetEntityCoords(ped), Native.GetEntityCoords(entity)) > reach * reach)
        {
            Log.Warning($"[Actions] {source} asked to delete a vehicle further than {reach}m away. Refused.");

            return ActionResponse.TooFar();
        }

        return Delete(entity);
    }

    // The outcome is deliberately not verified. The removal only lands on the next server tick, so
    // DoesEntityExist still reports the vehicle right here and every successful delete would be answered
    // as a failure. A handler runs to completion on the event's thread and replies from it.
    private static ActionResponse Delete(int entity)
    {
        Native.DeleteEntity(entity);

        return ActionResponse.Ok();
    }
}
