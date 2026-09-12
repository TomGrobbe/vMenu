using System.Text;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Http.Server;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;
using vMenu.Enhanced.Webhooks.Server;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

public static class IntegrationConnectCheck
{
    private const long IntervalMs = 10L * 60L * 1000L;

    private const int TimeoutMs = 8000;

    private const string ConnectPath = "connect/status";

    private static bool _running;

    // Null until the first check answers, so both the first success and first failure log.
    private static bool? _reachable;

    public static void Initialize()
    {
        var tick = ServerTickRegistry.Register(
            "Integration.Connect",
            RunAsync,
            TickRate.Every(IntervalMs),
            condition: () => IsConfigured);

        ServerConfig.AddEventListenerFor([IntegrationSettings.ApiKey, IntegrationSettings.Endpoint], tick.Reevaluate);

        _ = RunAsync();
    }

    private static bool IsConfigured =>
        ServerConfig.Value(IntegrationSettings.ApiKey).Length > 0
        && ServerConfig.Value(IntegrationSettings.Endpoint).Length > 0;

    private static async Task RunAsync()
    {
        if (_running || !IsConfigured)
        {
            return;
        }

        _running = true;

        try
        {
            var url = Combine(ServerConfig.Value(IntegrationSettings.Endpoint), ConnectPath);
            var key = ServerConfig.Value(IntegrationSettings.ApiKey);
            var body = ServerJson.Serialize(ConnectPayload.Current());

            var reply = await HttpSend.SendAsync(
                new HttpRequest(url, "application/json", WebhookIdentity.UserAgent(), TimeoutMs)
                {
                    Method = "POST",
                    Body = body,
                    Headers = IntegrationAuth.SignHeaders(IntegrationActions.ConnectStatus, key, Encoding.UTF8.GetBytes(body)),
                });

            Report(url, reply);
        }
        finally
        {
            _running = false;
        }
    }

    private static void Report(string url, HttpReply reply)
    {
        if (reply.IsAccepted)
        {
            if (_reachable != true)
            {
                Log.Info($"[Integration] Connected to the remote at {url}.");
            }
            else
            {
                Log.Debug($"[Integration] Checked in with {url} ({reply.Status}).");
            }

            _reachable = true;

            return;
        }

        if (_reachable != false)
        {
            Log.Warning($"[Integration] Could not reach the remote at {url} ({reply.Reason ?? "status " + reply.Status}).");
        }

        _reachable = false;
    }

    private static string Combine(string baseUrl, string path) =>
        baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
}
