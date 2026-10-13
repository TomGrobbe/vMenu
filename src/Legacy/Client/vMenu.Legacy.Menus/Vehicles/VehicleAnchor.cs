using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleAnchor
{
    private static bool _watching;

    private static int _anchored;

    public static bool Enabled => UserDefaults.VehicleAnchorBoat.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.AnchorBoat);

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

        UserDefaults.VehicleAnchorBoat.Value = enabled;

        Apply();
    }

    public static bool CanAnchorHere()
    {
        var vehicle = OwnVehicle.Driven();

        return vehicle != 0 && API.IsThisModelABoat(NativeFixer.GetEntityModel(vehicle))
            && API.CanAnchorBoatHere(vehicle);
    }

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

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
