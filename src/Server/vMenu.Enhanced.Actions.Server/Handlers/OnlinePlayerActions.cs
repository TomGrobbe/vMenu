using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Ticks.Server;

using OnlinePlayersPermissions = vMenu.Enhanced.Data.Permissions.Menus.OnlinePlayers;

namespace vMenu.Enhanced.Actions.Server.Handlers;

/// <summary>
/// The online players list, and everything the menu can do to somebody on it.
/// </summary>
/// <remarks>
/// The list has to come from here rather than from the client: under OneSync most players are not
/// streamed in, so a client asking the game who is online only ever sees its own neighbourhood.
/// </remarks>
public static class OnlinePlayerActions
{
    /// <summary>Identifiers a client has no business knowing about, matched or not.</summary>
    // vMenu has never let an IP address out of the server and is not about to start.
    private const string HiddenIdentifierPrefix = "ip:";

    private const string DefaultKickReason = "You have been kicked.";

    /// <summary>
    /// How often the connected player set is checked, so the menu can tell a player their list has
    /// gone stale.
    /// </summary>
    // Polled rather than hooked onto a join/leave event, because a second of lag before a subtitle
    // turns red costs nothing and this cannot miss an event it was not listening for.
    private const long RevisionPollMs = 1000;

    /// <summary>
    /// How long the sender waits to hear that their message actually landed.
    /// </summary>
    // Comfortably inside the client's own ten second timeout on the action, so a message that is
    // never acknowledged still comes back as a real answer rather than as a timeout.
    private const int AckTimeoutMs = 5000;

    private static readonly HashSet<int> Connected = [];

    private static readonly Dictionary<int, PendingMessage> Unacknowledged = [];

    private static int _revision;

    private static int _lastMessageId;

    public static void Register()
    {
        API.OnNetEvent(PlayerEvents.MessageAck, new Action<Player, string>(OnMessageAcknowledged), false);

        ActionRegistry.Register(ActionIds.OnlinePlayers.GetList, OnlinePlayersPermissions.Menu, GetList);

        ActionRegistry.Register(ActionIds.OnlinePlayers.GetCoordsForTeleport, OnlinePlayersPermissions.TeleportTo, GetCoords);
        ActionRegistry.Register(ActionIds.OnlinePlayers.GetCoordsForWaypoint, OnlinePlayersPermissions.Waypoint, GetCoords);

        ActionRegistry.Register(ActionIds.OnlinePlayers.Kick, OnlinePlayersPermissions.Kick, Kick);
        ActionRegistry.Register(ActionIds.OnlinePlayers.Kill, OnlinePlayersPermissions.Kill, Kill);
        ActionRegistry.Register(ActionIds.OnlinePlayers.Summon, OnlinePlayersPermissions.Summon, Summon);
        ActionRegistry.Register(ActionIds.OnlinePlayers.SendMessage, OnlinePlayersPermissions.SendMessage, SendMessage);
        ActionRegistry.Register(ActionIds.OnlinePlayers.GetIdentifiers, OnlinePlayersPermissions.Identifiers, GetIdentifiers);

        PublishRevision();

        ServerTickRegistry.Register(
            "OnlinePlayers.Revision",
            TrackConnectedPlayers,
            TickRate.Every(RevisionPollMs));
    }

    /// <summary>
    /// Everybody online, or everybody matching a search.
    /// </summary>
    /// <remarks>
    /// Only a server id and a name go back. Identifiers are matched here and never sent, which is the
    /// whole reason searching is a server action instead of something the client does to a list it
    /// already has.
    /// </remarks>
    private static ActionResponse GetList(Player source, string[] args)
    {
        var query = (args.Length > 0 ? args[0] : string.Empty).Trim();

        var rows = new List<string>();

        foreach (var player in ConnectedPlayers())
        {
            if (Matches(player, query))
            {
                rows.Add(PlayerRow.Format(player.ServerId, player.Name));
            }
        }

        return ActionResponse.Ok([.. rows]);
    }

