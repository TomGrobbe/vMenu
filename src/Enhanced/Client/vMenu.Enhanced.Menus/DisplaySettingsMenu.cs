using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
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
        TimecycleFavorites.Changed += () => MenuRegistry.Refresh(menu.Menu);

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.MenuRightAlignment)
        {
            ReadState = () => UserPreferences.IsRightAligned,
            OnChanged = changed => UserPreferences.SetRightAligned(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.DeathNotifications)
        {
            ReadState = () => UserPreferences.AreDeathNotificationsEnabled,
            OnChanged = changed => UserPreferences.SetDeathNotificationsEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.JoinLeaveNotifications)
        {
            ReadState = () => UserPreferences.AreJoinLeaveNotificationsEnabled,
            OnChanged = changed => UserPreferences.SetJoinLeaveNotificationsEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry(
            Loc.DisplaySettings.Speedometer,
            [
                MenuText.Key(Loc.DisplaySettings.SpeedometerOff),
                MenuText.Key(Loc.DisplaySettings.SpeedometerKmh),
                MenuText.Key(Loc.DisplaySettings.SpeedometerMph),
                MenuText.Key(Loc.DisplaySettings.SpeedometerBoth),
            ])
        {
            ReadSelectedIndex = () => Speedometer.Mode,
            OnIndexChanged = changed =>
            {
                Speedometer.Mode = changed.NewIndex;

                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(new ListEntry(
            Loc.DisplaySettings.SpeedometerPosition,
            [
                MenuText.Key(Loc.DisplaySettings.SpeedometerPositionRight),
                MenuText.Key(Loc.DisplaySettings.SpeedometerPositionCenter),
            ])
        {
            LockedDescription = MenuText.Key(Loc.DisplaySettings.SpeedometerPositionLocked),
            Gate = MenuGate.When(() => Speedometer.Mode != Speedometer.Off),
            ReadSelectedIndex = () => Speedometer.Position,
            OnIndexChanged = changed => Speedometer.Position = changed.NewIndex,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.VehicleHealth)
        {
            Gate = DisplaySettingsPermissions.VehicleHealth,
            ReadState = () => Speedometer.ShowHealth,
            OnChanged = changed => Speedometer.ShowHealth = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.LocationDisplay)
        {
            Gate = DisplaySettingsPermissions.ShowLocation,
            ReadState = () => UserDefaults.DisplayShowLocation.Value,
            OnChanged = changed => LocationDisplay.SetShowLocation(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.CoordinatesDisplay)
        {
            Gate = DisplaySettingsPermissions.ShowCoordinates,
            ReadState = () => UserDefaults.DisplayShowCoordinates.Value,
            OnChanged = changed => LocationDisplay.SetShowCoordinates(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.Forecast)
        {
            Gate = WeatherForecast.Allowed,
            ReadState = () => UserDefaults.DisplayWeatherForecast.Value,
            OnChanged = changed =>
            {
                WeatherForecast.SetEnabled(changed.Checked);

                // The style row's gate reads this, and a gate is only re-read on a refresh.
                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(new ListEntry(
            Loc.DisplaySettings.ForecastStyle,
            [
                MenuText.Key(Loc.DisplaySettings.ForecastStyleFull),
                MenuText.Key(Loc.DisplaySettings.ForecastStyleCompact),
            ])
        {
            LockedDescription = MenuText.Key(Loc.DisplaySettings.ForecastStyleLocked),
            Gate = WeatherForecast.Allowed & MenuGate.When(() => UserDefaults.DisplayWeatherForecast.Value),
            ReadSelectedIndex = () => WeatherForecast.Style,
            OnIndexChanged = changed => WeatherForecast.SetStyle(changed.NewIndex),
        });

        menu.Entries.Add(new ListEntry(
            Loc.DisplaySettings.TemperatureUnit,
            [
                MenuText.Key(Loc.DisplaySettings.TemperatureUnitCelsius),
                MenuText.Key(Loc.DisplaySettings.TemperatureUnitFahrenheit),
            ])
        {
            LockedDescription = MenuText.Key(Loc.DisplaySettings.TemperatureUnitLocked),
            Gate = WeatherForecast.Allowed & MenuGate.When(() => UserDefaults.DisplayWeatherForecast.Value),
            ReadSelectedIndex = () => WeatherTemperature.Unit,
            OnIndexChanged = changed => WeatherForecast.SetTemperatureUnit(changed.NewIndex),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.ShowTime)
        {
            ReadState = () => UserDefaults.DisplayShowTime.Value,
            OnChanged = changed => WeatherForecast.SetClockEnabled(changed.Checked),
        });

        menu.Entries.Add(new ListEntry(
            Loc.DisplaySettings.MinimapAction,
            [
                MenuText.Key(Loc.DisplaySettings.MinimapActionOff),
                MenuText.Key(Loc.DisplaySettings.MinimapActionExpand),
                MenuText.Key(Loc.DisplaySettings.MinimapActionZoom),
            ])
        {
            ReadSelectedIndex = () => MinimapControls.Action,
            OnIndexChanged = changed =>
            {
                MinimapControls.Action = changed.NewIndex;

                MenuRegistry.Refresh(changed.Menu);
            },
        });

        menu.Entries.Add(new SliderEntry(
            Loc.DisplaySettings.MinimapZoom,
            MinimapControls.MinZoom,
            MinimapControls.MaxZoom)
        {
            LockedDescription = MenuText.Key(Loc.DisplaySettings.MinimapZoomLocked),
            Gate = MenuGate.When(() => MinimapControls.Action == MinimapControls.Zoom),
            ReadPosition = () => MinimapControls.ZoomAmount,
            OnMoved = moved => MinimapControls.ZoomAmount = moved.NewPosition,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.MinimapAlwaysOn)
        {
            LockedDescription = MenuText.Key(Loc.DisplaySettings.MinimapAlwaysOnLocked),
            Gate = MenuGate.When(() => MinimapControls.Action != MinimapControls.Off),
            ReadState = () => MinimapControls.AlwaysOn,
            OnChanged = changed => MinimapControls.AlwaysOn = changed.Checked,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.HideHud)
        {
            Gate = DisplaySettingsPermissions.HideHud,
            ReadState = () => HudVisibility.HudHidden,
            OnChanged = changed => HudVisibility.SetHudHidden(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.HideRadar)
        {
            Gate = DisplaySettingsPermissions.HideRadar,
            ReadState = () => HudVisibility.RadarHidden,
            OnChanged = changed => HudVisibility.SetRadarHidden(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.NightVision)
        {
            Gate = DisplaySettingsPermissions.NightVision,
            ReadState = () => VisionModes.NightVision,
            OnChanged = changed => VisionModes.SetNightVision(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.ThermalVision)
        {
            Gate = DisplaySettingsPermissions.ThermalVision,
            ReadState = () => VisionModes.ThermalVision,
            OnChanged = changed => VisionModes.SetThermalVision(changed.Checked),
        });

        menu.Entries.Add(new SubmenuEntry(Loc.DisplaySettings.Timecycles)
        {
            MenuSubtitle = MenuText.Key(Loc.DisplaySettings.TimecycleSubtitle),
            Gate = DisplaySettingsPermissions.Timecycles,
            Build = Timecycles.Build,
        });

        menu.Entries.Add(new SubmenuEntry(Loc.DisplaySettings.TimecycleFavorites)
        {
            MenuSubtitle = MenuText.Key(Loc.DisplaySettings.TimecycleFavorites),
            Gate = DisplaySettingsPermissions.Timecycles,
            ReadEnabled = () => TimecycleFavorites.Count > 0,
            Build = TimecycleFavoritesMenu.Build,
        });

        menu.Entries.Add(new SliderEntry(
            Loc.DisplaySettings.TimecycleIntensity,
            TimecycleState.MinIntensity,
            TimecycleState.MaxIntensity)
        {
            Gate = DisplaySettingsPermissions.Timecycles & MenuGate.When(() => TimecycleState.AnyActive),
            Behaviour = GateBehaviour.Hide,
            ReadPosition = () => TimecycleState.Intensity,
            OnMoved = moved => TimecycleState.SetIntensity(moved.NewPosition),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.LocationBlips)
        {
            Gate = DisplaySettingsPermissions.LocationBlips,
            ReadState = () => LocationBlips.ToggleableShown,
            OnChanged = changed => LocationBlips.SetToggleableShown(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.PlayerBlips)
        {
            Gate = DisplaySettingsPermissions.PlayerBlips,
            ReadState = () => UserDefaults.DisplayShowPlayerBlips.Value,
            OnChanged = changed =>
            {
                UserDefaults.DisplayShowPlayerBlips.Value = changed.Checked;

                PlayerPresence.Reevaluate();
            },
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.OverheadNames)
        {
            Gate = DisplaySettingsPermissions.OverheadNames,
            ReadState = () => UserDefaults.DisplayShowOverheadNames.Value,
            OnChanged = changed =>
            {
                UserDefaults.DisplayShowOverheadNames.Value = changed.Checked;

                PlayerPresence.Reevaluate();
            },
        });

        menu.Entries.Add(new CheckboxEntry(Loc.DisplaySettings.SeeNoClipPlayers)
        {
            Gate = DisplaySettingsPermissions.SeeNoClipPlayers,
            Behaviour = GateBehaviour.Hide,
            ReadState = () => UserDefaults.DisplaySeeNoClipPlayers.Value,
            OnChanged = changed => UserDefaults.DisplaySeeNoClipPlayers.Value = changed.Checked,
        });

        menu.Entries.Add(new SubmenuEntry(Loc.DisplaySettings.ManageBlips)
        {
            MenuSubtitle = MenuText.Key(Loc.DisplaySettings.ManageBlipsSubtitle),
            Gate = DisplaySettingsPermissions.ManageBlips,
            Behaviour = GateBehaviour.Hide,
            Build = ManageBlipsMenu.Build,
        });
    }

    private static readonly TimecycleFilterMenu Timecycles = new(false);

    private static readonly TimecycleFilterMenu TimecycleFavoritesMenu = new(true);
}
