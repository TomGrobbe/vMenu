namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class StaffAlerts
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.StaffAlerts.Enabled")
    {
        Description =
            "Allows players to alert staff via the 'Alert staff' button in the misc settings. " +
            "You should usually keep this enabled, unless you have your own reporting system.",
        Default = true,
    };

    public static readonly IntSetting CooldownSeconds = new("vMenu.Enhanced.StaffAlerts.CooldownSeconds")
    {
        Description =
            "How long one player has to wait between alerts, in seconds. This will prevent staff alert spam.",
        Default = 60,
    };

    public static readonly IntSetting ExpireSeconds = new("vMenu.Enhanced.StaffAlerts.ExpireSeconds")
    {
        Description =
            "This is how long an alert will stay active before it is discarded in seconds. " +
            "It's discarded early if a staff member responds to it.",
        Default = 300,
    };

    public static readonly IntSetting DisplaySeconds = new("vMenu.Enhanced.StaffAlerts.DisplaySeconds")
    {
        Description =
            "How long an alert stays on screen in seconds. You can always see the alerts in the staff alerts menu. " +
            "This setting is just for the initial warning on screen that a new alert has been received.",
        Default = 30,
    };
}
