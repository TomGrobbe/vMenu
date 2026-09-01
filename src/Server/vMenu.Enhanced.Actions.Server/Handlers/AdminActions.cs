using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.Admin;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;
using AdminSettings = vMenu.Enhanced.Data.Configuration.Settings.Admin;
using OnlinePlayerSettings = vMenu.Enhanced.Data.Configuration.Settings.OnlinePlayers;
using VehicleOptionsSettings = vMenu.Enhanced.Data.Configuration.Settings.VehicleOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class AdminActions
{
    private const string On = "1";

    private const string Off = "0";

    private const string DroppedEvent = "playerDropped";

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

        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);
    }

    private static ActionResponse ClearArea(Player source, string[] args)
    {
        var ped = source.PedIndex;

        if (ped <= 0 || !Native.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        var position = Native.GetEntityCoords(ped);
        var bucket = Native.GetPlayerRoutingBucket(Handle(source.Handle));
        var radius = Radius();
        var cleared = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            if (Native.GetPlayerRoutingBucket(Handle(player.ServerId)) != bucket)
            {
                continue;
            }

            API.EmitClient(player.ServerId, AdminEvents.ClearArea, position.X, position.Y, position.Z, radius);

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

        API.EmitClient(target, AdminEvents.Freeze, frozen ? On : Off);

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

        if (target == source.Handle)
        {
            return ActionResponse.InvalidRequest();
        }

        var name = NameOf(target);

        if (Held.TryGetValue(target, out var holder))
        {
            if (holder != source.Handle)
            {
                return ActionResponse.Refused();
            }

            Held.Remove(target);

            API.EmitClient(target, AdminEvents.Hold, Off);

            Log.Info($"[Admin] {source.Name} put {name} down.");

            return ActionResponse.Ok(Off, name);
        }

        if (Carrying(source.Handle).Count > 0)
        {
            return ActionResponse.Refused();
        }

        if (!WithinReach(source, targetPed))
        {
            return ActionResponse.TooFar();
        }

        Held[target] = source.Handle;

        API.EmitClient(target, AdminEvents.Hold, Handle(source.Handle));

        Log.Info($"[Admin] {source.Name} picked {name} up.");

        return ActionResponse.Ok(On, name);
    }

    private static ActionResponse DeleteVehicle(Player source, string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var networkId))
        {
            return ActionResponse.InvalidRequest();
        }

        var entity = Native.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0 || !Native.DoesEntityExist(entity))
        {
            return ActionResponse.NotFound();
        }

        if (Native.GetEntityType(entity) != VehicleEntityType)
        {
            return ActionResponse.InvalidRequest();
        }

        var ped = source.PedIndex;

        if (ped <= 0 || !Native.DoesEntityExist(ped))
        {
            return ActionResponse.NotFound();
        }

        if (Native.GetVehiclePedIsIn(ped, false) == entity)
        {
            return Delete(entity);
        }

        var reach = ServerConfig.Value(VehicleOptionsSettings.DeleteVehicleDistance) + RangeSlack;

        if (Vector3.DistanceSquared(Native.GetEntityCoords(ped), Native.GetEntityCoords(entity)) > reach * reach)
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

        foreach (var entity in Native.GetAllVehicles())
        {
            if (!Native.DoesEntityExist(entity))
            {
                continue;
            }

            if (!occupiedToo && (occupied.Contains(entity) || IsWorldTraffic(entity)))
            {
                spared++;

                continue;
            }

            Native.DeleteEntity(entity);

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
        var population = Native.GetEntityPopulationType(entity);

        return population is >= FirstRandomPopulation and <= LastRandomPopulation;
    }

    private static HashSet<int> OccupiedVehicles()
    {
        var occupied = new HashSet<int>();

        foreach (var ped in Native.GetAllPeds())
        {
            if (!Native.DoesEntityExist(ped))
            {
                continue;
            }

            var vehicle = Native.GetVehiclePedIsIn(ped, false);

            if (vehicle != 0)
            {
                occupied.Add(vehicle);
            }
        }

        // Not redundant: a player ped the ped pool has not caught up with yet would have the car deleted
        // out from under them, which is the one outcome this must never produce.
        foreach (var player in ConnectedPlayers.All())
        {
            var ped = Native.GetPlayerPed(Handle(player.ServerId));

            if (ped == 0 || !Native.DoesEntityExist(ped))
            {
                continue;
            }

            var vehicle = Native.GetVehiclePedIsIn(ped, false);

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

    internal static int Broadcast(string text)
    {
        var reached = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            API.EmitClient(player.ServerId, AdminEvents.Announce, text);

            reached++;
        }

        return reached;
    }

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

    // Answers with the bucket the player was in, not the one they end up in, so the client can tell
    // "you have been moved back" from "you were already there" without asking a second time.
    private static ActionResponse ResetRoutingBucket(Player source, string[] args)
    {
        var current = Native.GetPlayerRoutingBucket(Handle(source.Handle));

        if (current == DefaultRoutingBucket)
        {
            return ActionResponse.Ok(DefaultRoutingBucket.ToString(CultureInfo.InvariantCulture));
        }

        Native.SetPlayerRoutingBucket(Handle(source.Handle), DefaultRoutingBucket);

        Log.Info($"[Admin] {source.Name} moved themselves out of routing bucket {current}.");

        return ActionResponse.Ok(current.ToString(CultureInfo.InvariantCulture));
    }

    // Never verify this. The removal only lands on the next server tick, so DoesEntityExist still
    // reports the vehicle here and every successful delete would answer as a failure.
    private static ActionResponse Delete(int entity)
    {
        Native.DeleteEntity(entity);

        return ActionResponse.Ok();
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source <= 0)
        {
            return;
        }

        Frozen.Remove(source);

        if (Held.Remove(source, out var holder))
        {
            API.EmitClient(holder, AdminEvents.HoldEnded);
        }

        foreach (var carried in Carrying(source))
        {
            Held.Remove(carried);

            API.EmitClient(carried, AdminEvents.Hold, Off);
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
        var ped = source.PedIndex;

        if (ped <= 0 || !Native.DoesEntityExist(ped))
        {
            return false;
        }

        var reach = AdminSettings.ClampClosestPlayerRange(ServerConfig.Value(AdminSettings.ClosestPlayerRange))
            + RangeSlack;

        return Vector3.DistanceSquared(Native.GetEntityCoords(ped), Native.GetEntityCoords(targetPed))
            <= reach * reach;
    }

    private static bool TryResolveTarget(string[] args, out int serverId)
    {
        serverId = 0;

        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out serverId))
        {
            return false;
        }

        return Native.DoesPlayerExist(Handle(serverId));
    }

    private static int? PedOf(int serverId)
    {
        var ped = Native.GetPlayerPed(Handle(serverId));

        return ped != 0 && Native.DoesEntityExist(ped) ? ped : null;
    }

    private static string NameOf(int serverId)
    {
        var name = Native.GetPlayerName(Handle(serverId));

        return string.IsNullOrWhiteSpace(name) ? $"#{serverId}" : name;
    }

    private static string Handle(int serverId) => serverId.ToString(CultureInfo.InvariantCulture);
}
