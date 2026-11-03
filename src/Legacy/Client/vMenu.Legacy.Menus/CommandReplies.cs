using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Menus;

// Server commands log to the server console, which the player who typed them cannot see.
public static class CommandReplies
{
    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        Script.EventHandlers[CommandEvents.Reply] += new Action<int, string>(OnReply);
    }

    private static void OnReply(int level, string message)
    {
        switch ((LogLevel)level)
        {
            case LogLevel.Debug:
                Log.Debug($"[Server] {message}");
                break;
            case LogLevel.Info:
                Log.Info($"[Server] {message}");
                break;
            case LogLevel.Warning:
                Log.Warning($"[Server] {message}");
                Notifications.Warning(WithoutTag(message));
                break;
            default:
                Log.Error($"[Server] {message}");
                Notifications.Error(WithoutTag(message));
                break;
        }
    }

    private static string WithoutTag(string message)
    {
        if (!message.StartsWith("[", StringComparison.Ordinal))
        {
            return message;
        }

        var end = message.IndexOf("] ", StringComparison.Ordinal);

        return end < 0 ? message : message.Substring(end + 2);
    }
}
