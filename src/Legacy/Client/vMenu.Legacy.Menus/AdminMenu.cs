using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
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
        | AdminPermissions.ResetRoutingBucket
        | AdminPermissions.SeePrivateMessages;

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
        menu.Entries.Add(new SeparatorEntry(Loc.Admin.PlayersGroup)
        {
            Gate = PlayersGate,
        });

        menu.Entries.Add(new ButtonEntry(Loc.Admin.Freeze)
        {
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

        menu.Entries.Add(new ButtonEntry(Loc.Admin.ResetRoutingBucket)
        {
            Gate = AdminPermissions.ResetRoutingBucket,
            OnSelectedAsync = _ => AdminPlayerActions.ResetRoutingBucketAsync(),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.Admin.SeePrivateMessages)
        {
            Gate = AdminPermissions.SeePrivateMessages,
            ReadState = () => UserDefaults.AdminSeePrivateMessages.Value,
            OnChanged = changed => PrivateMessageWatch.SetWatching(changed.Checked),
        });

        menu.Entries.Add(new SeparatorEntry(Loc.Admin.VehiclesGroup)
        {
            Gate = VehiclesGate,
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.Admin.DeleteVehicle)
        {
            ConfirmationDescription = MenuText.Key(Loc.VehicleOptions.DeleteVehicleConfirm),
            Gate = AdminPermissions.DeleteVehicle,
            OnConfirmedAsync = _ => VehicleDeletion.DeleteTargetAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.Admin.DeleteEmptyVehicles)
        {
            ConfirmationDescription = MenuText.Key(Loc.Admin.DeleteEmptyVehiclesConfirm),
            Gate = AdminPermissions.DeleteEmptyVehicles,
            OnConfirmedAsync = _ => AdminVehicleActions.DeleteEmptyAsync(),
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.Admin.DeleteAllVehicles)
        {
            ConfirmationDescription = MenuText.Key(Loc.Admin.DeleteAllVehiclesConfirm),
            Gate = AdminPermissions.DeleteAllVehicles,
            OnConfirmedAsync = _ => AdminVehicleActions.DeleteEverythingAsync(),
        });

        menu.Entries.Add(new SeparatorEntry(Loc.Admin.ServerGroup)
        {
            Gate = ServerGate,
        });

        menu.Entries.Add(new ConfirmButtonEntry(Loc.Admin.ClearArea)
        {
            ConfirmationDescription = MenuText.Key(Loc.Admin.ClearAreaConfirm),
            Gate = AdminPermissions.ClearArea,
            OnConfirmedAsync = _ => ClearArea.RequestAsync(),
        });

        menu.Entries.Add(new ButtonEntry(Loc.Admin.Announce)
        {
            Gate = AdminPermissions.Announce,
            OnSelectedAsync = _ => Announcements.SendAsync(),
        });

        menu.Entries.Add(SubmenuEntry.For(new ScheduledAnnouncementsMenu()));

        menu.Entries.Add(new ConfirmButtonEntry(Loc.Admin.RefreshPermissions)
        {
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
