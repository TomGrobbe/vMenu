using System.Diagnostics.CodeAnalysis;

using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Logging;

public static class Log
{
    public static LogLevel Level { get; private set; } = LogLevel.Info;

    public static bool IsEnabled(LogLevel level) => level >= Level;

    /// <summary>Debug mode opens up the Debug level, everything else stays on Info.</summary>
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
