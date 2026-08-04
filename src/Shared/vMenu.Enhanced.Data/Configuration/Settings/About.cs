namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class About
{
    public static readonly StringSetting DocumentationUrl = new("vMenu.Enhanced.DocumentationUrl")
    {
        Description =
            "The documentation link shown in the About menu. Point it at your own guide if you run " +
            "a modified vMenu or want players sent somewhere else.",
        Default = "docs.vespura.com/vmenu/enhanced",
    };

    public static readonly StringSetting DiscordUrl = new("vMenu.Enhanced.DiscordUrl")
    {
        Description =
            "The Discord link shown in the About menu. Defaults to the Cfx.re Discord, so change " +
            "this to your own server's invite if you would rather players went there.",
        Default = "discord.gg/fivem",
    };
}
