namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class JoinLeave
{
    public static readonly BoolSetting LogToConsole = new("vMenu.Enhanced.JoinLeave.LogToConsole")
    {
        Description =
            "Writes a line to your server console every time somebody starts connecting, finishes " +
            "loading in, gives up before they get there, or leaves. Leaving includes why they went " +
            "when the server knows, so a kick or a ban shows its reason here. This is only about your " +
            "console. It is separate from the join and leave messages players see on screen, which " +
            "every player turns on or off for themselves in misc settings. On by default, since it is " +
            "how you work out who was on at the time something happened. Turn it off if your console " +
            "is busy enough that you would rather not have the lines.",
        Default = true,
    };
}
