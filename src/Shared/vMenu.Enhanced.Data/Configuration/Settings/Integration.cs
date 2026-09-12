namespace vMenu.Enhanced.Data.Configuration.Settings;

public static class Integration
{
    public static readonly StringSetting ApiKey = new("vMenu.Enhanced.Integration.ApiKey")
    {
        Description =
            "Please read https://docs.vespura.com/vmenu/enhanced/integrations. Uses 'set' and not 'setr'! This is important to keep your API key secret!",
        ServerOnly = true,
    };

    public static readonly StringSetting Endpoint = new("vMenu.Enhanced.Integration.Endpoint")
    {
        Description =
            "The base URL of the external tool this server connects to, for example " +
            "'https://example.com/api'. Value can be http or https, and an IP or a domain both work. " +
            "If using HTTPS, the certificate must be valid and trusted. This server makes outgoing " +
            "HTTP(S) calls to this URL, and also opens a websocket derived from it, so " +
            "'https://example.com/api' becomes 'wss://example.com/api/socket' (uses 'ws' for http instead). " +
            "Leave it empty and this server makes no outgoing calls at all. " +
            "Please read the documentation if you're planning on using this. " +
            "Uses 'set' and not 'setr'.",
        ServerOnly = true,
    };

    public static readonly BoolSetting AllowActions = new("vMenu.Enhanced.Integration.AllowActions")
    {
        Description =
            "If set to true, it will allow external tools that connect using the websocket to perform actions against players. " +
            "For example: kill, kick, heal, etc. " +
            "Uses 'set' and not 'setr'.",
        Default = false,
        ServerOnly = true,
    };
}
