namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class VehicleSpawner
{
    public const int DeleteWhenNotRelevant = 0;

    public const int DeleteOnOwnerDisconnect = 1;

    public const int KeepEntity = 2;

    public static readonly IntSetting OrphanMode =
        new("vMenu.Enhanced.VehicleSpawner.OrphanMode")
        {
            Description =
                "What the server does to a vehicle if the player that spawned it leaves the server or crashes their game. " +
                $"{nameof(DeleteWhenNotRelevant)} = {DeleteWhenNotRelevant}, {nameof(DeleteOnOwnerDisconnect)} = {DeleteOnOwnerDisconnect}, {nameof(KeepEntity)} = {KeepEntity}.",
            Default = DeleteOnOwnerDisconnect,
        };

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
                "The stretch of time, in seconds, the three allowances below are counted over. " +
                "Set this to zero to switch spawn limits off entirely.",
            Default = 60,
        };

    public static readonly IntSetting SpawnLimitTier1 =
        new("vMenu.Enhanced.VehicleSpawner.SpawnLimitTier1")
        {
            Description =
                "How many vehicles a tier 1 player may spawn within the window above. Zero means no limit.",
            Default = 5,
        };

    public static readonly IntSetting SpawnLimitTier2 =
        new("vMenu.Enhanced.VehicleSpawner.SpawnLimitTier2")
        {
            Description =
                "How many vehicles a tier 2 player may spawn within the window above. Zero means no limit.",
            Default = 15,
        };

    public static readonly IntSetting SpawnLimitTier3 =
        new("vMenu.Enhanced.VehicleSpawner.SpawnLimitTier3")
        {
            Description =
                "How many vehicles a tier 3 player may spawn within the window above. Zero means no limit.",
            Default = 0,
        };

    public static bool IsKnownOrphanMode(int mode) =>
        mode is DeleteWhenNotRelevant or DeleteOnOwnerDisconnect or KeepEntity;

    public static int NormaliseOrphanMode(int mode) =>
        IsKnownOrphanMode(mode) ? mode : OrphanMode.Default;
}
