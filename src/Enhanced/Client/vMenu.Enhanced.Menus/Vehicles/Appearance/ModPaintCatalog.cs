using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

internal enum ModPaintType
{
    Metallic = 0,
    Classic = 1,
    Pearlescent = 2,
    Matte = 3,
    Metals = 4,
    Chrome = 5,
    Chameleon = 6,
}

internal sealed class ModPaint(ModPaintType type, int index, int paint, int pearl, string label)
{
    public ModPaintType Type { get; } = type;

    public int Index { get; } = index;

    public int Paint { get; } = paint;

    public int Pearl { get; } = pearl;

    public string Label { get; } = label;
}

// The game keeps the mod shop paints in its own data files and the shop reads them back through a
// vehicle, so this does the same rather than copying a list that differs per game build.
internal static class ModPaintCatalog
{
    private static readonly List<ModPaint>?[] Cache = new List<ModPaint>?[7];

    public static IReadOnlyList<ModPaint> Load(int vehicle, ModPaintType type)
    {
        if (Cache[(int)type] is { } cached)
        {
            return cached;
        }

        var count = Native.GetNumModColors((int)type, true);
        var paints = new List<ModPaint>(Math.Max(0, count));

        if (count > 0)
        {
            var snapshot = PaintSnapshot.Take(vehicle);

            for (var index = 0; index < count; index++)
            {
                Native.SetVehicleModColor_1(vehicle, (int)type, index, 0);
                Native.GetVehicleColours(vehicle, out var paint, out _);
                Native.GetVehicleExtraColours(vehicle, out var pearl, out _);

                paints.Add(new ModPaint(type, index, paint, pearl, Name(vehicle)));
            }

            snapshot.Restore();
        }

        Cache[(int)type] = paints;

        return paints;
    }

    public static ModPaint? FindCached(int paint)
    {
        foreach (var paints in Cache)
        {
            if (paints is null)
            {
                continue;
            }

            foreach (var entry in paints)
            {
                if (entry.Paint == paint)
                {
                    return entry;
                }
            }
        }

        return null;
    }

    private static string Name(int vehicle)
    {
        try
        {
            return Native.GetVehicleModColor_1Name(vehicle, false) ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
