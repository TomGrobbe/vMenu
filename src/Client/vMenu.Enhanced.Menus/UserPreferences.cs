using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus;

/// <summary>Turns stored preferences into applied state, and back when the player changes one.</summary>
// Sits here rather than in the storage module so that stays a store, knowing how to persist a value
// but not what it means.
public static class UserPreferences
{
    /// <summary>
    /// Applies everything stored. Call before the menus are built: the language decides what every
    /// item is labelled.
    /// </summary>
    public static void Restore()
    {
        RestoreLanguage();

        ApplyRightAligned(UserDefaults.MiscRightAlignMenu.Value, persist: false);
    }

    /// <summary>Whether the menu is currently right aligned. The live value, not the stored one.</summary>
    public static bool IsRightAligned =>
        MenuController.MenuAlignment == MenuController.MenuAlignmentOption.Right;

    public static void SetRightAligned(bool rightAligned) => ApplyRightAligned(rightAligned, persist: true);

    public static void SetLanguage(LanguageId language) => UserDefaults.Language.Value = language.Code;

    private static void RestoreLanguage()
    {
        var stored = UserDefaults.Language.Value;

        if (string.IsNullOrWhiteSpace(stored))
        {
            return;
        }

        if (Localizer.TrySetLanguage(LanguageId.FromCode(stored)))
        {
            return;
        }

        // A language vMenu used to ship and no longer does, or a hand-edited value.
        Localizer.TrySetLanguage(LanguageId.English);

        UserDefaults.Language.Value = LanguageId.English.Code;
    }

    // MenuAPI declines a right alignment on some aspect ratios, so this checks that it took. A
    // rejection is written back even when persist is false, so the player does not meet the same
    // message on every restart.
    private static void ApplyRightAligned(bool rightAligned, bool persist)
    {
        MenuController.MenuAlignment = rightAligned
            ? MenuController.MenuAlignmentOption.Right
            : MenuController.MenuAlignmentOption.Left;

        if (rightAligned && !IsRightAligned)
        {
            Notifications.Error(MenuText.Key(Loc.MiscSettings.MenuRightAlignmentUnsupported));

            UserDefaults.MiscRightAlignMenu.Value = false;

            return;
        }

        if (persist)
        {
            UserDefaults.MiscRightAlignMenu.Value = rightAligned;
        }
    }
}
