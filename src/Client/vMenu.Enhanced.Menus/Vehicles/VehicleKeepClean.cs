using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// Keeps the dust off the vehicle the player is driving.
/// </summary>
/// <remarks>
/// Dirt only. Mud, snow and the scrapes a crash leaves are decals, which this does not touch, the
/// same distinction <see cref="VehicleWash"/> makes.
/// </remarks>
public static class VehicleKeepClean
{
    private static bool _watching;

    public static bool Enabled => UserDefaults.VehicleKeepClean.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.KeepClean);

    /// <summary>Call once at startup, before permissions have arrived.</summary>
    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetEnabled(bool enabled)
    {
        // The checkbox follows the permission, but a revoke can land between the two.
        if (enabled && !IsAllowed)
        {
            return;
        }

        UserDefaults.VehicleKeepClean.Value = enabled;

        Apply();
    }

    private static void Apply()
    {
        var on = Enabled;

        Watch(on);

        if (!on)
        {
            return;
        }

        Wash(OwnVehicle.Driven());
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
            LocalVehicleTicks.VehicleDirtied += OnDirtied;
            LocalVehicleTicks.VehicleChanged += OnChanged;

            return;
        }

        LocalVehicleTicks.VehicleDirtied -= OnDirtied;
        LocalVehicleTicks.VehicleChanged -= OnChanged;
    }

    private static void OnDirtied(VehicleDirtied _) => Wash(OwnVehicle.Driven());

    // The dirt watcher re-seeds on a new vehicle rather than reporting it, so getting into one that is
    // already filthy arrives here instead.
    private static void OnChanged(VehicleChanged _) => Wash(OwnVehicle.Driven());

    private static void Wash(int vehicle)
    {
        if (vehicle == 0 || !Native.DoesEntityExist(vehicle) || Native.GetVehicleDirtLevel(vehicle) <= 0f)
        {
            return;
        }

        Native.SetVehicleDirtLevel(vehicle, 0f);
    }
}
