using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

// Dirt only. Mud, snow and the scrapes a crash leaves are decals, which this does not touch, the
// same distinction VehicleWash makes.
public static class VehicleKeepClean
{
    // The dirt watcher only reports a gain big enough to be worth an event, so a slow drizzle of dirt can
    // sit under it. This sweep is what catches that.
    private const long SweepIntervalMs = 10000;

    private static bool _watching;

    private static TickHandle? _sweep;

    public static bool Enabled => UserDefaults.VehicleKeepClean.Value && IsAllowed;

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.KeepClean);

    // Call once at startup, before permissions have arrived.
    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        _sweep = TickRegistry.Register("Vehicle.KeepClean", Sweep, TickRate.Every(SweepIntervalMs), () => Enabled);

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

        _sweep?.Reevaluate();

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

    private static void Sweep() => Wash(OwnVehicle.Driven());

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
