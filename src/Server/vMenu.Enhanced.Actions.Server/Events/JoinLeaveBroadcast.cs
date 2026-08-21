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

using JoinLeaveSettings = vMenu.Enhanced.Data.Configuration.Settings.JoinLeave;
using MiscSettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.MiscSettings;

namespace vMenu.Enhanced.Actions.Server.Events;

/// <summary>
/// Tells everybody when somebody arrives on the server or leaves it, and optionally writes a log entry about it to the server console.
/// </summary>
public static class JoinLeaveBroadcast
{
    private const string DroppedEvent = "playerDropped";

    private const long TickMs = 1000;

    // Used for not scanning too many players each pass to not cause server hitches
    private const int ScannedPerPass = 16;

    // Long enough for a real kick reason, short enough not to fill the notification stack.
    private const int MaxReasonLength = 96;

    private static readonly Dictionary<int, KnownPlayer> Known = [];

    // Why somebody left, waiting for the pass that notices they are gone.
    private static readonly Dictionary<int, string> Reasons = [];

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

        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);

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
                Reasons.Remove(serverId);
            }
        }

        if (Reasons.Count == 0)
        {
            return;
        }

        // A reason belonging to a server id nothing is tracking any more. Nothing will ever come to
        // collect it, so it goes now rather than waiting around to attach itself to whoever inherits
        // the server id.
        List<int>? stale = null;

        foreach (var serverId in Reasons.Keys)
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
            Reasons.Remove(serverId);
        }
    }

    // Somebody is gone.
    private static void Depart(List<ConnectedPlayer> players, int serverId, KnownPlayer known)
    {
        if (known.Arrived)
        {
            EmitLeft(players, serverId, known.Name);
        }

        Record(
            serverId,
            known.Name,
            known.Arrived ? "left the server" : "disconnected while connecting",
            Reasons.GetValueOrDefault(serverId));
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

        // The first pass records who is already here without reporting any of it, so restarting the
        // resource under a running server does not read as everybody arriving at once. It ignores the
        // per pass ceiling on purpose: catching up a slice at a time would report the stragglers as
        // arrivals while it worked through them.
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
            // Never seen before, or the slot just changed hands. Either way the arrival check decides
            // whether this is somebody who is here or somebody who is still on their way.
            Reasons.Remove(player.ServerId);

            var arrived = HasArrived(handle);

            Known[player.ServerId] = new KnownPlayer(
                Identify(handle, player, out var real),
                player.Name,
                arrived,
                provisional: !real);

            if (!report)
            {
                return;
            }

            if (arrived)
            {
                EmitJoined(players, player);
            }

            Record(player.ServerId, player.Name, arrived ? "joined the server" : "is connecting");

            return;
        }

        if (known.Arrived || !HasArrived(handle))
        {
            return;
        }

        Known[player.ServerId] = known.NowArrived();

        if (!report)
        {
            return;
        }

        EmitJoined(players, player);

        Record(player.ServerId, player.Name, "joined the server");
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

        // Identifiers may not be ready yet, just assume it's the same person for now until we can actually verify
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

    
    // Somebody still on the loading screen holds a server id without having a character yet, that's the difference between connecting and having arrived.
    private static bool HasArrived(string handle)
    {
        var ped = Native.GetPlayerPed(handle);

        return ped != 0 && Native.DoesEntityExist(ped);
    }

    // Server id's are now reused, so we use identifiers to identify unique players
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

    // server console logging (optional)
    private static void Record(int serverId, string name, string what, string? reason = null)
    {
        if (!ServerConfig.Value(JoinLeaveSettings.LogToConsole))
        {
            return;
        }

        var because = string.IsNullOrEmpty(reason) ? string.Empty : $" Reason: {reason}";

        Log.Info($"[JoinLeave] {name} ({serverId}) {what}.{because}");
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
        var reason = Reasons.TryGetValue(serverId, out var stored) ? stored : string.Empty;

        foreach (var player in players)
        {
            if (player.ServerId == serverId)
            {
                continue;
            }

            var allowed = reason.Length > 0
                && ServerPermissions.IsPlayerAllowed(
                    player.ServerId.ToString(CultureInfo.InvariantCulture),
                    MiscSettingsPermissions.SeeLeaveReasons);

            API.EmitClient(player.ServerId, JoinLeaveEvents.Left, name, allowed ? reason : string.Empty);
        }
    }

    
    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (!_reportedDrop)
        {
            _reportedDrop = true;

            Log.Trace($"[JoinLeave] {DroppedEvent} is firing. First one: source {source}, reason \"{reason}\".");
        }

        // An unparseable source arrives as -1, and there is nobody to record a reason against.
        if (source <= 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return;
        }

        var text = reason.Trim();

        Reasons[source] = text.Length > MaxReasonLength ? text[..MaxReasonLength] : text;
    }

    // A plain class rather than a record, matching the rest of this codebase: the generated equality
    // routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class KnownPlayer(string identity, string name, bool arrived, bool provisional)
    {
        public string Identity { get; } = identity;

        public string Name { get; } = name;

        /// <summary>False while they are still on their way in.</summary>
        public bool Arrived { get; } = arrived;

        /// <summary>Whether <see cref="Identity"/> is the name based stand in rather than identifiers.</summary>
        public bool Provisional { get; } = provisional;

        public KnownPlayer NowArrived() => new(Identity, Name, arrived: true, Provisional);

        public KnownPlayer WithIdentity(string identity) => new(identity, Name, Arrived, provisional: false);
    }
}
