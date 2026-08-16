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
}
