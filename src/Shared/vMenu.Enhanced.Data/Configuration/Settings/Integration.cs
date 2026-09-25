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

    public static readonly BoolSetting Allowlist = new("vMenu.Enhanced.Integration.Allowlist")
    {
        Description =
            "Only lets in players with the 'vMenu.Enhanced.Integration.Allowlist.Bypass' permission, " +
            "or players a connected integration (like SnowstormBot) allows. Uses 'set', not 'setr'.",
        Default = false,
        ServerOnly = true,
    };

    public static readonly StringSetting AllowlistMessage = new("vMenu.Enhanced.Integration.AllowlistMessage")
    {
        Description =
            "Customize the message someone sees if they're not on the allowlist.",
        Default = "You are not on the allowlist for this server.",
        ServerOnly = true,
    };

    public static readonly BoolSetting Queue = new("vMenu.Enhanced.Integration.Queue")
    {
        Description =
            "If set to true, connecting players are held in a join queue that a connected external tool (like SnowstormBot) manages. " +
            "Unlike the allowlist, this feature does not work without an external tool managing it! " +
            "The tool decides the queues, priorities and delays. This convar only turns the queue on or off. " +
            "A player holding the 'vMenu.Enhanced.Integration.Queue.Bypass' permission can skip the queue if they choose to (there is a button in the join screen they can click to bypass the queue). " +
            "Uses 'set' and not 'setr'.",
        Default = false,
        ServerOnly = true,
    };
}
