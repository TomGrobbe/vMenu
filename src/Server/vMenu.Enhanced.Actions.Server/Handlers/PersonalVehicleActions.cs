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
        API.OnNetEvent(PersonalVehicleEvents.Spawned, new Action<Player, int>(OnSpawned), false);

        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);

        ActionRegistry.Register(ActionIds.PersonalVehicle.Set, PersonalVehiclePermissions.Menu, Set, Limit);
        ActionRegistry.Register(ActionIds.PersonalVehicle.Forget, PersonalVehiclePermissions.Menu, Forget);
        ActionRegistry.Register(ActionIds.PersonalVehicle.Delete, PersonalVehiclePermissions.Delete, Delete, Limit);

        ActionRegistry.Register(
            ActionIds.PersonalVehicle.KickOccupants,
            PersonalVehiclePermissions.Kick,
            KickOccupants,
            Limit);
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

    private static void OnSpawned([FromSource] Player source, int networkId) =>
        PersonalVehicleRegistry.RecordSpawn(source.Handle, networkId);

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

        PersonalVehicleRegistry.PruneSpawned(source.Handle, StillAVehicle);

        if (!PersonalVehicleRegistry.WasSpawnedBy(source.Handle, networkId))
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

    private static bool StillAVehicle(int networkId) => Resolve(networkId) != 0;
}
