using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// Undoes the damage on the vehicle the player is driving.
/// </summary>
public static class VehicleRepair
{
    /// <summary>
    /// Stays client side, unlike deleting. This only ever touches the vehicle the player is already
    /// sitting in, so there is no reach to police and nothing a server check could add.
    /// </summary>
    public static void RepairCurrent()
    {
        var vehicle = OwnVehicle.RequireDriven(Loc.VehicleOptions.RepairNoVehicle, Loc.VehicleOptions.RepairNotDriver);

        if (vehicle is null)
        {
            return;
        }

        vehicle.SetVehicleFixed();

        // Health alone leaves the dents in place, and a wreck stays flagged as undriveable.
        vehicle.SetVehicleDeformationFixed();
        vehicle.IsDriveable = true;

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.Repaired));
    }
}
