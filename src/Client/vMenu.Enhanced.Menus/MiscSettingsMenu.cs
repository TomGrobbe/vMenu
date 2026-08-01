using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Settings that belong to the player rather than to the server.
/// </summary>
/// <remarks>
/// Deliberately ungated. Everything here changes how vMenu presents itself to one player, so there
/// is nothing for a server owner to grant — and gating the language picker would mean a server could
/// lock a player out of reading their own menu.
/// </remarks>
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

            // Endonyms: a language names itself the same way whatever language you are reading in,
            // so these are literals and never looked up as keys.
            Options = [.. Localizer.Current.AvailableLanguages.Select(NativeLanguageName)],

            // Read rather than stored, so the highlighted row follows the language even when
            // something other than this list changed it.
            ReadSelectedIndex = CurrentIndex,

            // On select rather than on scroll: applying a language re-labels every menu, and doing
            // that on each arrow press would rebuild thousands of vehicle rows per keystroke.
            OnSelected = Apply,
        });

        menu.Entries.Add(new CheckboxEntry
        {
            Text = MenuText.Key(Loc.MiscSettings.MenuRightAlignment),
            Description = MenuText.Key(Loc.MiscSettings.MenuRightAlignmentDescription),
            ReadState = RightAlignedMenuCheck,
            OnChangedAsync = OnRightAlignMenuCheckboxChanged
        });
    }

    private async Task OnRightAlignMenuCheckboxChanged(CheckboxChanged changed)
    {
        if (changed.Checked)
        {
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;
        }
        else
        {
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
        }
    }

    private bool RightAlignedMenuCheck() =>
        MenuController.MenuAlignment == MenuController.MenuAlignmentOption.Right;

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

        if ((uint)selected.SelectedIndex < (uint)languages.Count)
        {
            // Fires Localizer.Changed, which the registry turns into one relabel pass over every
            // built menu. No menu is rebuilt.
            Localizer.TrySetLanguage(languages[selected.SelectedIndex]);
        }
    }
}
