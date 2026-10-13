using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehiclePerformance
{
    public static readonly int[] Steps = [2, 4, 8, 16, 32, 64, 128, 256, 512, 1024];

    private const float Neutral = 1f;

    private static TickHandle? _torqueTick;

    private static bool _watching;

    private static int _poweredVehicle;

    public static bool PowerEnabled => UserDefaults.VehiclePowerMultiplierEnabled.Value && IsPowerAllowed;

    public static bool TorqueEnabled => UserDefaults.VehicleTorqueMultiplierEnabled.Value && IsTorqueAllowed;

    public static int PowerMultiplier => UserDefaults.VehiclePowerMultiplier.Value;

    public static int TorqueMultiplier => UserDefaults.VehicleTorqueMultiplier.Value;

    private static bool IsPowerAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.PowerMultiplier);

    private static bool IsTorqueAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.TorqueMultiplier);

    public static void Initialize()
    {
        _torqueTick = TickRegistry.Register(
            "Vehicles.TorqueMultiplier",
            HoldTorque,
            TickRate.PerFrame,
            () => TorqueEnabled,
            autoStart: false);

        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetPowerEnabled(bool enabled)
    {
        if (enabled && !IsPowerAllowed)
        {
            return;
        }

        UserDefaults.VehiclePowerMultiplierEnabled.Value = enabled;

        Apply();
    }

    public static void SetTorqueEnabled(bool enabled)
    {
        if (enabled && !IsTorqueAllowed)
        {
            return;
        }

        UserDefaults.VehicleTorqueMultiplierEnabled.Value = enabled;

        Apply();
    }

    public static void SetPowerMultiplier(int multiplier)
    {
        UserDefaults.VehiclePowerMultiplier.Value = multiplier;

        Apply();
    }

    public static void SetTorqueMultiplier(int multiplier)
    {
        UserDefaults.VehicleTorqueMultiplier.Value = multiplier;

        Apply();
    }

    private static void Apply()
    {
        Watch(PowerEnabled || TorqueEnabled);

        _torqueTick?.Reevaluate();

        WritePower();
    }

    private static void WritePower()
    {
        var vehicle = OwnVehicle.Driven();

        if (_poweredVehicle != 0 && _poweredVehicle != vehicle && Native.DoesEntityExist(_poweredVehicle))
        {
            Native.SetVehicleEnginePowerMultiplier(_poweredVehicle, Neutral);
        }

        _poweredVehicle = 0;

        if (vehicle == 0)
        {
            return;
        }

        if (!PowerEnabled)
        {
            Native.SetVehicleEnginePowerMultiplier(vehicle, Neutral);

            return;
        }

        Native.SetVehicleEnginePowerMultiplier(vehicle, PowerMultiplier);

        _poweredVehicle = vehicle;
    }

    private static void HoldTorque()
    {
        var vehicle = OwnVehicle.Driven();

        if (vehicle == 0)
        {
            return;
        }

        Native.SetVehicleEngineTorqueMultiplier(vehicle, TorqueMultiplier);
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
