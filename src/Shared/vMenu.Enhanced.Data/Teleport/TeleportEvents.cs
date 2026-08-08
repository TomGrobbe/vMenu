namespace vMenu.Enhanced.Data.Teleport;

/// <summary>
/// Network events used to keep every client's copy of the teleport locations current.
/// </summary>
public static class TeleportEvents
{
    /// <summary>Client to server. Asks for the current locations, once the client has its permissions.</summary>
    public const string Request = "vMenu.Enhanced:Teleport:Request";

    /// <summary>
    /// Server to client. Carries the whole category list as one JSON string, sent in answer to a
    /// <see cref="Request"/> and again to everyone whenever a player adds something.
    /// </summary>
    public const string Set = "vMenu.Enhanced:Teleport:Set";
}
