using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

public static class VehicleEngine
{
    public const float FullHealth = 1000f;

    private const float DestroyedHealth = -4000f;

    private static bool _watching;

    private static int _written;

    public static bool AlwaysOn => UserDefaults.VehicleEngineAlwaysOn.Value && IsAlwaysOnAllowed;

    private static bool IsAlwaysOnAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.EngineAlwaysOn);

    private static bool IsToggleAllowed => ClientPermissions.IsAllowed(VehicleOptionsPermissions.ToggleEngine);

    public static void Initialize()
    {
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    public static void SetAlwaysOn(bool enabled)
    {
        if (enabled && !IsAlwaysOnAllowed)
        {
            return;
        }

        UserDefaults.VehicleEngineAlwaysOn.Value = enabled;

        Apply();
    }

    public static void Toggle()
    {
        if (!IsToggleAllowed)
        {
            return;
        }

        var vehicle = OwnVehicle.RequireDriven(
            Loc.VehicleOptions.EngineNoVehicle,
            Loc.VehicleOptions.EngineNotDriver);

        if (vehicle is null)
        {
            return;
        }

        var running = Native.IsVehicleEngineOn(vehicle.Handle);

        Native.SetVehicleEngineOn(
            VehicleIndex: vehicle.Handle,
            EngineOnFlag: !running,
            bNoDelay: false,
            bOnlyStartWithPlayerInput: true);

        Notifications.Info(MenuText.Key(running ? Loc.VehicleOptions.EngineStopped : Loc.VehicleOptions.EngineStarted));
    }

    public static float Health()
    {
        var vehicle = OwnVehicle.Driven();

        return vehicle == 0 ? FullHealth : Math.Clamp(Native.GetVehicleEngineHealth(vehicle), 0f, FullHealth);
    }

    public static void SetHealth(float health)
    {
        var vehicle = OwnVehicle.Driven();

        if (vehicle == 0)
        {
            return;
        }

        Native.SetVehicleEngineHealth(vehicle, Math.Clamp(health, 0f, FullHealth));
    }

    public static void Destroy()
    {
        var vehicle = OwnVehicle.RequireDriven(
            Loc.VehicleOptions.EngineNoVehicle,
            Loc.VehicleOptions.EngineNotDriver);

        if (vehicle is null)
        {
            return;
        }

        Native.SetVehicleEngineHealth(vehicle.Handle, DestroyedHealth);

        Notifications.Success(MenuText.Key(Loc.VehicleOptions.EngineDestroyed));
    }

    private static void Apply()
    {
        var on = AlwaysOn;

        Watch(on);

        if (_written != 0 && Native.DoesEntityExist(_written))
        {
            Native.SetVehicleKeepEngineOnWhenAbandoned(_written, false);
        }

        _written = 0;

        if (!on)
        {
            return;
        }

        var vehicle = OwnVehicle.Driven();

        if (vehicle == 0)
        {
            return;
        }

        Native.SetVehicleKeepEngineOnWhenAbandoned(vehicle, true);

        _written = vehicle;
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
