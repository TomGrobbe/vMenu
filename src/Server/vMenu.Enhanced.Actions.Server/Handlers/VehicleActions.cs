using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Data.Actions;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class VehicleActions
{
    // There is no IsEntityAVehicle server side, so the entity type is the check.
    private const int VehicleEntityType = 2;

    private const int DriverSeat = -1;

    public static void Register() =>
        ActionRegistry.Register(
            ActionIds.VehicleOptions.DeleteVehicle,
            VehicleOptionsPermissions.DeleteVehicle,
            DeleteVehicle);

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

        if (Native.GetVehiclePedIsIn(ped, false) != entity
            || Native.GetPedInVehicleSeat(entity, DriverSeat) != ped)
        {
            return ActionResponse.Refused();
        }

        // Never verify this. The removal only lands on the next server tick, so DoesEntityExist still
        // reports the vehicle here and every successful delete would answer as a failure.
        Native.DeleteEntity(entity);

        return ActionResponse.Ok();
    }
}
