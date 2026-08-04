using MenuAPI;

using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Storage;

namespace vMenu.Enhanced.Menus;

/// <summary>
/// Turns stored preferences into applied state, and back again when the player changes one.
/// </summary>
/// <remarks>
/// Sits here rather than in the storage module so that stays a store: it knows how to persist a
/// value, not what the value means. Pairing the two in one place is what keeps "read it at startup"
/// and "write it on change" from drifting apart.
/// </remarks>
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

    /// <remarks>
    /// MenuAPI declines a right alignment on some aspect ratios, so this checks that it took. A
    /// rejection is written back even when <paramref name="persist"/> is false, so the player does
    /// not meet the same message on every restart.
    /// </remarks>
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
