using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Data.Actions;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class VehicleActions
{
    private const int DriverSeat = -1;

    public static void Register() =>
        ActionRegistry.Register(
            ActionIds.VehicleOptions.DeleteVehicle,
            VehicleOptionsPermissions.DeleteVehicle,
            DeleteVehicle);

    private static ActionResponse DeleteVehicle(Player source, string[] args)
    {
        if (!ActionArgs.TryInt(args, 0, out var networkId))
        {
            return ActionResponse.InvalidRequest();
        }

        var entity = Native.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return ActionResponse.NotFound();
        }

        if (!Targets.IsVehicle(entity))
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
