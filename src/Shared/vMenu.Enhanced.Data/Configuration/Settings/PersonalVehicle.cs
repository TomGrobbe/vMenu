namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class PersonalVehicle
{
    public static readonly IntSetting ActionLimit =
        new("vMenu.Enhanced.PersonalVehicle.ActionLimit")
        {
            Description =
                "Rate limit of personal vehicle remote actions within the configured time limit. Zero means no limit.",
            Default = 8,
        };

    public static readonly IntSetting ActionLimitSeconds =
        new("vMenu.Enhanced.PersonalVehicle.ActionLimitSeconds")
        {
            Description =
                "Time in seconds for the rate limit from above. " +
                "With the default values that would be 8 actions within 10 seconds. " +
                "Zero switches the limit off entirely.",
            Default = 10,
        };

    public static readonly FloatSetting ControlRange =
        new("vMenu.Enhanced.PersonalVehicle.ControlRange")
        {
            Description =
                "Leave this as 350.0 for now, this convar only exists for debugging purposes.",
            Default = 350.0f,
        };

    public static readonly IntSetting ControlTimeout =
        new("vMenu.Enhanced.PersonalVehicle.ControlTimeout")
        {
            Description =
                "Just leave this at 1500 unless you know what you're doing. " +
                "Timeout for moving on to the next client to try and act on a vehicle within range. " +
                "Three players are tried before giving up, so keep this under 5 seconds.",
            Default = 1500,
        };
}
