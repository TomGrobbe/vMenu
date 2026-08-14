namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Debugging
{
    public static readonly BoolSetting Client = new("vMenu.Enhanced.Debugging.Client")
    {
        Description =
            "Turns on vMenu's diagnostic commands in a player's own console, the one opened with F8. " +
            "Those commands only print what vMenu currently thinks is going on, they never change " +
            "anything, but they are noise nobody needs during normal play. Turn this on while you are " +
            "chasing a problem and turn it back off afterwards.",
        Default = false,
    };

    public static readonly BoolSetting Server = new("vMenu.Enhanced.Debugging.Server")
    {
        Description =
            "The same thing for the server console. These commands are already limited to the console " +
            "and cannot be run by a player, so leaving this on is harmless, it just adds output you " +
            "would otherwise not see.",
        Default = false,
    };

    public static readonly StringSetting LogLevel = new("vMenu.Enhanced.Debugging.LogLevel")
    {
        Description =
            "How much vMenu writes to the console, on both the client and the server. Pick Trace, " +
            "Debug or Info. Trace prints everything, Debug leaves out the noisiest of it, and Info " +
            "prints only the handful of lines worth reading during normal play. Anything below the " +
            "level you pick is dropped and never reaches a console. Warnings and errors are always " +
            "printed whichever you choose, because something has gone wrong by the time vMenu writes " +
            "one. Use Trace or Debug while you are chasing a problem, and leave it on Info the rest " +
            "of the time.",
        Default = "Info",
    };

    public static readonly BoolSetting ExperimentalFeatures = new("vMenu.Enhanced.Debugging.ExperimentalFeatures")
    {
        Description =
            "Switches on features that are not finished. They may be broken, they may change without " +
            "warning, and they may disappear in a later version. Do not turn this on for a server that " +
            "people actually play on.",
        Default = false,
    };
}
