namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>
/// Every translation key, as constants.
/// </summary>
/// <remarks>
/// Keys are constants rather than inline strings so a typo is a compile error and renaming one is a
/// safe refactor. The constant only guarantees the key exists in code — that the English table
/// actually has text for it is what <see cref="LocalizationSelfCheck"/> reports at startup.
/// Partial across one file per area; keep each area's keys next to their English text.
/// </remarks>
public static partial class Loc
{
    /// <summary>Keys the menu framework itself resolves, rather than any particular menu.</summary>
    public static class Framework
    {
        public const string RestrictedDescription = "framework.restricted";
    }

    public static class MainMenu
    {
        public const string Title = "mainmenu.title";

        public const string Subtitle = "mainmenu.subtitle";
    }
}
