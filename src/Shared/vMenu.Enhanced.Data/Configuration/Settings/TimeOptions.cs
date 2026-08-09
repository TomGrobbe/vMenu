using vMenu.Enhanced.Data.World;

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

    public static readonly StringSetting Presets = new("vMenu.Enhanced.TimeOptions.Presets")
    {
        Description =
            "The ready made times that show up as a list in the Weather & Time menu, so somebody can " +
            "jump the clock to a common time without typing one in. Write them as a comma separated " +
            "list of four digit 24 hour times, where 0000 is midnight, 0930 is half past nine in the " +
            "morning and 2100 is nine in the evening. No colons, no dots and no spaces, and anything " +
            "that is not four digits is skipped with a warning in the client console. They appear in " +
            "the order you write them, so put the ones you use most first if you like. Leave this " +
            "empty to hide the list entirely, which still leaves the option to type a time in. " +
            "Choosing one needs the same permission as typing one in.",
        Default = TimePresets.Default,
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
