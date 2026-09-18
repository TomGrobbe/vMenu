namespace vMenu.Enhanced.Actions.Server;

public enum DropOrigin
{
    MenuKick,
    IntegrationKick,
}

// vMenu does its own kicking, so it notes who and why here before dropping the player. The drop is
// noticed a moment later by the join/leave watch, which reads this back to label the leave line.
public static class PlayerDrops
{
    private static readonly Dictionary<int, PendingKick> Pending = [];

    public static void RecordKick(int serverId, DropOrigin origin, string by, string? reason)
    {
        if (serverId <= 0)
        {
            return;
        }

        Pending[serverId] = new PendingKick(origin, by, reason);
    }

    public static PendingKick? TakeKick(int serverId)
    {
        if (!Pending.TryGetValue(serverId, out var kick))
        {
            return null;
        }

        Pending.Remove(serverId);

        return kick;
    }

    public static void Forget(int serverId) => Pending.Remove(serverId);
}

// A class rather than a record, matching the join/leave watch: generated record equality routes
// through a default comparer the resource sandbox refuses to load.
public sealed class PendingKick(DropOrigin origin, string by, string? reason)
{
    public DropOrigin Origin { get; } = origin;

    public string By { get; } = by;

    public string? Reason { get; } = reason;
}
