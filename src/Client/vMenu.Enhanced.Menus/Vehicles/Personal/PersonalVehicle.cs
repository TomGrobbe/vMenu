using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Actions;
using vMenu.Enhanced.Data.Actions;
using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Permissions;
using vMenu.Enhanced.Storage;

using PersonalVehiclePermissions = vMenu.Enhanced.Data.Permissions.Menus.PersonalVehicle;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

public static class PersonalVehicle
{
    private const int LeaveNormally = 0;

    private static readonly List<string> NoOccupants = [];

    private static List<string> _occupants = NoOccupants;

    private static bool _busy;

    public static event Action? Changed;

    public static int NetworkId { get; private set; }

    public static bool IsMarked => NetworkId != 0;

    public static Vector3 Position { get; private set; }

    public static uint Model { get; private set; }

    public static bool InRange { get; private set; }

    public static bool HasPosition { get; private set; }

    public static IReadOnlyList<string> Occupants => _occupants;

    public static bool BlipWanted =>
        UserDefaults.PersonalVehicleBlip.Value && ClientPermissions.IsAllowed(PersonalVehiclePermissions.Blip);

    public static void Initialize()
    {
        API.OnNetEvent(PersonalVehicleEvents.Update, new Action<string>(OnUpdate), false);
        API.OnNetEvent(PersonalVehicleEvents.Lost, new Action(OnLost), false);
        API.OnNetEvent(PersonalVehicleEvents.Leave, new Action<int>(OnLeave), false);

        ClientPermissions.PermissionsChanged += PersonalVehicleBlip.Reevaluate;

        ResourceShutdown.Stopping += PersonalVehicleBlip.RemoveAll;
    }

    public static void ReportSpawned(int entity)
    {
        if (entity == 0 || !Native.NetworkGetEntityIsNetworked(entity))
        {
            return;
        }

        API.EmitServer(PersonalVehicleEvents.Spawned, Native.NetworkGetNetworkIdFromEntity(entity));
    }

    public static bool Owns(int entity) =>
        NetworkId != 0
        && entity != 0
        && Native.DoesEntityExist(entity)
        && Native.NetworkGetEntityIsNetworked(entity)
        && Native.NetworkGetNetworkIdFromEntity(entity) == NetworkId;

    public static void SetBlipEnabled(bool enabled)
    {
        UserDefaults.PersonalVehicleBlip.Value = enabled;

        PersonalVehicleBlip.Reevaluate();
    }

