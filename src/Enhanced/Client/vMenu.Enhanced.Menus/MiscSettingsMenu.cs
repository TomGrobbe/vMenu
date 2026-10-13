using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;

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

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.FingerPointing),
            Description = MenuText.Key(Loc.MiscSettings.FingerPointingDescription),
            ReadState = () => FingerPointing.Enabled,
            OnChanged = changed => FingerPointing.SetEnabled(changed.Checked),
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
