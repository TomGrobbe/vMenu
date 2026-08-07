namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Per vehicle category spawn permissions. A category is either one of the game's own vehicle
/// classes, declared here, or one a server owner defined in <c>config/vehicle-categories.json</c>,
/// registered at runtime under the same container. A category permission covers every vehicle in
/// it except models on the server whitelist, which answer to
/// <see cref="SupplementalPermissions.VehicleModels"/> instead.
/// </summary>
[PermissionCategory(Prefix = Prefix)]
public static class VehicleSpawnerCategories
{
    /// <summary>Not a permission itself; it is not deeper than the category prefix.</summary>
    public const string Prefix = "vMenu.Enhanced.Menus.VehicleSpawner.Categories";

    public const string All = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.All";

    public const string Compacts = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Compacts";

    public const string Sedans = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Sedans";

    public const string Suvs = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Suvs";

    public const string Coupes = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Coupes";

    public const string Muscle = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Muscle";

    public const string SportsClassics = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.SportsClassics";

    public const string Sports = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Sports";

    public const string Super = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Super";

    public const string Motorcycles = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Motorcycles";

    public const string OffRoad = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.OffRoad";

    public const string Industrial = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Industrial";

    public const string Utility = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Utility";

    public const string Vans = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Vans";

    public const string Cycles = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Cycles";

    public const string Boats = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Boats";

    public const string Helicopters = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Helicopters";

    public const string Planes = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Planes";

    public const string Service = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Service";

    public const string Emergency = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Emergency";

    public const string Military = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Military";

    public const string Commercial = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Commercial";

    public const string Trains = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.Trains";

    public const string OpenWheel = "vMenu.Enhanced.Menus.VehicleSpawner.Categories.OpenWheel";

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

    /// <summary>
    /// The permission for a category a server owner defined. Feed it a segment from
    /// <see cref="VehicleCategoryName.ToPermissionSegment"/>, never a raw name.
    /// </summary>
    public static string ForCustom(string segment) =>
        $"{Prefix}{PermissionPath.Separator}{segment}";
}
