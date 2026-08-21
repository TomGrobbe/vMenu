namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class OnlinePlayers
{
    public static readonly IntSetting ActionLimit = new("vMenu.Enhanced.OnlinePlayers.ActionLimit")
    {
        Description =
            "How many things one player may do to other players from the online players menu " +
            "within the time window below.",
        Default = 8,
    };

    public static readonly IntSetting ActionLimitSeconds = new("vMenu.Enhanced.OnlinePlayers.ActionLimitSeconds")
    {
        Description =
            "The length of the window the allowance above is counted over, in seconds. " +
            "With the defaults, a player gets 8 actions per 10 seconds.",
        Default = 10,
    };
}
