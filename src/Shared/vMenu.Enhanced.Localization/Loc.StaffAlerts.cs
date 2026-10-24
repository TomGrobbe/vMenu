namespace vMenu.Enhanced.Localization;

public static partial class Loc
{
    public static class StaffAlerts
    {
        [English("Staff Alerts")]
        public const string Title = "staffalerts.title";

        [English("{count} waiting")]
        public const string Subtitle = "staffalerts.subtitle";

        [English("The players who have asked for a staff member and are still waiting for one.")]
        public const string LinkDescription = "staffalerts.link.desc";

        [English("Your Own Settings")]
        public const string HideGroup = "staffalerts.hide.group";

        [English("Settings that change what you see, and nobody else.")]
        public const string HideGroupDescription = "staffalerts.hide.group.desc";

        [English("Hide Staff Alerts")]
        public const string Hide = "staffalerts.hide";

        [English(
            "No longer receive staff alerts on screen. The list above still works, so you can come and look whenever you feel like it. Remembered between sessions.")]
        public const string HideDescription = "staffalerts.hide.desc";

        [English(
            "~y~{player}~s~ says: {description}\n\nRaised as ~y~#{id}~s~, gone in ~y~{time}~s~. Press enter for what you can do about it.")]
        public const string RowDescription = "staffalerts.row.desc";

        [English("Nobody needs help")]
        public const string Empty = "staffalerts.empty";

        [English("No alerts are waiting. Anything raised from now on turns up here as well as on your screen.")]
        public const string EmptyDescription = "staffalerts.empty.desc";

        [English("Alert #{id} · gone in {time}")]
        public const string ActionsSubtitle = "staffalerts.actions.subtitle";

        [English("Respond")]
        public const string Respond = "staffalerts.respond";

        [English(
            "Go to this player, in noclip if you are allowed it. The rest of the team is told you took it and it leaves their list.")]
        public const string RespondDescription = "staffalerts.respond.desc";

        [English("Dismiss For Everyone")]
        public const string Dismiss = "staffalerts.dismiss";

        [English(
            "Throw this alert away without going. It leaves everybody's list, and the player who raised it is told a staff member closed it, so they are not left waiting on help that is not coming.")]
        public const string DismissDescription = "staffalerts.dismiss.desc";

        [English("Closed ~y~{player}~s~'s request and let them know.")]
        public const string DismissDone = "staffalerts.dismiss.done";

        [English("Show On Screen Again")]
        public const string Show = "staffalerts.show";

        [English("Put this alert back on your screen the way it first arrived. Only you see it, and nobody is told.")]
        public const string ShowDescription = "staffalerts.show.desc";

        [English("~g~{staff}~s~ closed alert ~y~#{id}~s~ without going.")]
        public const string DismissedByStaff = "staffalerts.dismissed";

        [English(
            "A staff member (~y~{staff}~s~) has closed your help request. Please ask again if you still need somebody.")]
        public const string YourAlertDismissed = "staffalerts.dismissed.yours";

        [English("That did not work. The alert may have just been taken or run out.")]
        public const string Failed = "staffalerts.failed";
    }
}
