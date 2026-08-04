using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Configuration;
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
    private const string DeleteCommand = "dv";

    // Cached, because the func ref registry keys on the delegate, so a new lambda per cycle leaks.
    private static readonly Action<int, MessagePackBuffer, string> DeleteHandler = (_, _, _) => RunDelete();

    /// <summary>Null while the command is not registered.</summary>
    private static int? _deleteCommandId;

    /// <summary>Call after <see cref="ClientConfig.Initialize"/>.</summary>
    public static void Initialize()
    {
        ClientConfig.Changed += Apply;
        ClientPermissions.PermissionsChanged += Apply;

        Apply();
    }

    private static void Apply()
    {
        var shouldRegister = ClientConfig.Value(VehicleOptionsSettings.DeleteVehicleCommand)
            && ClientPermissions.IsAllowed(VehicleOptionsPermissions.DeleteVehicle);

        var registered = _deleteCommandId;

        if (shouldRegister && registered is null)
        {
            _deleteCommandId = NativeFixer.RegisterCommand(DeleteCommand, restricted: false, DeleteHandler);

            API.Log.Debug($"[VehicleOptions] Registered /{DeleteCommand}.");
        }
        else if (!shouldRegister && registered is not null)
        {
            Native.UnregisterCommand(registered.Value);
            _deleteCommandId = null;

            API.Log.Debug($"[VehicleOptions] Unregistered /{DeleteCommand}.");
        }
    }

    /// <summary>A command handler cannot await, so this is the fire and forget boundary.</summary>
    private static async void RunDelete()
    {
        try
        {
            // Registration follows the permission, but a revoke can land between the two.
            if (!ClientPermissions.IsAllowed(VehicleOptionsPermissions.DeleteVehicle))
            {
                Notifications.Error(MenuText.Key(Loc.VehicleOptions.DeleteDenied));

                return;
            }

            await VehicleDeletion.DeleteTargetAsync();
        }
        catch (Exception exception)
        {
            API.Log.Error($"[VehicleOptions] /{DeleteCommand} threw: {exception}");
        }
    }
}
