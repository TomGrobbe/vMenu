using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

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

        return vehicle != 0 && API.IsThisModelABoat(NativeFixer.GetEntityModel(vehicle))
            && API.CanAnchorBoatHere(vehicle);
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

        if (!API.IsThisModelABoat(NativeFixer.GetEntityModel(vehicle)) || !API.CanAnchorBoatHere(vehicle))
        {
            return;
        }

        Write(vehicle, true);

        _anchored = vehicle;
    }

    private static void Write(int vehicle, bool anchored)
    {
        if (!API.DoesEntityExist(vehicle))
        {
            return;
        }

        API.SetBoatAnchor(vehicle, anchored);
        API.SetBoatFrozenWhenAnchored(vehicle, anchored);
        API.SetForcedBoatLocationWhenAnchored(vehicle, anchored);
    }
}
