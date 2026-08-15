using System.Globalization;
using System.Numerics;
using System.Text;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Permissions;
using vMenu.Enhanced.Data.PlayerState;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Ticks.Server;

using MiscSettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.MiscSettings;

namespace vMenu.Enhanced.Actions.Server.Events;

public static class PlayerPresenceBroadcast
{
    /// <summary>How often this runs. Every interval below is a multiple of it.</summary>
    private const long TickMs = 250;

    /// <summary>Distances, squared, so nothing here needs a square root.</summary>
    private const float NearRangeSquared = 500f * 500f;

    private const float MidRangeSquared = 1500f * 1500f;

    private const float FarRangeSquared = 5000f * 5000f;

    /// <summary>How many ticks apart each tier's updates are.</summary>
    private const int NearEveryTicks = 2;      // 500ms

    private const int MidEveryTicks = 4;       // 1s

    private const int FarEveryTicks = 8;       // 2s

    private const int VeryFarEveryTicks = 20;  // 5s

    /// <summary>Slower Far tier used once the server is busy enough to notice.</summary>
    private const int CrowdedFarEveryTicks = 16; // 4s

    /// <summary>Above this many players, the far tiers start being cut back.</summary>
    private const int CrowdedThreshold = 256;

    /// <summary>
    /// Above this many players, nothing is sent at all and clients blip only what OneSync streams
    /// them.
    /// </summary>
    private const int GiveUpThreshold = 512;

    private static readonly HashSet<int> Subscribers = [];

    private static readonly List<Presence> Snapshot = [];

    private static readonly StringBuilder Payload = new();

    private static bool _registered;

    private static int _tick;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(PresenceEvents.Subscribe, new Action<Player>(OnSubscribe), false);
        API.OnNetEvent(PresenceEvents.Unsubscribe, new Action<Player>(OnUnsubscribe), false);

