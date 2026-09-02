namespace vMenu.Enhanced.Data.Logging;

public enum LogCategory
{
    Event,

    Action,

    Staff,

    Security,
}

public static class LogCategories
{
    public static string NameOf(LogCategory category) => category switch
    {
        LogCategory.Action => "action",
        LogCategory.Staff => "staff",
        LogCategory.Security => "security",
        _ => "event",
    };

    public static string TitleOf(LogCategory category) => category switch
    {
        LogCategory.Action => "Player actions",
        LogCategory.Staff => "Staff actions",
        LogCategory.Security => "Security",
        _ => "Server events",
    };

    public static int ColourOf(LogCategory category) => category switch
    {
        LogCategory.Action => 0x2ECC71,
        LogCategory.Staff => 0xE67E22,
        LogCategory.Security => 0xE74C3C,
        _ => 0x3498DB,
    };
}
