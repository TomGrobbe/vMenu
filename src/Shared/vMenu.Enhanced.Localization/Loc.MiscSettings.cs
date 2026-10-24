namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class MiscSettings
    {
        [English("Miscellaneous Settings")]
        public const string Title = "miscsettings.title";

        [English("Miscellaneous Settings")]
        public const string Subtitle = "miscsettings.subtitle";

        [English("Change various settings about the menu.")]
        public const string LinkDescription = "miscsettings.link.desc";

        [English("Language", "Select a language and press enter to apply it.")]
        public const string Language = "miscsettings.language";

        [English(
            "This server does not offer ~y~{language}~s~, so vMenu is in English. Your preference is kept for servers that do, unless you decide to choose another language from the list of available languages.")]
        public const string LanguageUnavailable = "miscsettings.language.unavailable";

        [English(
            "Disable Idle Camera",
            "Stops the camera drifting around on its own when you stop touching the controls on foot.")]
        public const string DisableIdleCamera = "miscsettings.idlecam";

        [English(
            "Disable Vehicle Idle Camera",
            "Stops the camera drifting around on its own while you sit in a vehicle. ~y~Note:~s~ this also stops you turning the cinematic camera on yourself.")]
        public const string DisableVehicleIdleCamera = "miscsettings.idlecam.vehicle";

        [English(
            "Finger Pointing",
            "Point your finger at whatever you are looking at, just like in GTA Online. Press ~b~B~s~ to start and press it again to stop, or click the right stick on a controller. It only works with your hands empty, a drawn weapon breaks the aiming animation. You can change the keybind in the pause menu under ~y~Settings~s~, ~y~Key Bindings~s~, ~y~FiveM~s~.")]
        public const string FingerPointing = "miscsettings.pointing";

        [English(
            "Alert Staff",
            "Tell everybody on the staff team who is online that you need a hand, with a short note saying what is going on. Please only use this when you really do need somebody.")]
        public const string AlertStaff = "miscsettings.alertstaff";

        [English("What do you need staff for?")]
        public const string AlertStaffPrompt = "miscsettings.alertstaff.prompt";

        [English("Staff cannot help without knowing what is wrong, so write a few words first.")]
        public const string AlertStaffEmpty = "miscsettings.alertstaff.empty";

        [English("Your alert went out to ~g~{count}~s~ staff member(s). Please stay where you are.")]
        public const string AlertStaffSent = "miscsettings.alertstaff.sent";

        [English("There is ~r~nobody~s~ from the staff team online right now, so your alert reached no one.")]
        public const string AlertStaffNobody = "miscsettings.alertstaff.nobody";

        [English("You have already alerted staff. You can do it again in ~y~{seconds}~s~ second(s).")]
        public const string AlertStaffCooldown = "miscsettings.alertstaff.cooldown";

        [English("~y~{player}~s~ needs staff~n~{description}~n~~b~/vmenu-respond {id}~s~")]
        public const string AlertStaffBanner = "miscsettings.alertstaff.banner";

        [English("Alert ~y~#{id}~s~ was taken by ~g~{staff}~s~.")]
        public const string AlertStaffTaken = "miscsettings.alertstaff.taken";

        [English("Use it as ~b~/vmenu-respond 12~s~, putting the number shown on the alert in place of the 12.")]
        public const string AlertRespondUsage = "miscsettings.alertrespond.usage";

        [English("You are with ~g~{player}~s~ now.")]
        public const string AlertRespondDone = "miscsettings.alertrespond.done";

        [English("There is no alert ~y~#{id}~s~, or the player who raised it has left.")]
        public const string AlertRespondGone = "miscsettings.alertrespond.gone";

        [English("Alert ~y~#{id}~s~ is too old to answer now.")]
        public const string AlertRespondExpired = "miscsettings.alertrespond.expired";

        [English("~g~{staff}~s~ already took alert ~y~#{id}~s~.")]
        public const string AlertRespondAlreadyTaken = "miscsettings.alertrespond.alreadytaken";

        [English("~r~Nobody~s~ answered ~y~{player}~s~'s help request (~y~#{id}~s~).")]
        public const string AlertStaffExpired = "miscsettings.alertstaff.expired";

        [English("Cleared ~y~{count}~s~ alert(s) from your screen. Everybody else still has theirs.")]
        public const string AlertDismissed = "miscsettings.alertdismiss.done";

        [English("There are no alerts on your screen to clear.")]
        public const string AlertDismissNothing = "miscsettings.alertdismiss.nothing";
    }
}
