namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Updates
{
    public static readonly StringSetting CheckMode = new("vMenu.Enhanced.Updates.CheckMode")
    {
        Description =
            "Whether vMenu looks for a newer version of itself, and which kind it looks for. Use " +
            "'prerelease' to hear about every new build including the alpha ones, 'stable' to hear " +
            "only about finished releases, or 'off' to never look at all. The check runs once when " +
            "your server starts and every six hours after that, and you can run it yourself at any " +
            "time with vmenu_checkupdates. It reads a public list of releases from github.com and " +
            "nuget.org, it sends nothing whatsoever about your server, and it never downloads or " +
            "installs anything for you. What it finds goes into your server console, and any of " +
            "your staff who are online get a quiet notice on screen. Prerelease by default, " +
            "because vMenu Enhanced is still in alpha and every build so far is a prerelease, so " +
            "'stable' will quietly find nothing at all until version 1.0.0 lands.",
        Default = "prerelease",
    };
}
