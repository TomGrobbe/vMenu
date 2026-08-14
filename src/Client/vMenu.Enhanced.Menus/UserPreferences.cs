using CitizenFX.FiveM.Client;

using MenuAPI;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.MenuFramework.Localization;
using vMenu.Enhanced.Menus.Misc;
using vMenu.Enhanced.Storage;
using vMenu.Enhanced.Ticks;

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

        SetIdleCameraDisabled(UserDefaults.MiscDisableIdleCamera.Value);
        SetVehicleIdleCameraDisabled(UserDefaults.MiscDisableVehicleIdleCamera.Value);

        MinimapControls.Apply();

        // After the alignment, which decides the side the panel sits on.
        TickOverlay.Restore();
    }

    /// <summary>Whether the menu is currently right aligned. The live value, not the stored one.</summary>
    public static bool IsRightAligned =>
        MenuController.MenuAlignment == MenuController.MenuAlignmentOption.Right;

    public static void SetRightAligned(bool rightAligned) => ApplyRightAligned(rightAligned, persist: true);

    public static void SetLanguage(LanguageId language) => UserDefaults.Language.Value = language.Code;

    public static bool IsIdleCameraDisabled => UserDefaults.MiscDisableIdleCamera.Value;

    public static bool IsVehicleIdleCameraDisabled => UserDefaults.MiscDisableVehicleIdleCamera.Value;

    public static bool AreDeathNotificationsEnabled => UserDefaults.MiscDeathNotifications.Value;

    public static void SetDeathNotificationsEnabled(bool enabled) =>
        UserDefaults.MiscDeathNotifications.Value = enabled;

    // Both natives are plain flags the game remembers, so they are set when the value moves rather
    // than held down by a tick.
    public static void SetIdleCameraDisabled(bool disabled)
    {
        Native.DisableIdleCamera(disabled);

        UserDefaults.MiscDisableIdleCamera.Value = disabled;
    }

    public static void SetVehicleIdleCameraDisabled(bool disabled)
    {
        Native.DisableVehiclePassengerIdleCamera(disabled);

        UserDefaults.MiscDisableVehicleIdleCamera.Value = disabled;
    }

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

        Log.Debug(
            $"[Localization] '{stored}' is not available here, so English is being used. The preference is "
            + "kept for servers that do offer it.");

        Localizer.TrySetLanguage(LanguageId.English);

        // Always resolves in English, since English is what the fallback just selected. Deferred
        // because this runs before the player has spawned.
        _ = Notifications.ShowWhenVisibleAsync(
            NotificationStyle.Warning,
            MenuText.Key(Loc.MiscSettings.LanguageUnavailable, ("language", MenuText.Literal(stored))));
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
            // Deferred, because this also runs from Restore before the player has spawned. At
            // runtime the wait is skipped and it shows straight away.
            _ = Notifications.ShowWhenVisibleAsync(
                NotificationStyle.Error,
                MenuText.Key(Loc.MiscSettings.MenuRightAlignmentUnsupported));

            UserDefaults.MiscRightAlignMenu.Value = false;

            return;
        }

        if (persist)
        {
            UserDefaults.MiscRightAlignMenu.Value = rightAligned;
        }
    }
}
