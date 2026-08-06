namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class TimeOptions
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.TimeOptions.Enabled")
    {
        Description =
            "Lets vMenu drive the clock, derived from the server's own time so every player agrees " +
            "on it regardless of their computer's clock. Turn this off if another resource on your " +
            "server already owns the time, because both will fight over it every frame.",
        Default = true,
    };

    public static readonly IntSetting TransitionSeconds = new("vMenu.Enhanced.TimeOptions.TransitionSeconds")
    {
        Description =
            "How long, in real seconds, the sky takes to sweep to a new time when somebody changes " +
            "it. Two seconds reads as a deliberate time lapse. Zero makes it an instant cut, which " +
            "is jarring when the jump crosses sunrise or sunset.",
        Default = 2,
    };
}
