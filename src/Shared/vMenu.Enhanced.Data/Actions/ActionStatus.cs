namespace vMenu.Enhanced.Data.Actions;

// How a server action ended, sent over the wire as an int. A code rather than a message: the
// translation tables live on the client, so the server cannot phrase one.
public enum ActionStatus
{
    Ok = 0,

    // Missing the permission the action is registered against.
    Denied = 1,

    // Has the permission; this particular request breaks a rule of the action itself.
    Refused = 8,

    UnknownAction = 2,

    // Arguments missing, malformed, or describing something of the wrong kind.
    InvalidRequest = 3,

    NotFound = 4,

    // The target is there, but not in a state where this can be done to it yet. A player who is still
    // connecting has a server id already and no character in the world yet, so without this they are
    // indistinguishable from one who has left.
    NotReady = 9,

    TooFar = 5,

    Failed = 6,

    // Never sent. Produced client side when no reply arrives.
    Timeout = 7,

    // Allowed, but asked for too many times in a row. Carries the seconds left before the next one is
    // let through.
    RateLimited = 10,
}
