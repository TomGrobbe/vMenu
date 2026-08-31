namespace vMenu.Enhanced.Data.Logging;

public enum LogCategory
{
    Event,

    Action,

    Staff,
}

public static class LogCategories
{
    public static string NameOf(LogCategory category) => category switch
    {
        LogCategory.Action => "action",
        LogCategory.Staff => "staff",
        _ => "event",
    };

    public static string TitleOf(LogCategory category) => category switch
    {
        LogCategory.Action => "Player actions",
        LogCategory.Staff => "Staff actions",
        _ => "Server events",
    };

    public static int ColourOf(LogCategory category) => category switch
    {
        LogCategory.Action => 0x2ECC71,
        LogCategory.Staff => 0xE67E22,
        _ => 0x3498DB,
    };
}
