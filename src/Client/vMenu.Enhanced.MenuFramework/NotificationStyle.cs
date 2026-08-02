namespace vMenu.Enhanced.MenuFramework;

/// <summary>
/// What a notification is telling the player, which is all that decides how it looks.
/// </summary>
public enum NotificationStyle
{
    /// <summary>Something happened. No judgement attached.</summary>
    Info,

    /// <summary>What was asked for was done.</summary>
    Success,

    /// <summary>It went through, but not the way it was asked for.</summary>
    Warning,

    /// <summary>It did not happen.</summary>
    Error,
}
