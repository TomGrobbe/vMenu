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
            Text = MenuText.Key(Loc.VehicleOptions.RepairVehicle),
            Description = MenuText.Key(Loc.VehicleOptions.RepairVehicleDescription),
            Gate = VehicleOptionsPermissions.RepairVehicle,
            OnSelected = _ => VehicleRepair.RepairCurrent(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.WashVehicle),
            Description = MenuText.Key(Loc.VehicleOptions.WashVehicleDescription),
            Gate = VehicleOptionsPermissions.WashVehicle,
            OnSelected = _ => VehicleWash.WashCurrent(),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.DeleteVehicle),
            Description = MenuText.Key(Loc.VehicleOptions.DeleteVehicleDescription),
            ConfirmationDescription = MenuText.Key(Loc.VehicleOptions.DeleteVehicleConfirm),
            Gate = VehicleOptionsPermissions.DeleteVehicle,
            OnConfirmedAsync = _ => VehicleDeletion.DeleteTargetAsync(),
        });
    }
}
