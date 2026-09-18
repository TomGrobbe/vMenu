using System.Globalization;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Actions.Server.Events;
using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Webhooks.Server;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public enum RemoteCommandOutcome
{
    Ok,
    IdentityMismatch,
    NotReady,
    UnknownAction,
    BadRequest,
}

public readonly record struct RemoteCommand(
    string Action,
    int ServerId,
    string Name,
    string? Discord,
    string Operator,
    float X,
    float Y,
    string? Text,
    string? Style,
    string? Model,
    bool HasPoint = false,
    string? Footer = null);

public static class RemotePlayerCommands
{
    private const string StaffLabel = "a staff member";

    private const string On = "1";

    private const string Off = "0";

    public static RemoteCommandOutcome Run(RemoteCommand cmd)
    {
        if (!VerifyIdentity(cmd.ServerId, cmd.Name, cmd.Discord))
        {
            return RemoteCommandOutcome.IdentityMismatch;
        }

        if (PedOf(cmd.ServerId) is null)
        {
            return RemoteCommandOutcome.NotReady;
        }

        var target = cmd.ServerId;

        var targetName = Native.GetPlayerName(target.ToString(CultureInfo.InvariantCulture));
        var actor = WebhookActor.For(target);

        switch (cmd.Action)
        {
            case "kill":
                API.EmitClient(target, PlayerEvents.Kill, StaffLabel);
                break;

            case "kick":
                var by = string.IsNullOrWhiteSpace(cmd.Operator) ? StaffLabel : cmd.Operator.Trim();
                PlayerDrops.RecordKick(target, DropOrigin.IntegrationKick, by, cmd.Text);
                Native.DropPlayer(target.ToString(CultureInfo.InvariantCulture), KickText.For(by, cmd.Text));
                break;

            case "noclip":
                var on = !PlayerNoClipState.IsActive(target);
                PlayerNoClipState.SetForced(target, on);
                API.EmitClient(target, PlayerEvents.SetNoClip, on ? On : Off);
                break;

            case "notify":
                if (string.IsNullOrWhiteSpace(cmd.Text))
                {
                    return RemoteCommandOutcome.BadRequest;
                }

                API.EmitClient(target, PlayerEvents.Notify, cmd.Style ?? "info", cmd.Text, cmd.Footer ?? string.Empty);
                break;

            case "waypoint":
                if (!cmd.HasPoint)
                {
                    return RemoteCommandOutcome.BadRequest;
                }

                API.EmitClient(target, PlayerEvents.SetWaypoint, Coord(cmd.X), Coord(cmd.Y));
                break;

            case "teleport":
                if (!cmd.HasPoint)
                {
                    return RemoteCommandOutcome.BadRequest;
                }

                API.EmitClient(target, PlayerEvents.TeleportToGround, Coord(cmd.X), Coord(cmd.Y));
                break;

            case "heal":
                API.EmitClient(target, PlayerEvents.Restore, "heal");
                break;

            case "armor":
                API.EmitClient(target, PlayerEvents.Restore, "armor");
                break;

            case "spawnvehicle":
                if (string.IsNullOrWhiteSpace(cmd.Model))
                {
                    return RemoteCommandOutcome.BadRequest;
                }

                API.EmitClient(target, PlayerEvents.SpawnVehicle, cmd.Model.Trim());
                break;

            default:
                return RemoteCommandOutcome.UnknownAction;
        }

        Audit(cmd, targetName, actor);

        return RemoteCommandOutcome.Ok;
    }

    private static bool VerifyIdentity(int serverId, string name, string? discord)
    {
        if (serverId <= 0 || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var handle = serverId.ToString(CultureInfo.InvariantCulture);

        if (!Native.DoesPlayerExist(handle))
        {
            return false;
        }

        var current = Native.GetPlayerName(handle);

        if (string.IsNullOrWhiteSpace(current)
            || !string.Equals(current.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(discord)
            && !string.Equals(PlayerIdentifiers.Discord(handle), discord, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static int? PedOf(int serverId)
    {
        var ped = Native.GetPlayerPed(serverId.ToString(CultureInfo.InvariantCulture));

        return ped != 0 && Native.DoesEntityExist(ped) ? ped : null;
    }

    private static void Audit(RemoteCommand cmd, string targetName, WebhookActor actor)
    {
        var op = string.IsNullOrWhiteSpace(cmd.Operator) ? "a linked tool" : cmd.Operator.Trim();

        Log.Info($"[Integration] {op} used the '{cmd.Action}' action on {targetName} (#{cmd.ServerId}) from the live map.");

        WebhookLog.Staff(
            actor,
            null,
            $"had the '{cmd.Action}' action used on them by {op} from the live map.");
    }

    private static string Coord(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
