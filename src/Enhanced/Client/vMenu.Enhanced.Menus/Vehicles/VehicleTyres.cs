using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleTyres
{
    private const int MaxWheels = 10;

    private const float FullBurst = 1f;

    public static IReadOnlyList<int> Present(int vehicle)
    {
        var wheels = new List<int>();
        var count = Math.Min(Native.GetVehicleNumberOfWheels(vehicle), MaxWheels);

        for (var wheel = 0; wheel < count; wheel++)
        {
            if (Native.DoesVehicleTyreExist(vehicle, wheel))
            {
                wheels.Add(wheel);
            }
        }

        return wheels;
    }

    public static void Toggle(int vehicle, int wheel)
    {
        if (!Native.DoesEntityExist(vehicle))
        {
            return;
        }

        if (Native.IsVehicleTyreBurst(vehicle, wheel, false))
        {
            Native.SetVehicleTyreFixed(vehicle, wheel);

            Notifications.Success(MenuText.Key(Loc.VehicleOptions.TyreFixed));

            return;
        }

        Native.SetVehicleTyreBurst(vehicle, wheel, false, FullBurst);

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.TyreBurst));
    }

    public static void ToggleAll(int vehicle)
    {
        if (!Native.DoesEntityExist(vehicle))
        {
            return;
        }

        var wheels = Present(vehicle);

        if (wheels.Count == 0)
        {
            return;
        }

        var anyBurst = false;

        foreach (var wheel in wheels)
        {
            if (Native.IsVehicleTyreBurst(vehicle, wheel, false))
            {
                anyBurst = true;

                break;
            }
        }

        foreach (var wheel in wheels)
        {
            if (anyBurst)
            {
                Native.SetVehicleTyreFixed(vehicle, wheel);

                continue;
            }

            Native.SetVehicleTyreBurst(vehicle, wheel, false, FullBurst);
        }

        Notifications.Success(MenuText.Key(anyBurst ? Loc.VehicleOptions.TyresFixed : Loc.VehicleOptions.TyresBurst));
    }
}
