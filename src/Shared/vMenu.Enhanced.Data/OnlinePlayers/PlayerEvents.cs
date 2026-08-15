namespace vMenu.Enhanced.Data.OnlinePlayers;

/// <summary>
/// Network events the server sends to the player being acted on.
/// </summary>
/// <remarks>
/// The action layer only ever answers whoever asked, so anything that has to reach a third player
/// needs its own event. These all travel server to client, never the other way.
/// </remarks>
public static class PlayerEvents
{
    /// <summary>Server to client: name of whoever asked (string). Kills the receiving player.</summary>
    public const string Kill = "vMenu.Enhanced:OnlinePlayers:Kill";

    /// <summary>Server to client: message id, sender name, message (string, string, string).</summary>
    public const string Message = "vMenu.Enhanced:OnlinePlayers:Message";

    /// <summary>
    /// Client to server: message id (string). Sent once the message is actually on screen.
    /// </summary>
    // The server holds the sender's answer open until this arrives, so "sent" means the other player
    // saw it rather than only that the server passed it along.
    public const string MessageAck = "vMenu.Enhanced:OnlinePlayers:MessageAck";

    /// <summary>
    /// Server to client: name of whoever asked, x, y, z (string, string, string, string). Moves the
    /// receiving player there.
    /// </summary>
    // The target's own client does the moving so it can wait for the world to stream in first. The
    // server setting coordinates directly drops people through the map.
    public const string Teleport = "vMenu.Enhanced:OnlinePlayers:Teleport";

    /// <summary>
    /// Replicated convar holding a number that changes whenever somebody joins or leaves.
    /// </summary>
    // A convar rather than a broadcast: it needs no event, and a client that connects halfway
    // through reads the current value instead of having missed the announcement. Named under the
    // same root as everything else, because the configuration module only takes convars it can
    // recognise as vMenu's own.
    public const string RevisionConvar = "vMenu.Enhanced.State.PlayersRevision";
}
