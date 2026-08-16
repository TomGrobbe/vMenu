namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class StaffAlerts
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.StaffAlerts.Enabled")
    {
        Description =
            "Gives every player an Alert Staff button in the misc settings menu, which sends a short " +
            "message to everybody on your staff team who is online. This is the only thing standing " +
            "between your players and that button: there is no permission for it, on purpose, because " +
            "a player being harassed is exactly the player least likely to have been given one. Off " +
            "by default, since a server with nobody watching the alerts is worse than no button at all.",
        Default = false,
    };

    public static readonly IntSetting CooldownSeconds = new("vMenu.Enhanced.StaffAlerts.CooldownSeconds")
    {
        Description =
            "How long one player has to wait between alerts, in seconds. This is what stops somebody " +
            "holding the button down and burying your staff team, so lowering it a long way is not " +
            "recommended. Set it to 0 to allow alerts as fast as players can type them.",
        Default = 60,
    };

    public static readonly IntSetting ExpireSeconds = new("vMenu.Enhanced.StaffAlerts.ExpireSeconds")
    {
        Description =
            "How long an alert lives for, in seconds, counted from the moment it was raised. For all " +
            "of that time it sits in the staff alerts menu waiting for somebody, and can still be " +
            "answered. After it, the alert is gone, the team is told nobody went, and a staff member " +
            "answering it late is told so rather than being dropped on top of somebody who sorted the " +
            "problem out an hour ago. This is much longer than the time an alert spends on screen, " +
            "which is the setting below.",
        Default = 300,
    };

    public static readonly IntSetting DisplaySeconds = new("vMenu.Enhanced.StaffAlerts.DisplaySeconds")
    {
        Description =
            "How long an alert stays on a staff member's screen, in seconds. Short on purpose, and " +
            "nothing is lost when it fades: the alert stays in the staff alerts menu until it expires, " +
            "where it can be read again, answered or thrown away.",
        Default = 30,
    };
}
