using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus.Vehicles.Sections;

internal static class DamageSection
{
    private const int HealthStep = 100;

    private const int HealthStops = 10;

    public static void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows(menu));

        SectionRows.AutoFill(menu, () => Rows(menu));
    }

    private static List<MenuEntry> Rows(MenuBuilder menu)
    {
        if (SectionRows.Driven() is not { } handle)
        {
            return SectionRows.BlockedOnly();
        }

        return
        [
            new SliderEntry(Loc.VehicleOptions.EngineHealth, 0, HealthStops)
            {
                Gate = VehicleOptionsPermissions.EngineHealth,
                ReadPosition = () => (int)MathF.Round(VehicleEngine.Health() / HealthStep),
                OnMoved = moved => VehicleEngine.SetHealth(moved.NewPosition * HealthStep),
            },
            new ConfirmButtonEntry(Loc.VehicleOptions.DestroyEngine)
            {
                ConfirmationDescription = MenuText.Key(Loc.VehicleOptions.DestroyEngineConfirm),
                Gate = VehicleOptionsPermissions.EngineHealth,
                OnConfirmed = _ =>
                {
                    VehicleEngine.Destroy();

                    menu.ReplaceEntries(Rows(menu));
                },
            },
            TyreRow(handle),
        ];
    }

    private static ListEntry TyreRow(int handle)
    {
        var wheels = VehicleTyres.Present(handle);

        var options = new List<MenuText> { MenuText.Key(Loc.VehicleOptions.AllTyres) };

        foreach (var wheel in wheels)
        {
            options.Add(MenuText.Key(
                Loc.VehicleOptions.TyreNumbered,
                ("number", MenuText.Literal((wheel + 1).ToString()))));
        }

        return new ListEntry(Loc.VehicleOptions.Tyres, options)
        {
            Gate = VehicleOptionsPermissions.Tyres,
            OnSelected = selected =>
            {
                if (SectionRows.Driven() is not { } vehicle)
                {
                    return;
                }

                if (selected.SelectedIndex == 0)
                {
                    VehicleTyres.ToggleAll(vehicle);

                    return;
                }

                var wheel = selected.SelectedIndex - 1;

                if (wheel < wheels.Count)
                {
                    VehicleTyres.Toggle(vehicle, wheels[wheel]);
                }
            },
        };
    }
}
