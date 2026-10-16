using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Menus.Vehicles.Sections;

using vMenu.Legacy.Hosting;

using vMenu.Enhanced.BrokenNatives;

namespace vMenu.Enhanced.Menus.Vehicles.Appearance;

internal sealed class PaintSnapshot
{
    private PaintSnapshot()
    {
    }

    public int Vehicle { get; private init; }

    public int PrimaryType { get; private init; } = -1;

    public int PrimaryIndex { get; private init; }

    public int PearlIndex { get; private init; }

    public int SecondaryType { get; private init; } = -1;

    public int SecondaryIndex { get; private init; }

    public int Primary { get; private init; }

    public int Secondary { get; private init; }

    public int Pearl { get; private init; }

    public int Wheel { get; private init; }

    public int Interior { get; private init; }

    public int Dashboard { get; private init; }

    public RgbValue? CustomPrimary { get; private init; }

    public RgbValue? CustomSecondary { get; private init; }

    public static PaintSnapshot Take(int vehicle)
    {
        var (primaryType, primaryIndex, pearlIndex) = ReadModColor1(vehicle);
        var (secondaryType, secondaryIndex) = ReadModColor2(vehicle);

        NativeFixer.GetVehicleColours(vehicle, out var primary, out var secondary);
        NativeFixer.GetVehicleExtraColours(vehicle, out var pearl, out var wheel);
        NativeFixer.GetVehicleInteriorColour(vehicle, out var interior);
        NativeFixer.GetVehicleDashboardColour(vehicle, out var dashboard);

        return new PaintSnapshot
        {
            Vehicle = vehicle,
            PrimaryType = primaryType,
            PrimaryIndex = primaryIndex,
            PearlIndex = pearlIndex,
            SecondaryType = secondaryType,
            SecondaryIndex = secondaryIndex,
            Primary = primary,
            Secondary = secondary,
            Pearl = pearl,
            Wheel = wheel,
            Interior = interior,
            Dashboard = dashboard,
            CustomPrimary = ReadCustomPrimary(vehicle),
            CustomSecondary = ReadCustomSecondary(vehicle),
        };
    }

    public void Restore()
    {
        if (!API.DoesEntityExist(Vehicle))
        {
            return;
        }

        // The mod colour natives only record which shop paint is on, the colours below are what is drawn.
        if (PrimaryType >= 0)
        {
            API.SetVehicleModColor_1(Vehicle, PrimaryType, PrimaryIndex, PearlIndex);
        }

        if (SecondaryType >= 0)
        {
            API.SetVehicleModColor_2(Vehicle, SecondaryType, SecondaryIndex);
        }

        API.SetVehicleColours(Vehicle, Primary, Secondary);
        API.SetVehicleExtraColours(Vehicle, Pearl, Wheel);
        API.SetVehicleInteriorColour(Vehicle, Interior);
        API.SetVehicleDashboardColour(Vehicle, Dashboard);

        RestoreCustomPrimary();
        RestoreCustomSecondary();
    }

    public void RestoreCustomPrimary()
    {
        if (CustomPrimary is { } rgb)
        {
            API.SetVehicleCustomPrimaryColour(Vehicle, rgb.Red, rgb.Green, rgb.Blue);
        }
        else
        {
            NativeFixer.ClearVehicleCustomPrimaryColour(Vehicle);
        }
    }

    public void RestoreCustomSecondary()
    {
        if (CustomSecondary is { } rgb)
        {
            API.SetVehicleCustomSecondaryColour(Vehicle, rgb.Red, rgb.Green, rgb.Blue);
        }
        else
        {
            NativeFixer.ClearVehicleCustomSecondaryColour(Vehicle);
        }
    }

    // The generated wrapper throws instead of returning zero for an output slot the game left unfilled.
    private static (int Type, int Index, int Pearl) ReadModColor1(int vehicle)
    {
        try
        {
            NativeFixer.GetVehicleModColor_1(vehicle, out var type, out var index, out var pearl);

            return (type, index, pearl);
        }
        catch (Exception)
        {
            return (-1, 0, 0);
        }
    }

    private static (int Type, int Index) ReadModColor2(int vehicle)
    {
        try
        {
            NativeFixer.GetVehicleModColor_2(vehicle, out var type, out var index);

            return (type, index);
        }
        catch (Exception)
        {
            return (-1, 0);
        }
    }

    private static RgbValue? ReadCustomPrimary(int vehicle)
    {
        if (!API.GetIsVehiclePrimaryColourCustom(vehicle))
        {
            return null;
        }

        NativeFixer.GetVehicleCustomPrimaryColour(vehicle, out var red, out var green, out var blue);

        return new RgbValue(red, green, blue);
    }

    private static RgbValue? ReadCustomSecondary(int vehicle)
    {
        if (!API.GetIsVehicleSecondaryColourCustom(vehicle))
        {
            return null;
        }

        NativeFixer.GetVehicleCustomSecondaryColour(vehicle, out var red, out var green, out var blue);

        return new RgbValue(red, green, blue);
    }
}
