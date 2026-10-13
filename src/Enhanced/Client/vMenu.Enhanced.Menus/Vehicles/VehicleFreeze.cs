using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleFreeze
{
    private static bool _enabled;

    private static bool _watching;

    private static int _frozen;

    public static bool Enabled => _enabled && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.Freeze);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        if (enabled && !IsAllowed)
        {
            return;
        }

        _enabled = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        var vehicle = OwnVehicle.Driven();

        if (_frozen != 0 && (_frozen != vehicle || !on))
        {
            if (Native.DoesEntityExist(_frozen))
            {
                Native.FreezeEntityPosition(_frozen, false);
            }

            _frozen = 0;
        }

        if (!on || vehicle == 0)
        {
            return;
        }

        Native.FreezeEntityPosition(vehicle, true);

        _frozen = vehicle;
    }

    private static void Watch(bool watching)
    {
        if (watching == _watching)
        {
            return;
        }

        _watching = watching;

        if (watching)
        {
            LocalVehicleTicks.VehicleChanged += OnChanged;

            return;
        }

        LocalVehicleTicks.VehicleChanged -= OnChanged;
    }

    private static void OnChanged(VehicleChanged _) => Apply();
}
