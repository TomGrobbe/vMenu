namespace vMenu.Enhanced.ClientAPI;

/// <summary>The player a player action was invoked on, read from vMenu's Online Players snapshot.
/// The snapshot can be stale and the player may have left, so your server side must check the target
/// still exists before acting on it.</summary>
// A class rather than a record, for the sandbox's equality rules.
public sealed class PlayerTarget
{
    internal PlayerTarget(int serverId, string name)
    {
        ServerId = serverId;
        Name = name;
    }

    public int ServerId { get; }

    public string Name { get; }
}
