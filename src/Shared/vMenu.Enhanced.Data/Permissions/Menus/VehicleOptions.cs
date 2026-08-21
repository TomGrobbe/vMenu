namespace vMenu.Enhanced.Data.Permissions.Menus;

/// <summary>
/// Permissions for the vehicle options menu: things done to a vehicle that already exists, rather
/// than to one being created.
/// </summary>
[PermissionCategory]
public static class VehicleOptions
{
    public const string All = "vMenu.Enhanced.Menus.VehicleOptions.All";

    public const string Menu = "vMenu.Enhanced.Menus.VehicleOptions.Menu";

    /// <summary>Not <c>[StaffOnly]</c>: the server refuses anything outside the player's own reach.</summary>
    public const string DeleteVehicle = "vMenu.Enhanced.Menus.VehicleOptions.DeleteVehicle";

    public const string RepairVehicle = "vMenu.Enhanced.Menus.VehicleOptions.RepairVehicle";

    public const string WashVehicle = "vMenu.Enhanced.Menus.VehicleOptions.WashVehicle";

    /// <summary>
    /// The modification menu: every upgrade slot the vehicle offers, plus turbo, bulletproof tyres,
    /// tyre smoke and window tint.
    /// </summary>
    // One permission for the whole section rather than one per slot. A server that wants to stop
    // players making a car faster is really asking about the whole menu, and forty permissions
    // nobody reads is worse than one they do.
    public const string Modify = "vMenu.Enhanced.Menus.VehicleOptions.Modify";

    /// <summary>Paint, including pearlescent, wheel, dashboard, interior and the paint fade scale.</summary>
    public const string Colors = "vMenu.Enhanced.Menus.VehicleOptions.Colors";

    public const string Liveries = "vMenu.Enhanced.Menus.VehicleOptions.Liveries";

    public const string Extras = "vMenu.Enhanced.Menus.VehicleOptions.Extras";

    /// <summary>Underglow: which sides are lit, and what colour.</summary>
    public const string Neon = "vMenu.Enhanced.Menus.VehicleOptions.Neon";

    /// <summary>Wheel type, rims, custom tyres and drift tyres.</summary>
    public const string Wheels = "vMenu.Enhanced.Menus.VehicleOptions.Wheels";

    /// <summary>Xenon headlights and their colour.</summary>
    public const string Lights = "vMenu.Enhanced.Menus.VehicleOptions.Lights";

    /// <summary>The licence plate's text and its style.</summary>
    public const string Plate = "vMenu.Enhanced.Menus.VehicleOptions.Plate";

    public const string Doors = "vMenu.Enhanced.Menus.VehicleOptions.Doors";

    public const string Windows = "vMenu.Enhanced.Menus.VehicleOptions.Windows";

    /// <summary>Setting how dirty the vehicle looks, which is not the same as washing it clean.</summary>
    public const string Dirt = "vMenu.Enhanced.Menus.VehicleOptions.Dirt";

    /// <summary>The god mode toggle and every one of the seven damage types it can turn off.</summary>
    // One permission for the lot, for the same reason Modify is one: a server that does not want
    // invincible cars does not want unbreakable wheels either.
    public const string God = "vMenu.Enhanced.Menus.VehicleOptions.God";

    /// <summary>Washing dust off as it appears, which is not the same as the wash option.</summary>
    public const string KeepClean = "vMenu.Enhanced.Menus.VehicleOptions.KeepClean";
}
