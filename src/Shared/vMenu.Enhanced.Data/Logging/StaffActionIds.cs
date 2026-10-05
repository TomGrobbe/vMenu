using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Bullying;

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
        [ActionIds.OnlinePlayers.ExplodeVehicle] = "blew up the vehicle of",
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

        [ActionIds.Bully.StopAll] = "stopped every bully effect",
    };

    static StaffActionIds()
    {
        foreach (var effect in BullyEffects.All)
        {
            Verbs[ActionIds.Bully.Effect(effect.Id)] = effect.Verb;
            Verbs[ActionIds.Bully.Everyone(effect.Id)] = effect.Verb + " everyone";
        }

        foreach (var toggle in BullyToggles.All)
        {
            var name = BullyToggles.LogName(toggle);

            Verbs[ActionIds.Bully.Toggle(toggle, true)] = $"switched {name} on for";
            Verbs[ActionIds.Bully.Toggle(toggle, false)] = $"switched {name} off for";
            Verbs[ActionIds.Bully.ServerToggle(toggle, true)] = $"switched the server wide {name} on";
            Verbs[ActionIds.Bully.ServerToggle(toggle, false)] = $"switched the server wide {name} off";
        }
    }

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
            or ActionIds.Admin.ResetRoutingBucket
            or ActionIds.Bully.StopAll => false,
        _ when actionId.StartsWith(ActionIds.Bully.EveryonePrefix, StringComparison.Ordinal)
            || actionId.StartsWith(ActionIds.Bully.ServerTogglePrefix, StringComparison.Ordinal) => false,
        _ => true,
    };
}
