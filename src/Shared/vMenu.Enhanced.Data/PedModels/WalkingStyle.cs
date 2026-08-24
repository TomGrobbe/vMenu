namespace vMenu.Enhanced.Data.PedModels;

// A class rather than a record: generated equality reaches for EqualityComparer<T>.Default, which
// the client sandbox refuses to load.
public sealed class WalkingStyle
{
    // The clip set the game loads, such as move_m@casual@a.
    public string Clipset { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}

public static class WalkingStyleEvents
{
    // Client to server. Asks for the list, once the client has its permissions.
    public const string Request = "vMenu.Enhanced:WalkingStyles:Request";

    // Server to client. Carries the whole list as one JSON string, sent in answer to a Request.
    public const string Set = "vMenu.Enhanced:WalkingStyles:Set";
}
