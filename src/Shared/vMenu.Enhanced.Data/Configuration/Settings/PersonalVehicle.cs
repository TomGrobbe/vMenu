namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class PersonalVehicle
{
    public static readonly IntSetting ActionLimit =
        new("vMenu.Enhanced.PersonalVehicle.ActionLimit")
        {
            Description =
                "How many times one player may act on their personal vehicle within the time window " +
                "below. Marking a vehicle, deleting it and emptying it all count towards the same " +
                "allowance. Zero means no limit.",
            Default = 8,
        };

    public static readonly IntSetting ActionLimitSeconds =
        new("vMenu.Enhanced.PersonalVehicle.ActionLimitSeconds")
        {
            Description =
                "The stretch of time, in seconds, the allowance above is counted over. Zero switches " +
                "the limit off entirely.",
            Default = 10,
        };
}
