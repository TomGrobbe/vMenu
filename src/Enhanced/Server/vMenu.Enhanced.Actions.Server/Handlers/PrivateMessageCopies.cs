using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Webhooks.Server;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class PrivateMessageCopies
{
    private static readonly HashSet<int> Watching = [];

    public static void Register()
    {
        API.OnNetEvent(PlayerEvents.ReportWatchingMessages, new Action<Player, bool>(OnWatchingReported), false);
    }

    public static void Send(Player source, int target, string targetName, string message)
    {
        Watching.RemoveWhere(serverId => !Native.DoesPlayerExist(serverId.ToString(CultureInfo.InvariantCulture)));

        foreach (var watcher in Watching)
        {
            if (watcher == source.Handle || watcher == target)
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

            API.EmitClient(watcher, PlayerEvents.MessageCopy, source.Name, targetName, message);
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

        var changed = watching ? Watching.Add(source.Handle) : Watching.Remove(source.Handle);

        if (!changed)
        {
            return;
        }

        Log.Info($"[OnlinePlayers] {source.Name} ({source.Handle}) is {(watching ? "now" : "no longer")} reading private messages.");
    }
}
