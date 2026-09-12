namespace vMenu.Enhanced.Data.OnlinePlayers;

public static class PlayerEvents
{
    public const string Kill = "vMenu.Enhanced:OnlinePlayers:Kill";

    public const string Message = "vMenu.Enhanced:OnlinePlayers:Message";

    public const string MessageAck = "vMenu.Enhanced:OnlinePlayers:MessageAck";

    public const string Teleport = "vMenu.Enhanced:OnlinePlayers:Teleport";

    public const string TeleportIntoVehicle = "vMenu.Enhanced:OnlinePlayers:TeleportIntoVehicle";

    public const string SetWantedLevel = "vMenu.Enhanced:OnlinePlayers:SetWantedLevel";

    public const string WantedLevelAck = "vMenu.Enhanced:OnlinePlayers:WantedLevelAck";

    public const string GetGodMode = "vMenu.Enhanced:OnlinePlayers:GetGodMode";

    public const string GodModeAck = "vMenu.Enhanced:OnlinePlayers:GodModeAck";

    public const string SetNoClip = "vMenu.Enhanced:OnlinePlayers:SetNoClip";

    public const string SetNoClipAccess = "vMenu.Enhanced:OnlinePlayers:SetNoClipAccess";

    // Pushed by the web integration rather than the in-game menu. The target's own client does the work.
    public const string SetWaypoint = "vMenu.Enhanced:OnlinePlayers:SetWaypoint";

    public const string TeleportToGround = "vMenu.Enhanced:OnlinePlayers:TeleportToGround";

    public const string Restore = "vMenu.Enhanced:OnlinePlayers:Restore";

    public const string SpawnVehicle = "vMenu.Enhanced:OnlinePlayers:SpawnVehicle";

    public const string Notify = "vMenu.Enhanced:OnlinePlayers:Notify";

    public const string RevisionConvar = "vMenu.Enhanced.State.PlayersRevision";
}
