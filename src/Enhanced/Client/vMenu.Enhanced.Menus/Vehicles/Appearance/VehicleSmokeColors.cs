using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

public sealed class VehicleSmokeColor(string nameKey, int red, int green, int blue)
{
    public string NameKey { get; } = nameKey;

    public int Red { get; } = red;

    public int Green { get; } = green;

    public int Blue { get; } = blue;
}

// Its own table rather than the neon and headlight palette, for one reason: the game treats pure
// white as "no tyre smoke", so a white entry in the list would look like a colour and behave like an
// off switch. These are named by vMenu because the game has no text of its own for them.
public static class VehicleSmokeColors
{
    // What the game reads as no tyre smoke at all.
    public const int OffRed = 255;

    public const int OffGreen = 255;

    public const int OffBlue = 255;

    public static IReadOnlyList<VehicleSmokeColor> All { get; } =
    [
        new(Loc.VehicleOptions.SmokeRed, 244, 65, 65),
        new(Loc.VehicleOptions.SmokeOrange, 244, 167, 66),
        new(Loc.VehicleOptions.SmokeYellow, 244, 217, 65),
        new(Loc.VehicleOptions.SmokeGold, 181, 120, 0),
        new(Loc.VehicleOptions.SmokeLightGreen, 158, 255, 84),
        new(Loc.VehicleOptions.SmokeDarkGreen, 44, 94, 5),
        new(Loc.VehicleOptions.SmokeLightBlue, 65, 211, 244),
        new(Loc.VehicleOptions.SmokeDarkBlue, 24, 54, 163),
        new(Loc.VehicleOptions.SmokePurple, 108, 24, 192),
        new(Loc.VehicleOptions.SmokePink, 192, 24, 172),
        new(Loc.VehicleOptions.SmokeBlack, 1, 1, 1),
    ];

    // Whether this colour is the game's way of saying there is no smoke.
    public static bool IsOff(int red, int green, int blue) =>
        red == OffRed && green == OffGreen && blue == OffBlue;

    // The matching colour's position, or -1 for one mixed by hand.
    public static int IndexOfRgb(int red, int green, int blue)
    {
        for (var index = 0; index < All.Count; index++)
        {
            var color = All[index];

            if (color.Red == red && color.Green == green && color.Blue == blue)
            {
                return index;
            }
        }

        return -1;
    }

    public static VehicleSmokeColor? At(int index) => index >= 0 && index < All.Count ? All[index] : null;
}
