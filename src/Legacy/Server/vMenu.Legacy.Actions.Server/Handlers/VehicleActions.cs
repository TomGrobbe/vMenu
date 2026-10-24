using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Actions;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives.Server;

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

        var entity = API.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0 || !API.DoesEntityExist(entity))
        {
            return ActionResponse.NotFound();
        }

        if (!Targets.IsVehicle(entity))
        {
            return ActionResponse.InvalidRequest();
        }

        var ped = API.GetPlayerPed(source.Handle);

        if (ped <= 0 || !API.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        if (API.GetVehiclePedIsIn(ped, false) != entity
            || API.GetPedInVehicleSeat(entity, DriverSeat) != ped)
        {
            return ActionResponse.Refused();
        }

        // Never verify this. The removal only lands on the next server tick, so DoesEntityExist still
        // reports the vehicle here and every successful delete would answer as a failure.
        NativeFixer.DeleteEntity(entity);

        return ActionResponse.Ok();
    }
}
