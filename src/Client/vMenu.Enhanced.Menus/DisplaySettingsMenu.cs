using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Menus.World;
using vMenu.Enhanced.Storage;

using DisplaySettingsPermissions = vMenu.Enhanced.Data.Permissions.Menus.DisplaySettings;

namespace vMenu.Enhanced.Menus;

[VMenu(
    TitleKey = Loc.DisplaySettings.Title,
    SubtitleKey = Loc.DisplaySettings.Subtitle,
    DescriptionKey = Loc.DisplaySettings.LinkDescription)]
public sealed class DisplaySettingsMenu : MenuDefinition
{
    protected override void Build(MenuBuilder menu)
    {
        // The sliders are gated on state the submenu changes, and opening a menu re-gates nothing.
        TimecycleState.Changed += () => MenuRegistry.Refresh(menu.Menu);

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.MenuRightAlignment),
            Description = MenuText.Key(Loc.DisplaySettings.MenuRightAlignmentDescription),
            ReadState = () => UserPreferences.IsRightAligned,
            OnChanged = changed => UserPreferences.SetRightAligned(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.DeathNotifications),
            Description = MenuText.Key(Loc.DisplaySettings.DeathNotificationsDescription),
            ReadState = () => UserPreferences.AreDeathNotificationsEnabled,
            OnChanged = changed => UserPreferences.SetDeathNotificationsEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.JoinLeaveNotifications),
            Description = MenuText.Key(Loc.DisplaySettings.JoinLeaveNotificationsDescription),
            ReadState = () => UserPreferences.AreJoinLeaveNotificationsEnabled,
            OnChanged = changed => UserPreferences.SetJoinLeaveNotificationsEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.Speedometer),
            Description = MenuText.Key(Loc.DisplaySettings.SpeedometerDescription),
            Options =
            [
                MenuText.Key(Loc.DisplaySettings.SpeedometerOff),
                MenuText.Key(Loc.DisplaySettings.SpeedometerKmh),
                MenuText.Key(Loc.DisplaySettings.SpeedometerMph),
                MenuText.Key(Loc.DisplaySettings.SpeedometerBoth),
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
            Text = MenuText.Key(Loc.DisplaySettings.SpeedometerPosition),
            Description = MenuText.Key(Loc.DisplaySettings.SpeedometerPositionDescription),
            LockedDescription = MenuText.Key(Loc.DisplaySettings.SpeedometerPositionLocked),
            Gate = MenuGate.When(() => Speedometer.Mode != Speedometer.Off),
            Options =
            [
                MenuText.Key(Loc.DisplaySettings.SpeedometerPositionRight),
                MenuText.Key(Loc.DisplaySettings.SpeedometerPositionCenter),
            ],
            ReadSelectedIndex = () => Speedometer.Position,
            OnIndexChanged = changed => Speedometer.Position = changed.NewIndex,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.VehicleHealth),
            Description = MenuText.Key(Loc.DisplaySettings.VehicleHealthDescription),
            Gate = DisplaySettingsPermissions.VehicleHealth,
            ReadState = () => Speedometer.ShowHealth,
            OnChanged = changed => Speedometer.ShowHealth = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.LocationDisplay),
            Description = MenuText.Key(Loc.DisplaySettings.LocationDisplayDescription),
            Gate = DisplaySettingsPermissions.ShowLocation,
            ReadState = () => UserDefaults.DisplayShowLocation.Value,
            OnChanged = changed => LocationDisplay.SetShowLocation(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.CoordinatesDisplay),
            Description = MenuText.Key(Loc.DisplaySettings.CoordinatesDisplayDescription),
            Gate = DisplaySettingsPermissions.ShowCoordinates,
            ReadState = () => UserDefaults.DisplayShowCoordinates.Value,
            OnChanged = changed => LocationDisplay.SetShowCoordinates(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.Forecast),
            Description = MenuText.Key(Loc.DisplaySettings.ForecastDescription),
            Gate = WeatherForecast.Allowed,
            ReadState = () => UserDefaults.DisplayWeatherForecast.Value,
            OnChanged = changed =>
            {
                WeatherForecast.SetEnabled(changed.Checked);

                // The style row's gate reads this, and a gate is only re-read on a refresh.
                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.ForecastStyle),
            Description = MenuText.Key(Loc.DisplaySettings.ForecastStyleDescription),
            LockedDescription = MenuText.Key(Loc.DisplaySettings.ForecastStyleLocked),
            Gate = WeatherForecast.Allowed & MenuGate.When(() => UserDefaults.DisplayWeatherForecast.Value),
            Options =
            [
                MenuText.Key(Loc.DisplaySettings.ForecastStyleFull),
                MenuText.Key(Loc.DisplaySettings.ForecastStyleCompact),
            ],
            ReadSelectedIndex = () => WeatherForecast.Style,
            OnIndexChanged = changed => WeatherForecast.SetStyle(changed.NewIndex),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.ShowTime),
            Description = MenuText.Key(Loc.DisplaySettings.ShowTimeDescription),
            ReadState = () => UserDefaults.DisplayShowTime.Value,
            OnChanged = changed => WeatherForecast.SetClockEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.MinimapAction),
            Description = MenuText.Key(Loc.DisplaySettings.MinimapActionDescription),
            Options =
            [
                MenuText.Key(Loc.DisplaySettings.MinimapActionOff),
                MenuText.Key(Loc.DisplaySettings.MinimapActionExpand),
                MenuText.Key(Loc.DisplaySettings.MinimapActionZoom),
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
            Text = MenuText.Key(Loc.DisplaySettings.MinimapZoom),
            Description = MenuText.Key(Loc.DisplaySettings.MinimapZoomDescription),
            LockedDescription = MenuText.Key(Loc.DisplaySettings.MinimapZoomLocked),
            Gate = MenuGate.When(() => MinimapControls.Action == MinimapControls.Zoom),
            Min = MinimapControls.MinZoom,
            Max = MinimapControls.MaxZoom,
            ReadPosition = () => MinimapControls.ZoomAmount,
            OnMoved = moved => MinimapControls.ZoomAmount = moved.NewPosition,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.MinimapAlwaysOn),
            Description = MenuText.Key(Loc.DisplaySettings.MinimapAlwaysOnDescription),
            LockedDescription = MenuText.Key(Loc.DisplaySettings.MinimapAlwaysOnLocked),
            Gate = MenuGate.When(() => MinimapControls.Action != MinimapControls.Off),
            ReadState = () => MinimapControls.AlwaysOn,
            OnChanged = changed => MinimapControls.AlwaysOn = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.HideHud),
            Description = MenuText.Key(Loc.DisplaySettings.HideHudDescription),
            Gate = DisplaySettingsPermissions.HideHud,
            ReadState = () => HudVisibility.HudHidden,
            OnChanged = changed => HudVisibility.SetHudHidden(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.HideRadar),
            Description = MenuText.Key(Loc.DisplaySettings.HideRadarDescription),
            Gate = DisplaySettingsPermissions.HideRadar,
            ReadState = () => HudVisibility.RadarHidden,
            OnChanged = changed => HudVisibility.SetRadarHidden(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.NightVision),
            Description = MenuText.Key(Loc.DisplaySettings.NightVisionDescription),
            Gate = DisplaySettingsPermissions.NightVision,
            ReadState = () => VisionModes.NightVision,
            OnChanged = changed => VisionModes.SetNightVision(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.ThermalVision),
            Description = MenuText.Key(Loc.DisplaySettings.ThermalVisionDescription),
            Gate = DisplaySettingsPermissions.ThermalVision,
            ReadState = () => VisionModes.ThermalVision,
            OnChanged = changed => VisionModes.SetThermalVision(changed.Checked),
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.Timecycles),
            Description = MenuText.Key(Loc.DisplaySettings.TimecyclesDescription),
            MenuSubtitle = MenuText.Key(Loc.DisplaySettings.TimecycleSubtitle),
            Gate = DisplaySettingsPermissions.Timecycles,
            Build = Timecycles.Build,
        });

        menu.Entries.Add(new SliderEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.TimecycleIntensity),
            Description = MenuText.Key(Loc.DisplaySettings.TimecycleIntensityDescription),
            Gate = DisplaySettingsPermissions.Timecycles & MenuGate.When(() => TimecycleState.AnyActive),
            Behaviour = GateBehaviour.Hide,
            Min = TimecycleState.MinIntensity,
            Max = TimecycleState.MaxIntensity,
            ReadPosition = () => TimecycleState.Intensity,
            OnMoved = moved => TimecycleState.SetIntensity(moved.NewPosition),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.LocationBlips),
            Description = MenuText.Key(Loc.DisplaySettings.LocationBlipsDescription),
            Gate = DisplaySettingsPermissions.LocationBlips,
            ReadState = () => LocationBlips.ToggleableShown,
            OnChanged = changed => LocationBlips.SetToggleableShown(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.PlayerBlips),
            Description = MenuText.Key(Loc.DisplaySettings.PlayerBlipsDescription),
            Gate = DisplaySettingsPermissions.PlayerBlips,
            ReadState = () => UserDefaults.DisplayShowPlayerBlips.Value,
            OnChanged = changed =>
            {
                UserDefaults.DisplayShowPlayerBlips.Value = changed.Checked;

                PlayerPresence.Reevaluate();
            },
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.OverheadNames),
            Description = MenuText.Key(Loc.DisplaySettings.OverheadNamesDescription),
            Gate = DisplaySettingsPermissions.OverheadNames,
            ReadState = () => UserDefaults.DisplayShowOverheadNames.Value,
            OnChanged = changed =>
            {
                UserDefaults.DisplayShowOverheadNames.Value = changed.Checked;

                PlayerPresence.Reevaluate();
            },
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.SeeNoClipPlayers),
            Description = MenuText.Key(Loc.DisplaySettings.SeeNoClipPlayersDescription),
            Gate = DisplaySettingsPermissions.SeeNoClipPlayers,
            Behaviour = GateBehaviour.Hide,
            ReadState = () => UserDefaults.DisplaySeeNoClipPlayers.Value,
            OnChanged = changed => UserDefaults.DisplaySeeNoClipPlayers.Value = changed.Checked,
        });

        menu.Entries.Add(new SubmenuEntry
        {
            Text = MenuText.Key(Loc.DisplaySettings.ManageBlips),
            Description = MenuText.Key(Loc.DisplaySettings.ManageBlipsDescription),
            MenuSubtitle = MenuText.Key(Loc.DisplaySettings.ManageBlipsSubtitle),
            Gate = DisplaySettingsPermissions.ManageBlips,
            Behaviour = GateBehaviour.Hide,
            Build = ManageBlipsMenu.Build,
        });
    }

    private static readonly TimecycleFilterMenu Timecycles = new();
}
