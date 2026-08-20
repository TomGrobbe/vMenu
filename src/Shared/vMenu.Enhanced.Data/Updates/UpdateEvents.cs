namespace vMenu.Enhanced.Data.Updates;

/// <summary>
/// Network events used to tell staff that a newer vMenu Enhanced is out.
/// </summary>
public static class UpdateEvents
{
    /// <summary>Client to server, once it has its permissions. Answered only for staff.</summary>
    public const string Request = "vMenu.Enhanced:Updates:Request";

    /// <summary>Server to client: the version, then the page to get it from.</summary>
    public const string Available = "vMenu.Enhanced:Updates:Available";
}
