using System.Numerics;

using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Bully;

internal static class SpawnSpots
{
    private const float MinimumShare = 0.75f;

    private const float MaximumStretch = 1.5f;

    private const int SpawnDirections = 8;

    private const int RoadNodes = 40;

    private const int RandomAttempts = 16;

    private const float GroundProbeHeight = 50f;

    private const float MaxHeightDifference = 20f;

    // Tries the game's own spawn point finder, the roadside and the pavement, in that order of trust, and
    // prefers a spot the player cannot see. Plain ground is the last resort, never the player's own spot.
    public static Vector3? FindSafeSpotAround(Vector3 centre, float minDistance, float maxDistance, bool hidden)
    {
        var start = Dice.Float(0f, MathF.Tau);
        var ideal = (minDistance + maxDistance) / 2f;

        var spawnPoints = Enumerable.Range(0, SpawnDirections)
            .Select(index => start + index * MathF.Tau / SpawnDirections)
            .Select(angle => Native.FindSpawnPointInDirection(
                centre.X, centre.Y, centre.Z, MathF.Cos(angle), MathF.Sin(angle), 0f, ideal, out var spot)
                ? spot
                : (Vector3?)null);

        var roadsides = Enumerable.Range(1, RoadNodes)
            .Select(node => Native.GetNthClosestVehicleNode(centre.X, centre.Y, centre.Z, node, out var road, 0, 3f, 0f)
                ? Native.GetPointOnRoadSide(road.X, road.Y, road.Z, 0, out var side) ? side : road
                : (Vector3?)null);

        var pavements = RandomPoints(centre, minDistance, maxDistance)
            .Select(point => Native.GetSafeCoordForPed(point.X, point.Y, centre.Z, false, out var safe, 16) ? safe : (Vector3?)null);

        var candidates = spawnPoints.Concat(roadsides).Concat(pavements)
            .OfType<Vector3>()
            .Where(spot => FlatDistance(spot, centre) is var distance
                && distance >= minDistance * MinimumShare
                && distance <= maxDistance * MaximumStretch
                && MathF.Abs(spot.Z - centre.Z) <= MaxHeightDifference)
            .OrderBy(_ => Dice.Next(int.MaxValue))
            .ToList();

        if (candidates.Count > 0)
        {
            return candidates
                .Where(spot => !hidden || !Native.IsSphereVisible(spot.X, spot.Y, spot.Z + 1f, 1f))
                .Cast<Vector3?>()
                .FirstOrDefault() ?? candidates[0];
        }

        return RandomPoints(centre, minDistance, maxDistance)
            .Select(point => Native.GetGroundZFor_3dCoord(point.X, point.Y, centre.Z + GroundProbeHeight, out var groundZ, false, false)
                && MathF.Abs(groundZ - centre.Z) <= MaxHeightDifference
                    ? new Vector3(point.X, point.Y, groundZ)
                    : (Vector3?)null)
            .FirstOrDefault(point => point is not null);
    }

    private static IEnumerable<Vector2> RandomPoints(Vector3 centre, float minDistance, float maxDistance) =>
        Enumerable.Range(0, RandomAttempts)
            .Select(_ => (Angle: Dice.Float(0f, MathF.Tau), Distance: Dice.Float(minDistance, maxDistance)))
            .Select(pick => new Vector2(centre.X + MathF.Cos(pick.Angle) * pick.Distance, centre.Y + MathF.Sin(pick.Angle) * pick.Distance));

    private static float FlatDistance(Vector3 a, Vector3 b) =>
        Vector2.Distance(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
}
