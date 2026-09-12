namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Updates
{
    public static readonly StringSetting CheckMode = new("vMenu.Enhanced.Updates.CheckMode")
    {
        Description =
            "Configures the automatic version checks vMenu performs. " +
            "Can be set to 'off', 'prerelease' or 'stable'.",
        Default = "prerelease",
    };
}
