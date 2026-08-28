using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;

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

        SharedAPI.Commands.RegisterCommand(RefreshCommand, true, new Action<int, MessagePackBuffer, string>(OnRefreshCommand));
    }

    // Recomputes and re-sends permissions to every connected player, so ACL edits apply without a
    public static int RefreshAll()
    {
        if (!ServerPermissions.IsReady)
        {
            Log.Error("[Permissions] Cannot refresh permissions: the registry is not ready yet.");

            return -1;
        }

        var refreshed = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            ServerPermissions.SendPermissions(player.ServerId);
            refreshed++;
        }

        Log.Info($"[Permissions] Refreshed permissions for {refreshed} player(s).");

        return refreshed;
    }

    public static bool RefreshOne(int serverId)
    {
        if (!ServerPermissions.IsReady)
        {
            Log.Error("[Permissions] Cannot refresh permissions: the registry is not ready yet.");

            return false;
        }

        var handle = serverId.ToString(CultureInfo.InvariantCulture);

        if (serverId <= 0 || !Native.DoesPlayerExist(handle))
        {
            return false;
        }

        ServerPermissions.SendPermissions(serverId);

        Log.Info($"[Permissions] Refreshed permissions for {Native.GetPlayerName(handle)} (#{serverId}).");

        return true;
    }

    private static void OnRefreshCommand(int source, MessagePackBuffer args, string raw)
    {
        var parts = (raw ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            RefreshAll();

            return;
        }

        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId))
        {
            Log.Error($"[Permissions] '{parts[1]}' is not a server id. Use {RefreshCommand} on its own to refresh everybody.");

            return;
        }

        if (!RefreshOne(serverId))
        {
            Log.Error($"[Permissions] Nobody on this server has id {serverId}.");
        }
    }

    private static void OnPermissionsRequested([FromSource] Player source) =>
        ServerPermissions.SendPermissions(source);
}
