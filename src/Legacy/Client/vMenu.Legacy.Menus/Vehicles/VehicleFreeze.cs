using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Common;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleFreeze
{
    private static int _frozen;

    public static readonly ToggleFeature Toggle = new(
        VehicleOptionsPermissions.Freeze,
        Apply,
        ToggleWatch.VehicleChanged);

    private static void Apply(bool on)
    {
        var vehicle = OwnVehicle.Driven();

        if (_frozen != 0 && (_frozen != vehicle || !on))
        {
            if (API.DoesEntityExist(_frozen))
            {
                API.FreezeEntityPosition(_frozen, false);
            }

            _frozen = 0;
        }

        if (!on || vehicle == 0)
        {
            return;
        }

        API.FreezeEntityPosition(vehicle, true);

        _frozen = vehicle;
    }
}
