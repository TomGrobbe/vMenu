using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Permissions.Server;

// Handlers are registered imperatively rather than with [OnNetEvent] because attribute discovery
// only scans the assembly named as the server_script, and this one is a project reference.
public static class PermissionsSync
{
    private const string RefreshCommand = "vmenu_refresh_permissions";

    // Call after ServerPermissions.Initialize.
    public static void RegisterEventHandlers()
    {
        API.OnNetEvent(PermissionEvents.Request, new Action<Player>(OnPermissionsRequested), false);

        SharedAPI.Commands.RegisterCommand(RefreshCommand, true, new Action(RefreshAll));
    }

    // Recomputes and re-sends permissions to every connected player, so ACL edits apply without a
    // restart.
    public static void RefreshAll()
    {
        if (!ServerPermissions.IsReady)
        {
            Log.Error("[Permissions] Cannot refresh permissions: the registry is not ready yet.");
            return;
        }

        var refreshed = 0;

        foreach (var player in API.Players.All)
        {
            ServerPermissions.SendPermissions(player);
            refreshed++;
        }

        Log.Info($"[Permissions] Refreshed permissions for {refreshed} player(s).");
    }

    private static void OnPermissionsRequested([FromSource] Player source) =>
        ServerPermissions.SendPermissions(source);
}
