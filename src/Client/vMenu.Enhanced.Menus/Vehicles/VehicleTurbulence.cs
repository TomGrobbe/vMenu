using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleTurbulence
{
    public const int Stock = 100;

    public const int Step = 5;

    private static bool _watching;

    public static int Helicopter => Resolve(UserDefaults.VehicleHeliTurbulence.Value);

    public static int Plane => Resolve(UserDefaults.VehiclePlaneTurbulence.Value);

    private static bool IsAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.Turbulence);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetHelicopter(int percent) => Set(UserDefaults.VehicleHeliTurbulence, percent);

    public static void SetPlane(int percent) => Set(UserDefaults.VehiclePlaneTurbulence, percent);

    public static void Write(int vehicle, uint model)
    {
        if (vehicle == 0 || !Native.DoesEntityExist(vehicle))
        {
            return;
        }

        if (Native.IsThisModelAHeli(model))
        {
            Native.SetHeliTurbulenceScalar(vehicle, Helicopter / 100f);

            return;
        }

        if (Native.IsThisModelAPlane(model))
        {
            Native.SetPlaneTurbulenceMultiplier(vehicle, Plane / 100f);
        }
    }

    private static void Set(IntDefault preference, int percent)
    {
        preference.Value = Math.Clamp(percent, 0, Stock);

        Apply();
    }

    private static void Apply()
    {
        Watch(true);

        var vehicle = OwnVehicle.Driven();

        if (vehicle == 0)
        {
            return;
        }

        Write(vehicle, (uint)Native.GetEntityModel(vehicle));
    }

    private static int Resolve(int stored) => IsAllowed ? Math.Clamp(stored, 0, Stock) : Stock;

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
