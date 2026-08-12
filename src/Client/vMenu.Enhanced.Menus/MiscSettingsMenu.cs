using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus;

/// <summary>Settings that belong to the player rather than to the server.</summary>
// Ungated on purpose. Everything here changes how vMenu presents itself to one player, and gating
// the language picker would let a server lock someone out of reading their own menu.
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

            // Endonyms, so these are literals and never looked up as keys.
            Options = [.. Localizer.Current.AvailableLanguages.Select(NativeLanguageName)],

            // Read, so the highlighted row follows the language even when something else changed it.
            ReadSelectedIndex = CurrentIndex,

            // On select, not on scroll: applying a language relabels every menu, which on each arrow
            // press would rebuild thousands of vehicle rows per keystroke.
            OnSelected = Apply,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.MenuRightAlignment),
            Description = MenuText.Key(Loc.MiscSettings.MenuRightAlignmentDescription),

            // What MenuAPI is actually doing rather than what was stored, so an alignment it
            // declined shows as off instead of as a tick that does nothing.
            ReadState = () => UserPreferences.IsRightAligned,
            OnChanged = changed => UserPreferences.SetRightAligned(changed.Checked),
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
            Text = MenuText.Key(Loc.MiscSettings.DeathNotifications),
            Description = MenuText.Key(Loc.MiscSettings.DeathNotificationsDescription),
            ReadState = () => UserPreferences.AreDeathNotificationsEnabled,
            OnChanged = changed => UserPreferences.SetDeathNotificationsEnabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.DisableVehicleIdleCamera),
            Description = MenuText.Key(Loc.MiscSettings.DisableVehicleIdleCameraDescription),
            ReadState = () => UserPreferences.IsVehicleIdleCameraDisabled,
            OnChanged = changed => UserPreferences.SetVehicleIdleCameraDisabled(changed.Checked),
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
