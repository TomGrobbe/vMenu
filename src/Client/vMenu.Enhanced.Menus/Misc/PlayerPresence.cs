using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using MiscSettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.MiscSettings;

namespace vMenu.Enhanced.Menus.Misc;

/// <summary>Everything the blips and the name tags need to know about one player this pass.</summary>
public sealed class PresenceView(
    int serverId,
    int slot,
    int ped,
    Vector3 position,
    int heading,
    uint vehicleModel,
    bool noClip,
    bool dead,
    bool staff,
    string name)
{
    public int ServerId { get; } = serverId;

    /// <summary>Only needed for players the game has not got, whose name it cannot look up itself.</summary>
    public string Name { get; } = name;

    /// <summary>The local player index, or -1 when this player is not streamed in.</summary>
    public int Slot { get; } = slot;

    /// <summary>Their character, or zero when they are not streamed in.</summary>
    public int Ped { get; } = ped;

    public Vector3 Position { get; } = position;

    public int Heading { get; } = heading;

    /// <summary>What they are driving, or zero on foot.</summary>
    public uint VehicleModel { get; } = vehicleModel;

    public bool NoClip { get; } = noClip;

    public bool Dead { get; } = dead;

    /// <summary>Whether this player is staff, which is what marks their name and blip out in orange.</summary>
    public bool IsStaff { get; } = staff;

    /// <summary>Whether the game has this player loaded, which decides almost everything else.</summary>
    public bool IsStreamed => Ped != 0;

    /// <summary>Whether they should be shown at all.</summary>
    // Noclip is what hides somebody, and the permission below is what lifts that for the person
    // looking. Asked per player rather than worked out once a pass because the answer is a dictionary
    // lookup behind a cache, and a pass only ever looks at a slice of the server.
    public bool IsHidden => NoClip && !PlayerPresence.SeesHiddenPlayers;
}

/// <summary>
/// The one loop behind player blips and overhead names, and the store of where everybody is.
/// </summary>
public static class PlayerPresence
{
    /// <summary>How often the loop runs.</summary>
    private const long TickMs = 250;

    /// <summary>How much of the tracked set one pass gets through.</summary>
    // Four passes to cover everybody, so a player's blip is never more than a second out of date.
    private const int PassesPerSweep = 4;

    /// <summary>The fewest players a pass will take on, so a quiet server still keeps up.</summary>
    private const int MinimumSlice = 8;

    /// <summary>How long a server update is trusted for before the player is treated as gone.</summary>
    // A time to live rather than a "player left" message, because it heals itself. A dropped message
    // cannot leave a blip stuck on the map forever, which is exactly the failure worth designing out.
    private const int StaleMs = 15_000;

    private static readonly Dictionary<int, PresenceEntry> Remote = [];

    private static readonly Dictionary<int, int> ReceivedAt = [];

    /// <summary>Every player either feature might have something on screen for.</summary>
    private static readonly List<int> Tracked = [];

    private static readonly List<PresenceView> Slice = [];

    private static TickHandle? _tick;

    private static int _cursor;

    private static bool _subscribed;

    /// <summary>Whether the server is currently sending us anybody's position.</summary>
    public static bool IsSubscribed => _subscribed;

    public static void Initialize()
    {
        API.OnNetEvent(PresenceEvents.Snapshot, new Action<string>(OnSnapshot), false);

        _tick = TickRegistry.Register("Player.Presence", Pass, TickRate.Every(TickMs), Wanted, onStopped: Teardown);

        OverheadNames.Initialize();

        ClientPermissions.PermissionsChanged += Reevaluate;

        // Blips and name tags outlive the code that made them, so stopping the resource without
        // this leaves them on screen with nothing left running to ever take them off again.
        ResourceShutdown.Stopping += Teardown;
    }

    /// <summary>Starts or stops the loop, and tells the server whether to keep sending positions.</summary>
    public static void Reevaluate()
    {
        _tick?.Reevaluate();

        UpdateSubscription();
    }

    /// <summary>Where a player was last reported to be, for anything that needs it off the loop.</summary>
    public static bool TryGetRemote(int serverId, out PresenceEntry entry) => Remote.TryGetValue(serverId, out entry!);

    /// <summary>Takes a snapshot as if the server had sent it, for <see cref="PlayerBlipsDebugCommands"/>.</summary>
    // The test command goes in through the front door on purpose: anything that let it skip the
    // parsing or the staleness rules would be testing a shorter version of the code than ships.
    internal static void InjectSnapshot(string snapshot) => OnSnapshot(snapshot);

    /// <summary>Forgets one player immediately, rather than waiting out the time to live.</summary>
    internal static void DropRemote(int serverId)
    {
        Remote.Remove(serverId);
        ReceivedAt.Remove(serverId);
    }

    private static bool Wanted() => BlipsWanted || NamesWanted;

    internal static bool BlipsWanted =>
        UserDefaults.MiscShowPlayerBlips.Value && ClientPermissions.IsAllowed(MiscSettingsPermissions.PlayerBlips);

    private static bool NamesWanted =>
        UserDefaults.MiscShowOverheadNames.Value && ClientPermissions.IsAllowed(MiscSettingsPermissions.OverheadNames);

    /// <summary>Whether people who are noclipping should be shown to this player rather than hidden.</summary>
    // Reads like the two above, but it decides nothing about whether either feature runs. Both loops
    // start and stop on their own preferences alone, and this only changes who they draw once they
    // are running, which is why Wanted below does not consult it.
    internal static bool SeesHiddenPlayers =>
        UserDefaults.MiscSeeNoClipPlayers.Value && ClientPermissions.IsAllowed(MiscSettingsPermissions.SeeNoClipPlayers);

