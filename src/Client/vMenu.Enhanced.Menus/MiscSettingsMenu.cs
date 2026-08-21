using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Storage;

using MiscSettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.MiscSettings;
using StaffAlertSettings = vMenu.Enhanced.Data.Configuration.Settings.StaffAlerts;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.MiscSettings.Title,
    SubtitleKey = Loc.MiscSettings.Subtitle,
    DescriptionKey = Loc.MiscSettings.LinkDescription)]
public sealed class MiscSettingsMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.Language),
            Description = MenuText.Key(Loc.MiscSettings.LanguageDescription),
            Options = [.. Localizer.Current.AvailableLanguages.Select(NativeLanguageName)],
            ReadSelectedIndex = CurrentIndex,
            OnSelected = Apply,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.MenuRightAlignment),
            Description = MenuText.Key(Loc.MiscSettings.MenuRightAlignmentDescription),

            ReadState = () => UserPreferences.IsRightAligned,
            OnChanged = changed => UserPreferences.SetRightAligned(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.DeathNotifications),
            Description = MenuText.Key(Loc.MiscSettings.DeathNotificationsDescription),
            ReadState = () => UserPreferences.AreDeathNotificationsEnabled,
            OnChanged = changed => UserPreferences.SetDeathNotificationsEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.JoinLeaveNotifications),
            Description = MenuText.Key(Loc.MiscSettings.JoinLeaveNotificationsDescription),
            ReadState = () => UserPreferences.AreJoinLeaveNotificationsEnabled,
            OnChanged = changed => UserPreferences.SetJoinLeaveNotificationsEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.DisableIdleCamera),
            Description = MenuText.Key(Loc.MiscSettings.DisableIdleCameraDescription),
            ReadState = () => UserPreferences.IsIdleCameraDisabled,
            OnChanged = changed => UserPreferences.SetIdleCameraDisabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.DisableVehicleIdleCamera),
            Description = MenuText.Key(Loc.MiscSettings.DisableVehicleIdleCameraDescription),
            ReadState = () => UserPreferences.IsVehicleIdleCameraDisabled,
            OnChanged = changed => UserPreferences.SetVehicleIdleCameraDisabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.Speedometer),
            Description = MenuText.Key(Loc.MiscSettings.SpeedometerDescription),
            Options =
            [
                MenuText.Key(Loc.MiscSettings.SpeedometerOff),
                MenuText.Key(Loc.MiscSettings.SpeedometerKmh),
                MenuText.Key(Loc.MiscSettings.SpeedometerMph),
                MenuText.Key(Loc.MiscSettings.SpeedometerBoth),
            ],
            ReadSelectedIndex = () => Speedometer.Mode,
            OnIndexChanged = changed =>
            {
                Speedometer.Mode = changed.NewIndex;

                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.SpeedometerPosition),
            Description = MenuText.Key(Loc.MiscSettings.SpeedometerPositionDescription),
            LockedDescription = MenuText.Key(Loc.MiscSettings.SpeedometerPositionLocked),
            Gate = MenuGate.When(() => Speedometer.Mode != Speedometer.Off),
            Options =
            [
                MenuText.Key(Loc.MiscSettings.SpeedometerPositionRight),
                MenuText.Key(Loc.MiscSettings.SpeedometerPositionCenter),
            ],
            ReadSelectedIndex = () => Speedometer.Position,
            OnIndexChanged = changed => Speedometer.Position = changed.NewIndex,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.LocationDisplay),
            Description = MenuText.Key(Loc.MiscSettings.LocationDisplayDescription),
            Gate = MiscSettingsPermissions.ShowLocation,
            ReadState = () => UserDefaults.MiscShowLocation.Value,
            OnChanged = changed => LocationDisplay.SetShowLocation(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.CoordinatesDisplay),
            Description = MenuText.Key(Loc.MiscSettings.CoordinatesDisplayDescription),
            Gate = MiscSettingsPermissions.ShowCoordinates,
            ReadState = () => UserDefaults.MiscShowCoordinates.Value,
            OnChanged = changed => LocationDisplay.SetShowCoordinates(changed.Checked),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.MinimapAction),
            Description = MenuText.Key(Loc.MiscSettings.MinimapActionDescription),
            Options =
            [
                MenuText.Key(Loc.MiscSettings.MinimapActionOff),
                MenuText.Key(Loc.MiscSettings.MinimapActionExpand),
                MenuText.Key(Loc.MiscSettings.MinimapActionZoom),
            ],
            ReadSelectedIndex = () => MinimapControls.Action,
            OnIndexChanged = changed =>
            {
                MinimapControls.Action = changed.NewIndex;

                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(new SliderEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.MinimapZoom),
            Description = MenuText.Key(Loc.MiscSettings.MinimapZoomDescription),
            LockedDescription = MenuText.Key(Loc.MiscSettings.MinimapZoomLocked),
            Gate = MenuGate.When(() => MinimapControls.Action == MinimapControls.Zoom),
            Min = MinimapControls.MinZoom,
            Max = MinimapControls.MaxZoom,
            ReadPosition = () => MinimapControls.ZoomAmount,
            OnMoved = moved => MinimapControls.ZoomAmount = moved.NewPosition,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.MinimapAlwaysOn),
            Description = MenuText.Key(Loc.MiscSettings.MinimapAlwaysOnDescription),
            LockedDescription = MenuText.Key(Loc.MiscSettings.MinimapAlwaysOnLocked),
            Gate = MenuGate.When(() => MinimapControls.Action != MinimapControls.Off),
            ReadState = () => MinimapControls.AlwaysOn,
            OnChanged = changed => MinimapControls.AlwaysOn = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.FingerPointing),
            Description = MenuText.Key(Loc.MiscSettings.FingerPointingDescription),
            ReadState = () => FingerPointing.Enabled,
            OnChanged = changed => FingerPointing.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.PlayerBlips),
            Description = MenuText.Key(Loc.MiscSettings.PlayerBlipsDescription),
            Gate = MiscSettingsPermissions.PlayerBlips,
            ReadState = () => UserDefaults.MiscShowPlayerBlips.Value,
            OnChanged = changed =>
            {
                UserDefaults.MiscShowPlayerBlips.Value = changed.Checked;

                PlayerPresence.Reevaluate();
            },
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.OverheadNames),
            Description = MenuText.Key(Loc.MiscSettings.OverheadNamesDescription),
            Gate = MiscSettingsPermissions.OverheadNames,
            ReadState = () => UserDefaults.MiscShowOverheadNames.Value,
            OnChanged = changed =>
            {
                UserDefaults.MiscShowOverheadNames.Value = changed.Checked;

                PlayerPresence.Reevaluate();
            },
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.SeeNoClipPlayers),
            Description = MenuText.Key(Loc.MiscSettings.SeeNoClipPlayersDescription),
            Gate = MiscSettingsPermissions.SeeNoClipPlayers,
            ReadState = () => UserDefaults.MiscSeeNoClipPlayers.Value,
            OnChanged = changed => UserDefaults.MiscSeeNoClipPlayers.Value = changed.Checked,
        });

        menu.Entries.Add(new ConfirmButtonEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.ClearArea),
            Description = MenuText.Key(Loc.MiscSettings.ClearAreaDescription),
            ConfirmationDescription = MenuText.Key(Loc.MiscSettings.ClearAreaConfirm),
            Gate = MiscSettingsPermissions.ClearArea,
            OnConfirmedAsync = _ => ClearArea.RequestAsync(),
        });

        menu.Entries.Add(new ButtonEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.AlertStaff),
            Description = MenuText.Key(Loc.MiscSettings.AlertStaffDescription),
            Gate = MenuGate.Setting(StaffAlertSettings.Enabled),
            Behaviour = GateBehaviour.Hide,
            OnSelectedAsync = _ => StaffAlerts.RaiseAsync(),
        });

        menu.Entries.Add(SubmenuEntry.For(new DataTransferMenu()));
    }

    private static MenuText NativeLanguageName(LanguageId language) =>
        MenuText.Literal(LanguageCatalog.TryGet(language, out var table) ? table.NativeName : language.Code);

    private static int CurrentIndex()
    {
        var languages = Localizer.Current.AvailableLanguages;

        for (var index = 0; index < languages.Count; index++)
        {
            if (languages[index] == Localizer.Current.CurrentLanguage)
            {
                return index;
            }
        }

        return 0;
    }

    private static void Apply(ListSelected selected)
    {
        var languages = Localizer.Current.AvailableLanguages;

        if ((uint)selected.SelectedIndex >= (uint)languages.Count)
        {
            return;
        }

        var language = languages[selected.SelectedIndex];

        // Fires Localizer.Changed, which the registry turns into one relabel pass. Nothing rebuilds.
        if (Localizer.TrySetLanguage(language))
        {
            UserPreferences.SetLanguage(language);
        }
    }
}
