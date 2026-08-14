using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Permissions;

/// <summary>Client side of the permission handshake.</summary>
// The client asks rather than waiting to be told, because the server side join event fires before
// this script runs. Handlers are registered imperatively because attribute discovery only scans the
// assembly named as the client_script, and this one is a referenced assembly.
public static class PermissionsSync
{
    private const int MaxRequestAttempts = 10;

    private const int RequestRetryDelay = 1000;

    /// <summary>Call before building menus.</summary>
    public static void RegisterEventHandlers()
    {
        API.OnNetEvent(PermissionEvents.Set, new Action<string[], string[], string[], string[], string[], string[]>(OnPermissionsReceived), false);

        RequestPermissions();
    }

    // Retries cover this script starting before the server resource has registered its handler,
    // which happens routinely on a restart with players connected.
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

        Log.Error($"[Permissions] No permissions received after {MaxRequestAttempts} attempts. Everything stays locked.");
    }

    private static void OnPermissionsReceived(
        string[] granted,
        string[] whitelistedVehicles,
        string[] categorisedVehicles,
        string[] vehicleCategories,
        string[] whitelistedPeds,
        string[] whitelistedWeapons)
    {
        // Model data first, so the single change notification sees consistent state.
        ClientVehiclePermissions.ApplyWhitelistedVehicleModels(whitelistedVehicles);
        ClientVehiclePermissions.ApplyCustomCategories(categorisedVehicles, vehicleCategories);
        ClientPedPermissions.ApplyWhitelistedPedModels(whitelistedPeds);
        ClientWeaponPermissions.ApplyWhitelistedWeapons(whitelistedWeapons);
        ClientPermissions.ApplyPermissions(granted);

        Log.Debug($"[Permissions] Received {granted.Length} permission(s), {whitelistedVehicles.Length} whitelisted vehicle(s), {categorisedVehicles.Length} categorised vehicle(s), {whitelistedPeds.Length} whitelisted ped(s) and {whitelistedWeapons.Length} whitelisted weapon(s).");
    }
}
