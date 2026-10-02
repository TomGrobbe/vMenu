namespace vMenu.Enhanced.Logging;

// Stands in for vMenu's own Log, which needs the FiveM runtime to write anything.
public static class Log
{
    public static int Errors { get; private set; }

    public static void Debug(string message)
    {
    }

    public static void Info(string message) => Console.WriteLine("ConfigDefaults: " + message);

    public static void Warning(string message) => Console.WriteLine("ConfigDefaults: warning: " + message);

    public static void Error(string message)
    {
        Errors++;
        Console.Error.WriteLine("ConfigDefaults: error: " + message);
    }
}
