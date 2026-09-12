namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Debugging
{
    public static readonly BoolSetting Client = new("vMenu.Enhanced.Debugging.Client")
    {
        Description =
            "Enables additional console logging on the client side.",
        Default = false,
    };

    public static readonly BoolSetting Server = new("vMenu.Enhanced.Debugging.Server")
    {
        Description =
            "Enables additional console logging on the server side.",
        Default = false,
    };

    public static readonly BoolSetting ExperimentalFeatures = new("vMenu.Enhanced.Debugging.ExperimentalFeatures")
    {
        Description =
            "Currently doesn't do anything, leave this as false for now.",
        Default = false,
    };
}