        ServerTickRegistry.Register("Presence.Broadcast", Broadcast, TickRate.Every(TickMs));
    }

    private static void OnSubscribe([FromSource] Player source)
    {
        if (!ServerPermissions.IsPlayerAllowed(source, MiscSettingsPermissions.PlayerBlips))
        {
            return;
        }

        Subscribers.Add(source.Handle);
    }

    private static void OnUnsubscribe([FromSource] Player source) => Subscribers.Remove(source.Handle);

    private static void Broadcast()
    {
        _tick++;

        // Nothing subscribed and nobody noclipping means there is nothing to send and nothing to
        // forget, which is the state a server with this feature switched off sits in permanently.
        if (Subscribers.Count == 0 && !PlayerNoClipState.HasAny)
        {
            return;
        }

        var players = ConnectedPlayers.All();

        Prune(players);

        if (Subscribers.Count == 0)
        {
            return;
        }

        BuildSnapshot(players);

        if (Snapshot.Count > GiveUpThreshold)
        {
            return;
        }

        var crowded = Snapshot.Count > CrowdedThreshold;

        foreach (var viewer in Snapshot)
        {
            if (!Subscribers.Contains(viewer.ServerId))
            {
                continue;
            }

            SendTo(viewer, crowded);
        }
    }

    /// <summary>
    /// Forgets anybody who has left, everywhere the server was remembering them.
    /// </summary>
    private static void Prune(List<ConnectedPlayer> players)
    {
        var connected = new HashSet<int>(players.Count);

        foreach (var player in players)
        {
            connected.Add(player.ServerId);
        }

        Subscribers.RemoveWhere(serverId => !connected.Contains(serverId));

        PlayerNoClipState.Prune(connected);
    }

    /// <summary>Reads every connected player once, so the per viewer pass costs no natives at all.</summary>
    private static void BuildSnapshot(List<ConnectedPlayer> players)
    {
        Snapshot.Clear();

        foreach (var player in players)
        {
            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);
            var ped = Native.GetPlayerPed(handle);

            // Somebody still on the loading screen holds a server id without having a character yet.
            if (ped == 0 || !Native.DoesEntityExist(ped))
            {
                continue;
            }

            var vehicle = Native.GetVehiclePedIsIn(ped, false);
            var model = vehicle != 0 && Native.DoesEntityExist(vehicle) ? Native.GetEntityModel(vehicle) : 0;

            var flags = 0;

            if (PlayerNoClipState.IsActive(player.ServerId))
            {
                flags |= PresenceRow.FlagNoClip;
            }

            if (Native.GetEntityHealth(ped) <= 0)
            {
                flags |= PresenceRow.FlagDead;
            }

            if (model != 0)
            {
                flags |= PresenceRow.FlagInVehicle;
            }

            // Asked here rather than read from the player's state bag, because a bag only reaches
            // clients that have this player in scope and the whole point of this message is the
            // players who are not. Two ace lookups per player per tick, on top of the handful of
            // natives above, and it costs nothing else: the answer is not cached anywhere, so an
            // add_ace shows up on somebody's blip within a tick instead of on their next reconnect.
            if (ServerPermissions.IsPlayerAllowed(handle, Global.Staff))
            {
                flags |= PresenceRow.FlagStaff;
            }

            Snapshot.Add(new Presence(
                player.ServerId,
                Native.GetEntityCoords(ped),
                (int)Native.GetEntityHeading(ped),
                unchecked((uint)model),
                flags,
                Native.GetPlayerRoutingBucket(handle),
                player.Name));
        }
    }

    private static void SendTo(Presence viewer, bool crowded)
    {
        Payload.Clear();

        foreach (var target in Snapshot)
        {
            if (target.ServerId == viewer.ServerId || target.RoutingBucket != viewer.RoutingBucket)
            {
                continue;
            }

            var interval = IntervalFor(Vector3.DistanceSquared(viewer.Position, target.Position), crowded);

            // Zero means this tier is switched off at the current headcount.
            if (interval == 0)
            {
                continue;
            }

            // The server id is folded in so different players land on different ticks. Without it
            // every player in a tier would be due on the same tick and the payload would arrive in
            // one lump instead of spread across the interval.
            if ((_tick + target.ServerId) % interval != 0)
            {
                continue;
            }

            PresenceRow.Append(
                Payload,
                target.ServerId,
                target.Position.X,
                target.Position.Y,
                target.Position.Z,
                target.Heading,
                target.VehicleModel,
                target.Flags,
                target.Name);
        }

        if (Payload.Length == 0)
        {
            return;
        }

        API.EmitClient(viewer.ServerId, PresenceEvents.Snapshot, Payload.ToString());
    }

    /// <summary>How many ticks apart this target's updates should be, or zero for "do not send".</summary>
    private static int IntervalFor(float distanceSquared, bool crowded)
    {
        if (distanceSquared < NearRangeSquared)
        {
            return NearEveryTicks;
        }

        if (distanceSquared < MidRangeSquared)
        {
            return MidEveryTicks;
        }

        if (distanceSquared < FarRangeSquared)
        {
            return crowded ? CrowdedFarEveryTicks : FarEveryTicks;
        }

        return crowded ? 0 : VeryFarEveryTicks;
    }

    // A plain class rather than a record, matching the rest of this codebase: the generated equality
    // routes through EqualityComparer<string>.Default, which the sandbox refuses to load.
    private sealed class Presence(int serverId, Vector3 position, int heading, uint vehicleModel, int flags, int routingBucket, string name)
    {
        public int ServerId { get; } = serverId;

        public string Name { get; } = name;

        public Vector3 Position { get; } = position;

        public int Heading { get; } = heading;

        public uint VehicleModel { get; } = vehicleModel;

        public int Flags { get; } = flags;

        /// <summary>Buckets are separate worlds, so players in one never appear on another's map.</summary>
        public int RoutingBucket { get; } = routingBucket;
    }
}
