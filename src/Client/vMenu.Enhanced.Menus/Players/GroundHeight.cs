using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Menus.Players;

/// <summary>
/// Finds the height of the ground at a spot on the map, for a destination that only says where on
/// the map to go and not how high up.
/// </summary>
// A ray fired straight down the column, not GetGroundZFor_3dCoord or its ExcludingObjects sibling.
// Those two fault inside the game itself: the call reaches CitizenFX.Base.NativeApi.Invoke and dies
// there with an SEHException, on every form of them, including a hand written one that pushes and
// reads exactly what the generated wrapper does. Nothing on this side of the call fixes that.
// The game only answers for terrain it has streamed in, and it only streams terrain near something
// it is rendering, which is why the entity is walked up the column and the ray fired again at each
// stop rather than once from the top.
internal static class GroundHeight
{
    /// <summary>Put down slightly above what was found, so the player is not standing in the floor.</summary>
    private const float Clearance = 1f;

    /// <summary>Nothing in the map is above this, so a column that gets here has no ground in it.</summary>
    private const float Ceiling = 1200f;

    /// <summary>Above everything, so one ray covers the whole column.</summary>
    private const float ColumnTop = 1500f;

    /// <summary>Below the sea bed, so a ray that hits nothing really had nothing to hit.</summary>
    private const float ColumnBottom = -300f;

    private const int Map = 1;

    private const int MapAndObjects = 1 | 16;

    /// <summary>Cargo culted from every script in the wild, this one included. Not a considered value.</summary>
    private const int ShapeTestOptions = 7;

    /// <summary>Let the probe through everything, so only water can stop it.</summary>
    private const int NothingBlocks = 0;

    /// <summary>The probe reached water. The other two answers are nothing found, and blocked short of it.</summary>
    private const int WaterTestWater = 1;

    private const int ShapeTestNotReady = 1;

    private const int ShapeTestReady = 2;

    private const int MaxResultFrames = 10;

    private static readonly float[] ProbeHeights =
    [
        0f, 25f, 50f, 75f, 100f, 150f, 200f, 250f, 300f, 350f,
        400f, 450f, 500f, 550f, 600f, 650f, 700f, 750f, 800f, Ceiling,
    ];

    /// <returns>The height to put the entity at, or <see langword="null"/> if nothing answered.</returns>
    public static async Task<float?> FindAsync(int entity, float x, float y)
    {
        // Probing moves the entity, so a search that comes back with nothing has to put it back
        // rather than leave the player standing at whatever height it gave up on.
        var origin = Native.GetEntityCoords(entity, false);

        try
        {
            foreach (var height in ProbeHeights)
            {
                Native.SetEntityCoords(entity, x, y, height, false, false, false, true);

                await API.Delay(0);

                if (await AskAsync(entity, x, y) is { } found)
                {
                    return found + Clearance;
                }
            }

            if (FromEntity(entity, x, y) is { } last)
            {
                return last;
            }
        }
        catch (Exception exception)
        {
            // Answered as "no ground" rather than left to unwind, because the entity is sitting at a
            // probe height by now and a throw would leave the player standing there.
            Log.Error($"[Teleport] Looking for the ground at {x}, {y} threw: {exception}");
        }

        Native.SetEntityCoords(entity, origin.X, origin.Y, origin.Z, false, false, false, true);

        return null;
    }

    private static async Task<float?> AskAsync(int entity, float x, float y)
    {
        var solid = await CastAsync(entity, x, y, MapAndObjects)
            // Without objects, for a spot where a prop is all the first ray found and the map
            // underneath it is what the player actually wants.
            ?? await CastAsync(entity, x, y, Map);

        if (solid is not { } ground)
        {
            return null;
        }

        // The ray goes straight through water and hits the bed, so a lake would put the player on
        // the bottom of it. Anything solid below a surface loses to the surface.
        return Surface(x, y) is { } water && water > ground ? water : ground;
    }

    /// <summary>The water surface over this spot, or <see langword="null"/> where there is none.</summary>
    // Nothing is allowed to block the probe, because the question here is only ever where the water
    // is. What the player stands on has already been settled by the ray above.
    // Guarded on its own rather than left to the caller's catch: this only refines a ground height
    // that has already been found, so it must never be what turns a good answer into no answer.
    private static float? Surface(float x, float y)
    {
        try
        {
            return NativeFixer.TestVerticalProbeAgainstAllWater(x, y, ColumnTop, NothingBlocks, out var height) == WaterTestWater
                ? height
                : null;
        }
        catch (Exception exception)
        {
            Log.Error($"[Teleport] Looking for water at {x}, {y} threw, so the solid ground stands: {exception}");

            return null;
        }
    }

    private static async Task<float?> CastAsync(int entity, float x, float y, int flags)
    {
        // Synchronous, so it answers in the frame it was started rather than costing one per probe.
        // The entity is excluded because it is sitting in the column the ray is fired down.
        var test = Native.StartExpensiveSynchronousShapeTestLosProbe(
            x, y, ColumnTop,
            x, y, ColumnBottom,
            flags,
            entity,
            ShapeTestOptions);

        for (var frame = 0; frame < MaxResultFrames; frame++)
        {
            var status = Native.GetShapeTestResult(test, out var hit, out var end, out _, out _);

            if (status == ShapeTestNotReady)
            {
                await API.Delay(0);

                continue;
            }

            return status == ShapeTestReady && hit != 0 ? end.Z : null;
        }

        return null;
    }

    /// <summary>Last resort: ask the entity itself how far it is off the ground it is over.</summary>
    // A different mechanism to the ray, so it sometimes answers when that has not.
    private static float? FromEntity(int entity, float x, float y)
    {
        Native.SetEntityCoords(entity, x, y, Ceiling, false, false, false, true);

        var above = Native.GetEntityHeightAboveGround(entity);

        return above is > 0f and < Ceiling ? Ceiling - above + Clearance : null;
    }
}
