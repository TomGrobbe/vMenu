using vMenu.Enhanced.Data.World;

namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class TimeOptions
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.TimeOptions.Enabled")
    {
        Description =
            "Enables vMenu time control/sync. Set this to 'false' if you have another resource controlling game time.",
        Default = true,
    };

    public static readonly FloatSetting SpeedMultiplier = new("vMenu.Enhanced.TimeOptions.SpeedMultiplier")
    {
        Description =
            "How fast the in-game clock runs compared to how GTA normally runs it. " +
            "Value must be between 0.01 and 1000.",
        Default = 1.0f,
    };

    public static readonly StringSetting Presets = new("vMenu.Enhanced.TimeOptions.Presets")
    {
        Description =
            "A list of preset times to choose from in the time options menu. ",
        Default = TimePresets.Default,
    };

    public static readonly IntSetting TransitionSeconds = new("vMenu.Enhanced.TimeOptions.TransitionSeconds")
    {
        Description =
            "How long time transitions take. Zero instantly jumps to the desired time.",
        Default = 4,
    };
}
