namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class StaffAlerts
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.StaffAlerts.Enabled")
    {
        Description =
            "Allows players to alert staff via the 'Alert staff' button in the misc settings.",
        Default = true,
    };

    public static readonly IntSetting CooldownSeconds = new("vMenu.Enhanced.StaffAlerts.CooldownSeconds")
    {
        Description =
            "How long one player has to wait between sending multiple alerts, in seconds. This is to prevent spam.",
        Default = 60,
    };

    public static readonly IntSetting ExpireSeconds = new("vMenu.Enhanced.StaffAlerts.ExpireSeconds")
    {
        Description =
            "This is how long an alert will stay active before it is discarded in seconds.",
        Default = 300,
    };

    public static readonly IntSetting DisplaySeconds = new("vMenu.Enhanced.StaffAlerts.DisplaySeconds")
    {
        Description =
            "How long an alert stays visible on screen in seconds. You can always see all active alerts in the staff alerts menu.",
        Default = 30,
    };
}
