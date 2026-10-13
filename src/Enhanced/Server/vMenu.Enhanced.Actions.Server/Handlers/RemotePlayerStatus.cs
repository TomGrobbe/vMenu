using System.Globalization;
using System.Numerics;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;
using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class RemotePlayerStatus
{
    private const int AnswerTimeoutMs = 3000;

    private const int VehicleEntityType = 2;

    private static readonly Dictionary<int, PendingReport> Unanswered = [];

    private static int _lastRequestId;

    private static bool _registered;

    public static void RegisterEventHandlers()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(
            PlayerEvents.StatusReportAck,
            new Action<Player, string, bool, bool, string, string, string>(OnReported),
            false);
    }

    // Must start on the tick thread. Returns the status as JSON.
    public static async Task<string> CaptureAsync(int target, int ped)
    {
        var requestId = ++_lastRequestId;
        var answered = new TaskCompletionSource<ClientReport>();

        Unanswered[requestId] = new PendingReport(target, answered);

        API.EmitClient(target, PlayerEvents.GetStatusReport, requestId.ToString(CultureInfo.InvariantCulture));

        ClientReport? report = null;

        try
        {
            var timeout = API.Delay(AnswerTimeoutMs);

            if (await Task.WhenAny(answered.Task, timeout) != timeout)
            {
                report = answered.Task.Result;
            }
        }
        finally
        {
            Unanswered.Remove(requestId);

            await API.Delay(0);
        }

        return ServerJson.Serialize(Build(target, ped, report));
    }

    private static StatusResponse Build(int target, int ped, ClientReport? report)
    {
        var handle = target.ToString(CultureInfo.InvariantCulture);

        bool? playerGod = report is null
            ? null
            : report.PlayerGod && ServerPermissions.IsPlayerAllowed(handle, PlayerOptionsPermissions.Godmode);

        var player = new PlayerStatus
        {
            Health = Native.GetEntityHealth(ped),
            MaxHealth = Native.GetEntityMaxHealth(ped),
            Armor = Native.GetPedArmour(ped),
            Position = Point.Of(Native.GetEntityCoords(ped)),
            Rotation = Point.Of(Native.GetEntityRotation(ped)),
            Heading = Round(Native.GetEntityHeading(ped)),
            God = playerGod,
            Weapon = report?.Weapon is { Length: > 0 } weapon ? weapon : null,
        };

        var vehicle = Native.GetVehiclePedIsIn(ped, false);

        if (vehicle == 0 || !Native.DoesEntityExist(vehicle) || Native.GetEntityType(vehicle) != VehicleEntityType)
        {
            return new StatusResponse { Player = player };
        }

        bool? vehicleGod = report is null
            ? null
            : report.VehicleGod && ServerPermissions.IsPlayerAllowed(handle, VehicleOptionsPermissions.God);

        return new StatusResponse
        {
            Player = player,
            Vehicle = new VehicleStatus
            {
                Name = report?.VehicleName is { Length: > 0 } name ? name : null,
                Model = report?.VehicleModel is { Length: > 0 } model ? model : null,
                BodyHealth = Round(Native.GetVehicleBodyHealth(vehicle)),
                EngineHealth = Round(Native.GetVehicleEngineHealth(vehicle)),
                TankHealth = Round(Native.GetVehiclePetrolTankHealth(vehicle)),
                Position = Point.Of(Native.GetEntityCoords(vehicle)),
                Rotation = Point.Of(Native.GetEntityRotation(vehicle)),
                Heading = Round(Native.GetEntityHeading(vehicle)),
                God = vehicleGod,
            },
        };
    }

    private static float Round(float value) => MathF.Round(value, 2);

    private static void OnReported(
        [FromSource] Player source,
        string requestId,
        bool playerGod,
        bool vehicleGod,
        string vehicleName,
        string vehicleModel,
        string weapon)
    {
        if (!int.TryParse(requestId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            || !Unanswered.TryGetValue(id, out var pending)
            || pending.Target != source.Handle)
        {
            return;
        }

        pending.Answered.TrySetResult(new ClientReport(playerGod, vehicleGod, vehicleName, vehicleModel, weapon));
    }

    private sealed record ClientReport(bool PlayerGod, bool VehicleGod, string VehicleName, string VehicleModel, string Weapon);

    private sealed class PendingReport(int target, TaskCompletionSource<ClientReport> answered)
    {
        public int Target { get; } = target;

        public TaskCompletionSource<ClientReport> Answered { get; } = answered;
    }

    private sealed class StatusResponse
    {
        public bool Ok { get; init; } = true;

        public required PlayerStatus Player { get; init; }

        public VehicleStatus? Vehicle { get; init; }
    }

    private sealed class PlayerStatus
    {
        public required int Health { get; init; }

        public required int MaxHealth { get; init; }

        public required int Armor { get; init; }

        public required Point Position { get; init; }

        public required Point Rotation { get; init; }

        public required float Heading { get; init; }

        public bool? God { get; init; }

        public string? Weapon { get; init; }
    }

    private sealed class VehicleStatus
    {
        public string? Name { get; init; }

        public string? Model { get; init; }

        public required float BodyHealth { get; init; }

        public required float EngineHealth { get; init; }

        public required float TankHealth { get; init; }

        public required Point Position { get; init; }

        public required Point Rotation { get; init; }

        public required float Heading { get; init; }

        public bool? God { get; init; }
    }

    private sealed class Point
    {
        public required float X { get; init; }

        public required float Y { get; init; }

        public required float Z { get; init; }

        public static Point Of(Vector3 value) => new()
        {
            X = Round(value.X),
            Y = Round(value.Y),
            Z = Round(value.Z),
        };
    }
}
