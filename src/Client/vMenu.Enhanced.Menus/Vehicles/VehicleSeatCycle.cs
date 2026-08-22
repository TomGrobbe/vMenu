using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleSeatCycle
{
    private const int DriverSeat = -1;

    public static bool CanCycle => Seated(out _, out _);

    public static void CycleToNextFreeSeat()
    {
        if (!Seated(out var ped, out var vehicle))
        {
            Notifications.Error(MenuText.Key(Loc.VehicleOptions.CycleSeatNoVehicle));

            return;
        }

        var order = SeatOrder(vehicle);
        var from = CurrentIndex(vehicle, ped.Handle, order);

        for (var step = 1; step < order.Length; step++)
        {
            var seat = order[(from + step) % order.Length];

            if (!Native.IsVehicleSeatFree(vehicle, seat, false))
            {
                continue;
            }

            Native.SetPedIntoVehicle(ped.Handle, vehicle, seat);

            return;
        }

        Notifications.Error(MenuText.Key(Loc.VehicleOptions.CycleSeatNoFreeSeat));
    }

    private static bool Seated(out Ped ped, out int vehicle)
    {
        ped = null!;
        vehicle = 0;

        var local = API.Players.Local.Ped;

        if (local is null || local.IsDeadOrDying)
        {
            return false;
        }

        var target = VehicleTargeting.Current(local);

        if (!target.Found)
        {
            return false;
        }

        ped = local;
        vehicle = target.Handle;

        return true;
    }

    private static int[] SeatOrder(int vehicle)
    {
        var seats = Native.GetVehicleModelNumberOfSeats(Native.GetEntityModel(vehicle));
        var order = new int[Math.Max(1, seats)];

        order[0] = DriverSeat;

        for (var seat = 0; seat <= seats - 2; seat++)
        {
            order[seat + 1] = seat;
        }

        return order;
    }

    private static int CurrentIndex(int vehicle, int ped, int[] order)
    {
        for (var index = 0; index < order.Length; index++)
        {
            if (Native.GetPedInVehicleSeat(vehicle, order[index], false) == ped)
            {
                return index;
            }
        }

        return 0;
    }
}
