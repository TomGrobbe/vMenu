namespace vMenu.Enhanced.Data.PedModels;

/// <summary>One way of walking that a server owner offered.</summary>
// A plain class rather than a record, because the generated equality reaches for
// EqualityComparer<T>.Default and the client sandbox refuses to load it.
public sealed class WalkingStyle
{
    /// <summary>The clip set the game loads, such as <c>move_m@casual@a</c>.</summary>
    public string Clipset { get; set; } = string.Empty;

    /// <summary>What players see on the row.</summary>
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Network events used to give every client the walking styles the server owner defined.
/// </summary>
public static class WalkingStyleEvents
{
    /// <summary>Client to server. Asks for the list, once the client has its permissions.</summary>
    public const string Request = "vMenu.Enhanced:WalkingStyles:Request";

    /// <summary>
    /// Server to client. Carries the whole list as one JSON string, sent in answer to a
    /// <see cref="Request"/>.
    /// </summary>
    public const string Set = "vMenu.Enhanced:WalkingStyles:Set";
}
