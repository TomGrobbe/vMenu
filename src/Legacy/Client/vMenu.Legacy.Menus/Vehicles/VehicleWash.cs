using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleWash
{
    public static void WashCurrent()
    {
        var vehicle = OwnVehicle.RequireDriven(Loc.VehicleOptions.WashNoVehicle, Loc.VehicleOptions.WashNotDriver);

        if (vehicle is null)
        {
            return;
        }

        vehicle.DirtLevel = 0f;

        // Dirt and decals are separate: mud sprayed on by driving is dirt, a bullet hole or a scrape mark is
        // a decal, and clearing one leaves the other on the paint.
        API.RemoveDecalsFromVehicle(vehicle.Handle);

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.Washed));
    }
}
