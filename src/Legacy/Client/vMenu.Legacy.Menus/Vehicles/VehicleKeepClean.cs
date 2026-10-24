using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Menus.Common;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus.Vehicles;

// Dirt only. Mud, snow and the scrapes a crash leaves are decals, which this does not touch, the
// same distinction VehicleWash makes.
public static class VehicleKeepClean
{
    // The dirt watcher only reports a gain big enough to be worth an event, so a slow drizzle of dirt can
    // sit under it. This sweep is what catches that.
    private const long SweepIntervalMs = 10000;

    private static TickHandle? _sweep;

    // The dirt watcher re-seeds on a new vehicle rather than reporting it, so getting into one that is
    // already filthy arrives as a vehicle change instead.
    public static readonly ToggleFeature Toggle = new(
        UserDefaults.VehicleKeepClean,
        VehicleOptionsPermissions.KeepClean,
        Apply,
        ToggleWatch.VehicleChanged | ToggleWatch.VehicleDirtied);

    // Call once at startup, before permissions have arrived.
    public static void Initialize()
    {
        _sweep = TickRegistry.Register("Vehicle.KeepClean", Wash, TickRate.Every(SweepIntervalMs), () => Toggle.Enabled);

        Toggle.Initialize();
    }

    private static void Apply(bool on)
    {
        _sweep?.Reevaluate();

        if (on)
        {
            Wash();
        }
    }

    private static void Wash()
    {
        var vehicle = OwnVehicle.Driven();

        if (vehicle == 0 || !API.DoesEntityExist(vehicle) || API.GetVehicleDirtLevel(vehicle) <= 0f)
        {
            return;
        }

        API.SetVehicleDirtLevel(vehicle, 0f);
    }
}
