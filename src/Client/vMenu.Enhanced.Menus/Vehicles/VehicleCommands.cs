using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Configuration;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;
using VehicleOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles;

/// <summary>
/// Chat commands for vehicle options, kept in step with the settings and permissions behind them.
/// </summary>
// Registered unrestricted, because FiveM's restricted flag answers to an ACE on the client's own
// principal. vMenu checks its own permission instead.
public static class VehicleCommands
{
    private static readonly ToggledCommand[] Commands =
    [
        new("dv",
            VehicleOptionsSettings.DeleteVehicleCommand,
            VehicleOptionsPermissions.DeleteVehicle,
            Loc.VehicleOptions.DeleteDenied,
            VehicleDeletion.DeleteTargetAsync),

        new("fixveh",
            VehicleOptionsSettings.RepairVehicleCommand,
            VehicleOptionsPermissions.RepairVehicle,
            Loc.VehicleOptions.RepairDenied,
            VehicleRepair.RepairCurrentAsync),

        new("washveh",
            VehicleOptionsSettings.WashVehicleCommand,
            VehicleOptionsPermissions.WashVehicle,
            Loc.VehicleOptions.WashDenied,
            () =>
            {
                VehicleWash.WashCurrent();

                return Task.CompletedTask;
            }),
    ];

    /// <summary>Call after <see cref="ClientConfig.Initialize"/>.</summary>
    public static void Initialize()
    {
        ClientConfig.AddEventListenerFor(
            [
                VehicleOptionsSettings.DeleteVehicleCommand,
                VehicleOptionsSettings.RepairVehicleCommand,
                VehicleOptionsSettings.WashVehicleCommand,
            ],
            Apply);

        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    private static void Apply()
    {
        foreach (var command in Commands)
        {
            command.Apply();
        }
    }

    /// <summary>A command that exists only while its setting is on and its permission is granted.</summary>
    private sealed class ToggledCommand
    {
        private readonly string _name;
        private readonly BoolSetting _setting;
        private readonly string _permission;
        private readonly string _deniedKey;
        private readonly Func<Task> _run;

        // Cached, because the func ref registry keys on the delegate, so a new lambda per cycle leaks.
        private readonly Action<int, MessagePackBuffer, string> _handler;

        /// <summary>Null while the command is not registered.</summary>
        private int? _id;

        public ToggledCommand(string name, BoolSetting setting, string permission, string deniedKey, Func<Task> run)
        {
            _name = name;
            _setting = setting;
            _permission = permission;
            _deniedKey = deniedKey;
            _run = run;
            _handler = (_, _, _) => Run();
        }

        public void Apply()
        {
            var shouldRegister = ClientConfig.Value(_setting) && ClientPermissions.IsAllowed(_permission);

            if (shouldRegister && _id is null)
            {
                _id = NativeFixer.RegisterCommand(_name, restricted: false, _handler);

                Log.Debug($"[VehicleOptions] Registered /{_name}.");
            }
            else if (!shouldRegister && _id is not null)
            {
                Native.UnregisterCommand(_id.Value);
                _id = null;

                Log.Debug($"[VehicleOptions] Unregistered /{_name}.");
            }
        }

        /// <summary>A command handler cannot await, so this is the fire and forget boundary.</summary>
        private async void Run()
        {
            try
            {
                // Registration follows the permission, but a revoke can land between the two.
                if (!ClientPermissions.IsAllowed(_permission))
                {
                    Notifications.Error(MenuText.Key(_deniedKey));

                    return;
                }

                await _run();
            }
            catch (Exception exception)
            {
                Log.Error($"[VehicleOptions] /{_name} threw: {exception}");
            }
        }
    }
}
