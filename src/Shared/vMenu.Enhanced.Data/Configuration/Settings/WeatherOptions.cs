namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class WeatherOptions
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.WeatherOptions.Enabled")
    {
        Description =
            "Enables or disables vMenu's weather control and sync. Turn this off if you want another resource to control the weather.",
        Default = true,
    };

    public static readonly StringSetting WeatherCycle = new("vMenu.Enhanced.WeatherOptions.WeatherCycle")
    {
        Description =
            "Which weather schedule the server follows: 'default' (the normal GTA Online one), 'snowy' (a winter one that keeps snow on the ground while the snow setting is on automatic) or 'custom' (your own schedule from config/weather-cycle.json, which is the only one that can also schedule blackouts).",
        Default = "default",
    };

    public static readonly BoolSetting SyncClouds = new("vMenu.Enhanced.WeatherOptions.SyncClouds")
    {
        Description =
            "Syncs clouds with all players.",
        Default = true,
    };

    public static readonly IntSetting TransitionSeconds = new("vMenu.Enhanced.WeatherOptions.TransitionSeconds")
    {
        Description =
            "How long, in seconds, it takes for weather to transition from one type to another when manually changing types in the menu.",
        Default = 45,
    };
}
