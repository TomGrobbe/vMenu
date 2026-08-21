using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;

using PersonalVehiclePermissions = vMenu.Enhanced.Data.Permissions.Menus.PersonalVehicle;
using PersonalVehicleSettings = vMenu.Enhanced.Data.Configuration.Settings.PersonalVehicle;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class PersonalVehicleActions
{
    private const int VehicleEntityType = 2;

    private const string DroppedEvent = "playerDropped";

    private static readonly ActionRateLimit Limit = new(
        "personal vehicle",
        PersonalVehicleSettings.ActionLimit,
        PersonalVehicleSettings.ActionLimitSeconds);

    public static void Register()
    {
        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);

        ActionRegistry.Register(ActionIds.PersonalVehicle.Set, PersonalVehiclePermissions.Menu, Set, Limit);
        ActionRegistry.Register(ActionIds.PersonalVehicle.Forget, PersonalVehiclePermissions.Menu, Forget);
        ActionRegistry.Register(ActionIds.PersonalVehicle.Delete, PersonalVehiclePermissions.Delete, Delete, Limit);

        ActionRegistry.Register(
            ActionIds.PersonalVehicle.KickOccupants,
            PersonalVehiclePermissions.Kick,
            KickOccupants,
            Limit);

        ActionRegistry.Register(ActionIds.PersonalVehicle.SetLocked, PersonalVehiclePermissions.Lock, SetLocked, Limit);
        ActionRegistry.Register(ActionIds.PersonalVehicle.SetEngine, PersonalVehiclePermissions.Engine, SetEngine, Limit);
        ActionRegistry.Register(ActionIds.PersonalVehicle.SetLights, PersonalVehiclePermissions.Lights, SetLights, Limit);
        ActionRegistry.Register(ActionIds.PersonalVehicle.SetDoor, PersonalVehiclePermissions.Doors, SetDoor, Limit);
        ActionRegistry.Register(ActionIds.PersonalVehicle.SetAllDoors, PersonalVehiclePermissions.Doors, SetAllDoors, Limit);
        ActionRegistry.Register(ActionIds.PersonalVehicle.SetWindow, PersonalVehiclePermissions.Windows, SetWindow, Limit);

        ActionRegistry.Register(
            ActionIds.PersonalVehicle.SetAllWindows,
            PersonalVehiclePermissions.Windows,
            SetAllWindows,
            Limit);

        ActionRegistry.Register(
            ActionIds.PersonalVehicle.PlayHornTune,
            PersonalVehiclePermissions.Horn,
            PlayHornTune,
            Limit);

        ActionRegistry.Register(ActionIds.PersonalVehicle.Explode, PersonalVehiclePermissions.Explode, Explode, Limit);
    }

    internal static int Resolve(int networkId)
    {
        if (networkId == 0)
        {
            return 0;
        }

        var entity = Native.NetworkGetEntityFromNetworkId(networkId);

        if (entity == 0 || !Native.DoesEntityExist(entity) || Native.GetEntityType(entity) != VehicleEntityType)
        {
            return 0;
        }

        return entity;
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source > 0)
        {
            PersonalVehicleRegistry.Drop(source);
        }
    }

    private static ActionResponse Set(Player source, string[] args)
    {
        if (args.Length < 1
            || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var networkId))
        {
            return ActionResponse.InvalidRequest();
        }

        SpawnedVehicleRegistry.PruneSpawned(source.Handle, StillAVehicle);

        if (!SpawnedVehicleRegistry.WasSpawnedBy(source.Handle, networkId))
        {
            Log.Info($"[PersonalVehicle] {source.Name} tried to claim a vehicle they did not spawn. Refused.");

            return ActionResponse.Refused();
        }

        if (Resolve(networkId) == 0)
        {
            return ActionResponse.NotFound();
        }

        PersonalVehicleRegistry.SetMarked(source.Handle, networkId);

        return ActionResponse.Ok();
    }

    private static ActionResponse Forget(Player source, string[] args)
    {
        PersonalVehicleRegistry.ClearMarked(source.Handle);

        return ActionResponse.Ok();
    }

    private static ActionResponse Delete(Player source, string[] args)
    {
        var networkId = PersonalVehicleRegistry.Marked(source.Handle);

        if (networkId == 0)
        {
            return ActionResponse.NotFound();
        }

        var entity = Resolve(networkId);

        PersonalVehicleRegistry.ClearMarked(source.Handle);

        if (entity == 0)
        {
            return ActionResponse.NotFound();
        }

        Log.Debug($"[PersonalVehicle] {source.Name} deleted their personal vehicle.");

        Native.DeleteEntity(entity);

        return ActionResponse.Ok();
    }

    private static ActionResponse KickOccupants(Player source, string[] args)
    {
        var networkId = PersonalVehicleRegistry.Marked(source.Handle);

        if (networkId == 0)
        {
            return ActionResponse.NotFound();
        }

        var entity = Resolve(networkId);

        if (entity == 0)
        {
            PersonalVehicleRegistry.ClearMarked(source.Handle);

            return ActionResponse.NotFound();
        }

        var asked = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            if (player.ServerId == source.Handle)
            {
                continue;
            }

            var ped = Native.GetPlayerPed(player.ServerId.ToString(CultureInfo.InvariantCulture));

            if (ped == 0 || !Native.DoesEntityExist(ped) || Native.GetVehiclePedIsIn(ped, false) != entity)
            {
                continue;
            }

            API.EmitClient(player.ServerId, PersonalVehicleEvents.Leave, networkId);

            asked++;
        }

        if (asked > 0)
        {
            Log.Info($"[PersonalVehicle] {source.Name} emptied their personal vehicle of {asked} player(s).");
        }

        return ActionResponse.Ok(asked.ToString(CultureInfo.InvariantCulture));
    }

    private static Task<ActionResponse> SetLocked(Player source, string[] args) =>
        Remote(source, RemoteVehicleAction.Lock, Flag(args, 0));

    private static Task<ActionResponse> SetEngine(Player source, string[] args) =>
        Remote(source, RemoteVehicleAction.Engine, Flag(args, 0));

    private static Task<ActionResponse> SetLights(Player source, string[] args)
    {
        if (!TryIndex(args, 0, out var state)
            || (state != RemoteVehicleAction.LightsAutomatic
                && state != RemoteVehicleAction.LightsOff
                && state != RemoteVehicleAction.LightsOn))
        {
            return Task.FromResult(ActionResponse.InvalidRequest());
        }

        return Remote(source, RemoteVehicleAction.Lights, state.ToString(CultureInfo.InvariantCulture));
    }

    private static Task<ActionResponse> SetDoor(Player source, string[] args)
    {
        if (!TryIndex(args, 0, out var door) || door < 0 || door >= RemoteVehicleAction.DoorCount)
        {
            return Task.FromResult(ActionResponse.InvalidRequest());
        }

        return Remote(
            source,
            RemoteVehicleAction.Door,
            door.ToString(CultureInfo.InvariantCulture),
            Mode(args, 1, RemoteVehicleAction.Toggle, RemoteVehicleAction.Open, RemoteVehicleAction.Shut));
    }

    private static Task<ActionResponse> SetAllDoors(Player source, string[] args) =>
        Remote(
            source,
            RemoteVehicleAction.AllDoors,
            Mode(args, 0, RemoteVehicleAction.Shut, RemoteVehicleAction.Open, RemoteVehicleAction.Shut));

    private static Task<ActionResponse> SetWindow(Player source, string[] args)
    {
        if (!TryIndex(args, 0, out var window) || window < 0 || window >= RemoteVehicleAction.WindowCount)
        {
            return Task.FromResult(ActionResponse.InvalidRequest());
        }

        return Remote(
            source,
            RemoteVehicleAction.Window,
            window.ToString(CultureInfo.InvariantCulture),
            Mode(args, 1, RemoteVehicleAction.Up, RemoteVehicleAction.Down, RemoteVehicleAction.Up));
    }

    private static Task<ActionResponse> SetAllWindows(Player source, string[] args) =>
        Remote(
            source,
            RemoteVehicleAction.AllWindows,
            Mode(args, 0, RemoteVehicleAction.Up, RemoteVehicleAction.Down, RemoteVehicleAction.Up));

    private static async Task<ActionResponse> Explode(Player source, string[] args)
    {
        if (!TryMarked(source, out var networkId, out var entity))
        {
            return ActionResponse.NotFound();
        }

        var response = await RemoteVehicleControl.PerformAsync(
            source,
            networkId,
            entity,
            RemoteVehicleAction.Explode);

        if (response.Status != ActionStatus.Ok)
        {
            return response;
        }

        PersonalVehicleRegistry.ClearMarked(source.Handle);

        Log.Info($"[PersonalVehicle] {source.Name} blew up their personal vehicle.");

        return response;
    }

    private static ActionResponse PlayHornTune(Player source, string[] args)
    {
        if (!TryMarked(source, out var networkId, out var entity))
        {
            return ActionResponse.NotFound();
        }

        var tune = Native.GetGameTimer() >> 4 & int.MaxValue;

        tune %= HornTunes.Count;

        var bucket = Native.GetEntityRoutingBucket(entity);
        var heard = 0;

        foreach (var player in ConnectedPlayers.All())
        {
            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);

            if (Native.GetPlayerRoutingBucket(handle) != bucket)
            {
                continue;
            }

            API.EmitClient(player.ServerId, PersonalVehicleEvents.HornTune, networkId, tune);

            heard++;
        }

        Log.Debug($"[PersonalVehicle] {source.Name} sounded tune {tune} on {networkId} for {heard} player(s).");

        return ActionResponse.Ok();
    }

    private static Task<ActionResponse> Remote(Player source, string action, params string[] args) =>
        TryMarked(source, out var networkId, out var entity)
            ? RemoteVehicleControl.PerformAsync(source, networkId, entity, action, args)
            : Task.FromResult(ActionResponse.NotFound());

    private static bool TryMarked(Player source, out int networkId, out int entity)
    {
        networkId = PersonalVehicleRegistry.Marked(source.Handle);
        entity = networkId == 0 ? 0 : Resolve(networkId);

        if (networkId != 0 && entity == 0)
        {
            PersonalVehicleRegistry.ClearMarked(source.Handle);
        }

        return entity != 0;
    }

    private static string Flag(string[] args, int index) =>
        args.Length > index && args[index] == RemoteVehicleAction.On
            ? RemoteVehicleAction.On
            : RemoteVehicleAction.Off;

    private static string Mode(string[] args, int index, string fallback, params string[] allowed)
    {
        if (args.Length <= index)
        {
            return fallback;
        }

        foreach (var mode in allowed)
        {
            if (string.Equals(args[index], mode, StringComparison.Ordinal))
            {
                return mode;
            }
        }

        return fallback;
    }

    private static bool TryIndex(string[] args, int index, out int value)
    {
        value = 0;

        return args.Length > index
            && int.TryParse(args[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool StillAVehicle(int networkId) => Resolve(networkId) != 0;
}
