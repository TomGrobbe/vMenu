using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Menus.Vehicles.Sections;

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
        // Every section fills itself from the vehicle the player is in when it opens, so the rows
        // here only say which menu goes where and who may open it.
        menu.Entries.Add(Section(
            Loc.VehicleOptions.Modifications,
            Loc.VehicleOptions.ModificationsDescription,
            Loc.VehicleOptions.ModificationsSubtitle,
            VehicleOptionsPermissions.Modify,
            ModsSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Wheels,
            Loc.VehicleOptions.WheelsDescription,
            Loc.VehicleOptions.WheelsSubtitle,
            VehicleOptionsPermissions.Wheels,
            WheelsSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Colors,
            Loc.VehicleOptions.ColorsDescription,
            Loc.VehicleOptions.ColorsSubtitle,
            VehicleOptionsPermissions.Colors,
            ColorsSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Liveries,
            Loc.VehicleOptions.LiveriesDescription,
            Loc.VehicleOptions.LiveriesSubtitle,
            VehicleOptionsPermissions.Liveries,
            LiveriesSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Extras,
            Loc.VehicleOptions.ExtrasDescription,
            Loc.VehicleOptions.ExtrasSubtitle,
            VehicleOptionsPermissions.Extras,
            ExtrasSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Lights,
            Loc.VehicleOptions.LightsDescription,
            Loc.VehicleOptions.LightsSubtitle,
            VehicleOptionsPermissions.Lights,
            LightsSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Neon,
            Loc.VehicleOptions.NeonDescription,
            Loc.VehicleOptions.NeonSubtitle,
            VehicleOptionsPermissions.Neon,
            NeonSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Plate,
            Loc.VehicleOptions.PlateDescription,
            Loc.VehicleOptions.PlateSubtitle,
            VehicleOptionsPermissions.Plate,
            PlateSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Doors,
            Loc.VehicleOptions.DoorsDescription,
            Loc.VehicleOptions.DoorsSubtitle,
            VehicleOptionsPermissions.Doors,
            DoorsSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Windows,
            Loc.VehicleOptions.WindowsDescription,
            Loc.VehicleOptions.WindowsSubtitle,
            VehicleOptionsPermissions.Windows,
            WindowsSection.Build));

        menu.Entries.Add(DirtSection.Row(VehicleOptionsPermissions.Dirt));

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

        SectionRows.AutoRefresh(menu, MenuRegistry.RefreshAll);
    }

    private static SubmenuEntry Section(
        string textKey,
        string descriptionKey,
        string subtitleKey,
        string permission,
        Action<MenuBuilder> build) => new()
        {
            Text = MenuText.Key(textKey),
            Description = MenuText.Key(descriptionKey),
            MenuTitle = MenuText.Key(Loc.VehicleOptions.Title),
            MenuSubtitle = MenuText.Key(subtitleKey),
            Gate = permission,
            Build = build,
        };
}
