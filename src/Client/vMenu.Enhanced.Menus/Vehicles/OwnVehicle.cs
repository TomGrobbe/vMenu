using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Client.Entities;
using CitizenFX.FiveM.Client.Extensions;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// The shared way in for options that act on the vehicle the player is driving themselves.
/// </summary>
public static class OwnVehicle
{
    /// <summary>
    /// The vehicle the player is driving, or null after telling them why there is not one. The
    /// refusal is worded per option, so each one names what it was about to do.
    /// </summary>
    public static Vehicle? RequireDriven(string noVehicleKey, string notDriverKey)
    {
        var ped = API.Players.Local.Ped;

        if (ped is null || ped.IsDeadOrDying)
        {
            return null;
        }

        var target = VehicleTargeting.Current(ped);

        if (!target.Found)
        {
            Notifications.Error(MenuText.Key(noVehicleKey));

            return null;
        }

        // Passengers do not get to touch somebody else's car, and would not have control of it anyway.
        if (target.Kind is VehicleTargetKind.Passenger)
        {
            Notifications.Error(MenuText.Key(notDriverKey));

            return null;
        }

        return ped.Vehicle;
    }
}
