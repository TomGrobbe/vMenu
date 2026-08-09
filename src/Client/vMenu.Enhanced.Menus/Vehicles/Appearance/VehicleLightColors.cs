namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

/// <summary>One of the game's thirteen light colours, and what it is in red, green and blue.</summary>
public sealed class VehicleLightColor(int index, string gxtKey, int red, int green, int blue)
{
    /// <summary>What <c>SetVehicleHeadlightsColour</c> wants, and the position in the neon list.</summary>
    public int Index { get; } = index;

    public string GxtKey { get; } = gxtKey;

    public int Red { get; } = red;

    public int Green { get; } = green;

    public int Blue { get; } = blue;
}

/// <summary>
/// The thirteen colours the game's mod shop offers for neon tubes and xenon headlights.
/// </summary>
/// <remarks>
/// Headlights are set by index and neon tubes by red, green and blue, so both are carried here. The
/// tyre smoke list uses the same palette, since the game names these colours and does not name a
/// separate set for smoke.
/// </remarks>
public static class VehicleLightColors
{
    public static IReadOnlyList<VehicleLightColor> All { get; } =
    [
        new(0, "CMOD_NEONCOL_0", 255, 255, 255),
        new(1, "CMOD_NEONCOL_1", 2, 21, 255),
        new(2, "CMOD_NEONCOL_2", 3, 83, 255),
        new(3, "CMOD_NEONCOL_3", 0, 255, 140),
        new(4, "CMOD_NEONCOL_4", 94, 255, 1),
        new(5, "CMOD_NEONCOL_5", 255, 255, 0),
        new(6, "CMOD_NEONCOL_6", 255, 150, 5),
        new(7, "CMOD_NEONCOL_7", 255, 62, 0),
        new(8, "CMOD_NEONCOL_8", 255, 0, 0),
        new(9, "CMOD_NEONCOL_9", 255, 50, 100),
        new(10, "CMOD_NEONCOL_10", 255, 5, 190),
        new(11, "CMOD_NEONCOL_11", 35, 1, 255),
        new(12, "CMOD_NEONCOL_12", 15, 3, 255),
    ];

    /// <summary>What <c>SetVehicleHeadlightsColour</c> wants for the headlights the vehicle came with.</summary>
    public const int DefaultHeadlightColor = 255;

    /// <summary>The colour whose red, green and blue match exactly, or -1 for a mix of its own.</summary>
    public static int IndexOfRgb(int red, int green, int blue)
    {
        foreach (var color in All)
        {
            if (color.Red == red && color.Green == green && color.Blue == blue)
            {
                return color.Index;
            }
        }

        return -1;
    }

    public static VehicleLightColor? At(int index) =>
        index >= 0 && index < All.Count ? All[index] : null;
}
