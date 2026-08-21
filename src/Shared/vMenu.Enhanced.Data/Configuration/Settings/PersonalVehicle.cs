namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class PersonalVehicle
{
    public static readonly IntSetting ActionLimit =
        new("vMenu.Enhanced.PersonalVehicle.ActionLimit")
        {
            Description =
                "How many times one player may act on their personal vehicle within the time window " +
                "below. Marking a vehicle, deleting it, emptying it, locking it, starting it, opening " +
                "its doors and everything else the menu can do to it all count towards the same " +
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

    public static readonly FloatSetting ControlRange =
        new("vMenu.Enhanced.PersonalVehicle.ControlRange")
        {
            Description =
                "How close a player has to be standing to a personal vehicle, in metres, before the " +
                "server will ask their game to act on it. Locking a car, starting it or opening its " +
                "doors can only be done by a machine that has the car loaded, so this is a technical " +
                "reach rather than a rule: the owner themselves can be anywhere on the map. Raising " +
                "it much past the game's own streaming distance only means asking players who cannot " +
                "help.",
            Default = 350.0f,
        };

    public static readonly IntSetting ControlTimeout =
        new("vMenu.Enhanced.PersonalVehicle.ControlTimeout")
        {
            Description =
                "How long the server waits, in milliseconds, for one player's game to report back " +
                "before asking the next player instead. Three players are tried before giving up, so " +
                "keep this well under ten seconds.",
            Default = 1500,
        };
}
