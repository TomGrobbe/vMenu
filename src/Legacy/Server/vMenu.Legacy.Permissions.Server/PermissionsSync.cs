using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Permissions.Server;

// Handlers are registered imperatively rather than with [OnNetEvent] because attribute discovery
// only scans the assembly named as the server_script, and this one is a project reference.
public static class PermissionsSync
{
    private const string RefreshCommand = "vmenu_refresh_permissions";

    private const string DroppedEvent = "playerDropped";

    // Call after ServerPermissions.Initialize.
    public static void RegisterEventHandlers()
    {
        Script.EventHandlers[PermissionEvents.Request] += new Action<Player>(OnPermissionsRequested);

        Script.EventHandlers[PermissionEvents.Refresh] += new Action<string, object>(([FromSource] string source, object serverIds) =>
        {
            if (!Script.IsFromPlayer(source))
            {
                OnRefreshRequested(serverIds is null ? null : Payload.Ints(serverIds));
            }
        });

        Script.EventHandlers[DroppedEvent] += new Action<string, string?>(OnPlayerDropped);

        Commands.Register(RefreshCommand, true, new Action<int, List<object>, string>(OnRefreshCommand));
    }

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

        if (serverId <= 0 || !API.DoesPlayerExist(handle))
        {
            return false;
        }

        ServerPermissions.SendPermissions(serverId);

        Log.Info($"[Permissions] Refreshed permissions for {API.GetPlayerName(handle)} (#{serverId}).");

        return true;
    }

    private static void OnRefreshCommand(int source, List<object> args, string raw)
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

    private static void OnRefreshRequested(int[]? serverIds = null)
    {
        if (serverIds is null || serverIds.Length == 0)
        {
            RefreshAll();

            return;
        }

        foreach (var serverId in serverIds)
        {
            RefreshOne(serverId);
        }
    }

    private static void OnPermissionsRequested([FromSource] Player source) =>
        ServerPermissions.SendPermissions(source);

    private static void OnPlayerDropped([FromSource] string sourceString, string? reason = null)
    {
        if (Script.IsFromPlayer(sourceString))
        {
            return;
        }

        var source = Script.SourceId(sourceString);

        if (source > 0)
        {
            ServerPermissions.ForgetSync(source);
        }
    }
}
