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
        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.GodMode),
            Description = MenuText.Key(Loc.VehicleOptions.GodModeDescription),
            Gate = VehicleOptionsPermissions.God,
            ReadState = () => VehicleGodMode.Enabled,
            OnChanged = changed => VehicleGodMode.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.GodModeOptions),
            Description = MenuText.Key(Loc.VehicleOptions.GodModeOptionsDescription),
            MenuTitle = MenuText.Key(Loc.VehicleOptions.Title),
            MenuSubtitle = MenuText.Key(Loc.VehicleOptions.GodModeSubtitle),
            Gate = VehicleOptionsPermissions.God,
            Build = GodModeSection.Build,
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.RepairVehicle),
            Description = MenuText.Key(Loc.VehicleOptions.RepairVehicleDescription),
            Gate = VehicleOptionsPermissions.RepairVehicle,
            OnSelectedAsync = _ => VehicleRepair.RepairCurrentAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.WashVehicle),
            Description = MenuText.Key(Loc.VehicleOptions.WashVehicleDescription),
            Gate = VehicleOptionsPermissions.WashVehicle,
            OnSelected = _ => VehicleWash.WashCurrent(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.CycleSeat),
            Description = MenuText.Key(Loc.VehicleOptions.CycleSeatDescription),
            Gate = VehicleOptionsPermissions.CycleSeat,
            ReadEnabled = () => VehicleSeatCycle.CanCycle,
            OnSelected = _ => VehicleSeatCycle.CycleToNextFreeSeat(),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.KeepClean),
            Description = MenuText.Key(Loc.VehicleOptions.KeepCleanDescription),
            Gate = VehicleOptionsPermissions.KeepClean,
            ReadState = () => VehicleKeepClean.Enabled,
            OnChanged = changed => VehicleKeepClean.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(DirtSection.Row(VehicleOptionsPermissions.Dirt));

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

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Engine,
            Loc.VehicleOptions.EngineDescription,
            Loc.VehicleOptions.EngineSubtitle,
            VehicleOptionsPermissions.Engine,
            EngineSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Handling,
            Loc.VehicleOptions.HandlingDescription,
            Loc.VehicleOptions.HandlingSubtitle,
            VehicleOptionsPermissions.Handling,
            HandlingSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Damage,
            Loc.VehicleOptions.DamageDescription,
            Loc.VehicleOptions.DamageSubtitle,
            VehicleOptionsPermissions.Damage,
            DamageSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Aircraft,
            Loc.VehicleOptions.AircraftDescription,
            Loc.VehicleOptions.AircraftSubtitle,
            VehicleOptionsPermissions.Aircraft,
            AircraftBoatSection.Build));

        menu.Entries.Add(Section(
            Loc.VehicleOptions.Radio,
            Loc.VehicleOptions.RadioDescription,
            Loc.VehicleOptions.RadioSubtitle,
            VehicleOptionsPermissions.Radio,
            RadioSection.Build));

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.VehicleOptions.DeleteVehicle),
            Description = MenuText.Key(Loc.VehicleOptions.DeleteVehicleDescription),
            ConfirmationDescription = MenuText.Key(Loc.VehicleOptions.DeleteVehicleConfirm),
            Gate = VehicleOptionsPermissions.DeleteVehicle,
            OnConfirmedAsync = _ => VehicleDeletion.DeleteDrivenAsync(),
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
