using CitizenFX.FiveM.Client;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// Undoes the damage on the vehicle the player is driving.
/// </summary>
public static class VehicleRepair
{
    /// <summary>
    /// Long enough for the game to finish applying damage it had already decided on, short enough
    /// that nobody sees the vehicle flicker.
    /// </summary>
    private const int SecondPassDelayMs = 100;

    /// <summary>
    /// Stays client side, unlike deleting. This only ever touches the vehicle the player is already
    /// sitting in, so there is no reach to police and nothing a server check could add.
    /// </summary>
    public static async Task RepairCurrentAsync()
    {
        var vehicle = OwnVehicle.RequireDriven(Loc.VehicleOptions.RepairNoVehicle, Loc.VehicleOptions.RepairNotDriver);

        if (vehicle is null)
        {
            return;
        }

        await ApplyAsync(vehicle.Handle);

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.Repaired));
    }

    /// <summary>
    /// Repairs a vehicle by handle, silently, for callers that have already found one of their own.
    /// </summary>
    // Twice, because the game applies some damage a moment after it is dealt: a single pass leaves
    // windows that were already on their way to breaking bursting again just after the repair lands.
    internal static async Task ApplyAsync(int vehicle)
    {
        Fix(vehicle);

        await API.Delay(SecondPassDelayMs);

        Fix(vehicle);
    }

    private static void Fix(int vehicle)
    {
        // Re-checked on the second pass, by which point the vehicle may have been deleted underneath it.
        if (!Native.DoesEntityExist(vehicle) || !Native.IsEntityAVehicle(vehicle))
        {
            return;
        }

        Native.SetVehicleFixed(vehicle);

        // Health alone leaves the dents in place, and a wreck stays flagged as undriveable.
        Native.SetVehicleDeformationFixed(vehicle);
        Native.SetVehicleUndriveable(vehicle, false);
    }
}
