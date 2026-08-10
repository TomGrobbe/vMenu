namespace vMenu.Enhanced.Data.PedModels;

/// <summary>
/// Network events used to give every client the ped models the server owner defined.
/// </summary>
public static class PedModelEvents
{
    /// <summary>Client to server. Asks for the ped list, once the client has its permissions.</summary>
    public const string Request = "vMenu.Enhanced:PedModels:Request";

    /// <summary>
    /// Server to client. Carries the whole category list as one JSON string, sent in answer to a
    /// <see cref="Request"/>.
    /// </summary>
    public const string Set = "vMenu.Enhanced:PedModels:Set";
}
