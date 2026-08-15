namespace vMenu.Enhanced.Data.PlayerState;

/// <summary>
/// Network events a client uses to ask the server to change something on its state bag.
/// </summary>
public static class PlayerStateEvents
{
    /// <summary>
    /// Client to server: whether this player has just entered or left noclip (bool).
    /// </summary>
    public const string ReportNoClip = "vMenu.Enhanced:PlayerState:ReportNoClip";
}
