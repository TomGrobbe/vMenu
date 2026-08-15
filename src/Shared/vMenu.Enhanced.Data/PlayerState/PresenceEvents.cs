namespace vMenu.Enhanced.Data.PlayerState;

/// <summary>
/// The conversation that lets a client draw blips for players it cannot see.
/// </summary>
public static class PresenceEvents
{
    /// <summary>Client to server: start sending me snapshots. Checked against the blips permission.</summary>
    public const string Subscribe = "vMenu.Enhanced:Presence:Subscribe";

    /// <summary>Client to server: stop.</summary>
    public const string Unsubscribe = "vMenu.Enhanced:Presence:Unsubscribe";

    /// <summary>
    /// Server to client: where some of the other players are (string), packed by
    /// <see cref="PresenceRow" />.
    /// </summary>
    public const string Snapshot = "vMenu.Enhanced:Presence:Snapshot";
}
