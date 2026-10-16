using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Menus.Vehicles.Sections;

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

        Native.GetVehicleColours(vehicle, out var primary, out var secondary);
        Native.GetVehicleExtraColours(vehicle, out var pearl, out var wheel);
        Native.GetVehicleInteriorColour(vehicle, out var interior);
        Native.GetVehicleDashboardColour(vehicle, out var dashboard);

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
        if (!Native.DoesEntityExist(Vehicle))
        {
            return;
        }

        // The mod colour natives only record which shop paint is on, the colours below are what is drawn.
        if (PrimaryType >= 0)
        {
            Native.SetVehicleModColor_1(Vehicle, PrimaryType, PrimaryIndex, PearlIndex);
        }

        if (SecondaryType >= 0)
        {
            Native.SetVehicleModColor_2(Vehicle, SecondaryType, SecondaryIndex);
        }

        Native.SetVehicleColours(Vehicle, Primary, Secondary);
        Native.SetVehicleExtraColours(Vehicle, Pearl, Wheel);
        Native.SetVehicleInteriorColour(Vehicle, Interior);
        Native.SetVehicleDashboardColour(Vehicle, Dashboard);

        RestoreCustomPrimary();
        RestoreCustomSecondary();
    }

    public void RestoreCustomPrimary()
    {
        if (CustomPrimary is { } rgb)
        {
            Native.SetVehicleCustomPrimaryColour(Vehicle, rgb.Red, rgb.Green, rgb.Blue);
        }
        else
        {
            Native.ClearVehicleCustomPrimaryColour(Vehicle);
        }
    }

    public void RestoreCustomSecondary()
    {
        if (CustomSecondary is { } rgb)
        {
            Native.SetVehicleCustomSecondaryColour(Vehicle, rgb.Red, rgb.Green, rgb.Blue);
        }
        else
        {
            Native.ClearVehicleCustomSecondaryColour(Vehicle);
        }
    }

    // The generated wrapper throws instead of returning zero for an output slot the game left unfilled.
    private static (int Type, int Index, int Pearl) ReadModColor1(int vehicle)
    {
        try
        {
            Native.GetVehicleModColor_1(vehicle, out var type, out var index, out var pearl);

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
            Native.GetVehicleModColor_2(vehicle, out var type, out var index);

            return (type, index);
        }
        catch (Exception)
        {
            return (-1, 0);
        }
    }

    private static RgbValue? ReadCustomPrimary(int vehicle)
    {
        if (!Native.GetIsVehiclePrimaryColourCustom(vehicle))
        {
            return null;
        }

        Native.GetVehicleCustomPrimaryColour(vehicle, out var red, out var green, out var blue);

        return new RgbValue(red, green, blue);
    }

    private static RgbValue? ReadCustomSecondary(int vehicle)
    {
        if (!Native.GetIsVehicleSecondaryColourCustom(vehicle))
        {
            return null;
        }

        Native.GetVehicleCustomSecondaryColour(vehicle, out var red, out var green, out var blue);

        return new RgbValue(red, green, blue);
    }
}
