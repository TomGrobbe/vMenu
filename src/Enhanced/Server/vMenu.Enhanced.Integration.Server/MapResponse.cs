using System.Globalization;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.Players.Server;

namespace vMenu.Enhanced.Integration.Server;

public sealed class MapPlayer
{
    public required int ServerId { get; init; }

    public required string Name { get; init; }

    public string? Discord { get; init; }

    public required int X { get; init; }

    public required int Y { get; init; }

    public required int Z { get; init; }

    public required int Heading { get; init; }

    public required bool InVehicle { get; init; }

    public required int Sprite { get; init; }

    public required int RoutingBucket { get; init; }
}

public sealed class MapResponse
{
    public required int Count { get; init; }

    public required IReadOnlyList<MapPlayer> Players { get; init; }

    public static MapResponse Current()
    {
        var connected = ConnectedPlayers.All();
        var players = new List<MapPlayer>(connected.Count);

        foreach (var player in connected)
        {
            var handle = player.ServerId.ToString(CultureInfo.InvariantCulture);
            var ped = Native.GetPlayerPed(handle);

            if (ped == 0 || !Native.DoesEntityExist(ped))
            {
                continue;
            }

            var position = Native.GetEntityCoords(ped);
            var vehicle = Native.GetVehiclePedIsIn(ped, false);
            var inVehicle = vehicle != 0 && Native.DoesEntityExist(vehicle);

            players.Add(new MapPlayer
            {
                ServerId = player.ServerId,
                Name = player.Name,
                Discord = PlayerIdentifiers.Discord(handle),
                X = Round(position.X),
                Y = Round(position.Y),
                Z = Round(position.Z),
                // A passenger's ped heading freezes at the seat entered, so read the vehicle's heading aboard.
                Heading = (int)Native.GetEntityHeading(inVehicle ? vehicle : ped),
                InVehicle = inVehicle,
                Sprite = SpriteFor(vehicle, inVehicle),
                RoutingBucket = Native.GetPlayerRoutingBucket(handle),
            });
        }

        return new MapResponse
        {
            Count = players.Count,
            Players = players,
        };
    }

    // The server has no IsThisModelAPlane; GetVehicleType answers the same shape question.
    private static int SpriteFor(int vehicle, bool inVehicle)
    {
        if (!inVehicle)
        {
            return VehicleBlipSprites.SpriteFor(0, VehicleBlipKind.None);
        }

        var model = unchecked((uint)Native.GetEntityModel(vehicle));

        return VehicleBlipSprites.SpriteFor(model, KindOf(vehicle));
    }

    private static VehicleBlipKind KindOf(int vehicle) => Native.GetVehicleType(vehicle) switch
    {
        "plane" => VehicleBlipKind.Plane,
        "heli" => VehicleBlipKind.Heli,
        "boat" => VehicleBlipKind.Boat,
        _ => VehicleBlipKind.Land,
    };

    private static int Round(float value) => (int)MathF.Round(value);
}
