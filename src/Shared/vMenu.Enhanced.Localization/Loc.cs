namespace vMenu.Enhanced.Localization;

/// <summary>Every translation key, as constants rather than inline strings, so a typo is a compile error and
/// renaming is a safe refactor. Partial across one file per area, so each area's keys sit next to
/// their English text.</summary>
public static partial class Loc
{
    /// <summary>Keys the menu framework itself resolves, rather than any particular menu.</summary>
    public static class Framework
    {
        [English("Access to this has been restricted by the server owner.")]
        public const string RestrictedDescription = "framework.restricted";

        [English("~r~Are you sure you want to do this? Press enter again to confirm.")]
        public const string ConfirmDescription = "framework.confirm";

        [English("Start typing…")]
        public const string InputPlaceholder = "framework.input.placeholder";

        [English("Enter to confirm · Esc to cancel · ↑↓ to pick a suggestion · Tab to complete")]
        public const string InputHint = "framework.input.hint";

        [English("No matches")]
        public const string InputNoMatches = "framework.input.nomatches";

        [English("No Permissions")]
        public const string EmptyMenu = "framework.emptymenu";

        [English("There is nothing in here you are allowed to use. Ask the server owner if you think you should be.")]
        public const string EmptyMenuDescription = "framework.emptymenu.desc";

        [English("Hold to close")]
        public const string ForceCloseButton = "framework.forceclose.button";

        [English("Menus: hold to close a menu that asked not to be closed")]
        public const string ForceCloseBinding = "framework.forceclose.binding";

        [English("This menu asked not to be closed. Keep holding to close it anyway.")]
        public const string ForceCloseHolding = "framework.forceclose.holding";

        [English("Close this menu anyway? Whatever you were in the middle of might be lost.")]
        public const string ForceCloseConfirm = "framework.forceclose.confirm";

        [English("Menu locked")]
        public const string ForceCloseTitle = "framework.forceclose.title";

        [English("Close anyway?")]
        public const string ForceCloseConfirmTitle = "framework.forceclose.confirm.title";

        [English("Press {key} to close it anyway")]
        public const string ForceCloseAccept = "framework.forceclose.accept";

        [English("Press {key} to stay")]
        public const string ForceCloseCancel = "framework.forceclose.cancel";

        [English("Select")]
        public const string ForceCloseSelectKey = "framework.forceclose.key.select";

        [English("Back")]
        public const string ForceCloseBackKey = "framework.forceclose.key.back";
    }

    public static class MainMenu
    {
        [English("vMenu Enhanced")]
        public const string Title = "mainmenu.title";

        [English("Main Menu")]
        public const string Subtitle = "mainmenu.subtitle";
    }
}
