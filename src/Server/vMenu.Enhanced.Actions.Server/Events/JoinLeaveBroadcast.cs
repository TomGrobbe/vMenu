using System.Globalization;
using System.Text;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.JoinLeave;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Ticks.Server;
using vMenu.Enhanced.Webhooks.Server;

using JoinLeaveSettings = vMenu.Enhanced.Data.Configuration.Settings.JoinLeave;
using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Actions.Server.Events;

public static class JoinLeaveBroadcast
{
    private const string DroppedEvent = "playerDropped";

    private const long TickMs = 1000;

    // Keeps one pass from scanning so many players that the server hitches.
    private const int ScannedPerPass = 16;

    // Long enough for a real kick reason, short enough not to fill the notification stack.
    private const int MaxReasonLength = 96;

    // Resource names FiveM reports for its own drops rather than a real resource kicking somebody.
    private const string InternalResourcePrefix = "__cfx_internal:";

    private static readonly Dictionary<int, KnownPlayer> Known = [];

    // What the drop event said about somebody leaving, waiting for the pass that notices they are gone.
    private static readonly Dictionary<int, DropReport> Drops = [];

    private static bool _registered;

    private static bool _seeded;

    private static bool _reportedDrop;

    private static int _cursor;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnEvent(DroppedEvent, new Action<int, string?, string?, int>(OnPlayerDropped), false);

