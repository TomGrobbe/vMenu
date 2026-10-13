using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class AircraftBoatSection
{
    private const int Stops = VehicleTurbulence.Stock / VehicleTurbulence.Step;

    public static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(TurbulenceRow(
            Loc.VehicleOptions.HelicopterTurbulence,
            Loc.VehicleOptions.HelicopterTurbulenceDescription,
            static () => VehicleTurbulence.Helicopter,
            VehicleTurbulence.SetHelicopter));

        menu.Entries.Add(TurbulenceRow(
            Loc.VehicleOptions.PlaneTurbulence,
            Loc.VehicleOptions.PlaneTurbulenceDescription,
            static () => VehicleTurbulence.Plane,
            VehicleTurbulence.SetPlane));

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.AnchorBoat),
            Description = MenuText.Key(Loc.VehicleOptions.AnchorBoatDescription),
            Gate = VehicleOptionsPermissions.AnchorBoat,
            ReadEnabled = VehicleAnchor.CanAnchorHere,
            ReadState = () => VehicleAnchor.Enabled,
            OnChanged = changed => VehicleAnchor.SetEnabled(changed.Checked),
        });

        SectionRows.AutoRefresh(menu, () => MenuRegistry.Refresh(menu.Menu));
    }

    private static SliderEntry TurbulenceRow(
        string textKey,
        string descriptionKey,
        Func<int> read,
        Action<int> write) => new()
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            Gate = VehicleOptionsPermissions.Turbulence,
            Min = 0,
            Max = Stops,
            ReadPosition = () => read() / VehicleTurbulence.Step,
            OnMoved = moved => write(moved.NewPosition * VehicleTurbulence.Step),
        };
}
