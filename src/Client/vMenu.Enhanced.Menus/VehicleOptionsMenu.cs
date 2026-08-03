using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles;

using VehicleOptionsPermissions = vMenu.Enhanced.Data.Permissions.Menus.VehicleOptions;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.VehicleOptions.Title,
    SubtitleKey = Loc.VehicleOptions.Subtitle,
    DescriptionKey = Loc.VehicleOptions.LinkDescription,
    Permission = VehicleOptionsPermissions.Menu)]
public sealed class VehicleOptionsMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.DeleteVehicle),
            Description = MenuText.Key(Loc.VehicleOptions.DeleteVehicleDescription),
            Gate = VehicleOptionsPermissions.DeleteVehicle,
            OnSelectedAsync = _ => VehicleDeletion.DeleteTargetAsync(),
        });
    }
}
