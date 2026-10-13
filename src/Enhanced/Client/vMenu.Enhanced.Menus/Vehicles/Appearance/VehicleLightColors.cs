namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

public sealed class VehicleLightColor(int index, string gxtKey, int red, int green, int blue)
{
    // What SetVehicleHeadlightsColour wants, and the position in the neon list.
    public int Index { get; } = index;

    public string GxtKey { get; } = gxtKey;

    public int Red { get; } = red;

    public int Green { get; } = green;

    public int Blue { get; } = blue;
}

// Headlights are set by index and neon tubes by red, green and blue, so both are carried here. The
// tyre smoke list uses the same palette, since the game names these colours and does not name a
// separate set for smoke.
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

    // What SetVehicleHeadlightsColour wants for the headlights the vehicle came with.
    public const int DefaultHeadlightColor = 255;

    // The colour whose red, green and blue match exactly, or -1 for a mix of its own.
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
