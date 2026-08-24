namespace vMenu.Enhanced.Data.Teleport;

public static class TeleportEvents
{
    // Client to server. Asks for the current locations, once the client has its permissions.
    public const string Request = "vMenu.Enhanced:Teleport:Request";

    // Server to client. Carries the whole category list as one JSON string, sent in answer to a Request
    // and again to everyone whenever a player adds something.
    public const string Set = "vMenu.Enhanced:Teleport:Set";
}
