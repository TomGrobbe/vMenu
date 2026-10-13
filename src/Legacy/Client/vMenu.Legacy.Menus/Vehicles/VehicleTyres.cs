using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleTyres
{
    private const int MaxWheels = 10;

    private const float FullBurst = 1f;

    public static IReadOnlyList<int> Present(int vehicle)
    {
        var wheels = new List<int>();
        var count = Math.Min(API.GetVehicleNumberOfWheels(vehicle), MaxWheels);

        for (var wheel = 0; wheel < count; wheel++)
        {
            if (API.DoesVehicleTyreExist(vehicle, wheel))
            {
                wheels.Add(wheel);
            }
        }

        return wheels;
    }

    public static void Toggle(int vehicle, int wheel)
    {
        if (!API.DoesEntityExist(vehicle))
        {
            return;
        }

        if (API.IsVehicleTyreBurst(vehicle, wheel, false))
        {
            API.SetVehicleTyreFixed(vehicle, wheel);

            Notifications.Success(MenuText.Key(Loc.VehicleOptions.TyreFixed));

            return;
        }

        API.SetVehicleTyreBurst(vehicle, wheel, false, FullBurst);

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.TyreBurst));
    }

    public static void ToggleAll(int vehicle)
    {
        if (!API.DoesEntityExist(vehicle))
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
            if (API.IsVehicleTyreBurst(vehicle, wheel, false))
            {
                anyBurst = true;

                break;
            }
        }

        foreach (var wheel in wheels)
        {
            if (anyBurst)
            {
                API.SetVehicleTyreFixed(vehicle, wheel);

                continue;
            }

            API.SetVehicleTyreBurst(vehicle, wheel, false, FullBurst);
        }

        Notifications.Success(MenuText.Key(anyBurst ? Loc.VehicleOptions.TyresFixed : Loc.VehicleOptions.TyresBurst));
    }
}
