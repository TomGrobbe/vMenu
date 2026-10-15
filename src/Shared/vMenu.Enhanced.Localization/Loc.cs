namespace vMenu.Enhanced.Localization;

/// <summary>Every translation key, as constants rather than inline strings, so a typo is a compile error and
/// renaming is a safe refactor. Partial across one file per area, so each area's keys sit next to
/// their English text.</summary>
public static partial class Loc
{
    /// <summary>Keys the menu framework itself resolves, rather than any particular menu.</summary>
    public static class Framework
    {
        public const string RestrictedDescription = "framework.restricted";

        public const string ConfirmDescription = "framework.confirm";

        public const string InputPlaceholder = "framework.input.placeholder";

        public const string InputHint = "framework.input.hint";

        public const string InputNoMatches = "framework.input.nomatches";

        public const string EmptyMenu = "framework.emptymenu";

        public const string EmptyMenuDescription = "framework.emptymenu.desc";

        public const string ForceCloseButton = "framework.forceclose.button";

        public const string ForceCloseBinding = "framework.forceclose.binding";

        public const string ForceCloseHolding = "framework.forceclose.holding";

        public const string ForceCloseConfirm = "framework.forceclose.confirm";

        public const string ForceCloseTitle = "framework.forceclose.title";

        public const string ForceCloseConfirmTitle = "framework.forceclose.confirm.title";

        public const string ForceCloseAccept = "framework.forceclose.accept";

        public const string ForceCloseCancel = "framework.forceclose.cancel";

        public const string ForceCloseSelectKey = "framework.forceclose.key.select";

        public const string ForceCloseBackKey = "framework.forceclose.key.back";
    }

    public static class MainMenu
    {
        public const string Title = "mainmenu.title";

        public const string Subtitle = "mainmenu.subtitle";
    }
}
