using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Permissions;

namespace vMenu.Enhanced.Permissions;

/// <summary>
/// Client side of the permission handshake.
/// </summary>
/// <remarks>
/// The client asks rather than waiting to be told, because the server-side join event fires before
/// this script is running. Handlers are registered imperatively rather than with
/// <c>[OnNetEvent]</c> because attribute discovery only scans the assembly named as the
/// <c>client_script</c>, and this one is a referenced assembly.
/// </remarks>
public static class PermissionsSync
{
    private const int MaxRequestAttempts = 10;

    private const int RequestRetryDelay = 1000;

    /// <summary>Call before building menus.</summary>
    public static void RegisterEventHandlers()
    {
        API.OnNetEvent(PermissionEvents.Set, new Action<string[], string[]>(OnPermissionsReceived), false);

        RequestPermissions();
    }

    /// <summary>
    /// Retrying covers this script starting before the server resource has registered its handler,
    /// which happens routinely when the resource is restarted with players connected.
    /// </summary>
    public static async void RequestPermissions()
    {
        for (var attempt = 0; attempt < MaxRequestAttempts; attempt++)
        {
            API.EmitServer(PermissionEvents.Request);

            await API.Delay(RequestRetryDelay);

            if (ClientPermissions.HasReceivedPermissions)
            {
                return;
            }
        }

        API.Log.Error($"[Permissions] No permissions received after {MaxRequestAttempts} attempts. Everything stays locked.");
    }

    private static void OnPermissionsReceived(string[] granted, string[] whitelistedVehicles)
    {
        // Whitelist first, so the single change notification sees consistent state.
        ClientVehiclePermissions.ApplyWhitelistedVehicleModels(whitelistedVehicles);
        ClientPermissions.ApplyPermissions(granted);

        API.Log.Debug($"[Permissions] Received {granted.Length} permission(s) and {whitelistedVehicles.Length} whitelisted vehicle(s).");
    }
}
