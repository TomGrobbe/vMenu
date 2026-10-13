using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class OwnVehicle
{
    // The vehicle the player is driving, or null after telling them why there is not one. The refusal is
    // worded per option, so each one names what it was about to do.
    public static Vehicle? RequireDriven(string noVehicleKey, string notDriverKey)
    {
        var ped = Game.PlayerPed;

        if (ped is null || API.IsPedDeadOrDying(ped.Handle, true))
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

        return ped.CurrentVehicle;
    }

    // The handle of the vehicle the player is driving, or 0. Silent, unlike RequireDriven: a menu that
    // opens on a row explaining the problem beats one that fires a notification the moment it appears,
    // and a feature applying itself in the background has nobody to tell at all.
    public static int Driven()
    {
        var ped = Game.PlayerPed;

        if (ped is null || API.IsPedDeadOrDying(ped.Handle, true))
        {
            return 0;
        }

        var target = VehicleTargeting.Current(ped);

        // Passengers do not get to touch somebody else's car, and would not have control of it anyway.
        if (!target.Found || target.Kind is VehicleTargetKind.Passenger)
        {
            return 0;
        }

        return target.Handle;
    }
}
