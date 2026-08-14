using System.Diagnostics.CodeAnalysis;

using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Logging;

public static class Log
{
    public const string LevelNames = "Trace, Debug or Info";

    public static LogLevel Level { get; private set; } = LogLevel.Info;

    public static bool IsEnabled(LogLevel level) => level >= Level;

    public static void SetLevel(string? text)
    {
        if (!TryParseLevel(text, out var level))
        {
            Level = LogLevel.Info;

            Warning($"[Logging] '{text}' is not a log level. Use {LevelNames}. Staying on Info.");

            return;
        }

        if (level == Level)
        {
            return;
        }

        Level = level;

        Info($"[Logging] Log level is now {level}.");
    }

    public static bool TryParseLevel(string? text, out LogLevel level)
    {
        switch (text?.Trim().ToLowerInvariant())
        {
            case "trace":
                level = LogLevel.Trace;
                return true;
            case "debug":
                level = LogLevel.Debug;
                return true;
            case "info":
                level = LogLevel.Info;
                return true;
            default:
                level = LogLevel.Info;
                return false;
        }
    }

    public static void Trace(string message)
    {
        if (IsEnabled(LogLevel.Trace))
        {
            SharedAPI.Log.Trace(message);
        }
    }

    public static void Trace([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        if (IsEnabled(LogLevel.Trace))
        {
            SharedAPI.Log.Trace(format, args);
        }
    }

    public static void Debug(string message)
    {
        if (IsEnabled(LogLevel.Debug))
        {
            SharedAPI.Log.Debug(message);
        }
    }

    public static void Debug([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        if (IsEnabled(LogLevel.Debug))
        {
            SharedAPI.Log.Debug(format, args);
        }
    }

    public static void Info(string message)
    {
        if (IsEnabled(LogLevel.Info))
        {
            SharedAPI.Log.Info(message);
        }
    }

    public static void Info([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        if (IsEnabled(LogLevel.Info))
        {
            SharedAPI.Log.Info(format, args);
        }
    }

    public static void Warning(string message)
    {
        if (IsEnabled(LogLevel.Warning))
        {
            SharedAPI.Log.Warn(message);
        }
    }

    public static void Warning([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        if (IsEnabled(LogLevel.Warning))
        {
            SharedAPI.Log.Warn(format, args);
        }
    }

    public static void Error(string message)
    {
        if (IsEnabled(LogLevel.Error))
        {
            SharedAPI.Log.Error(message);
        }
    }

    public static void Error([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        if (IsEnabled(LogLevel.Error))
        {
            SharedAPI.Log.Error(format, args);
        }
    }
}
