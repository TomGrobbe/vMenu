using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace vMenu.Enhanced.Logging;

public static class Log
{
    public static LogLevel Level { get; private set; } = LogLevel.Info;

    public static Action<LogLevel, string> Writer { get; set; } = static (_, _) => { };

    public static bool IsEnabled(LogLevel level) => level >= Level;

    // Debug mode opens up the Debug level, everything else stays on Info.
    public static void SetDebug(bool enabled)
    {
        var level = enabled ? LogLevel.Debug : LogLevel.Info;

        if (level == Level)
        {
            return;
        }

        Level = level;

        Info($"[Logging] Log level is now {level}.");
    }

    public static void Debug(string message) => Write(LogLevel.Debug, message);

    public static void Debug([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
        Write(LogLevel.Debug, format, args);

    public static void Info(string message) => Write(LogLevel.Info, message);

    public static void Info([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
        Write(LogLevel.Info, format, args);

    public static void Warning(string message) => Write(LogLevel.Warning, message);

    public static void Warning([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
        Write(LogLevel.Warning, format, args);

    public static void Error(string message) => Write(LogLevel.Error, message);

    public static void Error([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) =>
        Write(LogLevel.Error, format, args);

    private static void Write(LogLevel level, string message)
    {
        if (IsEnabled(level))
        {
            Writer(level, message);
        }
    }

    private static void Write(LogLevel level, string format, object?[] args)
    {
        if (IsEnabled(level))
        {
            Writer(level, string.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}
