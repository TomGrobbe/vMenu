using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.StaffAlerts;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Ticks.Server;

using StaffAlertSettings = vMenu.Enhanced.Data.Configuration.Settings.StaffAlerts;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class StaffAlertActions
{
    private const int DescriptionLimit = 100;

    private const long PruneIntervalMs = 1000;

    private const int PruneGraceMs = 30000;

    private static readonly Dictionary<int, Alert> Alerts = [];

    private static readonly Dictionary<int, int> LastRaised = [];

    private static readonly HashSet<int> OffDuty = [];

    private static int _lastAlertId;

    public static void Register()
    {
        API.OnNetEvent(StaffAlertEvents.ReportHidden, new Action<Player, bool>(OnHiddenReported), false);

        ActionRegistry.RegisterUngated(ActionIds.StaffAlerts.Raise, Raise);

        ActionRegistry.Register(ActionIds.StaffAlerts.Respond, Global.Staff, Respond);
        ActionRegistry.Register(ActionIds.StaffAlerts.GetList, Global.Staff, GetList);
        ActionRegistry.Register(ActionIds.StaffAlerts.Dismiss, Global.Staff, Dismiss);

        ServerTickRegistry.Register(
            "StaffAlerts.Prune",
            Prune,
            TickRate.Every(PruneIntervalMs));
    }

    private static ActionResponse Raise(Player source, string[] args)
    {
        if (!ServerConfig.Value(StaffAlertSettings.Enabled))
        {
            return ActionResponse.Refused();
        }

        if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            return ActionResponse.InvalidRequest();
        }

        var remaining = CooldownRemaining(source.Handle);

        if (remaining > 0)
        {
            Log.Info($"[StaffAlerts] {source.Name} tried to alert staff again with {remaining}s of their cooldown left.");

            return ActionResponse.Refused(remaining.ToString(CultureInfo.InvariantCulture));
        }

        var description = args[0].Trim();

        if (description.Length > DescriptionLimit)
        {
            description = description[..DescriptionLimit];
        }

        var id = ++_lastAlertId;
        var recipients = new HashSet<int>();

        foreach (var player in ConnectedPlayers.All())
        {
            if (player.ServerId == source.Handle)
            {
                continue;
            }

            if (OffDuty.Contains(player.ServerId))
            {
                continue;
            }

            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);

            if (!ServerPermissions.IsPlayerAllowed(handle, Global.Staff))
            {
                continue;
            }

            recipients.Add(player.ServerId);

            API.EmitClient(
                player.ServerId,
                StaffAlertEvents.Show,
                id.ToString(CultureInfo.InvariantCulture),
                source.Name,
                description,
                DisplayMs().ToString(CultureInfo.InvariantCulture));
        }

        Alerts[id] = new Alert(id, source.Handle, source.Name, description, Now(), recipients);

        LastRaised[source.Handle] = Now();

        Log.Info(
            $"[StaffAlerts] #{id} raised by {source.Name} ({source.Handle}): \"{description}\". "
            + $"Sent to {recipients.Count} staff member(s).");

        return ActionResponse.Ok(recipients.Count.ToString(CultureInfo.InvariantCulture));
    }

    private static ActionResponse Respond(Player source, string[] args)
    {
        if (Resolve(args, out var failure) is not { } alert)
        {
            return failure;
        }

        var id = alert.Id;

        var handle = alert.AlerterServerId.ToString(CultureInfo.InvariantCulture);

        if (!Native.DoesPlayerExist(handle))
        {
            return ActionResponse.NotFound();
        }

        var ped = Native.GetPlayerPed(handle);

        if (ped == 0 || !Native.DoesEntityExist(ped))
        {
            return ActionResponse.NotReady();
        }

        alert.AnsweredBy = source.Name;

        var responder = source.Handle.ToString(CultureInfo.InvariantCulture);

        foreach (var recipient in alert.Recipients)
        {
            API.EmitClient(
                recipient,
                StaffAlertEvents.Resolved,
                id.ToString(CultureInfo.InvariantCulture),
                source.Name,
                responder);
        }

        var coords = Native.GetEntityCoords(ped);

        Log.Info($"[StaffAlerts] #{id} answered by {source.Name} ({source.Handle}), who is going to {alert.AlerterName}.");

        return ActionResponse.Ok(
            coords.X.ToString(CultureInfo.InvariantCulture),
            coords.Y.ToString(CultureInfo.InvariantCulture),
            coords.Z.ToString(CultureInfo.InvariantCulture),
            alert.AlerterName);
    }

    private static ActionResponse GetList(Player source, string[] args)
    {
        var now = Now();
        var window = ExpireMs();

        var waiting = new List<Alert>();

        foreach (var alert in Alerts.Values)
        {
            if (alert.AnsweredBy is null && now - alert.RaisedAt <= window)
            {
                waiting.Add(alert);
            }
        }

        waiting.Sort((left, right) => right.Id.CompareTo(left.Id));

        var rows = new List<string>(waiting.Count);

        foreach (var alert in waiting)
        {
            var left = (window - (now - alert.RaisedAt)) / 1000;

            rows.Add(AlertRow.Format(alert.Id, Math.Max(0, left), alert.AlerterName, alert.Description));
        }

        return ActionResponse.Ok([.. rows]);
    }

    private static ActionResponse Dismiss(Player source, string[] args)
    {
        if (Resolve(args, out var failure) is not { } alert)
        {
            return failure;
        }

        alert.AnsweredBy = source.Name;
        alert.Announced = true;

        var closer = source.Handle.ToString(CultureInfo.InvariantCulture);

        foreach (var recipient in alert.Recipients)
        {
            API.EmitClient(
                recipient,
                StaffAlertEvents.Dismissed,
                alert.Id.ToString(CultureInfo.InvariantCulture),
                source.Name,
                closer);
        }

        if (Native.DoesPlayerExist(alert.AlerterServerId.ToString(CultureInfo.InvariantCulture)))
        {
            API.EmitClient(alert.AlerterServerId, StaffAlertEvents.DismissedNotice, source.Name);
        }

        Log.Info($"[StaffAlerts] #{alert.Id} from {alert.AlerterName} was dismissed by {source.Name} ({source.Handle}).");

        return ActionResponse.Ok(alert.AlerterName);
    }

    private static Alert? Resolve(string[] args, out ActionResponse failure)
    {
        failure = ActionResponse.InvalidRequest();

        if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            return null;
        }

        var token = args[0].Trim();

        if (AlertSelector.IsOldest(token) || AlertSelector.IsLatest(token))
        {
            var picked = Waiting(newest: AlertSelector.IsLatest(token));

            if (picked is null)
            {
                failure = ActionResponse.NotFound();
            }

            return picked;
        }

        if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return null;
        }

        if (!Alerts.TryGetValue(id, out var alert))
        {
            failure = ActionResponse.NotFound();

            return null;
        }

        if (alert.AnsweredBy is { } taken)
        {
            failure = ActionResponse.Refused(taken);

            return null;
        }

        if (Now() - alert.RaisedAt > ExpireMs())
        {
            failure = ActionResponse.NotReady();

            return null;
        }

        return alert;
    }

    private static Alert? Waiting(bool newest)
    {
        var now = Now();
        var window = ExpireMs();

        Alert? best = null;

        foreach (var alert in Alerts.Values)
        {
            if (alert.AnsweredBy is not null || now - alert.RaisedAt > window)
            {
                continue;
            }

            if (best is null || (newest ? alert.Id > best.Id : alert.Id < best.Id))
            {
                best = alert;
            }
        }

        return best;
    }

    private static void OnHiddenReported([FromSource] Player source, bool hidden)
    {
        var changed = hidden ? OffDuty.Add(source.Handle) : OffDuty.Remove(source.Handle);

        if (!changed)
        {
            return;
        }

        Log.Info($"[StaffAlerts] {source.Name} ({source.Handle}) is {(hidden ? "no longer taking" : "taking")} alerts.");
    }

    private static int CooldownRemaining(int serverId)
    {
        var cooldown = Math.Max(0, ServerConfig.Value(StaffAlertSettings.CooldownSeconds)) * 1000;

        if (cooldown == 0 || !LastRaised.TryGetValue(serverId, out var raisedAt))
        {
            return 0;
        }

        var elapsed = Now() - raisedAt;

        return elapsed >= cooldown ? 0 : (int)Math.Ceiling((cooldown - elapsed) / 1000f);
    }

    private static void Prune()
    {
        var now = Now();
        var window = ExpireMs();
        var expiry = window + PruneGraceMs;

        var stale = new List<int>();

        foreach (var alert in Alerts.Values)
        {
            if (now - alert.RaisedAt > window)
            {
                Expire(alert);
            }

            if (now - alert.RaisedAt > expiry)
            {
                stale.Add(alert.Id);
            }
        }

        foreach (var id in stale)
        {
            Alerts.Remove(id);
        }

        Forget(LastRaised);

        OffDuty.RemoveWhere(serverId => !Native.DoesPlayerExist(serverId.ToString(CultureInfo.InvariantCulture)));
    }

    private static void Expire(Alert alert)
    {
        if (alert.Announced || alert.AnsweredBy is not null)
        {
            return;
        }

        alert.Announced = true;

        foreach (var recipient in alert.Recipients)
        {
            API.EmitClient(
                recipient,
                StaffAlertEvents.Expired,
                alert.Id.ToString(CultureInfo.InvariantCulture),
                alert.AlerterName);
        }

        Log.Warning($"[StaffAlerts] #{alert.Id} from {alert.AlerterName} ran out with nobody answering it.");
    }

    private static void Forget(Dictionary<int, int> tracked)
    {
        var gone = new List<int>();

        foreach (var serverId in tracked.Keys)
        {
            if (!Native.DoesPlayerExist(serverId.ToString(CultureInfo.InvariantCulture)))
            {
                gone.Add(serverId);
            }
        }

        foreach (var serverId in gone)
        {
            tracked.Remove(serverId);
        }
    }

    private static int ExpireMs() => Math.Max(0, ServerConfig.Value(StaffAlertSettings.ExpireSeconds)) * 1000;

    private static int DisplayMs() => Math.Max(1, ServerConfig.Value(StaffAlertSettings.DisplaySeconds)) * 1000;

    private static int Now() => Native.GetGameTimer();

    private sealed class Alert(
        int id,
        int alerterServerId,
        string alerterName,
        string description,
        int raisedAt,
        HashSet<int> recipients)
    {
        public int Id { get; } = id;

        public int AlerterServerId { get; } = alerterServerId;

        public string AlerterName { get; } = alerterName;

        public string Description { get; } = description;

        public int RaisedAt { get; } = raisedAt;

        public HashSet<int> Recipients { get; } = recipients;

        public string? AnsweredBy { get; set; }

        public bool Announced { get; set; }
    }
}
