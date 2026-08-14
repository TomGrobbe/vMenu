using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Permissions.Server;

/// <summary>
/// Server side of the permission handshake.
/// </summary>
/// <remarks>
/// Handlers are registered imperatively rather than with <c>[OnNetEvent]</c> because attribute
/// discovery only scans the assembly named as the <c>server_script</c>, and this one is a project
/// reference.
/// </remarks>
public static class PermissionsSync
{
    private const string RefreshCommand = "vmenu_refresh_permissions";

    /// <summary>Call after <see cref="ServerPermissions.Initialize"/>.</summary>
    public static void RegisterEventHandlers()
    {
        API.OnNetEvent(PermissionEvents.Request, new Action<Player>(OnPermissionsRequested), false);

        SharedAPI.Commands.RegisterCommand(RefreshCommand, true, new Action(RefreshAll));
    }

    /// <summary>
    /// Recomputes and re-sends permissions to every connected player, so ACL edits apply without a
    /// restart.
    /// </summary>
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
