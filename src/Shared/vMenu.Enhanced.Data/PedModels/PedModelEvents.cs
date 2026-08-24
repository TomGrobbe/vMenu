namespace vMenu.Enhanced.Data.PedModels;

public static class PedModelEvents
{
    // Client to server. Asks for the ped list, once the client has its permissions.
    public const string Request = "vMenu.Enhanced:PedModels:Request";

    // Server to client. Carries the whole category list as one JSON string, sent in answer to a Request.
    public const string Set = "vMenu.Enhanced:PedModels:Set";
}
