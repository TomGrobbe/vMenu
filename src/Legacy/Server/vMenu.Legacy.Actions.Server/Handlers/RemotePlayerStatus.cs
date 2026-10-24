using System.Globalization;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.OnlinePlayers;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Serialization.Server;

using PlayerOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.PlayerOptions;
using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Actions.Server.Handlers;

public static class RemotePlayerStatus
{
    private const int AnswerTimeoutMs = 3000;

    private static readonly ClientRequests<ClientReport> Reports = new();

    private static bool _registered;

    public static void RegisterEventHandlers()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Script.EventHandlers[PlayerEvents.StatusReportAck] += new Action<Player, string, bool, bool, string, string, string>(OnReported);
    }

    // Must start on the tick thread. Returns the status as JSON.
    public static async Task<string> CaptureAsync(int target, int ped)
    {
        var (_, report) = await Reports.AskAsync(
            target,
            id => Script.TriggerClientEvent(target, PlayerEvents.GetStatusReport, id),
            AnswerTimeoutMs);

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
            Health = API.GetEntityHealth(ped),
            MaxHealth = API.GetEntityMaxHealth(ped),
            Armor = API.GetPedArmour(ped),
            Position = Point.Of(API.GetEntityCoords(ped)),
            Rotation = Point.Of(API.GetEntityRotation(ped)),
            Heading = Round(API.GetEntityHeading(ped)),
            God = playerGod,
            Weapon = report?.Weapon is { Length: > 0 } weapon ? weapon : null,
        };

        var vehicle = API.GetVehiclePedIsIn(ped, false);

        if (!Targets.IsVehicle(vehicle))
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
                BodyHealth = Round(API.GetVehicleBodyHealth(vehicle)),
                EngineHealth = Round(API.GetVehicleEngineHealth(vehicle)),
                TankHealth = Round(API.GetVehiclePetrolTankHealth(vehicle)),
                Position = Point.Of(API.GetEntityCoords(vehicle)),
                Rotation = Point.Of(API.GetEntityRotation(vehicle)),
                Heading = Round(API.GetEntityHeading(vehicle)),
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
        string weapon) =>
        Reports.TryAnswer(
            source,
            requestId,
            new ClientReport(playerGod, vehicleGod, vehicleName, vehicleModel, weapon));

    private sealed record ClientReport(bool PlayerGod, bool VehicleGod, string VehicleName, string VehicleModel, string Weapon);

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
