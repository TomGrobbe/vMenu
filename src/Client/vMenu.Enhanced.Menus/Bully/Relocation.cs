using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Data.Bullying;
using vMenu.Enhanced.Events;
using vMenu.Enhanced.Menus.Players;

namespace vMenu.Enhanced.Menus.Bully;

internal static class Relocation
{
    private const float MinimumTravel = 250f;

    private const int MinAwayMs = 20000;

    private const int MaxAwayMs = 60000;

    private static readonly (Vector3 Position, float Heading)[] Spots =
    [
        (new(-74.17f, -837.13f, 318.43f), 192.15f),
        (new(-120.16f, -977.71f, 303.84f), 204.13f),
        (new(-552.25f, -2236.94f, 121.87f), 56.10f),
        (new(-728.12f, -3495.16f, 12.32f), 262.78f),
        (new(-506.03f, -2915.97f, 37.56f), 223.62f),
        (new(496.07f, -3382.99f, 5.57f), 0.85f),
        (new(2434.52f, -2151.27f, 1.52f), 155.11f),
        (new(2820.06f, -1454.47f, 8.81f), 84.34f),
        (new(2729.16f, 1576.32f, 66.04f), 123.05f),
        (new(3433.23f, 5172.43f, 32.13f), 230.26f),
        (new(501.65f, 5604.86f, 797.41f), 174.15f),
        (new(-839.34f, 5896.85f, 4.06f), 272.63f),
        (new(-1212.85f, 3849.71f, 489.83f), 133.87f),
        (new(-2501.36f, 3296.16f, 91.46f), 239.70f),
        (new(-3426.46f, 967.33f, 7.85f), 90.73f),
        (new(-1613.88f, 5260.82f, 3.47f), 125.05f),
        (new(-1141.94f, 4950.96f, 229.25f), 199.96f),
        (new(-223.96f, 3660.06f, 63.91f), 330.33f),
        (new(91.52f, 3749.78f, 40.27f), 341.22f),
        (new(1910.84f, 3476.56f, 62.36f), 62.03f),
        (new(1545.98f, 3811.88f, 29.60f), 351.84f),
        (new(1022.82f, 2246.67f, 79.10f), 138.28f),
        (new(741.97f, 1271.94f, 382.67f), 182.08f),
        (new(-783.17f, 313.94f, 230.14f), 179.06f),
        (new(-2302.46f, 216.64f, 167.10f), 15.45f),
    ];

    private static bool _away;

    public static bool IsAway => _away;

    public static void Initialize() => ResourceShutdown.Stopping += OnShutdown;

    private static void OnShutdown()
    {
        if (!_away)
        {
            return;
        }

        WakeUp.Uncover();

        Native.FreezeEntityPosition(Native.PlayerPedId(), false);
    }

    private static (Vector3 Position, float Heading) RandomSpot(Vector3 awayFrom)
    {
        var far = Spots.Where(spot => Vector3.Distance(spot.Position, awayFrom) >= MinimumTravel).ToArray();

        return far.Length > 0 ? far[Dice.Next(far.Length)] : Spots[0];
    }

    public static async Task TeleportAndReturn(BullyRun run)
    {
        var ped = Native.PlayerPedId();

        if (_away || AlienAbduction.IsActive)
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
            var stopAllCount = BullyState.StopAllCount;
            var origin = Native.GetEntityCoords(ped, false);
            var heading = Native.GetEntityHeading(ped);

            run.Started();

            var animated = await WakeUp.LoadAsync();
            var (spot, facing) = RandomSpot(origin);

            if (!await MoveAsync(spot, facing, animated))
            {
                return;
            }

            var returnAt = Native.GetGameTimer() + Dice.Next(MinAwayMs, MaxAwayMs + 1);

            while (Native.GetGameTimer() < returnAt && stopAllCount == BullyState.StopAllCount)
            {
                await API.Delay(250);
            }

            if (Native.IsPedDeadOrDying(Native.PlayerPedId(), true))
            {
                return;
            }

            await MoveAsync(origin, heading, animated);
        }
        finally
        {
            WakeUp.Uncover();

            Native.FreezeEntityPosition(Native.PlayerPedId(), false);

            WakeUp.Release();

            _away = false;
        }
    }

    private static async Task<bool> MoveAsync(Vector3 destination, float heading, bool animated)
    {
        await WakeUp.CoverAsync();

        if (!await PlayerTeleport.ToCoordsAsync(destination, heading))
        {
            await WakeUp.CancelAsync();

            return false;
        }

        await WakeUp.RevealAsync(animated);

        return true;
    }
}
