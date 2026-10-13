using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Admin;
using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;
using AdminSettings = vMenu.Enhanced.Data.Configuration.Settings.Admin;
using OnlinePlayerSettings = vMenu.Enhanced.Data.Configuration.Settings.OnlinePlayers;
using VehicleOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleOptions;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives.Server;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class AdminActions
{
    private const string On = "1";

    private const string Off = "0";

    private const string DroppedEvent = "playerDropped";

    private const float JitterSpacing = 1.5f;

    private static readonly Random JitterRng = new();

    private const int VehicleEntityType = 2;

    private const int FirstRandomPopulation = 1;

    private const int LastRandomPopulation = 5;

    private const float RangeSlack = 10f;

    private const int DefaultRoutingBucket = 0;

    private static readonly ActionRateLimit Limit = new(
        "admin action",
        OnlinePlayerSettings.ActionLimit,
        OnlinePlayerSettings.ActionLimitSeconds);

    private static readonly HashSet<int> Frozen = [];

    private static readonly Dictionary<int, int> Held = [];

    private static bool _reportedRadius;

    private static bool _registered;

    public static void Register()
    {
        ActionRegistry.Register(ActionIds.Admin.ClearArea, AdminPermissions.ClearArea, ClearArea, Limit);
        ActionRegistry.Register(ActionIds.Admin.SetFrozen, AdminPermissions.FreezePlayer, SetFrozen, Limit);
        ActionRegistry.Register(ActionIds.Admin.SetHeld, AdminPermissions.GrabPlayer, SetHeld, Limit);
        ActionRegistry.Register(ActionIds.Admin.DeleteVehicle, AdminPermissions.DeleteVehicle, DeleteVehicle);
        ActionRegistry.Register(ActionIds.Admin.DeleteEmptyVehicles, AdminPermissions.DeleteEmptyVehicles, DeleteEmpty, Limit);
        ActionRegistry.Register(ActionIds.Admin.DeleteAllVehicles, AdminPermissions.DeleteAllVehicles, DeleteEverything, Limit);
        ActionRegistry.Register(ActionIds.Admin.Announce, AdminPermissions.Announce, Announce, Limit);
        ActionRegistry.Register(ActionIds.Admin.RefreshPermissions, AdminPermissions.RefreshPermissions, RefreshPermissions, Limit);

        ActionRegistry.Register(
            ActionIds.Admin.ResetRoutingBucket,
            AdminPermissions.ResetRoutingBucket,
            ResetRoutingBucket,
            Limit);

        if (_registered)
        {
            return;
        }

        _registered = true;

        Script.EventHandlers[DroppedEvent] += new Action<string, string?>(OnPlayerDropped);
    }

    private static ActionResponse ClearArea(Player source, string[] args)
    {
        var ped = API.GetPlayerPed(source.Handle);

        if (ped <= 0 || !API.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        var position = API.GetEntityCoords(ped);
        var bucket = API.GetPlayerRoutingBucket(Handle(source.ServerId()));
        var radius = Radius();
        var cleared = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            if (API.GetPlayerRoutingBucket(Handle(player.ServerId)) != bucket)
            {
                continue;
            }

            Script.TriggerClientEvent(player.ServerId, AdminEvents.ClearArea, position.X, position.Y, position.Z, radius);

            cleared++;
        }

        Log.Debug($"[Admin] {source.Name} cleared {radius}m around themselves for {cleared} player(s).");

        return ActionResponse.Ok();
    }

    private static ActionResponse SetFrozen(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (PedOf(target) is not { } targetPed)
        {
            return ActionResponse.NotReady();
        }

        if (!WithinReach(source, targetPed))
        {
            return ActionResponse.TooFar();
        }

        var frozen = !Frozen.Remove(target);

        if (frozen)
        {
            Frozen.Add(target);
        }

        Script.TriggerClientEvent(target, AdminEvents.Freeze, frozen ? On : Off);

        var name = NameOf(target);

        Log.Info($"[Admin] {source.Name} {(frozen ? "froze" : "unfroze")} {name}.");

        return ActionResponse.Ok(frozen ? On : Off, name);
    }

    private static ActionResponse SetHeld(Player source, string[] args)
    {
        if (!TryResolveTarget(args, out var target))
        {
            return ActionResponse.NotFound();
        }

        if (PedOf(target) is not { } targetPed)
        {
            return ActionResponse.NotReady();
        }

        if (target == source.ServerId())
        {
            return ActionResponse.InvalidRequest();
        }

        var name = NameOf(target);

        if (Held.TryGetValue(target, out var holder))
        {
            if (holder != source.ServerId())
            {
                return ActionResponse.Refused();
            }

            Held.Remove(target);

            Script.TriggerClientEvent(target, AdminEvents.Hold, Off);

            Log.Info($"[Admin] {source.Name} put {name} down.");

            return ActionResponse.Ok(Off, name);
        }

        if (Carrying(source.ServerId()).Count > 0)
        {
            return ActionResponse.Refused();
        }

        if (!WithinReach(source, targetPed))
        {
            return ActionResponse.TooFar();
        }

        Held[target] = source.ServerId();

        Script.TriggerClientEvent(target, AdminEvents.Hold, Handle(source.ServerId()));

        Log.Info($"[Admin] {source.Name} picked {name} up.");

        return ActionResponse.Ok(On, name);
    }

    private static ActionResponse DeleteVehicle(Player source, string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var networkId))
        {
            return ActionResponse.InvalidRequest();
        }

        var entity = API.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0 || !API.DoesEntityExist(entity))
        {
            return ActionResponse.NotFound();
        }

        if (API.GetEntityType(entity) != VehicleEntityType)
        {
            return ActionResponse.InvalidRequest();
        }

        var ped = API.GetPlayerPed(source.Handle);

        if (ped <= 0 || !API.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        if (API.GetVehiclePedIsIn(ped, false) == entity)
        {
            return Delete(entity);
        }

        var reach = ServerConfig.Value(VehicleOptionsSettings.DeleteVehicleDistance) + RangeSlack;

        if (Vector3.DistanceSquared(API.GetEntityCoords(ped), API.GetEntityCoords(entity)) > reach * reach)
        {
            Log.Warning($"[Admin] {source} asked to delete a vehicle further than {reach}m away. Refused.");

            return ActionResponse.TooFar();
        }

        return Delete(entity);
    }

    private static ActionResponse DeleteEmpty(Player source, string[] args) => Wipe(source, occupiedToo: false);

    private static ActionResponse DeleteEverything(Player source, string[] args) => Wipe(source, occupiedToo: true);

    private static ActionResponse Wipe(Player source, bool occupiedToo)
    {
        var occupied = occupiedToo ? [] : OccupiedVehicles();
        var deleted = 0;
        var spared = 0;

        foreach (var entity in NativeFixer.GetAllVehicles())
        {
            if (!API.DoesEntityExist(entity))
            {
                continue;
            }

            if (!occupiedToo && (occupied.Contains(entity) || IsWorldTraffic(entity)))
            {
                spared++;

                continue;
            }

            NativeFixer.DeleteEntity(entity);

            deleted++;
        }

        if (occupiedToo && deleted > 0)
        {
            SpawnedVehicleRegistry.ForgetAll();
            PersonalVehicleRegistry.ForgetAll();
        }

        Log.Info(
            $"[Admin] {source.Name} deleted {deleted} "
            + (occupiedToo ? "vehicle(s)." : $"empty vehicle(s), sparing {spared} in use or belonging to the world."));

        return ActionResponse.Ok(deleted.ToString(CultureInfo.InvariantCulture));
    }

    private static bool IsWorldTraffic(int entity)
    {
        var population = API.GetEntityPopulationType(entity);

        return population is >= FirstRandomPopulation and <= LastRandomPopulation;
    }

    private static HashSet<int> OccupiedVehicles()
    {
        var occupied = new HashSet<int>();

        foreach (var ped in NativeFixer.GetAllPeds())
        {
            if (!API.DoesEntityExist(ped))
            {
                continue;
            }

            var vehicle = API.GetVehiclePedIsIn(ped, false);

            if (vehicle != 0)
            {
                occupied.Add(vehicle);
            }
        }

        // A player ped the pool has not caught up with yet must never have their car deleted under them.
        foreach (var player in ConnectedPlayers.All())
        {
            var ped = API.GetPlayerPed(Handle(player.ServerId));

            if (ped == 0 || !API.DoesEntityExist(ped))
            {
                continue;
            }

            var vehicle = API.GetVehiclePedIsIn(ped, false);

            if (vehicle != 0)
            {
                occupied.Add(vehicle);
            }
        }

        return occupied;
    }

    private static ActionResponse Announce(Player source, string[] args)
    {
        if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            return ActionResponse.InvalidRequest();
        }

        var text = args[0].Trim();
        var reached = Broadcast(text);

        Log.Info($"[Admin] {source.Name} announced to {reached} player(s): {text}");

        return ActionResponse.Ok(reached.ToString(CultureInfo.InvariantCulture));
    }

    public static int Broadcast(string text)
    {
        var reached = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            Script.TriggerClientEvent(player.ServerId, AdminEvents.Announce, text);

            reached++;
        }

        return reached;
    }

    public static int BroadcastWaypoint(float x, float y)
    {
        var reached = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            Script.TriggerClientEvent(player.ServerId, PlayerEvents.SetWaypoint, Coord(x), Coord(y));

            reached++;
        }

        return reached;
    }

    public static int BroadcastTeleport(float x, float y)
    {
        var players = ConnectedPlayers.All().ToList();

        var radius = JitterSpacing * MathF.Sqrt(Math.Max(0, players.Count - 1));

        var reached = 0;

        foreach (var player in players)
        {
            var (jx, jy) = Jitter(x, y, radius);

            Script.TriggerClientEvent(player.ServerId, PlayerEvents.TeleportToGround, Coord(jx), Coord(jy));

            reached++;
        }

        return reached;
    }

    // Prevents everyone being teleported into the exact same spot
    private static (float x, float y) Jitter(float x, float y, float radius)
    {
        if (radius <= 0f)
        {
            return (x, y);
        }

        var angle = JitterRng.NextDouble() * Math.PI * 2.0;

        var distance = radius * Math.Sqrt(JitterRng.NextDouble());

        return (x + (float)(Math.Cos(angle) * distance), y + (float)(Math.Sin(angle) * distance));
    }

    private static string Coord(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static ActionResponse RefreshPermissions(Player source, string[] args)
    {
        var refreshed = PermissionsSync.RefreshAll();

        if (refreshed < 0)
        {
            return ActionResponse.Failed();
        }

        Log.Info($"[Admin] {source.Name} refreshed permissions for {refreshed} player(s).");

        return ActionResponse.Ok(refreshed.ToString(CultureInfo.InvariantCulture));
    }

    // Answers with the bucket they were in, so the client can tell "moved back" from "already there".
    private static ActionResponse ResetRoutingBucket(Player source, string[] args)
    {
        var current = API.GetPlayerRoutingBucket(Handle(source.ServerId()));

        if (current == DefaultRoutingBucket)
        {
            return ActionResponse.Ok(DefaultRoutingBucket.ToString(CultureInfo.InvariantCulture));
        }

        NativeFixer.SetPlayerRoutingBucket(Handle(source.ServerId()), DefaultRoutingBucket);

        Log.Info($"[Admin] {source.Name} moved themselves out of routing bucket {current}.");

        return ActionResponse.Ok(current.ToString(CultureInfo.InvariantCulture));
    }

    // Never verify: the delete only lands next tick, so DoesEntityExist still sees the vehicle here.
    private static ActionResponse Delete(int entity)
    {
        NativeFixer.DeleteEntity(entity);

        return ActionResponse.Ok();
    }

    private static void OnPlayerDropped([FromSource] string sourceString, string? reason = null)
    {
        if (Script.IsFromPlayer(sourceString))
        {
            return;
        }

        var source = Script.SourceId(sourceString);

        if (source <= 0)
        {
            return;
        }

        Frozen.Remove(source);

        if (Held.Remove(source, out var holder))
        {
            Script.TriggerClientEvent(holder, AdminEvents.HoldEnded);
        }

        foreach (var carried in Carrying(source))
        {
            Held.Remove(carried);

            Script.TriggerClientEvent(carried, AdminEvents.Hold, Off);
        }
    }

    private static List<int> Carrying(int holder)
    {
        var carried = new List<int>();

        foreach (var pair in Held)
        {
            if (pair.Value == holder)
            {
                carried.Add(pair.Key);
            }
        }

        return carried;
    }

    private static float Radius()
    {
        var configured = ServerConfig.Value(AdminSettings.ClearAreaRadius);
        var clamped = AdminSettings.ClampClearAreaRadius(configured);

        if (clamped != configured && !_reportedRadius)
        {
            _reportedRadius = true;

            Log.Warning(
                $"{AdminSettings.ClearAreaRadius.Name} is set to {configured}, which is outside "
                + $"{AdminSettings.MinClearAreaRadius} to {AdminSettings.MaxClearAreaRadius}. Using {clamped}.");
        }

        return clamped;
    }

    private static bool WithinReach(Player source, int targetPed)
    {
        var ped = API.GetPlayerPed(source.Handle);

        if (ped <= 0 || !API.DoesEntityExist(ped))
        {
            return false;
        }

        var reach = AdminSettings.ClampClosestPlayerRange(ServerConfig.Value(AdminSettings.ClosestPlayerRange))
            + RangeSlack;

        return Vector3.DistanceSquared(API.GetEntityCoords(ped), API.GetEntityCoords(targetPed))
            <= reach * reach;
    }

    private static bool TryResolveTarget(string[] args, out int serverId)
    {
        serverId = 0;

        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out serverId))
        {
            return false;
        }

        return API.DoesPlayerExist(Handle(serverId));
    }

    private static int? PedOf(int serverId)
    {
        var ped = API.GetPlayerPed(Handle(serverId));

        return ped != 0 && API.DoesEntityExist(ped) ? ped : null;
    }

    private static string NameOf(int serverId)
    {
        var name = API.GetPlayerName(Handle(serverId));

        return string.IsNullOrWhiteSpace(name) ? $"#{serverId}" : name;
    }

    private static string Handle(int serverId) => serverId.ToString(CultureInfo.InvariantCulture);
}
