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

    public static readonly BoolSetting MatchRoutingBucket = new("vMenu.Enhanced.OnlinePlayers.MatchRoutingBucket")
    {
        Description =
            "Whether teleporting to a player, teleporting into their vehicle, and summoning a player " +
            "should also move somebody into the other player's world. Routing buckets are separate " +
            "worlds, so without this a teleport lands on the right spot in the wrong world. Turn it " +
            "off if another resource on your server owns routing buckets.",
        Default = true,
    };
}
