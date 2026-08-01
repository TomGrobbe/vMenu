namespace vMenu.Enhanced.Data.Permissions;

/// <summary>
/// Network events used to sync permissions from the server to a client.
/// </summary>
public static class PermissionEvents
{
    /// <summary>Client to server. Asks for this client's permission set.</summary>
    public const string Request = "vMenu.Enhanced:Permissions:Request";

    /// <summary>
    /// Server to client. Carries the granted permissions and the whitelisted vehicle models as two
    /// <see cref="string"/> arrays, in that order.
    /// </summary>
    public const string Set = "vMenu.Enhanced:Permissions:Set";
}
