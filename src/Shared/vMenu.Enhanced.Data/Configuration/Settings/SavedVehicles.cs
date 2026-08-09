namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class SavedVehicles
{
    public static readonly IntSetting MaxSavedVehicles = new("vMenu.Enhanced.SavedVehicles.MaxSavedVehicles")
    {
        Description =
            "How many vehicles a player may keep saved, or 0 for no limit. Saved vehicles live in " +
            "the player's own local storage rather than on your server, so this is only there to stop " +
            "that storage growing without bound. It is not a security control: the check happens on " +
            "the client, and a player who already has more saves than this keeps all of them and is " +
            "simply not allowed to add another.",
        Default = 0,
    };
}
