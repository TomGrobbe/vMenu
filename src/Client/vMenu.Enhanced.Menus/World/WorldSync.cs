namespace vMenu.Enhanced.Menus.World;

public static class WorldSync
{
    public static void Initialize()
    {
        WorldState.Initialize();
        WorldTime.Initialize();
        WorldWeather.Initialize();
        WorldBlackout.Initialize();
        WorldSnow.Initialize();
        SnowballPickup.Initialize();
        WeatherForecast.Initialize();
    }
}
