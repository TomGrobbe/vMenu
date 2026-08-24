namespace vMenu.Enhanced.MenuFramework;

// What a notification is telling the player, which is all that decides how it looks.
public enum NotificationStyle
{
    // Something happened. No judgement attached.
    Info,

    // What was asked for was done.
    Success,

    // It went through, but not the way it was asked for.
    Warning,

    // It did not happen.
    Error,
}
