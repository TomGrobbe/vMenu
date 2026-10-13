using vMenu.Enhanced.Data.VehicleData;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using PersonalVehiclePermissions = vMenu.Enhanced.Data.Permissions.Menus.PersonalVehicle;

namespace vMenu.Enhanced.Menus.Vehicles.Personal;

[VMenu(
    TitleKey = Loc.PersonalVehicle.Title,
    SubtitleKey = Loc.PersonalVehicle.DoorsSubtitle,
    DescriptionKey = Loc.PersonalVehicle.DoorsDescription,
    Permission = PersonalVehiclePermissions.Doors)]
public sealed class PersonalVehicleDoorsMenu : MenuDefinition
{
    public override MenuText LinkText => MenuText.Key(Loc.PersonalVehicle.Doors);

    private static readonly string[] DoorNames =
    [
        Loc.VehicleOptions.DoorFrontLeft,
        Loc.VehicleOptions.DoorFrontRight,
        Loc.VehicleOptions.DoorRearLeft,
        Loc.VehicleOptions.DoorRearRight,
        Loc.VehicleOptions.DoorHood,
        Loc.VehicleOptions.DoorTrunk,
        Loc.VehicleOptions.DoorExtraLeft,
        Loc.VehicleOptions.DoorExtraRight,
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

        for (var door = 0; door < RemoteVehicleAction.DoorCount; door++)
        {
            if (!Has(door))
            {
                continue;
            }

            var index = door;

            rows.Add(new ButtonEntry
            {
                Text = MenuText.Key(DoorNames[door]),
                Description = MenuText.Key(Loc.PersonalVehicle.DoorDescription),
                OnSelectedAsync = _ => PersonalVehicle.ToggleDoorAsync(index),
            });
        }

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.OpenAllDoors),
            Description = MenuText.Key(Loc.PersonalVehicle.OpenAllDoorsDescription),
            OnSelectedAsync = _ => PersonalVehicle.SetAllDoorsAsync(open: true),
        });

        rows.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.PersonalVehicle.ShutAllDoors),
            Description = MenuText.Key(Loc.PersonalVehicle.ShutAllDoorsDescription),
            OnSelectedAsync = _ => PersonalVehicle.SetAllDoorsAsync(open: false),
        });

        return rows;
    }

    private static bool Has(int door) =>
        PersonalVehicle.DoorMask == 0 || (PersonalVehicle.DoorMask & (1 << door)) != 0;
}