    /// <summary>
    /// Asks the server for other players' positions, or tells it to stop.
    /// </summary>
    // Opt in, so a server where nobody has blips switched on sends nothing at all. Only the blips
    // want it: name tags are drawn on peds, and a ped you have not got is a ped you cannot label.
    private static void UpdateSubscription()
    {
        var wanted = BlipsWanted;

        if (wanted == _subscribed)
        {
            return;
        }

        _subscribed = wanted;

        API.EmitServer(wanted ? PresenceEvents.Subscribe : PresenceEvents.Unsubscribe);

        if (!wanted)
        {
            Remote.Clear();
            ReceivedAt.Clear();
        }
    }

    private static void OnSnapshot(string payload)
    {
        var now = Native.GetGameTimer();

        foreach (var entry in PresenceRow.Parse(payload))
        {
            Remote[entry.ServerId] = entry;
            ReceivedAt[entry.ServerId] = now;
        }
    }

    private static void Pass()
    {
        PlayerRoster.Refresh();

        RebuildTracked();

        BuildSlice();

        // Called even with nobody left to look at, because both of these also clear up after
        // players who have gone, and an empty server is exactly when there is most to clear up.
        PlayerBlips.Apply(Slice, BlipsWanted);
        OverheadNames.Apply(Slice, NamesWanted);
    }

    /// <summary>Everybody worth looking at: streamed in, or recently reported by the server.</summary>
    private static void RebuildTracked()
    {
        Tracked.Clear();

        var now = Native.GetGameTimer();
        var self = Native.GetPlayerServerId(Native.PlayerId());

        foreach (var player in PlayerRoster.All)
        {
            if (player.ServerId != self)
            {
                Tracked.Add(player.ServerId);
            }
        }

        foreach (var serverId in Remote.Keys)
        {
            // Streamed players are already in, from the roster, where the numbers are live rather
            // than however old the last server update happens to be.
            if (serverId == self || PlayerRoster.IsStreamed(serverId))
            {
                continue;
            }

            // Looked up rather than defaulted. Defaulting to a value far in the past and subtracting
            // it from the game clock overflows into a negative number, which reads as "heard from
            // them a moment ago" and would keep a player nobody has reported on the map for good.
            if (ReceivedAt.TryGetValue(serverId, out var receivedAt) && now - receivedAt < StaleMs)
            {
                Tracked.Add(serverId);
            }
        }

        Forget(now);
    }

    /// <summary>Drops server updates nobody is going to trust again.</summary>
    private static void Forget(int now)
    {
        if (Remote.Count == 0)
        {
            return;
        }

        List<int>? expired = null;

        foreach (var pair in ReceivedAt)
        {
            if (now - pair.Value >= StaleMs)
            {
                (expired ??= []).Add(pair.Key);
            }
        }

        if (expired is null)
        {
            return;
        }

        foreach (var serverId in expired)
        {
            Remote.Remove(serverId);
            ReceivedAt.Remove(serverId);
        }
    }

    /// <summary>Reads the live state of this pass's share of the tracked players.</summary>
    private static void BuildSlice()
    {
        Slice.Clear();

        if (Tracked.Count == 0)
        {
            _cursor = 0;

            return;
        }

        var size = Math.Min(Tracked.Count, Math.Max(MinimumSlice, (Tracked.Count + PassesPerSweep - 1) / PassesPerSweep));

        if (_cursor >= Tracked.Count)
        {
            _cursor = 0;
        }

        for (var taken = 0; taken < size; taken++)
        {
            var serverId = Tracked[(_cursor + taken) % Tracked.Count];

            Slice.Add(View(serverId));
        }

        _cursor = (_cursor + size) % Tracked.Count;
    }

    /// <summary>
    /// What we know about one player, preferring the game over the server whenever it has an answer.
    /// </summary>
    // A streamed player is read live: the ped is right there and its position is exact, where a
    // server update is by definition a snapshot of a moment that has already passed.
    private static PresenceView View(int serverId)
    {
        if (PlayerRoster.TryGet(serverId, out var streamed))
        {
            var vehicle = Native.GetVehiclePedIsIn(streamed.Ped, false);

            return new PresenceView(
                serverId,
                streamed.Slot,
                streamed.Ped,
                streamed.Position,
                (int)Native.GetEntityHeading(streamed.Ped),
                vehicle != 0 ? unchecked((uint)Native.GetEntityModel(vehicle)) : 0,
                StateBags.GetPlayer<bool>(serverId, PlayerStateKeys.NoClip),
                Native.IsPlayerDead(streamed.Slot),
                StateBags.GetPlayer<bool>(serverId, PlayerStateKeys.Staff),

                // Empty on purpose. The game knows who owns a streamed player's slot, so anything
                // that wants their name asks it rather than trusting a copy that could be stale.
                string.Empty);
        }

        var entry = Remote[serverId];

        return new PresenceView(
            serverId,
            -1,
            0,
            new Vector3(entry.X, entry.Y, entry.Z),
            entry.Heading,
            entry.VehicleModel,
            entry.IsNoClipping,
            entry.IsDead,

            // From the row rather than the bag, unlike the streamed branch above. The game only
            // replicates a player's state bag to clients that have them in scope, so for somebody
            // this far away the staff key is simply not here to read: it disappears the moment they
            // leave OneSync range and takes the marking on their blip with it.
            entry.IsStaff,
            entry.Name);
    }

    private static void Teardown()
    {
        PlayerBlips.RemoveAll();
        OverheadNames.RemoveAll();

        _cursor = 0;
    }
}
