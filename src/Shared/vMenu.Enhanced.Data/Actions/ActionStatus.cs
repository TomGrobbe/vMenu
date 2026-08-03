namespace vMenu.Enhanced.Data.Actions;

/// <summary>
/// How a server action ended. Sent over the wire as an <see cref="int"/>.
/// </summary>
/// <remarks>
/// A code rather than a message: the translation tables live on the client, so the server cannot
/// phrase one.
/// </remarks>
public enum ActionStatus
{
    Ok = 0,

    /// <summary>Missing the permission the action is registered against.</summary>
    Denied = 1,

    /// <summary>Has the permission; this particular request breaks a rule of the action itself.</summary>
    Refused = 8,

    UnknownAction = 2,

    /// <summary>Arguments missing, malformed, or describing something of the wrong kind.</summary>
    InvalidRequest = 3,

    NotFound = 4,

    TooFar = 5,

    Failed = 6,

    /// <summary>Never sent. Produced client side when no reply arrives.</summary>
    Timeout = 7,
}
