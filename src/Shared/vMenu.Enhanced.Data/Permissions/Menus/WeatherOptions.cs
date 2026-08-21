namespace vMenu.Enhanced.Data.Permissions.Menus;

[PermissionCategory]
public static class WeatherOptions
{
    public const string All = "vMenu.Enhanced.Menus.WeatherOptions.All";

    /// <summary>Not <c>[StaffOnly]</c>: reading the forecast changes nothing.</summary>
    public const string Menu = "vMenu.Enhanced.Menus.WeatherOptions.Menu";

    [StaffOnly]
    public const string SetWeather = "vMenu.Enhanced.Menus.WeatherOptions.SetWeather";

    public const string Forecast = "vMenu.Enhanced.Menus.WeatherOptions.Forecast";
}
