namespace vMenu.Enhanced.Menus.World;

public static class WorldSync
{
    public static void Initialize()
    {
        WorldState.Initialize();
        WorldTime.Initialize();
        WorldWeather.Initialize();
        WeatherForecast.Initialize();
    }
}
