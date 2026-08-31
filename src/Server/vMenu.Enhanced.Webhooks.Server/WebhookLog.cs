using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Http.Server;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks.Server;

using LoggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Logging;

namespace vMenu.Enhanced.Webhooks.Server;

public static class WebhookLog
{
    private const string StopEvent = "onResourceStop";

    private const string TestCommand = "vmenu_webhook_test";

    private const string TickName = "Webhooks.Flush";

    private static readonly (string Key, string Value)[] NoData = [];

    private static readonly Setting[] Watched =
    [
        LoggingSettings.Enabled,
        LoggingSettings.EventsWebhook,
        LoggingSettings.ActionsWebhook,
        LoggingSettings.StaffWebhook,
        LoggingSettings.GenericWebhook,
    ];

    private static readonly DiscordChannel EventsChannel = new(LogCategory.Event, LoggingSettings.EventsWebhook);

    private static readonly DiscordChannel ActionsChannel = new(LogCategory.Action, LoggingSettings.ActionsWebhook);

    private static readonly DiscordChannel StaffChannel = new(LogCategory.Staff, LoggingSettings.StaffWebhook);

    private static readonly GenericChannel Generic = new();

    private static bool _initialized;

    public static bool Wants(LogCategory category) =>
        IsOn && (Channel(category).IsConfigured || Generic.IsConfigured);

    public static bool WantsMenuActions => IsOn && (ActionsChannel.IsConfigured || Generic.IsConfigured);

    private static bool IsOn => ServerConfig.Value(LoggingSettings.Enabled);

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        ServerTickRegistry.Register(TickName, FlushAsync, TickRate.Varying(FlushRate));

        ServerConfig.AddEventListenerFor(Watched, OnSettingsChanged);

        API.OnEvent(StopEvent, new Action<string>(OnResourceStop), false);

        SharedAPI.Commands.RegisterCommand(TestCommand, true, new Action(SendTest));

        Publish();

        Log.Debug($"[Webhooks] Ready. Logging is {(IsOn ? "on" : "off")}.");
    }

    public static void Event(string message, WebhookActor? actor = null, params (string Key, string Value)[] data) =>
        Queue(LogCategory.Event, actor ?? WebhookActor.Server, null, message, false, data);

    public static void Event(string message, WebhookActor actor, WebhookActor? target, params (string Key, string Value)[] data) =>
        Queue(LogCategory.Event, actor, target, message, false, data);

    public static void Connection(WebhookActor actor, string message, params (string Key, string Value)[] data) =>
        Queue(LogCategory.Event, actor, null, message, true, data);

    public static void Action(WebhookActor actor, string message, params (string Key, string Value)[] data) =>
        Queue(LogCategory.Action, actor, null, message, false, data);

    public static void Warn(WebhookActor actor, string message, params (string Key, string Value)[] data) =>
        Queue(LogCategory.Action, actor, null, message, false, data, warning: true);

    public static void Staff(WebhookActor actor, WebhookActor? target, string message, params (string Key, string Value)[] data) =>
        Queue(LogCategory.Staff, actor, target, message, false, data);

    public static void Staff(Player source, WebhookActor? target, string message, params (string Key, string Value)[] data) =>
        Queue(LogCategory.Staff, WebhookActor.For(source), target, message, false, data);

    private static void Queue(
        LogCategory category,
        WebhookActor actor,
        WebhookActor? target,
        string message,
        bool withIdentifiers,
        (string Key, string Value)[] data,
        bool warning = false)
    {
        if (!IsOn)
        {
            return;
        }

        var discord = Channel(category);

        if (!discord.IsConfigured && !Generic.IsConfigured)
        {
            return;
        }

        var entry = new WebhookEntry(
            category,
            DateTimeOffset.UtcNow,
            message,
            actor,
            target,
            data ?? NoData,
            withIdentifiers,
            warning);

        discord.Add(entry);
        Generic.Add(entry);
    }

    private static DiscordChannel Channel(LogCategory category) => category switch
    {
        LogCategory.Action => ActionsChannel,
        LogCategory.Staff => StaffChannel,
        _ => EventsChannel,
    };

    private static async Task FlushAsync()
    {
        if (!IsOn)
        {
            return;
        }

        await EventsChannel.FlushAsync();
        await ActionsChannel.FlushAsync();
        await StaffChannel.FlushAsync();
        await Generic.FlushAsync();
    }

    private static TickRate FlushRate() =>
        TickRate.Every(LoggingSettings.ClampFlushSeconds(ServerConfig.Value(LoggingSettings.FlushSeconds)) * 1000);

    private static void OnSettingsChanged()
    {
        EventsChannel.Reset();
        ActionsChannel.Reset();
        StaffChannel.Reset();
        Generic.Reset();

        Publish();
    }

    private static void Publish() =>
        Native.SetConvarReplicated(
            AuditStateConvars.MenuReporting,
            WantsMenuActions ? AuditStateConvars.On : AuditStateConvars.Off);

    private static void OnResourceStop(string stopped)
    {
        if (!string.Equals(stopped, Native.GetCurrentResourceName(), StringComparison.OrdinalIgnoreCase)
            || !IsOn)
        {
            return;
        }

        var url = ServerConfig.Value(LoggingSettings.EventsWebhook).Trim();

        if (url.Length == 0)
        {
            return;
        }

        var entry = new WebhookEntry(
            LogCategory.Event,
            DateTimeOffset.UtcNow,
            "vMenu Enhanced stopped.",
            WebhookActor.Server,
            null,
            NoData);

        _ = HttpSend.SendAsync(
            new HttpRequest(url, "application/json", WebhookIdentity.UserAgent(), 3000)
            {
                Method = "POST",
                Body = DiscordPayload.Build(LogCategory.Event, entry.Line(), WebhookIdentity.Footer()),
            });
    }

    private static void SendTest()
    {
        if (!IsOn)
        {
            Log.Info(
                $"[Webhooks] Logging is off. Set {LoggingSettings.Enabled.Name} to true before testing.");

            return;
        }

        Event("Webhook test from the server console.");
        Action(WebhookActor.Server, "This is a test player action.");
        Staff(WebhookActor.Server, null, "This is a test staff action.");

        Log.Info(
            "[Webhooks] Queued one test line per category. Anything you configured should see it within "
            + $"{LoggingSettings.ClampFlushSeconds(ServerConfig.Value(LoggingSettings.FlushSeconds))}s.");
    }
}
