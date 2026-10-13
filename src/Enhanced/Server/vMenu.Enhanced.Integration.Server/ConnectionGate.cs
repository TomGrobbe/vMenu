using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.FuncRef;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;

using IntegrationPermissions = vMenu.Enhanced.Data.Permissions.Integration;
using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

public static class ConnectionGate
{
    private const string ConnectingEvent = "playerConnecting";

    private const string DroppedEvent = "playerDropped";

    private const string AllowlistUnavailable =
        "The allowlist cannot be reached right now, and you do not have permission to bypass it, "
        + "so access to the server is denied.";

    private static bool _registered;

    private static bool QueueOn => IntegrationAuth.IsConfigured && ServerConfig.Value(IntegrationSettings.Queue);

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        var allowlistOn = ServerConfig.Value(IntegrationSettings.Allowlist);
        var queueOn = QueueOn;

        if (!allowlistOn && !queueOn)
        {
            return;
        }

        _registered = true;

        API.OnEvent(
            ConnectingEvent,
            new Action<int, string, FunctionReference, Dictionary<string, FunctionReference>>(OnConnecting),
            false);

        API.OnEvent(DroppedEvent, new Action<int>(OnDropped), false);

        ServerTickRegistry.Register("integration:gate", IntegrationConnections.Drain, TickRate.Every(100));

        Log.Debug($"[Integration] Join gate on. allowlist={allowlistOn}, queue={queueOn}.");
    }

    private static void OnConnecting(
        [FromSource] int source,
        string name,
        FunctionReference setKickReason,
        Dictionary<string, FunctionReference> deferrals)
    {
        if (deferrals is null
            || !deferrals.TryGetValue("defer", out var defer)
            || !deferrals.TryGetValue("done", out var done)
            || !deferrals.TryGetValue("update", out var update))
        {
            Log.Warning("[Integration] playerConnecting gave no deferral callbacks; letting the player in.");
            return;
        }

        deferrals.TryGetValue("presentCard", out var presentCard);

        defer.CallVoid([]);

        // update, done and presentCard live on in the pending connection and are disposed with it. defer was only
        // needed for the initial CallVoid, and the outcome goes through done, not setKickReason.
        void ReleaseDeferOnly()
        {
            try
            {
                defer.Dispose();
                setKickReason.Dispose();
            }
            catch (Exception exception)
            {
                Log.Debug($"[Integration] Disposing deferral references failed: {exception.Message}");
            }
        }

        var sourceId = source.ToString();

        var allowlistOn = ServerConfig.Value(IntegrationSettings.Allowlist);
        var queueOn = QueueOn;

        var allowlistBypass = allowlistOn
            && ServerPermissions.IsConnectingPlayerAllowed(sourceId, IntegrationPermissions.AllowlistBypass);
        var queueBypass = queueOn
            && ServerPermissions.IsConnectingPlayerAllowed(sourceId, IntegrationPermissions.QueueBypass);

        var allowlistSatisfied = !allowlistOn || allowlistBypass;

        // FiveM ignores a done() in the same tick as defer(), which leaves the player hanging on the loading
        // screen, so even an instant decision is finished by the gate tick.
        if ((allowlistSatisfied && !queueOn) || !IntegrationAuth.IsConfigured || !IntegrationSocket.IsConnected)
        {
            var refusal = allowlistSatisfied
                ? null
                : IntegrationAuth.IsConfigured
                    ? AllowlistUnavailable
                    : ServerConfig.Value(IntegrationSettings.AllowlistMessage);

            IntegrationConnections.RegisterDecided(source, name, update, done, presentCard, refusal);
            ReleaseDeferOnly();
            return;
        }

        var id = Guid.NewGuid().ToString("N");
        var pending = IntegrationConnections.Register(
            id, source, name, update, done, presentCard, ServerConfig.Value(IntegrationSettings.AllowlistMessage));

        var identifiers = PlayerIdentifiers.Collect(sourceId, IntegrationSocket.RequestedIdentifiers);

        var payload = ServerJson.Serialize(new GateRequest
        {
            Id = id,
            ServerId = source,
            Name = name,
            Identifiers = identifiers,
            Allowlist = allowlistOn,
            Queue = queueOn,
            AllowlistBypass = allowlistBypass,
            QueueBypass = queueBypass,
        });

        IntegrationSocket.TrySend("gate-request", payload);

        if (!IntegrationSocket.IsConnected)
        {
            IntegrationConnections.ReleaseOne(pending, IntegrationSocket.SocketDropped);
        }

        ReleaseDeferOnly();
    }

    private static void OnDropped([FromSource] int source)
    {
        if (IntegrationConnections.Cancel(source) is { } id)
        {
            IntegrationSocket.TrySend("gate-cancel", ServerJson.Serialize(new { id }));
            return;
        }

        if (ServerConfig.Value(IntegrationSettings.Queue) && IntegrationSocket.IsConnected)
        {
            IntegrationSocket.TrySend(
                "player-left",
                ServerJson.Serialize(new
                {
                    serverId = source,
                    count = ConnectedPlayers.All().Count,
                    identifiers = PlayerIdentifiers.Collect(source.ToString(), IntegrationSocket.RequestedIdentifiers),
                }));
        }
    }

    private sealed class GateRequest
    {
        public string Id { get; init; } = string.Empty;

        public int ServerId { get; init; }

        public string Name { get; init; } = string.Empty;

        public Dictionary<string, string> Identifiers { get; init; } = [];

        public bool Allowlist { get; init; }

        public bool Queue { get; init; }

        public bool AllowlistBypass { get; init; }

        public bool QueueBypass { get; init; }
    }
}
