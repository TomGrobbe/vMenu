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
}
