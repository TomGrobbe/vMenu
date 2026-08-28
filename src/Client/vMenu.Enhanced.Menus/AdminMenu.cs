using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Admin;
using vMenu.Enhanced.Menus.Vehicles;
using vMenu.Enhanced.Storage;

using AdminPermissions = vMenu.Enhanced.Data.Permissions.Menus.Admin;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.Admin.Title,
    SubtitleKey = Loc.Admin.Subtitle,
    DescriptionKey = Loc.Admin.LinkDescription,
    Permission = AdminPermissions.Menu)]
public sealed class AdminMenu : MenuDefinition
{
    public override GateBehaviour? LinkBehaviour => GateBehaviour.Hide;

    public override GateBehaviour? DefaultGateBehaviour => GateBehaviour.Hide;

    private static MenuGate PlayersGate =>
        MenuGate.Permission(AdminPermissions.FreezePlayer)
        | AdminPermissions.GrabPlayer
        | AdminPermissions.SeeNoClipPlayers;

    private static MenuGate VehiclesGate =>
        MenuGate.Permission(AdminPermissions.DeleteVehicle)
        | AdminPermissions.DeleteEmptyVehicles
        | AdminPermissions.DeleteAllVehicles;

    private static MenuGate ServerGate =>
        MenuGate.Permission(AdminPermissions.ClearArea)
        | AdminPermissions.Announce
        | AdminPermissions.ManageAnnouncements
        | AdminPermissions.RefreshPermissions;

    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new SeparatorEntry
        {
            Text = MenuText.Key(Loc.Admin.PlayersGroup),
            Description = MenuText.Key(Loc.Admin.PlayersGroupDescription),
            Gate = PlayersGate,
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.Freeze),
            Description = MenuText.Key(Loc.Admin.FreezeDescription),
            Gate = AdminPermissions.FreezePlayer,
            OnSelectedAsync = _ => AdminPlayerActions.ToggleFreezeAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.From(() => Localizer.Current.Get(
                AdminPlayerActions.IsCarrying ? Loc.Admin.Release : Loc.Admin.Grab)),
            Description = MenuText.From(() => Localizer.Current.Get(
                AdminPlayerActions.IsCarrying ? Loc.Admin.ReleaseDescription : Loc.Admin.GrabDescription)),
            Gate = AdminPermissions.GrabPlayer,
            OnSelectedAsync = _ => GrabAsync(menu),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.Admin.SeeNoClipPlayers),
            Description = MenuText.Key(Loc.Admin.SeeNoClipPlayersDescription),
            Gate = AdminPermissions.SeeNoClipPlayers,
            ReadState = () => UserDefaults.AdminSeeNoClipPlayers.Value,
            OnChanged = changed => UserDefaults.AdminSeeNoClipPlayers.Value = changed.Checked,
        });

        menu.Entries.Add(new SeparatorEntry
        {
            Text = MenuText.Key(Loc.Admin.VehiclesGroup),
            Description = MenuText.Key(Loc.Admin.VehiclesGroupDescription),
            Gate = VehiclesGate,
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.DeleteVehicle),
            Description = MenuText.Key(Loc.Admin.DeleteVehicleDescription),
            ConfirmationDescription = MenuText.Key(Loc.VehicleOptions.DeleteVehicleConfirm),
            Gate = AdminPermissions.DeleteVehicle,
            OnConfirmedAsync = _ => VehicleDeletion.DeleteTargetAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.DeleteEmptyVehicles),
            Description = MenuText.Key(Loc.Admin.DeleteEmptyVehiclesDescription),
            ConfirmationDescription = MenuText.Key(Loc.Admin.DeleteEmptyVehiclesConfirm),
            Gate = AdminPermissions.DeleteEmptyVehicles,
            OnConfirmedAsync = _ => AdminVehicleActions.DeleteEmptyAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.DeleteAllVehicles),
            Description = MenuText.Key(Loc.Admin.DeleteAllVehiclesDescription),
            ConfirmationDescription = MenuText.Key(Loc.Admin.DeleteAllVehiclesConfirm),
            Gate = AdminPermissions.DeleteAllVehicles,
            OnConfirmedAsync = _ => AdminVehicleActions.DeleteEverythingAsync(),
        });

        menu.Entries.Add(new SeparatorEntry
        {
            Text = MenuText.Key(Loc.Admin.ServerGroup),
            Description = MenuText.Key(Loc.Admin.ServerGroupDescription),
            Gate = ServerGate,
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.ClearArea),
            Description = MenuText.Key(Loc.Admin.ClearAreaDescription),
            ConfirmationDescription = MenuText.Key(Loc.Admin.ClearAreaConfirm),
            Gate = AdminPermissions.ClearArea,
            OnConfirmedAsync = _ => ClearArea.RequestAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.Announce),
            Description = MenuText.Key(Loc.Admin.AnnounceDescription),
            Gate = AdminPermissions.Announce,
            OnSelectedAsync = _ => Announcements.SendAsync(),
        });

        menu.Entries.Add(SubmenuEntry.For(new ScheduledAnnouncementsMenu()));

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.Admin.RefreshPermissions),
            Description = MenuText.Key(Loc.Admin.RefreshPermissionsDescription),
            Gate = AdminPermissions.RefreshPermissions,
            OnConfirmedAsync = _ => AdminPlayerActions.RefreshEveryonesPermissionsAsync(),
        });
    }

    private static async Task GrabAsync(MenuBuilder menu)
    {
        await AdminPlayerActions.ToggleHoldAsync();

        MenuRegistry.Refresh(menu.Menu);
    }
}
