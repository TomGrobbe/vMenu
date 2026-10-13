using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

using PersonalVehiclePermissions = vMenu.Enhanced.Data.Permissions.Menus.PersonalVehicle;
using SavedVehiclesPermissions = vMenu.Enhanced.Data.Permissions.Menus.SavedVehicles;
using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;
using VehicleSpawnerPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleSpawner;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.VehiclesMenu.Title,
    SubtitleKey = Loc.VehiclesMenu.Subtitle,
    DescriptionKey = Loc.VehiclesMenu.LinkDescription)]
public sealed class VehiclesMenu : MenuDefinition
{
    // Open to anybody who can reach at least one of the menus inside it. Deliberately not a permission
    // of its own: existing servers would have to grant a new one before the vehicle menus they already
    // allow came back.
    public override MenuGate Gate { get; } =
        MenuGate.Permission(VehicleOptionsPermissions.Menu)
        | MenuGate.Permission(PersonalVehiclePermissions.Menu)
        | MenuGate.Permission(VehicleSpawnerPermissions.Menu)
        | MenuGate.Permission(SavedVehiclesPermissions.Menu);

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(SubmenuEntry.For(new VehicleOptionsMenu()));
        menu.Entries.Add(SubmenuEntry.For(new VehicleSpawnerMenu()));
        menu.Entries.Add(SubmenuEntry.For(new SavedVehiclesMenu()));
        menu.Entries.Add(SubmenuEntry.For(new PersonalVehicleMenu()));
    }
}
