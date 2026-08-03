using System.Numerics;

using CitizenFX.FiveM.Client;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Developer;

/// <summary>
/// Draws an entity's model bounding box: shaded faces in the caller's colour, white edges.
/// </summary>
/// <remarks>
/// Allocation free on purpose. This runs for every tracked entity on every frame, and the legacy
/// version built a fresh eight corner array plus twenty-four jagged sub-arrays each time it was
/// called. The corners go in a shared buffer instead, and the faces and edges are described once as
/// index tables into it.
/// <para>
/// Sharing the buffer is safe because a draw never spans an await, and the client only ever runs one
/// tick body at a time.
/// </para>
/// </remarks>
internal static class EntityBox
{
    /// <summary>Keeps coplanar faces of touching boxes from fighting over the same pixels.</summary>
    private const float Pad = 0.001f;

    private const int EdgeRed = 255;

    private const int EdgeGreen = 255;

    private const int EdgeBlue = 255;

    private const int EdgeAlpha = 255;

    private static readonly Vector3[] Corners = new Vector3[8];

    /// <summary>Corner indices, three per triangle, two triangles per face.</summary>
    private static readonly byte[] PolyIndices =
    [
        2, 1, 0,   3, 2, 0,
        4, 5, 6,   4, 6, 7,
        2, 3, 6,   7, 6, 3,
        0, 1, 4,   5, 4, 1,
        1, 2, 5,   2, 6, 5,
        4, 7, 3,   4, 3, 0,
    ];

    /// <summary>Corner index pairs: the bottom ring, the top ring, then the four uprights.</summary>
    private static readonly byte[] EdgeIndices =
    [
        0, 1,   1, 2,   2, 3,   3, 0,
        4, 5,   5, 6,   6, 7,   7, 4,
        0, 4,   1, 5,   2, 6,   3, 7,
    ];

    /// <summary>Model bounds never change, and the native behind them is a slow reflective invoke.</summary>
    private static readonly Dictionary<uint, (Vector3 Min, Vector3 Max)> BoundsByModel = [];

    public static void Draw(int entity, uint model, int red, int green, int blue, int alpha)
    {
        var (min, max) = GetBounds(model);

        var minX = min.X - Pad;
        var minY = min.Y - Pad;
        var minZ = min.Z - Pad;
        var maxX = max.X + Pad;
        var maxY = max.Y + Pad;
        var maxZ = max.Z + Pad;

        Corners[0] = Native.GetOffsetFromEntityInWorldCoords(entity, minX, minY, minZ);
        Corners[1] = Native.GetOffsetFromEntityInWorldCoords(entity, maxX, minY, minZ);
        Corners[2] = Native.GetOffsetFromEntityInWorldCoords(entity, maxX, maxY, minZ);
        Corners[3] = Native.GetOffsetFromEntityInWorldCoords(entity, minX, maxY, minZ);
        Corners[4] = Native.GetOffsetFromEntityInWorldCoords(entity, minX, minY, maxZ);
        Corners[5] = Native.GetOffsetFromEntityInWorldCoords(entity, maxX, minY, maxZ);
        Corners[6] = Native.GetOffsetFromEntityInWorldCoords(entity, maxX, maxY, maxZ);
        Corners[7] = Native.GetOffsetFromEntityInWorldCoords(entity, minX, maxY, maxZ);

        for (var i = 0; i < PolyIndices.Length; i += 3)
        {
            ref var first = ref Corners[PolyIndices[i]];
            ref var second = ref Corners[PolyIndices[i + 1]];
            ref var third = ref Corners[PolyIndices[i + 2]];

            Native.DrawPoly(
                first.X, first.Y, first.Z,
                second.X, second.Y, second.Z,
                third.X, third.Y, third.Z,
                red, green, blue, alpha);
        }

        for (var i = 0; i < EdgeIndices.Length; i += 2)
        {
            ref var from = ref Corners[EdgeIndices[i]];
            ref var to = ref Corners[EdgeIndices[i + 1]];

            Native.DrawLine(from.X, from.Y, from.Z, to.X, to.Y, to.Z, EdgeRed, EdgeGreen, EdgeBlue, EdgeAlpha);
        }
    }

    public static void ClearCache() => BoundsByModel.Clear();

    private static (Vector3 Min, Vector3 Max) GetBounds(uint model)
    {
        if (BoundsByModel.TryGetValue(model, out var bounds))
        {
            return bounds;
        }

        NativeFixer.GetModelDimensions(model, out var min, out var max);

        bounds = (min, max);
        BoundsByModel[model] = bounds;

        return bounds;
    }
}
