using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleVisibility
{
    private static bool _initialized;

    private static int _hidden;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.Invisible);

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        LocalVehicleTicks.VehicleChanged += OnChanged;
    }

    public static void Toggle()
    {
        if (!IsAllowed)
        {
            return;
        }

        var vehicle = OwnVehicle.RequireDriven(
            Loc.VehicleOptions.VisibilityNoVehicle,
            Loc.VehicleOptions.VisibilityNotDriver);

        if (vehicle is null)
        {
            return;
        }

        var handle = vehicle.Handle;
        var visible = Native.IsEntityVisible(handle);

        var occupants = new List<(int Ped, bool Visible)>();

        if (visible)
        {
            foreach (var occupant in Occupants(handle))
            {
                occupants.Add((occupant, Native.IsEntityVisible(occupant)));
            }
        }

        Native.SetEntityVisible(handle, !visible, false);

        foreach (var occupant in occupants)
        {
            Native.SetEntityVisible(occupant.Ped, occupant.Visible, false);
        }

        _hidden = visible ? handle : 0;

        Notifications.Info(MenuText.Key(visible ? Loc.VehicleOptions.VisibilityHidden : Loc.VehicleOptions.VisibilityShown));
    }

    private static List<int> Occupants(int vehicle)
    {
        var seats = Native.GetVehicleMaxNumberOfPassengers(vehicle);
        var found = new List<int>();

        // Seat -1 is the driver, and GET_PED_IN_VEHICLE_SEAT counts passenger seats from zero.
        for (var seat = -1; seat < seats; seat++)
        {
            var ped = Native.GetPedInVehicleSeat(vehicle, seat, false);

            if (ped != 0 && Native.DoesEntityExist(ped))
            {
                found.Add(ped);
            }
        }

        return found;
    }

    private static void OnChanged(VehicleChanged changed)
    {
        if (_hidden == 0 || changed.Vehicle == _hidden)
        {
            return;
        }

        if (Native.DoesEntityExist(_hidden))
        {
            Native.SetEntityVisible(_hidden, true, false);
        }

        _hidden = 0;
    }
}
