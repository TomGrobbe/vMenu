namespace vMenu.Enhanced.PluginContracts;

/// <summary>Every event name in the plugin protocol. Plugin to vMenu events share one name and vMenu
/// reads the sender from the invoking resource, which the runtime sets and a payload cannot spoof.
/// vMenu to plugin events carry the resource name inside the event name, so each plugin only ever
/// registers handlers for its own names.</summary>
public static class PluginEvents
{
    private const string Prefix = "vMenu.Enhanced:Plugins";

    // Client side, plugin to vMenu.
    public const string Probe = Prefix + ":Probe";
    public const string Register = Prefix + ":Register";
    public const string Unregister = Prefix + ":Unregister";
    public const string Update = Prefix + ":Update";
    public const string Notify = Prefix + ":Notify";
    public const string Prompt = Prefix + ":Prompt";
    public const string SetTheme = Prefix + ":SetTheme";

    // Client side, vMenu broadcast.
    public const string Ready = Prefix + ":Ready";

    // Server side, plugin to vMenu.
    public const string ServerProbe = Prefix + ":Server:Probe";
    public const string ServerRegister = Prefix + ":Server:Register";

    // Server side, vMenu broadcast.
    public const string ServerReady = Prefix + ":Server:Ready";

    // vMenu to one plugin, client side.
    public static string ReadyFor(string resource) => $"{Prefix}:{resource}:Ready";
    public static string RegisterResultFor(string resource) => $"{Prefix}:{resource}:RegisterResult";
    public static string EventFor(string resource) => $"{Prefix}:{resource}:Event";
    public static string PromptResultFor(string resource) => $"{Prefix}:{resource}:PromptResult";
    public static string ThemesFor(string resource) => $"{Prefix}:{resource}:Themes";

    // vMenu to one plugin, server side.
    public static string ServerReadyFor(string resource) => $"{Prefix}:{resource}:Server:Ready";
    public static string ServerRegisterResultFor(string resource) => $"{Prefix}:{resource}:Server:RegisterResult";
}
