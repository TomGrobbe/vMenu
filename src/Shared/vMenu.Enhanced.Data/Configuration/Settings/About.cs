namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class About
{
    public static readonly StringSetting DocumentationUrl = new("vMenu.Enhanced.DocumentationUrl")
    {
        Description =
            "The documentation link shown in the About menu. Change this if you'd like to have " +
            "your own server documentation listed here.",
        Default = "https://docs.vespura.com/vmenu/enhanced",
    };

    public static readonly StringSetting DiscordUrl = new("vMenu.Enhanced.DiscordUrl")
    {
        Description =
            "The Discord invite link shown in the About menu.",
        Default = "https://discord.gg/fivem",
    };
}
