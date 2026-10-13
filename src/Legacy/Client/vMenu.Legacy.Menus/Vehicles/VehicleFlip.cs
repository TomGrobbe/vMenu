using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleFlip
{
    private const float GroundSearch = 5f;

    public static void FlipCurrent()
    {
        var vehicle = OwnVehicle.RequireDriven(
            Loc.VehicleOptions.FlipNoVehicle,
            Loc.VehicleOptions.FlipNotDriver);

        if (vehicle is null)
        {
            return;
        }

        NativeFixer.SetVehicleOnGroundProperly(vehicle.Handle, GroundSearch);

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.Flipped));
    }
}