    /// <summary>
    /// A player matches on any one of three things: their exact server id, part of their name, or one
    /// of their identifiers in full.
    /// </summary>
    /// <remarks>
    /// Only the name is a partial match. A server id is a number, so half of one means nothing, and a
    /// partial identifier match would turn this into a way to go fishing for other people's licenses
    /// a few characters at a time.
    /// </remarks>
    private static bool Matches(ConnectedPlayer player, string query)
    {
        if (query.Length == 0)
        {
            return true;
        }

        if (int.TryParse(query, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId)
            && serverId == player.ServerId)
        {
            return true;
        }

        if (player.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return MatchesIdentifier(player.ServerId, query);
    }

    /// <summary>
    /// Everybody actually connected right now.
    /// </summary>
    /// <remarks>
    /// Read straight from the game rather than from <c>API.Players.All</c>. That is a cache the
    /// runtime fills on demand and never prunes, so it keeps handing back players who have already
    /// left: they come through with an empty name, they fail every action, and nothing ever notices
    /// they are gone.
    /// </remarks>
    private static List<ConnectedPlayer> ConnectedPlayers()
    {
        var count = Native.GetNumPlayerIndices();
        var players = new List<ConnectedPlayer>(count);

        for (var index = 0; index < count; index++)
        {
            var handle = Native.GetPlayerFromIndex(index);

            if (string.IsNullOrEmpty(handle)
                || !int.TryParse(handle, NumberStyles.Integer, CultureInfo.InvariantCulture, out var serverId)
                || !Native.DoesPlayerExist(handle))
            {
                continue;
            }

            var name = Native.GetPlayerName(handle);

            // Falls back to the server id so a row is never blank. Somebody you cannot see is
            // somebody you cannot pick.
            players.Add(new ConnectedPlayer(
                serverId,
                string.IsNullOrWhiteSpace(name) ? $"#{serverId}" : name));
        }

        return players;
    }

    private static bool MatchesIdentifier(int handle, string query)
    {
        var source = handle.ToString(CultureInfo.InvariantCulture);
        var count = Native.GetNumPlayerIdentifiers(source);

        for (var index = 0; index < count; index++)
        {
            var identifier = Native.GetPlayerIdentifier(source, index);

            if (string.IsNullOrEmpty(identifier) || identifier.StartsWith(HiddenIdentifierPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(identifier, query, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Every identifier a player is connected with, bar their IP address.
    /// </summary>
    /// <remarks>
    /// Unlike the list itself, this really does send identifiers to a client, which is why it has its
    /// own permission. The IP is left out on purpose: it is the one identifier that says where
    /// somebody lives rather than which account they are, and no menu here has a use for it.
    /// </remarks>
    private static ActionResponse GetIdentifiers(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        var handle = target.ToString(CultureInfo.InvariantCulture);
        var count = Native.GetNumPlayerIdentifiers(handle);

        var identifiers = new List<string>(count);

        for (var index = 0; index < count; index++)
        {
            var identifier = Native.GetPlayerIdentifier(handle, index);

            if (string.IsNullOrEmpty(identifier) || identifier.StartsWith(HiddenIdentifierPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            identifiers.Add(identifier);
        }

        API.Log.Info($"[OnlinePlayers] {source} read the identifiers of {target}.");

        return ActionResponse.Ok([.. identifiers]);
    }

    /// <summary>Where a player is right now, for teleporting to them or pointing a waypoint at them.</summary>
    private static ActionResponse GetCoords(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (PedOf(target) is not { } ped)
        {
            return ActionResponse.NotReady();
        }

        var coords = Native.GetEntityCoords(ped);

        return ActionResponse.Ok(
            coords.X.ToString(CultureInfo.InvariantCulture),
            coords.Y.ToString(CultureInfo.InvariantCulture),
            coords.Z.ToString(CultureInfo.InvariantCulture));
    }

    private static ActionResponse Kick(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (target == source.Handle)
        {
            return ActionResponse.Refused();
        }

        var reason = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1].Trim() : DefaultKickReason;

        API.Log.Info($"[OnlinePlayers] {source} kicked {target}: {reason}");

        Native.DropPlayer(target.ToString(CultureInfo.InvariantCulture), reason);

        return ActionResponse.Ok();
    }

    private static ActionResponse Kill(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (target == source.Handle)
        {
            return ActionResponse.Refused();
        }

        if (PedOf(target) is null)
        {
            return ActionResponse.NotReady();
        }

        API.Log.Info($"[OnlinePlayers] {source} killed {target}.");

        API.EmitClient(target, PlayerEvents.Kill, source.Name);

        return ActionResponse.Ok();
    }

    /// <summary>
    /// Brings a player to whoever asked.
    /// </summary>
    /// <remarks>
    /// The coordinates go to the target's own client rather than being written here, so it can wait
    /// for the world around them to stream in. Moving somebody from the server drops them through an
    /// unloaded map.
    /// </remarks>
    private static ActionResponse Summon(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (target == source.Handle)
        {
            return ActionResponse.Refused();
        }

        // Somebody still on the loading screen has no character to move, and their client is not
        // listening for this yet either.
        if (PedOf(target) is null)
        {
            return ActionResponse.NotReady();
        }

        var ped = source.PedIndex;

        if (ped <= 0 || !Native.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        var coords = Native.GetEntityCoords(ped);

        API.Log.Info($"[OnlinePlayers] {source} summoned {target}.");

        // Strings, like every other argument that crosses the wire here, so a float never arrives as
        // something the receiving delegate refuses to bind.
        API.EmitClient(
            target,
            PlayerEvents.Teleport,
            source.Name,
            coords.X.ToString(CultureInfo.InvariantCulture),
            coords.Y.ToString(CultureInfo.InvariantCulture),
            coords.Z.ToString(CultureInfo.InvariantCulture));

        return ActionResponse.Ok();
    }

    /// <summary>
    /// Sends a private message, and does not answer the sender until the other player's client says
    /// it put the message on screen.
    /// </summary>
    /// <remarks>
    /// Answering as soon as the server had passed the message along would tell the sender "sent" even
    /// when the other end never showed it, which is the one thing somebody sending a message actually
    /// wants to know.
    /// </remarks>
    private static async Task<ActionResponse> SendMessage(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (target == source.Handle || args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            return ActionResponse.Refused();
        }

        if (PedOf(target) is null)
        {
            return ActionResponse.NotReady();
        }

        var messageId = ++_lastMessageId;
        var delivered = new TaskCompletionSource<bool>();

        Unacknowledged[messageId] = new PendingMessage(target, delivered);

        API.EmitClient(
            target,
            PlayerEvents.Message,
            messageId.ToString(CultureInfo.InvariantCulture),
            source.Name,
            args[1].Trim());

        try
        {
            var timeout = API.Delay(AckTimeoutMs);

            if (await Task.WhenAny(delivered.Task, timeout) == timeout)
            {
                API.Log.Info($"[OnlinePlayers] {source}'s message to {target} was never acknowledged.");

                return ActionResponse.NotReady();
            }
        }
        finally
        {
            Unacknowledged.Remove(messageId);

            // Back onto the thread the reply has to go out on, the awaits above having left it.
            await API.Delay(0);
        }

        return ActionResponse.Ok();
    }

    /// <summary>The other end confirming a message reached the screen.</summary>
    /// <remarks>
    /// A named method rather than a lambda, so the binder finds <see cref="FromSourceAttribute"/> on
    /// it. Without that the <see cref="Player"/> binds to wire argument 0 and the id shifts by one.
    /// </remarks>
    private static void OnMessageAcknowledged([FromSource] Player source, string messageId)
    {
        if (!int.TryParse(messageId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            || !Unacknowledged.TryGetValue(id, out var pending))
        {
            // Either it already timed out, or somebody is making them up. Neither is worth a log line.
            return;
        }

        // Only the player it was sent to can say it arrived, otherwise anyone could confirm delivery
        // of a message they never received.
        if (pending.Target != source.Handle)
        {
            API.Log.Warn($"[OnlinePlayers] {source} acknowledged message {id}, which was sent to {pending.Target}. Ignored.");

            return;
        }

        pending.Delivered.TrySetResult(true);
    }

    /// <summary>
    /// A player's character, or null when they have not got one yet.
    /// </summary>
    // Somebody still connecting already holds a server id, so they answer DoesPlayerExist while
    // having nothing in the world. This is what tells them apart from a player who has left.
    private static int? PedOf(int serverId)
    {
        var ped = Native.GetPlayerPed(serverId.ToString(CultureInfo.InvariantCulture));

        return ped != 0 && Native.DoesEntityExist(ped) ? ped : null;
    }

    /// <summary>One connected player, as the list sees them.</summary>
    // A plain class rather than a record, matching the rest of this codebase: the generated equality
    // routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class ConnectedPlayer(int serverId, string name)
    {
        public int ServerId { get; } = serverId;

        public string Name { get; } = name;
    }

    private sealed class PendingMessage(int target, TaskCompletionSource<bool> delivered)
    {
        public int Target { get; } = target;

        public TaskCompletionSource<bool> Delivered { get; } = delivered;
    }

    /// <summary>Reads a server id out of the arguments and checks that player is still here.</summary>
    private static bool TryResolveTarget(string[] args, out int serverId)
    {
        serverId = 0;

        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out serverId))
        {
            return false;
        }

        return Native.DoesPlayerExist(serverId.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Bumps the revision convar whenever the set of connected players changes.
    /// </summary>
    // Compared as a set rather than counted, so one player leaving as another joins still registers.
    private static void TrackConnectedPlayers()
    {
        var current = new HashSet<int>();

        foreach (var player in ConnectedPlayers())
        {
            current.Add(player.ServerId);
        }

        if (Connected.SetEquals(current))
        {
            return;
        }

        Connected.Clear();
        Connected.UnionWith(current);

        _revision++;

        PublishRevision();
    }

    private static void PublishRevision() =>
        Native.SetConvarReplicated(PlayerEvents.RevisionConvar, _revision.ToString(CultureInfo.InvariantCulture));
}
