namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Logging
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.Logging.Enabled")
    {
        Description =
            "The master switch for webhook logging. With this off, nothing leaves your server no matter " +
            "what you put in the webhook options below.",
        Default = false,
    };

    public static readonly StringSetting EventsWebhook = new("vMenu.Enhanced.Logging.Webhook.Events")
    {
        Description =
            "A Discord webhook URL for things that happen to your server: players joining and leaving, " +
            "deaths, weather and time changes, vMenu starting and stopping, and plugins and menu themes " +
            "coming and going. Leave it empty to log none of it. Uses 'set' and not 'setr': the URL is a " +
            "secret, and anybody holding it can post into that channel.",
        ServerOnly = true,
    };

    public static readonly StringSetting ActionsWebhook = new("vMenu.Enhanced.Logging.Webhook.Actions")
    {
        Description =
            "A Discord webhook URL for what players do to themselves in the menu, such as godmode, " +
            "noclip, healing, handing themselves a weapon and spawning a vehicle. This one is chatty on " +
            "a busy server, so give it its own channel. Leave it empty to log none of it. Uses 'set' and " +
            "not 'setr': the URL is a secret.",
        ServerOnly = true,
    };

    public static readonly StringSetting StaffWebhook = new("vMenu.Enhanced.Logging.Webhook.Staff")
    {
        Description =
            "A Discord webhook URL for what players do to each other, such as kicking, killing, " +
            "summoning, messaging, freezing and lending noclip. Attempts that were refused because " +
            "somebody lacked the permission are logged here too. Leave it empty to log none of it. Uses " +
            "'set' and not 'setr': the URL is a secret.",
        ServerOnly = true,
    };

    public static readonly StringSetting SecurityWebhook = new("vMenu.Enhanced.Logging.Webhook.Security")
    {
        Description =
            "A Discord webhook URL for people trying to use the menu in ways a normal game cannot: " +
            "firing actions they do not have the permission for, asking for actions that do not exist, " +
            "sending arguments the menu would never send, and hammering an action far past its limit. " +
            "Leave it empty and these go to the staff webhook instead. Uses 'set' and not 'setr': the " +
            "URL is a secret.",
        ServerOnly = true,
    };

    public static readonly StringSetting GenericWebhook = new("vMenu.Enhanced.Logging.Webhook.Generic")
    {
        Description =
            "A URL that receives the same lines as plain JSON instead of as Discord messages, for your " +
            "own tooling. vMenu posts and forgets: no retries, no interest in the answer, and no " +
            "certificate check, so a bare IP address or a self signed certificate is fine. Because the " +
            "certificate is not checked, only point this at something you control. Uses 'set' and not " +
            "'setr': the URL is a secret.",
        ServerOnly = true,
    };

    public static readonly IntSetting FlushSeconds = new("vMenu.Enhanced.Logging.FlushSeconds")
    {
        Description =
            "How often vMenu sends whatever it has collected, in seconds. Lines are batched rather than " +
            "sent one at a time, because Discord starts refusing you if you post too fast. Anything " +
            "outside 1 to 60 is pulled back into that range.",
        Default = 2,
    };

    public static readonly IntSetting QueueLimit = new("vMenu.Enhanced.Logging.QueueLimit")
    {
        Description =
            "How many lines vMenu holds per webhook while waiting to send them. Once this many are " +
            "waiting the oldest are dropped, and the next message that gets through says how many went. " +
            "This stops a webhook that has stopped answering eating your server's memory.",
        Default = 500,
    };

    public static readonly IntSetting MenuActionLimit = new("vMenu.Enhanced.Logging.MenuActionLimit")
    {
        Description =
            "How many menu actions one player may have logged per window. Menu actions are reported by " +
            "the player's own game, so this is what stops somebody with a modified client filling your " +
            "channel. Set it to 0 to turn the limit off, which is not recommended.",
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
            "How many security lines one player may have logged per window. Somebody running a modified " +
            "client can try thousands of things a second, and this is what stops that filling your " +
            "channel. The next line that gets through says how many were left out. Set it to 0 to turn " +
            "the limit off, which is not recommended.",
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
