namespace vMenu.Enhanced.Data.Permissions.Menus;

[PermissionCategory]
public static class WeatherOptions
{
    public const string All = "vMenu.Enhanced.Menus.WeatherOptions.All";

    // Not [StaffOnly]: reading the forecast changes nothing.
    public const string Menu = "vMenu.Enhanced.Menus.WeatherOptions.Menu";

    [StaffOnly]
    public const string SetWeather = "vMenu.Enhanced.Menus.WeatherOptions.SetWeather";
}
