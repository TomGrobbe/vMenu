namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class VehicleSpawner
{
    public static readonly BoolSetting KeepSpawnedVehiclesPersistent =
        new("vMenu.Enhanced.VehicleSpawner.KeepSpawnedVehiclesPersistent")
        {
            Description =
                "Keeps a vehicle a player spawned loaded after they spawn another one with Replace " +
                "Previous Vehicle turned off, instead of letting the game clean it up on its own. " +
                "Turning this on means abandoned vehicles pile up until somebody deletes them.",
            Default = false,
        };

    public static readonly IntSetting SpawnLimitSeconds =
        new("vMenu.Enhanced.VehicleSpawner.SpawnLimitSeconds")
        {
            Description =
                "The stretch of time, in seconds, the three allowances below are counted over. With " +
                "the defaults a player without any of the tier permissions gets five vehicles per " +
                "minute. Set this to zero to switch spawn limits off entirely.",
            Default = 60,
        };

    public static readonly IntSetting SpawnLimitTier1 =
        new("vMenu.Enhanced.VehicleSpawner.SpawnLimitTier1")
        {
            Description =
                "How many vehicles a tier one player may spawn within the window above. Everybody " +
                "who does not hold a tier two or tier three permission lands here, so this is the " +
                "limit most players will be on. Zero means no limit.",
            Default = 5,
        };

    public static readonly IntSetting SpawnLimitTier2 =
        new("vMenu.Enhanced.VehicleSpawner.SpawnLimitTier2")
        {
            Description =
                "How many vehicles a player holding the tier two permission may spawn within the " +
                "window above. Zero means no limit.",
            Default = 15,
        };

    public static readonly IntSetting SpawnLimitTier3 =
        new("vMenu.Enhanced.VehicleSpawner.SpawnLimitTier3")
        {
            Description =
                "How many vehicles a player holding the tier three permission may spawn within the " +
                "window above. It defaults to zero, which means no limit, so handing out the tier " +
                "three permission is how you exempt somebody you trust.",
            Default = 0,
        };
}
