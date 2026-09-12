namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Logging
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.Logging.Enabled")
    {
        Description =
            "Enables or disables all the webhooks, regardless of what you configured below.",
        Default = false,
    };

    public static readonly StringSetting EventsWebhook = new("vMenu.Enhanced.Logging.Webhook.Events")
    {
        Description =
            "A Discord webhook URL for server events. Use 'set' instead of 'setr' to keep your webhook " +
            "URL secure.",
        ServerOnly = true,
    };

    public static readonly StringSetting ActionsWebhook = new("vMenu.Enhanced.Logging.Webhook.Actions")
    {
        Description =
            "A Discord webhook URL for what players do to themselves. Chatty on a busy server, so give " +
            "it its own channel. Use 'set' instead of 'setr' to keep your webhook URL secure.",
        ServerOnly = true,
    };

    public static readonly StringSetting StaffWebhook = new("vMenu.Enhanced.Logging.Webhook.Staff")
    {
        Description =
            "A Discord webhook URL for what players do to each other, including attempts refused for " +
            "lack of permission. Use 'set' instead of 'setr' to keep your webhook URL secure.",
        ServerOnly = true,
    };

    public static readonly StringSetting SecurityWebhook = new("vMenu.Enhanced.Logging.Webhook.Security")
    {
        Description =
            "A Discord webhook URL when possible cheating is detected. If you leave this empty, the events will be sent to the " +
            "staff webhook instead. Use 'set' instead of 'setr' to keep your webhook URL secure.",
        ServerOnly = true,
    };

    public static readonly StringSetting GenericWebhook = new("vMenu.Enhanced.Logging.Webhook.Generic")
    {
        Description =
            "A URL that receives the same lines as plain JSON instead of Discord messages, for your own " +
            "tooling. If it uses HTTPS, the certificate must be valid and trusted. Use 'set' instead of " +
            "'setr' to keep your webhook URL secure.",
        ServerOnly = true,
    };

    public static readonly IntSetting FlushSeconds = new("vMenu.Enhanced.Logging.FlushSeconds")
    {
        Description =
            "How often vMenu sends what it has collected, in seconds. Lines are batched instead of sent " +
            "one at a time, since Discord throttles you for posting too fast. Values outside 1 to 60 are " +
            "clamped to that range.",
        Default = 4,
    };

    public static readonly IntSetting QueueLimit = new("vMenu.Enhanced.Logging.QueueLimit")
    {
        Description =
            "How many lines vMenu holds per webhook while waiting to send. Past this the oldest are " +
            "dropped, and the next message through says how many. Stops an unresponsive webhook eating " +
            "your server's memory.",
        Default = 500,
    };

    public static readonly IntSetting MenuActionLimit = new("vMenu.Enhanced.Logging.MenuActionLimit")
    {
        Description =
            "(Rate limiting) How many menu actions one player may log per window. These are reported by the player's own " +
            "game, so this stops a modified client filling your channel. Set to 0 to turn the limit off, " +
            "not recommended.",
        Default = 30,
    };

    public static readonly IntSetting MenuActionLimitSeconds = new("vMenu.Enhanced.Logging.MenuActionLimitSeconds")
    {
        Description = "How long the menu action window above lasts, in seconds.",
        Default = 10,
    };

    public static readonly IntSetting SecurityLimit = new("vMenu.Enhanced.Logging.SecurityLimit")
    {
        Description =
            "How many security lines one player may log per window. A modified client can try thousands " +
            "of things a second, and this stops that filling your channel. The next line through says " +
            "how many were left out. Set to 0 to turn the limit off, not recommended.",
        Default = 10,
    };

    public static readonly IntSetting SecurityLimitSeconds = new("vMenu.Enhanced.Logging.SecurityLimitSeconds")
    {
        Description = "How long the security window above lasts, in seconds.",
        Default = 60,
    };

    public const int MinFlushSeconds = 1;

    public const int MaxFlushSeconds = 60;

    public static int ClampFlushSeconds(int seconds) => Math.Clamp(seconds, MinFlushSeconds, MaxFlushSeconds);
}
