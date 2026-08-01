namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Per vehicle class spawn permissions. A class permission covers every vehicle in that class
/// except models on the server whitelist, which answer to
/// <see cref="SupplementalPermissions.VehicleModels"/> instead.
/// </summary>
[PermissionCategory(Prefix = "vMenu.Enhanced.Menus.VehicleSpawner.Classes")]
public static class VehicleSpawnerClasses
{
    public const string All = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.All";

    public const string Compacts = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Compacts";

    public const string Sedans = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Sedans";

    public const string Suvs = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Suvs";

    public const string Coupes = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Coupes";

    public const string Muscle = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Muscle";

    public const string SportsClassics = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.SportsClassics";

    public const string Sports = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Sports";

    public const string Super = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Super";

    public const string Motorcycles = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Motorcycles";

    public const string OffRoad = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.OffRoad";

    public const string Industrial = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Industrial";

    public const string Utility = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Utility";

    public const string Vans = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Vans";

    public const string Cycles = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Cycles";

    public const string Boats = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Boats";

    public const string Helicopters = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Helicopters";

    public const string Planes = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Planes";

    public const string Service = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Service";

    public const string Emergency = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Emergency";

    public const string Military = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Military";

    public const string Commercial = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Commercial";

    public const string Trains = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.Trains";

    public const string OpenWheel = "vMenu.Enhanced.Menus.VehicleSpawner.Classes.OpenWheel";

    // Indexed by native vehicle class id, so this order is not cosmetic.
    private static readonly string[] ByClassId =
    [
        Compacts,
        Sedans,
        Suvs,
        Coupes,
        Muscle,
        SportsClassics,
        Sports,
        Super,
        Motorcycles,
        OffRoad,
        Industrial,
        Utility,
        Vans,
        Cycles,
        Boats,
        Helicopters,
        Planes,
        Service,
        Emergency,
        Military,
        Commercial,
        Trains,
        OpenWheel,
    ];

    /// <summary>Falls back to <see cref="All"/> for an unknown class id.</summary>
    public static string FromClassId(int vehicleClass) =>
        (uint)vehicleClass < (uint)ByClassId.Length ? ByClassId[vehicleClass] : All;
}
