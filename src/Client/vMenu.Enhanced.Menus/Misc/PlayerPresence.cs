using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Players;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;
using MiscSettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.MiscSettings;

namespace vMenu.Enhanced.Menus.Misc;

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

    // Only needed for players the game has not got, whose name it cannot look up itself.
    public string Name { get; } = name;

    public int Slot { get; } = slot;

    public int Ped { get; } = ped;

    public Vector3 Position { get; } = position;

    public int Heading { get; } = heading;

    public uint VehicleModel { get; } = vehicleModel;

    public bool NoClip { get; } = noClip;

    public bool Dead { get; } = dead;

    public bool IsStaff { get; } = staff;

    public bool IsStreamed => Ped != 0;

    // Noclip is what hides somebody, and the permission is what lifts that for the person looking.
    public bool IsHidden => NoClip && !PlayerPresence.SeesHiddenPlayers;
}

public static class PlayerPresence
{
    private const long TickMs = 250;

    // Four passes to cover everybody, so a player's blip is never more than a second out of date.
    private const int PassesPerSweep = 4;

    private const int MinimumSlice = 8;

    // A time to live rather than a "player left" message, because it heals itself. A dropped message
    // cannot leave a blip stuck on the map forever, which is exactly the failure worth designing out.
    private const int StaleMs = 15_000;

    private static readonly Dictionary<int, PresenceEntry> Remote = [];

    private static readonly Dictionary<int, int> ReceivedAt = [];

    private static readonly List<int> Tracked = [];

    private static readonly List<PresenceView> Slice = [];

    private static TickHandle? _tick;

    private static int _cursor;

    private static bool _subscribed;

    public static bool IsSubscribed => _subscribed;

    public static void Initialize()
    {
        API.OnNetEvent(PresenceEvents.Snapshot, new Action<string>(OnSnapshot), false);

        _tick = TickRegistry.Register("Player.Presence", Pass, TickRate.Every(TickMs), Wanted, onStopped: Teardown);

        OverheadNames.Initialize();

        ClientPermissions.PermissionsChanged += Reevaluate;

        // Blips and name tags outlive the code that made them, so stopping the resource without this leaves
        // them on screen with nothing left running to ever take them off again.
        ResourceShutdown.Stopping += Teardown;
    }

    public static void Reevaluate()
    {
        _tick?.Reevaluate();

        UpdateSubscription();
    }

    public static bool TryGetRemote(int serverId, out PresenceEntry entry) => Remote.TryGetValue(serverId, out entry!);

    // The test command goes in through the front door on purpose: anything that let it skip the parsing
    // or the staleness rules would be testing a shorter version of the code than ships.
    internal static void InjectSnapshot(string snapshot) => OnSnapshot(snapshot);

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

    // This decides nothing about whether either feature runs. Both loops start and stop on their own
    // preferences alone, and this only changes who they draw once they are running.
    internal static bool SeesHiddenPlayers =>
        UserDefaults.AdminSeeNoClipPlayers.Value && ClientPermissions.IsAllowed(AdminPermissions.SeeNoClipPlayers);

    // Opt in, so a server where nobody has blips switched on sends nothing at all. Only the blips want
    // it: name tags are drawn on peds, and a ped you have not got is a ped you cannot label.
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

        // Called even with nobody left to look at, because both of these also clear up after players who
        // have gone, and an empty server is exactly when there is most to clear up.
        PlayerBlips.Apply(Slice, BlipsWanted);
        OverheadNames.Apply(Slice, NamesWanted);
    }

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
            // Streamed players are already in, from the roster, where the numbers are live rather than however
            // old the last server update happens to be.
            if (serverId == self || PlayerRoster.IsStreamed(serverId))
            {
                continue;
            }

            // Looked up rather than defaulted. Defaulting to a value far in the past and subtracting it from the
            // game clock overflows into a negative number, which reads as "heard from them a moment ago" and
            // would keep a player nobody has reported on the map for good.
            if (ReceivedAt.TryGetValue(serverId, out var receivedAt) && now - receivedAt < StaleMs)
            {
                Tracked.Add(serverId);
            }
        }

        Forget(now);
    }

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

    // A streamed player is read live: the ped is right there and its position is exact, where a server
    // update is by definition a snapshot of a moment that has already passed.
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

                // Empty on purpose. The game knows who owns a streamed player's slot, so anything that wants their
                // name asks it rather than trusting a copy that could be stale.
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

            // From the row rather than the bag, unlike the streamed branch above. The game only replicates a
            // player's state bag to clients that have them in scope, so for somebody this far away the staff key
            // is simply not here to read.
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
