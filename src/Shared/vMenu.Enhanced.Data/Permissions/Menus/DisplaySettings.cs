namespace vMenu.Enhanced.Data.Permissions.Menus;

[PermissionCategory]
public static class DisplaySettings
{
    public const string All = "vMenu.Enhanced.Menus.DisplaySettings.All";

    public const string ShowLocation = "vMenu.Enhanced.Menus.DisplaySettings.ShowLocation";

    public const string ShowCoordinates = "vMenu.Enhanced.Menus.DisplaySettings.ShowCoordinates";

    public const string Forecast = "vMenu.Enhanced.Menus.DisplaySettings.Forecast";

    public const string VehicleHealth = "vMenu.Enhanced.Menus.DisplaySettings.VehicleHealth";

    public const string HideHud = "vMenu.Enhanced.Menus.DisplaySettings.HideHud";

    public const string HideRadar = "vMenu.Enhanced.Menus.DisplaySettings.HideRadar";

    public const string NightVision = "vMenu.Enhanced.Menus.DisplaySettings.NightVision";

    public const string ThermalVision = "vMenu.Enhanced.Menus.DisplaySettings.ThermalVision";

    public const string Timecycles = "vMenu.Enhanced.Menus.DisplaySettings.Timecycles";

    public const string LocationBlips = "vMenu.Enhanced.Menus.DisplaySettings.LocationBlips";

    [StaffOnly]
    public const string ManageBlips = "vMenu.Enhanced.Menus.DisplaySettings.ManageBlips";
}
