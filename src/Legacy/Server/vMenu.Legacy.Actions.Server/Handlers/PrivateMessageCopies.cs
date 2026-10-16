using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Webhooks.Server;

using vMenu.Legacy.Hosting;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class PrivateMessageCopies
{
    private static readonly HashSet<int> Watching = [];

    public static void Register()
    {
        Script.EventHandlers[PlayerEvents.ReportWatchingMessages] += new Action<Player, bool>(OnWatchingReported);
    }

    public static void Send(Player source, int target, string targetName, string message)
    {
        Watching.RemoveWhere(serverId => !API.DoesPlayerExist(serverId.ToString(CultureInfo.InvariantCulture)));

        foreach (var watcher in Watching)
        {
            if (watcher == source.ServerId() || watcher == target)
            {
                continue;
            }

            // The client only reports its own toggle, so the permission is checked here as well.
            if (!ServerPermissions.IsPlayerAllowed(
                    watcher.ToString(CultureInfo.InvariantCulture),
                    AdminPermissions.SeePrivateMessages))
            {
                continue;
            }

            Script.TriggerClientEvent(watcher, PlayerEvents.MessageCopy, source.Name, targetName, message);
        }
    }

    public static void Record(Player source, int target, string targetName, string message, bool delivered) =>
        WebhookLog.Message(
            source,
            WebhookActor.For(target, targetName),
            "sent a private message to",
            ("message", message),
            ("delivered", delivered ? "yes" : "no"));

    // A named method rather than a lambda, so the binder finds FromSourceAttribute on it.
    private static void OnWatchingReported([FromSource] Player source, bool watching)
    {
        watching = watching && ServerPermissions.IsPlayerAllowed(source, AdminPermissions.SeePrivateMessages);

        var changed = watching ? Watching.Add(source.ServerId()) : Watching.Remove(source.ServerId());

        if (!changed)
        {
            return;
        }

        Log.Info($"[OnlinePlayers] {source.Name} ({source.ServerId()}) is {(watching ? "now" : "no longer")} reading private messages.");
    }
}
