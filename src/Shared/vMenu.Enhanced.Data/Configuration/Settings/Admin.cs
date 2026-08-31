namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Admin
{
    public const int MinClearAreaRadius = 1;

    public const int MaxClearAreaRadius = 1000;

    public const int MinGrabRange = 1;

    public const int MaxGrabRange = 15;

    public static readonly IntSetting ClearAreaRadius =
        new("vMenu.Enhanced.Admin.ClearAreaRadius")
        {
            Description =
                "How far around a player, in metres, the Clear Area button reaches.",
            Default = 100,
        };

    public static readonly IntSetting ClosestPlayerRange =
        new("vMenu.Enhanced.Admin.ClosestPlayerRange")
        {
            Description =
                "How far away, in metres, the freeze and grab buttons will still find a player. " +
                "Anybody further away than this is treated as nobody being nearby.",
            Default = 5,
        };

    public static readonly BoolSetting ScheduledAnnouncements =
        new("vMenu.Enhanced.Admin.ScheduledAnnouncements")
        {
            Description =
                "Turns the announcement schedule in config/announcements.json on or off. " +
                "Staff can still send announcements by hand while this is disabled.",
            Default = true,
        };

    public static readonly IntSetting AnnouncementSeconds =
        new("vMenu.Enhanced.Admin.AnnouncementSeconds")
        {
            Description =
                "How long an announcement stays on screen, in seconds.",
            Default = 20,
        };

    public static int ClampClearAreaRadius(int radius) =>
        radius < MinClearAreaRadius
            ? MinClearAreaRadius
            : radius > MaxClearAreaRadius
                ? MaxClearAreaRadius
                : radius;

    public static int ClampClosestPlayerRange(int range) =>
        range < MinGrabRange
            ? MinGrabRange
            : range > MaxGrabRange
                ? MaxGrabRange
                : range;
}
