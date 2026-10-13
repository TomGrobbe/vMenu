using System.Text;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Http.Server;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;
using vMenu.Enhanced.Webhooks.Server;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

public static class IntegrationPlayersPush
{
    private const long WatchMs = 2000;

    private const int TimeoutMs = 8000;

    private const string ChangedPath = "players/changed";

    private static bool _running;

    private static int _lastCount = -1;

    private static bool? _reachable;

    public static void Initialize()
    {
        var tick = ServerTickRegistry.Register(
            "Integration.PlayersPush",
            Watch,
            TickRate.Every(WatchMs),
            condition: () => IsConfigured);

        ServerConfig.AddEventListenerFor(
            [IntegrationSettings.ApiKey, IntegrationSettings.Endpoint],
            () =>
            {
                _lastCount = -1;
                tick.Reevaluate();
            });
    }

    private static bool IsConfigured =>
        ServerConfig.Value(IntegrationSettings.ApiKey).Length > 0
        && ServerConfig.Value(IntegrationSettings.Endpoint).Length > 0;

    private static async Task Watch()
    {
        if (_running || !IsConfigured)
        {
            return;
        }

        var count = ConnectedPlayers.All().Count;

        if (count == _lastCount)
        {
            return;
        }

        _running = true;

        try
        {
            var reply = await Push();

            if (reply.IsAccepted)
            {
                _lastCount = count;
            }

            Report(reply, count);
        }
        finally
        {
            _running = false;
        }
    }

    private static Task<HttpReply> Push()
    {
        var url = Combine(ServerConfig.Value(IntegrationSettings.Endpoint), ChangedPath);
        var key = ServerConfig.Value(IntegrationSettings.ApiKey);
        // Still on the tick thread here (Watch calls Push before its first await), so the roster natives are safe.
        var body = ServerJson.Serialize(PlayersChangedPayload.Of(OnlinePlayersResponse.Current()));

        return HttpSend.SendAsync(
            new HttpRequest(url, "application/json", WebhookIdentity.UserAgent(), TimeoutMs)
            {
                Method = "POST",
                Body = body,
                Headers = IntegrationAuth.SignHeaders(IntegrationActions.PlayersChanged, key, Encoding.UTF8.GetBytes(body)),
            });
    }

    private static void Report(HttpReply reply, int count)
    {
        if (reply.IsAccepted)
        {
            Log.Debug($"[Integration] Told the remote the player count is {count}.");

            _reachable = true;

            return;
        }

        if (_reachable != false)
        {
            Log.Debug(
                $"[Integration] Could not push the player count ({reply.Reason ?? "status " + reply.Status}).");
        }

        _reachable = false;
    }

    private static string Combine(string baseUrl, string path) =>
        baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
}
