using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Actions.Server;

// This is only for logging when someone uses cheats or a modified client to try and activate
// menu items or commands, or sends events to the server that they're not allowed to use based on permissions/config options.
public static class SecurityLog
{
    private const int StartupQuietMs = 30000;

    private const int SyncGraceMs = 15000;

    private const int RepairCooldownMs = 30000;

    private const int RefusalWindowMs = 10000;

    private const int RefusalFactor = 3;

    private const int MaxFieldLength = 96;

    private static readonly Dictionary<int, int> RepairedAt = [];

    private static readonly Dictionary<int, Refusals> RefusedBy = [];

    private static int _startedAt;

    public static void Initialize() => _startedAt = Native.GetGameTimer();

    public static void UnknownAction(Player source, string actionId)
    {
        if (!Watching())
        {
            return;
        }

        WebhookLog.Security(
            WebhookActor.For(source),
            "asked for an action that does not exist.",
            ("action", Field(actionId)));
    }

    public static void Denied(Player source, string actionId, string permission)
    {
        if (!Watching() || ServerPermissions.SyncedWithin(source.Handle, SyncGraceMs))
        {
            return;
        }

        var now = Native.GetGameTimer();

        if (!RepairedAt.TryGetValue(source.Handle, out var repairedAt) || now - repairedAt >= RepairCooldownMs)
        {
            RepairedAt[source.Handle] = now;

            PermissionsSync.RefreshOne(source.Handle);

            return;
        }

        WebhookLog.Security(
            WebhookActor.For(source),
            "fired an action they are not allowed to use.",
            ("action", Field(actionId)),
            ("missing", Field(permission)));
    }

    public static void RateLimited(Player source, string actionId, int allowance)
    {
        if (!Watching() || allowance <= 0)
        {
            return;
        }

        var now = Native.GetGameTimer();

        if (!RefusedBy.TryGetValue(source.Handle, out var refusals))
        {
            refusals = new Refusals();

            RefusedBy[source.Handle] = refusals;
        }

        while (refusals.Stamps.Count > 0 && now - refusals.Stamps[0] >= RefusalWindowMs)
        {
            refusals.Stamps.RemoveAt(0);
        }

        refusals.Stamps.Add(now);

        if (refusals.Stamps.Count < allowance * RefusalFactor)
        {
            return;
        }

        if (refusals.Reported && now - refusals.ReportedAt < RefusalWindowMs)
        {
            return;
        }

        refusals.Reported = true;
        refusals.ReportedAt = now;

        WebhookLog.Security(
            WebhookActor.For(source),
            "kept firing an action long after the server started refusing it.",
            ("action", Field(actionId)),
            ("refused", refusals.Stamps.Count + " time(s) in " + (RefusalWindowMs / 1000) + "s"));
    }

    public static void MalformedRequest(Player source, string actionId)
    {
        if (!Watching())
        {
            return;
        }

        WebhookLog.Security(
            WebhookActor.For(source),
            "sent arguments the menu would never send.",
            ("action", Field(actionId)));
    }

    public static void Forget(int serverId)
    {
        RepairedAt.Remove(serverId);
        RefusedBy.Remove(serverId);
    }

    private static bool Watching() =>
        WebhookLog.WantsSecurity
        && ServerPermissions.IsReady
        && Native.GetGameTimer() - _startedAt >= StartupQuietMs;

    private static string Field(string value) => WebhookText.Clean(value, MaxFieldLength);

    private sealed class Refusals
    {
        public List<int> Stamps { get; } = [];

        public bool Reported { get; set; }

        public int ReportedAt { get; set; }
    }
}
