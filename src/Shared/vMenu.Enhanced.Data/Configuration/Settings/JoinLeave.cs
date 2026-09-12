namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class JoinLeave
{
    public static readonly BoolSetting LogToConsole = new("vMenu.Enhanced.JoinLeave.LogToConsole")
    {
        Description =
            "Logs player connections to the server console.",
        Default = true,
    };
}