        ServerTickRegistry.Register("JoinLeave.Watch", Watch, TickRate.Every(TickMs));
    }

    private static void Watch()
    {
        var players = ConnectedPlayers.All();

        var connected = new HashSet<int>(players.Count);

        foreach (var player in players)
        {
            connected.Add(player.ServerId);
        }

        ReportDepartures(players, connected);

        Scan(players);
    }

    // Anybody the server was holding a server id for who is no longer connected at all.
    private static void ReportDepartures(List<ConnectedPlayer> players, HashSet<int> connected)
    {
        List<int>? gone = null;

        foreach (var (serverId, known) in Known)
        {
            if (connected.Contains(serverId))
            {
                continue;
            }

            (gone ??= []).Add(serverId);

            Depart(players, serverId, known);
        }

        if (gone is not null)
        {
            foreach (var serverId in gone)
            {
                Known.Remove(serverId);
                Drops.Remove(serverId);
                PlayerDrops.Forget(serverId);
            }
        }

        if (Drops.Count == 0)
        {
            return;
        }

        // A report belonging to a server id nothing is tracking any more. Nothing will ever come to collect
        // it, so it goes now rather than attaching itself to whoever inherits the server id.
        List<int>? stale = null;

        foreach (var serverId in Drops.Keys)
        {
            if (!connected.Contains(serverId))
            {
                (stale ??= []).Add(serverId);
            }
        }

        if (stale is null)
        {
            return;
        }

        foreach (var serverId in stale)
        {
            Drops.Remove(serverId);
            PlayerDrops.Forget(serverId);
        }
    }

    private static void Depart(List<ConnectedPlayer> players, int serverId, KnownPlayer known)
    {
        if (known.Arrived)
        {
            EmitLeft(players, serverId, known.Name);
        }

        var kick = PlayerDrops.TakeKick(serverId);
        var report = Drops.GetValueOrDefault(serverId);

        var (what, data) = Classify(known.Arrived, kick, report);

        Record(known.Actor, what, data);
    }

    // Walks a slice of the connected players looking for arrivals and for reused server ids.
    private static void Scan(List<ConnectedPlayer> players)
    {
        if (players.Count == 0)
        {
            _cursor = 0;
            _seeded = true;

            return;
        }

        // The first pass records who is already here without reporting any of it, so restarting the resource
        // under a running server does not read as everybody arriving at once. It ignores the per pass ceiling
        // on purpose: catching up a slice at a time would report the stragglers as arrivals.
        var seeding = !_seeded;

        var limit = seeding ? players.Count : Math.Min(ScannedPerPass, players.Count);

        for (var step = 0; step < limit; step++)
        {
            Examine(players, players[(_cursor + step) % players.Count], report: !seeding);
        }

        _cursor = (_cursor + limit) % players.Count;
        _seeded = true;
    }

    private static void Examine(List<ConnectedPlayer> players, ConnectedPlayer player, bool report)
    {
        var known = Known.GetValueOrDefault(player.ServerId);

        var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);

        if (known is not null && !StillTheSamePerson(players, handle, player, ref known, report))
        {
            known = null;
        }

        if (known is null)
        {
            // Never seen before, or the slot just changed hands. Either way the arrival check decides whether
            // this is somebody who is here or somebody who is still on their way. A drop noted against the old
            // holder of this server id must not follow the new one in.
            Drops.Remove(player.ServerId);
            PlayerDrops.Forget(player.ServerId);

            var arrived = HasArrived(handle);

            var arrival = new KnownPlayer(
                Identify(handle, player, out var real),
                player.Name,
                arrived,
                provisional: !real,
                WebhookActor.For(player.ServerId, player.Name));

            Known[player.ServerId] = arrival;

            if (!report)
            {
                return;
            }

            if (arrived)
            {
                EmitJoined(players, player);
            }

            Record(arrival.Actor, arrived ? "joined the server" : "is connecting");

            return;
        }

        if (known.Arrived || !HasArrived(handle))
        {
            return;
        }

        known = known.NowArrived(WebhookActor.For(player.ServerId, player.Name));

        Known[player.ServerId] = known;

        if (!report)
        {
            return;
        }

        EmitJoined(players, player);

        Record(known.Actor, "joined the server");
    }

    // Whether this server id still belongs to the person it was recorded for.
    private static bool StillTheSamePerson(
        List<ConnectedPlayer> players,
        string handle,
        ConnectedPlayer player,
        ref KnownPlayer known,
        bool report)
    {
        var identity = Identify(handle, player, out var real);

        if (identity == known.Identity)
        {
            return true;
        }

        // Identifiers may not be ready yet, so assume it is the same person until that can be checked.
        if (!real)
        {
            return true;
        }

        if (known.Provisional)
        {
            known = known.WithIdentity(identity);

            Known[player.ServerId] = known;

            return true;
        }

        if (report)
        {
            Depart(players, player.ServerId, known);
        }

        Known.Remove(player.ServerId);

        return false;
    }

    // Somebody still on the loading screen holds a server id without having a character yet, which is
    // the difference between connecting and having arrived.
    private static bool HasArrived(string handle)
    {
        var ped = Native.GetPlayerPed(handle);

        return ped != 0 && Native.DoesEntityExist(ped);
    }

    // Server ids are reused, so identifiers are what identify a unique player.
    private static string Identify(string handle, ConnectedPlayer player, out bool real)
    {
        var count = Native.GetNumPlayerIdentifiers(handle);

        real = count > 0;

        if (!real)
        {
            return $"{player.Name}:{player.ServerId}";
        }

        var identity = new StringBuilder();

        for (var index = 0; index < count; index++)
        {
            identity.Append(Native.GetPlayerIdentifier(handle, index)).Append('|');
        }

        return identity.ToString();
    }

    // Turns a drop into the words for the line and the structured fields that ride along with it.
    private static (string What, (string Key, string Value)[] Data) Classify(
        bool arrived,
        PendingKick? kick,
        DropReport? report)
    {
        var reason = report?.Reason ?? string.Empty;

        if (!arrived)
        {
            return ("disconnected while connecting", reason.Length > 0 ? [("reason", reason)] : []);
        }

        if (kick is not null)
        {
            var fields = new List<(string, string)>(4)
            {
                ("cause", "kick"),
                ("origin", kick.Origin == DropOrigin.MenuKick ? "menu" : "integration"),
                ("by", kick.By),
            };

            if (!string.IsNullOrEmpty(kick.Reason))
            {
                fields.Add(("reason", kick.Reason!));
            }

            return ("was kicked from the server", [.. fields]);
        }

        var code = report?.Code ?? 0;

        if (code == DropCode.Resource && IsRealResource(report?.Resource))
        {
            var fields = new List<(string, string)>(3) { ("cause", "kick"), ("origin", report!.Resource!) };

            if (reason.Length > 0)
            {
                fields.Add(("reason", reason));
            }

            return ("was kicked from the server", [.. fields]);
        }

        return code switch
        {
            DropCode.Client => ("left the server", WithReason("left", reason)),
            DropCode.ClientReplaced => ("was replaced by a new connection", [("cause", "replaced")]),
            DropCode.TimedOut or DropCode.TimedOutPending => ("timed out", [("cause", "timeout")]),
            DropCode.Server or DropCode.ServerShutdown => ("was dropped by the server", WithReason("server", reason)),
            DropCode.StateBagRateLimit or DropCode.NetEventRateLimit
                or DropCode.LatentNetEventRateLimit or DropCode.CommandRateLimit
                => ("was dropped for going over a rate limit", [("cause", "rate limit")]),
            DropCode.OneSyncMissedFrames => ("was dropped by OneSync", [("cause", "onesync")]),

            // An older build that does not report a drop code lands here, so the line stays exactly as it
            // used to be rather than gaining a made up cause.
            _ => ("left the server", reason.Length > 0 ? [("reason", reason)] : []),
        };
    }

    private static (string Key, string Value)[] WithReason(string cause, string reason) =>
        reason.Length > 0 ? [("cause", cause), ("reason", reason)] : [("cause", cause)];

    private static bool IsRealResource(string? resource) =>
        !string.IsNullOrEmpty(resource)
        && !resource.StartsWith(InternalResourcePrefix, StringComparison.Ordinal)
        && !string.Equals(resource, Native.GetCurrentResourceName(), StringComparison.OrdinalIgnoreCase);

    private static void Record(WebhookActor actor, string what, params (string Key, string Value)[] data)
    {
        WebhookLog.Connection(actor, what + ".", data);

        if (!ServerConfig.Value(JoinLeaveSettings.LogToConsole))
        {
            return;
        }

        Log.Info($"[JoinLeave] {actor.Name} ({actor.ServerId}) {what}.{Describe(data)}");
    }

    private static string Describe((string Key, string Value)[] data)
    {
        if (data.Length == 0)
        {
            return string.Empty;
        }

        var parts = new List<string>(data.Length);

        foreach (var (key, value) in data)
        {
            parts.Add($"{key}: {value}");
        }

        return " " + string.Join(", ", parts);
    }

    private static void EmitJoined(List<ConnectedPlayer> players, ConnectedPlayer joiner)
    {
        foreach (var player in players)
        {
            if (player.ServerId == joiner.ServerId)
            {
                continue;
            }

            API.EmitClient(player.ServerId, JoinLeaveEvents.Joined, joiner.Name);
        }
    }

    private static void EmitLeft(List<ConnectedPlayer> players, int serverId, string name)
    {
        var reason = Drops.TryGetValue(serverId, out var report) ? report.Reason : string.Empty;

        foreach (var player in players)
        {
            if (player.ServerId == serverId)
            {
                continue;
            }

            var allowed = reason.Length > 0
                && ServerPermissions.IsPlayerAllowed(
                    player.ServerId.ToString(CultureInfo.InvariantCulture),
                    DisplaySettingsPermissions.SeeLeaveReasons);

            API.EmitClient(player.ServerId, JoinLeaveEvents.Left, name, allowed ? reason : string.Empty);
        }
    }

    private static void OnPlayerDropped(
        [FromSource] int source,
        string? reason = null,
        string? resourceName = null,
        int clientDropReason = 0)
    {
        if (!_reportedDrop)
        {
            _reportedDrop = true;

            Log.Debug(
                $"[JoinLeave] {DroppedEvent} is firing. First one: source {source}, reason \"{reason}\", "
                + $"resource \"{resourceName}\", code {clientDropReason}.");
        }

        // An unparseable source arrives as -1, and there is nobody to record anything against.
        if (source <= 0)
        {
            return;
        }

        var text = string.IsNullOrWhiteSpace(reason) ? string.Empty : reason.Trim();

        if (text.Length > MaxReasonLength)
        {
            text = text[..MaxReasonLength];
        }

        Drops[source] = new DropReport(text, resourceName, clientDropReason);
    }

    // FiveM's ClientDropReasons.h. Anything not here is treated as an ordinary leave.
    private static class DropCode
    {
        public const int Resource = 1;

        public const int Client = 2;

        public const int Server = 3;

        public const int ClientReplaced = 4;

        public const int TimedOut = 5;

        public const int TimedOutPending = 6;

        public const int ServerShutdown = 7;

        public const int StateBagRateLimit = 8;

        public const int NetEventRateLimit = 9;

        public const int LatentNetEventRateLimit = 10;

        public const int CommandRateLimit = 11;

        public const int OneSyncMissedFrames = 12;
    }

    // A class rather than a record: generated equality routes through
    // EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class DropReport(string reason, string? resource, int code)
    {
        public string Reason { get; } = reason;

        public string? Resource { get; } = resource;

        public int Code { get; } = code;
    }

    // A class rather than a record: generated equality routes through
    // EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class KnownPlayer(string identity, string name, bool arrived, bool provisional, WebhookActor actor)
    {
        public string Identity { get; } = identity;

        public string Name { get; } = name;

        // False while they are still on their way in.
        public bool Arrived { get; } = arrived;

        // Whether Identity is the name based stand in rather than identifiers.
        public bool Provisional { get; } = provisional;

        public WebhookActor Actor { get; } = actor;

        public KnownPlayer NowArrived(WebhookActor actor) => new(Identity, Name, arrived: true, Provisional, actor);

        public KnownPlayer WithIdentity(string identity) => new(identity, Name, Arrived, provisional: false, Actor);
    }
}
