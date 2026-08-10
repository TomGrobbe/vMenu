namespace vMenu.Enhanced.Data.Permissions;

/// <summary>
/// Network events used to sync permissions from the server to a client.
/// </summary>
public static class PermissionEvents
{
    /// <summary>Client to server. Asks for this client's permission set.</summary>
    public const string Request = "vMenu.Enhanced:Permissions:Request";

    /// <summary>
    /// Server to client. Carries five <see cref="string"/> arrays, in this order: the granted
    /// permissions, the whitelisted vehicle models, the models a custom category claimed, the
    /// category each of those models belongs to, and the whitelisted ped models. The third and
    /// fourth are index aligned.
    /// </summary>
    public const string Set = "vMenu.Enhanced:Permissions:Set";
}