    public static async Task MarkCurrentAsync()
    {
        if (_busy)
        {
            return;
        }

        var vehicle = OwnVehicle.RequireDriven(Loc.PersonalVehicle.NoVehicle, Loc.PersonalVehicle.NotDriver);

        if (vehicle is null)
        {
            return;
        }

        if (!Native.NetworkGetEntityIsNetworked(vehicle.Handle))
        {
            Notifications.Error(MenuText.Key(Loc.PersonalVehicle.SetFailed));

            return;
        }

        var networkId = Native.NetworkGetNetworkIdFromEntity(vehicle.Handle);

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(
                ActionIds.PersonalVehicle.Set,
                networkId.ToString(CultureInfo.InvariantCulture));

            if (!result.IsOk)
            {
                Notify(result.Status, Loc.PersonalVehicle.Gone, Loc.PersonalVehicle.SetFailed);

                return;
            }

            Adopt(networkId);

            Notifications.Success(MenuText.Key(
                Loc.PersonalVehicle.Set,
                ("vehicle", MenuText.Literal(VehicleSpawning.DisplayName(Model)))));
        }
        finally
        {
            _busy = false;
        }
    }

    public static async Task ForgetAsync()
    {
        if (_busy || !Guarded())
        {
            return;
        }

        _busy = true;

        try
        {
            await ServerActions.InvokeAsync(ActionIds.PersonalVehicle.Forget);

            Release();

            Notifications.Info(MenuText.Key(Loc.PersonalVehicle.Forgotten));
        }
        finally
        {
            _busy = false;
        }
    }

    public static async Task DeleteAsync()
    {
        if (_busy || !Guarded())
        {
            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.PersonalVehicle.Delete);

            Release();

            if (result.IsOk)
            {
                Notifications.Success(MenuText.Key(Loc.PersonalVehicle.Deleted));

                return;
            }

            Notify(result.Status, Loc.PersonalVehicle.Gone, Loc.PersonalVehicle.DeleteFailed);
        }
        finally
        {
            _busy = false;
        }
    }

    public static async Task KickOccupantsAsync()
    {
        if (_busy || !Guarded())
        {
            return;
        }

        _busy = true;

        try
        {
            var result = await ServerActions.InvokeAsync(ActionIds.PersonalVehicle.KickOccupants);

            if (!result.IsOk)
            {
                Notify(result.Status, Loc.PersonalVehicle.Gone, Loc.PersonalVehicle.KickFailed);

                return;
            }

            var asked = result.Data.Length > 0
                && int.TryParse(result.Data[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var count)
                    ? count
                    : 0;

            if (asked == 0)
            {
                Notifications.Info(MenuText.Key(Loc.PersonalVehicle.KickedNobody));

                return;
            }

            Notifications.Success(MenuText.Key(
                Loc.PersonalVehicle.Kicked,
                ("count", MenuText.Literal(asked.ToString(CultureInfo.InvariantCulture)))));
        }
        finally
        {
            _busy = false;
        }
    }

    public static void SetWaypoint()
    {
        if (!Guarded())
        {
            return;
        }

        if (!HasPosition)
        {
            Notifications.Error(MenuText.Key(Loc.PersonalVehicle.NoPosition));

            return;
        }

        Native.SetNewWaypoint(Position.X, Position.Y);

        Notifications.Success(MenuText.Key(Loc.PersonalVehicle.WaypointSet));
    }

    private static bool Guarded()
    {
        if (IsMarked)
        {
            return true;
        }

        Notifications.Error(MenuText.Key(Loc.PersonalVehicle.NoneMarked));

        return false;
    }

    private static void Adopt(int networkId)
    {
        NetworkId = networkId;
        HasPosition = false;
        InRange = true;
        _occupants = NoOccupants;

        var entity = Native.NetworkGetEntityFromNetworkId(networkId);

        Model = entity != 0 && Native.DoesEntityExist(entity)
            ? unchecked((uint)Native.GetEntityModel(entity))
            : 0;

        PersonalVehicleBlip.Reevaluate();
        Changed?.Invoke();
    }

    private static void Release()
    {
        NetworkId = 0;
        HasPosition = false;
        Model = 0;
        _occupants = NoOccupants;

        PersonalVehicleBlip.RemoveAll();
        Changed?.Invoke();
    }

    private static void OnUpdate(string row)
    {
        if (PersonalVehicleRow.Parse(row) is not { } entry || entry.NetworkId != NetworkId)
        {
            return;
        }

        Position = new Vector3(entry.X, entry.Y, entry.Z);
        Model = entry.Model;
        InRange = entry.InRange;
        HasPosition = true;
        _occupants = PersonalVehicleRow.Occupants(entry.Occupants);

        PersonalVehicleBlip.Apply(entry.Heading);
        Changed?.Invoke();
    }

    private static void OnLost()
    {
        if (!IsMarked)
        {
            return;
        }

        Release();

        Notifications.Warning(MenuText.Key(Loc.PersonalVehicle.Gone));
    }

    private static void OnLeave(int networkId)
    {
        var vehicle = Native.NetworkGetEntityFromNetworkId(networkId);

        if (vehicle == 0 || !Native.DoesEntityExist(vehicle))
        {
            return;
        }

        var ped = Native.PlayerPedId();

        if (ped == 0 || Native.GetVehiclePedIsIn(ped, false) != vehicle)
        {
            return;
        }

        Native.TaskLeaveVehicle(ped, vehicle, LeaveNormally);

        Notifications.Warning(MenuText.Key(Loc.PersonalVehicle.AskedToLeave));
    }

    private static void Notify(ActionStatus status, string missingKey, string failedKey) =>
        Notifications.Error(MenuText.Key(status switch
        {
            ActionStatus.Denied => Loc.PersonalVehicle.Denied,
            ActionStatus.NotFound => missingKey,
            ActionStatus.Refused => Loc.PersonalVehicle.NotSpawnedByYou,
            ActionStatus.RateLimited => Loc.PersonalVehicle.TooFast,
            _ => failedKey,
        }));
}
