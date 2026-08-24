namespace vMenu.Enhanced.Data.PlayerState;

// Network events a client uses to ask the server to change something on its state bag.
public static class PlayerStateEvents
{
    // Client to server: whether this player has just entered or left noclip.
    public const string ReportNoClip = "vMenu.Enhanced:PlayerState:ReportNoClip";
}
