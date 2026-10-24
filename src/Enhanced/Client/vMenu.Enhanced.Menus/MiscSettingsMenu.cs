using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Localization;
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
        menu.Entries.Add(new ListEntry(
            Loc.MiscSettings.Language,
            [.. Localizer.Current.AvailableLanguages.Select(NativeLanguageName)])
        {
            ReadSelectedIndex = CurrentIndex,
            OnSelected = Apply,
        });

        menu.Entries.Add(new CheckboxEntry(Loc.MiscSettings.DisableIdleCamera)
        {
            ReadState = () => UserPreferences.IsIdleCameraDisabled,
            OnChanged = changed => UserPreferences.SetIdleCameraDisabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.MiscSettings.DisableVehicleIdleCamera)
        {
            ReadState = () => UserPreferences.IsVehicleIdleCameraDisabled,
            OnChanged = changed => UserPreferences.SetVehicleIdleCameraDisabled(changed.Checked),
        });

        menu.Entries.Add(new CheckboxEntry(Loc.MiscSettings.FingerPointing)
        {
            ReadState = () => FingerPointing.Enabled,
            OnChanged = changed => FingerPointing.SetEnabled(changed.Checked),
        });

        menu.Entries.Add(new ButtonEntry(Loc.MiscSettings.AlertStaff)
        {
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
