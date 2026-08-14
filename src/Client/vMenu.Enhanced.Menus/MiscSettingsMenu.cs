using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;

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
