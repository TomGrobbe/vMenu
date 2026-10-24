using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class AircraftBoatSection
{
    private const int Stops = VehicleTurbulence.Stock / VehicleTurbulence.Step;

    public static void Build(MenuBuilder menu)
    {
        menu.Entries.Add(TurbulenceRow(
            Loc.VehicleOptions.HelicopterTurbulence,
            static () => VehicleTurbulence.Helicopter,
            VehicleTurbulence.SetHelicopter));

        menu.Entries.Add(TurbulenceRow(
            Loc.VehicleOptions.PlaneTurbulence,
            static () => VehicleTurbulence.Plane,
            VehicleTurbulence.SetPlane));

        menu.Entries.Add(new CheckboxEntry(Loc.VehicleOptions.AnchorBoat)
        {
            Gate = VehicleOptionsPermissions.AnchorBoat,
            ReadEnabled = VehicleAnchor.CanAnchorHere,
            ReadState = () => VehicleAnchor.Enabled,
            OnChanged = changed => VehicleAnchor.SetEnabled(changed.Checked),
        });

        SectionRows.AutoRefresh(menu, () => MenuRegistry.Refresh(menu.Menu));
    }

    private static SliderEntry TurbulenceRow(
        string textKey,
        Func<int> read,
        Action<int> write) => new(textKey, 0, Stops)
        {
            Gate = VehicleOptionsPermissions.Turbulence,
            ReadPosition = () => read() / VehicleTurbulence.Step,
            OnMoved = moved => write(moved.NewPosition * VehicleTurbulence.Step),
        };
}
