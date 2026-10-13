using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using PersonalVehiclePermissions = vMenu.Enhanced.Data.Permissions.Menus.PersonalVehicle;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

[VMenu(
    TitleKey = Loc.PersonalVehicle.Title,
    SubtitleKey = Loc.PersonalVehicle.WindowsSubtitle,
    DescriptionKey = Loc.PersonalVehicle.WindowsDescription,
    Permission = PersonalVehiclePermissions.Windows)]
public sealed class PersonalVehicleWindowsMenu : MenuDefinition
{
    public override MenuText LinkText => MenuText.Key(Loc.PersonalVehicle.Windows);

    private const int RollUp = 0;

    private static readonly string[] WindowNames =
    [
        Loc.VehicleOptions.WindowFrontLeft,
        Loc.VehicleOptions.WindowFrontRight,
        Loc.VehicleOptions.WindowRearLeft,
        Loc.VehicleOptions.WindowRearRight,
    ];

    protected override void Build(MenuBuilder menu)
    {
        menu.AddRange(Rows());

        PersonalVehicleRows.AutoFill(menu, Rows);
    }

    private static IReadOnlyList<MenuEntry> Rows()
    {
        if (!PersonalVehicle.IsMarked)
        {
            return [PersonalVehicleRows.NoneMarked()];
        }

        var rows = new List<MenuEntry>();

        for (var window = 0; window < RemoteVehicleAction.WindowCount; window++)
        {
            var index = window;

            rows.Add(new ListEntry
            {
                Text = MenuText.Key(WindowNames[window]),
                Description = MenuText.Key(Loc.PersonalVehicle.WindowDescription),
                Options =
                [
                    MenuText.Key(Loc.PersonalVehicle.WindowRollUp),
                    MenuText.Key(Loc.PersonalVehicle.WindowRollDown),
                ],
                OnSelectedAsync = selected =>
                    PersonalVehicle.SetWindowAsync(index, selected.SelectedIndex == RollUp),
            });
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.RollDownAllWindows),
            Description = MenuText.Key(Loc.PersonalVehicle.RollDownAllWindowsDescription),
            OnSelectedAsync = _ => PersonalVehicle.SetAllWindowsAsync(up: false),
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.RollUpAllWindows),
            Description = MenuText.Key(Loc.PersonalVehicle.RollUpAllWindowsDescription),
            OnSelectedAsync = _ => PersonalVehicle.SetAllWindowsAsync(up: true),
        });

        return rows;
    }
}
