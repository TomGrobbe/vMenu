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

    public static readonly FloatSetting SpeedMultiplier = new("vMenu.Enhanced.TimeOptions.SpeedMultiplier")
    {
        Description =
            "How fast the in-game clock runs compared to how GTA normally runs it, where one in-game " +
            "hour takes two real minutes and a full in-game day takes 48 real minutes. Leave this at " +
            "1 to keep that normal speed. Set it to 2 and the day passes twice as fast, 5.5 makes it " +
            "five and a half times as fast, and 0.5 slows it down to half speed. The dynamic weather " +
            "schedule is measured in in-game hours, so it follows the same speed and the weather " +
            "changes faster too. Anything below 0.01 or above 1000 is pulled back to those limits, " +
            "so a typo can never stop the clock or spin it out of control. This only does anything " +
            "while the option above is on, because otherwise vMenu is not driving the clock at all.",
        Default = 1.0f,
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
