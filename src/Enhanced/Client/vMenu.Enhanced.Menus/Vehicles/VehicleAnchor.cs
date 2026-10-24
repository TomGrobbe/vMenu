using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleAnchor
{
    private static int _anchored;

    public static readonly ToggleFeature Toggle = new(
        UserDefaults.VehicleAnchorBoat,
        VehicleOptionsPermissions.AnchorBoat,
        Apply,
        ToggleWatch.VehicleChanged);

    public static bool CanAnchorHere()
    {
        var vehicle = OwnVehicle.Driven();

        return vehicle != 0 && Native.IsThisModelABoat(Native.GetEntityModel(vehicle))
            && Native.CanAnchorBoatHere(vehicle);
    }

    private static void Apply(bool on)
    {
        var vehicle = OwnVehicle.Driven();

        if (_anchored != 0 && (_anchored != vehicle || !on))
        {
            Write(_anchored, false);

            _anchored = 0;
        }

        if (!on || vehicle == 0)
        {
            return;
        }

        if (!Native.IsThisModelABoat(Native.GetEntityModel(vehicle)) || !Native.CanAnchorBoatHere(vehicle))
        {
            return;
        }

        Write(vehicle, true);

        _anchored = vehicle;
    }

    private static void Write(int vehicle, bool anchored)
    {
        if (!Native.DoesEntityExist(vehicle))
        {
            return;
        }

        Native.SetBoatAnchor(vehicle, anchored);
        Native.SetBoatFrozenWhenAnchored(vehicle, anchored);
        Native.SetForcedBoatLocationWhenAnchored(vehicle, anchored);
    }
}
