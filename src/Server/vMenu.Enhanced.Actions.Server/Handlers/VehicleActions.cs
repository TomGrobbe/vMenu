using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;

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

    public static void Register() =>
        ActionRegistry.Register(
            ActionIds.VehicleOptions.DeleteVehicle,
            VehicleOptionsPermissions.DeleteVehicle,
            DeleteVehicle);

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

    private static ActionResponse Delete(int entity)
    {
        Native.DeleteEntity(entity);

        // The native reports on the call, not on the outcome.
        return Native.DoesEntityExist(entity) ? ActionResponse.Failed() : ActionResponse.Ok();
    }
}
