using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// Cleans the vehicle the player is driving.
/// </summary>
public static class VehicleWash
{
    /// <inheritdoc cref="VehicleRepair.RepairCurrentAsync"/>
    public static void WashCurrent()
    {
        var vehicle = OwnVehicle.RequireDriven(Loc.VehicleOptions.WashNoVehicle, Loc.VehicleOptions.WashNotDriver);

        if (vehicle is null)
        {
            return;
        }

        vehicle.DirtLevel = 0f;

        // Dirt and decals are separate: mud sprayed on by driving is dirt, a bullet hole or a
        // scrape mark is a decal, and clearing one leaves the other on the paint.
        vehicle.RemoveDecalsFromVehicle();

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.Washed));
    }
}
