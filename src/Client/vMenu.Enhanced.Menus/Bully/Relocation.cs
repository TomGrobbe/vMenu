using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Relocation
{
    private const float MinimumTravel = 250f;

    private const int MinAwayMs = 5000;

    private const int MaxAwayMs = 25000;

    private static readonly Vector2[] Spots =
    [
        new(501.8f, 5604.2f),
        new(-75.0f, -818.0f),
        new(-2149.0f, 3200.0f),
        new(1747.0f, 3273.0f),
        new(-275.5f, 6635.0f),
        new(-1850.0f, -1231.0f),
        new(-1336.0f, -3044.0f),
        new(1690.0f, 2591.0f),
        new(3426.0f, 5174.0f),
        new(-1170.0f, 4926.0f),
        new(-1600.0f, 2100.0f),
    ];

    private static bool _away;

    private static Task<bool> ToRandomSpotAsync(Vector3 awayFrom)
    {
        var spot = Spots[0];

        for (var attempt = 0; attempt < 8; attempt++)
        {
            spot = Spots[Dice.Next(Spots.Length)];

            if (Vector2.Distance(spot, new Vector2(awayFrom.X, awayFrom.Y)) >= MinimumTravel)
            {
                break;
            }
        }

        return PlayerTeleport.ToGroundAsync(spot.X, spot.Y);
    }

    public static async Task TeleportAndReturn(BullyRun run)
    {
        var ped = Native.PlayerPedId();

        if (_away)
        {
            run.Busy();

            return;
        }

        if (!Native.IsPedOnFoot(ped))
        {
            run.Skip(BullyEvents.RefusedOnFoot);

            return;
        }

        _away = true;

        try
        {
            var origin = Native.GetEntityCoords(ped, false);
            var heading = Native.GetEntityHeading(ped);

            run.Started();

            if (!await ToRandomSpotAsync(origin))
            {
                return;
            }

            await API.Delay(Dice.Next(MinAwayMs, MaxAwayMs + 1));

            if (Native.IsPedDeadOrDying(Native.PlayerPedId(), true))
            {
                return;
            }

            await PlayerTeleport.ToCoordsAsync(origin, heading);
        }
        finally
        {
            _away = false;
        }
    }
}
