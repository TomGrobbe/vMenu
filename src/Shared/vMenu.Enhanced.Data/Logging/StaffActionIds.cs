using vMenu.Enhanced.Data.Actions;

namespace vMenu.Enhanced.Data.Logging;

public static class StaffActionIds
{
    private static readonly Dictionary<string, string> Verbs = new(StringComparer.Ordinal)
    {
        [ActionIds.OnlinePlayers.Kick] = "kicked",
        [ActionIds.OnlinePlayers.Kill] = "killed",
        [ActionIds.OnlinePlayers.Summon] = "summoned",
        [ActionIds.OnlinePlayers.SummonIntoVehicle] = "summoned into their vehicle",
        [ActionIds.OnlinePlayers.SendMessage] = "messaged",
        [ActionIds.OnlinePlayers.SetWantedLevel] = "set the wanted level of",
        [ActionIds.OnlinePlayers.DeleteVehicle] = "deleted the vehicle of",
        [ActionIds.OnlinePlayers.GetIdentifiers] = "looked up the identifiers of",
        [ActionIds.OnlinePlayers.GetStatus] = "checked the status of",
        [ActionIds.OnlinePlayers.RefreshPermissions] = "refreshed the permissions of",
        [ActionIds.OnlinePlayers.SetNoClip] = "changed the noclip state of",
        [ActionIds.OnlinePlayers.SetNoClipAccess] = "changed noclip access for",
        [ActionIds.OnlinePlayers.GetCoordsForTeleport] = "teleported to",
        [ActionIds.OnlinePlayers.GetCoordsForWaypoint] = "set a waypoint on",
        [ActionIds.OnlinePlayers.GetVehicleForTeleport] = "teleported into the vehicle of",

        [ActionIds.Admin.SetFrozen] = "changed the frozen state of",
        [ActionIds.Admin.SetHeld] = "picked up or put down",
        [ActionIds.Admin.ClearArea] = "cleared the area around themselves",
        [ActionIds.Admin.DeleteVehicle] = "deleted a vehicle",
        [ActionIds.Admin.DeleteEmptyVehicles] = "deleted every empty vehicle",
        [ActionIds.Admin.DeleteAllVehicles] = "deleted every vehicle",
        [ActionIds.Admin.Announce] = "announced to the server",
        [ActionIds.Admin.RefreshPermissions] = "refreshed everybody's permissions",
        [ActionIds.Admin.AddAnnouncement] = "added a scheduled announcement",
        [ActionIds.Admin.RemoveAnnouncement] = "removed a scheduled announcement",
        [ActionIds.Admin.ResetRoutingBucket] = "put themselves back in the default world",

    };

    public static bool Includes(string actionId) => Verbs.ContainsKey(actionId);

    public static string VerbFor(string actionId) =>
        Verbs.TryGetValue(actionId, out var verb) ? verb : actionId;

    public static bool TakesTarget(string actionId) => actionId switch
    {
        ActionIds.Admin.ClearArea
            or ActionIds.Admin.DeleteVehicle
            or ActionIds.Admin.DeleteEmptyVehicles
            or ActionIds.Admin.DeleteAllVehicles
            or ActionIds.Admin.Announce
            or ActionIds.Admin.RefreshPermissions
            or ActionIds.Admin.AddAnnouncement
            or ActionIds.Admin.RemoveAnnouncement
            or ActionIds.Admin.ResetRoutingBucket => false,
        _ => true,
    };
}
