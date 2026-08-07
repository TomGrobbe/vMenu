namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class WeatherOptions
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.WeatherOptions.Enabled")
    {
        Description =
            "Lets vMenu drive the weather, following the same schedule GTA Online uses so every " +
            "player sees the same sky. Turn this off if another resource on your server already " +
            "owns the weather, because both will fight over it every frame and your players will " +
            "see the sky flicker between them.",
        Default = true,
    };

    public static readonly BoolSetting SyncClouds = new("vMenu.Enhanced.WeatherOptions.SyncClouds")
    {
        Description =
            "Also picks the cloud shape in the sky to match the weather, so every player sees the " +
            "same clouds. Left to itself the game picks a shape at random on each player's machine " +
            "and swaps it every few minutes, so everybody ends up under a different sky. Turn this " +
            "off if another resource on your server sets the clouds.",
        Default = true,
    };

    public static readonly IntSetting TransitionSeconds = new("vMenu.Enhanced.WeatherOptions.TransitionSeconds")
    {
        Description =
            "How long, in real seconds, the sky takes to blend when somebody forces a weather type " +
            "or hands it back to the schedule. Low values look like a hard cut. This does not " +
            "affect the schedule's own changes, which always blend gently.",
        Default = 45,
    };
}
