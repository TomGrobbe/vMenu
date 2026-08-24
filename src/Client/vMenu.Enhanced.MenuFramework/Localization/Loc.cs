namespace vMenu.Enhanced.MenuFramework.Localization;

// Every translation key, as constants rather than inline strings, so a typo is a compile error and
// renaming is a safe refactor. Partial across one file per area, so each area's keys sit next to
// their English text.
public static partial class Loc
{
    // Keys the menu framework itself resolves, rather than any particular menu.
    public static class Framework
    {
        public const string RestrictedDescription = "framework.restricted";

        public const string ConfirmDescription = "framework.confirm";

        public const string InputPlaceholder = "framework.input.placeholder";

        public const string InputHint = "framework.input.hint";

        public const string InputNoMatches = "framework.input.nomatches";

        public const string EmptyMenu = "framework.emptymenu";

        public const string EmptyMenuDescription = "framework.emptymenu.desc";
    }

    public static class MainMenu
    {
        public const string Title = "mainmenu.title";

        public const string Subtitle = "mainmenu.subtitle";
    }
}
