namespace vMenu.Enhanced.Data.PlayerState;

public static class PresenceEvents
{
    // Client to server: start sending me snapshots. Checked against the blips permission.
    public const string Subscribe = "vMenu.Enhanced:Presence:Subscribe";

    // Client to server: stop.
    public const string Unsubscribe = "vMenu.Enhanced:Presence:Unsubscribe";

    // Server to client: where some of the other players are, packed by PresenceRow.
    public const string Snapshot = "vMenu.Enhanced:Presence:Snapshot";
}
