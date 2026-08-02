namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class DeveloperFeatures
{
    public static readonly BoolSetting Enabled = new("vMenu.Enhanced.DeveloperFeatures")
    {
        Description =
            "Enables the Developer Features menu. Pretty much all features in here are generally harmless. " +
        "Give them a try, and then decide if you want to have them enabled or disabled for your server. " +
        "If you're unsure, leave this off on a public, production, server.",
        Default = false,
    };
}
