namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Debugging
{
    public static readonly BoolSetting Client = new("vMenu.Enhanced.Debugging.Client")
    {
        Description =
            "Turns on vMenu's diagnostic commands in a player's own console, the one opened with F8. " +
            "Most of them only print what vMenu currently thinks is going on. A couple also put test " +
            "data on your own screen, such as the fake players the blip test draws, which nobody else " +
            "can see and which go away when you switch them off. None of them change anything on the " +
            "server. It also makes the client print its debug lines, which are the extra ones that are " +
            "far too noisy to read during normal play. With this off the client only prints the handful " +
            "of lines worth seeing, plus any warnings and errors, which are always printed either way " +
            "because something has gone wrong by the time vMenu writes one. Turn this on while you are " +
            "chasing a problem and turn it back off afterwards.",
        Default = false,
    };

    public static readonly BoolSetting Server = new("vMenu.Enhanced.Debugging.Server")
    {
        Description =
            "The same thing for the server console, and it is a separate switch, so turning one on does " +
            "not turn the other on. These commands are already limited to the console and cannot be run " +
            "by a player, so leaving this on is harmless, it just adds output you would otherwise not see.",
        Default = false,
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
