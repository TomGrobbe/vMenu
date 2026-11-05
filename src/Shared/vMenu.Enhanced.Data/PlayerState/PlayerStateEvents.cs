namespace vMenu.Enhanced.Data.PlayerState;

/// <summary>
///  Network events a client uses to ask the server to change something on its state bag.
/// </summary>
public static class PlayerStateEvents
{
    public const string ReportNoClip = "vMenu.Enhanced:PlayerState:ReportNoClip";

    public const string ReportFreecam = "vMenu.Enhanced:PlayerState:ReportFreecam";

    public const string FreecamVehicle = "vMenu.Enhanced:PlayerState:FreecamVehicle";

    public const string FreecamDone = "vMenu.Enhanced:PlayerState:FreecamDone";
}
