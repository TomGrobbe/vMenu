using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Ticks.Server;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

// Two things break on FiveM's C# runtime:
// 1. Cancelling a socket call crashes instead of stopping it. So on shutdown we just abort the socket.
// 2. Game functions (Log included) only work on the tick thread. So this socket loop never calls one:
//    it only reads plain strings and drops work onto queues, and the ticks do the game calls.
public static class IntegrationSocket
{
    private const string StopEvent = "onResourceStop";

    private const int ConnectTimeoutMs = 10000;

    private const int BaseReconnectMs = 5000;

    private const int MaxReconnectMs = 300000;

    private const int MaxMessageBytes = 64 * 1024;

    private const int CommandTimeoutMs = 5000;

    private const string TypePing = "ping";
    private const string TypePong = "pong";
    private const string TypeReady = "ready";
    private const string TypeStartStream = "start-stream";
    private const string TypeStopStream = "stop-stream";
    private const string TypeCommand = "command";
    private const string TypeResync = "resync";
    private const string TypePlayers = "players";
    private const string TypeBlips = "blips";
    private const string TypeWorld = "world";
    private const string TypeBuckets = "buckets";
    private const string TypeCommandAck = "command-ack";
    private const string TypeGate = "gate";
    private const string TypeRoleSync = "role-sync";
    internal const string TypeRoleSyncRequest = "role-sync-request";

    internal const string SocketDropped = "The connection to the server manager dropped. Please try joining again.";

    private static readonly TimeSpan StreamInterval = TimeSpan.FromSeconds(1);

    private static readonly TimeSpan WorldHeartbeat = TimeSpan.FromSeconds(15);

    private static readonly TimeSpan MinHealthySession = TimeSpan.FromSeconds(15);

    private static readonly Random Jitter = new();

    private static readonly ConcurrentQueue<(LogLevel Level, string Message)> Logs = new();

    private static readonly SemaphoreSlim SendGate = new(1, 1);

    private static volatile bool _stopping;

    private static volatile bool _streaming;

    private static volatile bool _connected;

    private static volatile string[] _requestedIdentifiers = ["discord"];

    private static int _consecutiveFailures;

    private static bool _started;

    private static ClientWebSocket? _socket;

    private static string _url = "";

    public static bool IsConnected => _connected;

    public static IReadOnlyCollection<string> RequestedIdentifiers => _requestedIdentifiers;

    public static void TrySend(string type, string payloadJson)
    {
        var ws = _socket;
        if (ws is not null && ws.State == WebSocketState.Open)
        {
            _ = SendAsync(ws, Frame(type, payloadJson));
        }
    }

    public static void Initialize()
    {
        if (_started)
        {
            return;
        }

        var endpoint = ServerConfig.Value(IntegrationSettings.Endpoint);
        var key = ServerConfig.Value(IntegrationSettings.ApiKey);
        if (endpoint.Length == 0 || key.Length == 0)
        {
            Log.Debug(
                $"[Integration] Socket stays idle until {IntegrationSettings.ApiKey.Name} and " +
                $"{IntegrationSettings.Endpoint.Name} are set.");

            return;
        }

        _started = true;
        _url = ToSocketUrl(endpoint);

        // Proves the crypto loads in this runtime, up front.
        IntegrationSigning.Verify();

        IntegrationSnapshots.Update();
        ServerTickRegistry.Register(
            "integration:snapshots", IntegrationSnapshots.Update, TickRate.Every(500), condition: () => IntegrationAuth.IsConfigured);
        ServerTickRegistry.Register(
            "integration:commands", IntegrationCommands.Drain, TickRate.Every(50), condition: () => IntegrationAuth.IsConfigured);
        ServerTickRegistry.Register("integration:socketlogs", DrainLogs, TickRate.Every(500));

        API.OnEvent(StopEvent, new Action<string>(OnResourceStop), false);

        _ = Task.Run(RunAsync);
    }

    private static string ToSocketUrl(string endpoint)
    {
        var trimmed = endpoint.TrimEnd('/');
        if (trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "wss://" + trimmed["https://".Length..];
        }
        else if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "ws://" + trimmed["http://".Length..];
        }

        return trimmed + "/socket";
    }

    private static void OnResourceStop(string stopped)
    {
        if (!string.Equals(stopped, Native.GetCurrentResourceName(), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _stopping = true;

        try
        {
            _socket?.Abort();
        }
        catch (Exception exception)
        {
            Emit(LogLevel.Debug, $"[Integration] Aborting the socket failed: {exception.Message}");
        }
    }

    private static async Task RunAsync()
    {
        while (!_stopping)
        {
            try
            {
                using var ws = new ClientWebSocket();
                ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

                // Proxy off: system-proxy lookup loads Microsoft.Win32.Registry, not shipped, and throws.
                ws.Options.Proxy = null;

                var key = IntegrationSnapshots.Key;
                if (key.Length == 0)
                {
                    await Task.Delay(2000);
                    continue;
                }

                foreach (var header in IntegrationAuth.SignHeaders(IntegrationActions.Socket, key, []))
                {
                    ws.Options.SetRequestHeader(header.Key, header.Value);
                }

                _socket = ws;
                _streaming = false;

                if (await ConnectAsync(ws))
                {
                    Emit(LogLevel.Info, "[Integration] Socket connected.");

                    _connected = true;
                    var connectedAt = DateTime.UtcNow;
                    var stream = StreamLoopAsync(ws);
                    await ReceiveLoopAsync(ws);
                    await stream;

                    // A real session outlives this. A link that accepts the upgrade then drops us straight
                    // away, or flaps, stays counted as a failure so the backoff keeps growing.
                    if (DateTime.UtcNow - connectedAt >= MinHealthySession)
                    {
                        _consecutiveFailures = 0;
                    }
                    else
                    {
                        NoteFailure("Socket closed right after connecting.");
                    }
                }
                else
                {
                    NoteFailure("Socket connect timed out.");
                }
            }
            catch (Exception exception)
            {
                NoteFailure($"Socket error: {exception.Message}");
            }
            finally
            {
                _socket = null;
                _streaming = false;

                // Anyone still on the loading screen cannot be admitted over a dead socket, so kick them.
                if (_connected)
                {
                    _connected = false;
                    IntegrationConnections.ReleaseAll(SocketDropped);
                }
            }

            if (_stopping)
            {
                break;
            }

            var delay = ReconnectDelay();
            Emit(LogLevel.Debug, $"[Integration] Reconnecting socket in {delay / 1000.0:0.0}s.");
            await Task.Delay(delay);
        }

        Emit(LogLevel.Debug, "[Integration] Socket stopped.");
    }

    private static async Task<bool> ConnectAsync(ClientWebSocket ws)
    {
        var connect = ws.ConnectAsync(new Uri(_url), CancellationToken.None);
        var finished = await Task.WhenAny(connect, Task.Delay(ConnectTimeoutMs));
        if (finished != connect)
        {
            ws.Abort();

            Observe(connect);

            return false;
        }

        await connect;

        return true;
    }

    // Observe a task we stopped awaiting; an unobserved fault crashes this runtime via the finalizer.
    private static void Observe(Task task) =>
        task.ContinueWith(
            static t => _ = t.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

    private static async Task StreamLoopAsync(ClientWebSocket ws)
    {
        var lastWorldSignature = string.Empty;
        var lastWorldSentAt = DateTime.MinValue;
        var lastBuckets = string.Empty;

        try
        {
            while (ws.State == WebSocketState.Open && !_stopping)
            {
                if (_streaming)
                {
                    await SendAsync(ws, Frame(TypePlayers, IntegrationSnapshots.Map));
                }

                var signature = IntegrationSnapshots.WorldSignature;
                if (signature != lastWorldSignature || DateTime.UtcNow - lastWorldSentAt >= WorldHeartbeat)
                {
                    await SendAsync(ws, Frame(TypeWorld, IntegrationSnapshots.World));
                    lastWorldSignature = signature;
                    lastWorldSentAt = DateTime.UtcNow;
                }

                var buckets = IntegrationSnapshots.Buckets;
                if (buckets != lastBuckets)
                {
                    await SendAsync(ws, Frame(TypeBuckets, buckets));
                    lastBuckets = buckets;
                }

                await Task.Delay(StreamInterval);
            }
        }
        catch (Exception exception)
        {
            Emit(LogLevel.Debug, $"[Integration] Socket stream loop ended: {exception.Message}");
        }
    }

    private static async Task ReceiveLoopAsync(ClientWebSocket ws)
    {
        var buffer = new byte[MaxMessageBytes];

        while (ws.State == WebSocketState.Open && !_stopping)
        {
            var (ok, count) = await ReadMessageAsync(ws, buffer);
            if (!ok)
            {
                return;
            }

            HandleFrame(ws, Encoding.UTF8.GetString(buffer, 0, count));
        }
    }

    private static async Task<(bool Ok, int Count)> ReadMessageAsync(ClientWebSocket ws, byte[] buffer)
    {
        var total = 0;

        while (true)
        {
            if (total >= buffer.Length)
            {
                return (false, 0);
            }

            WebSocketReceiveResult result;
            try
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer, total, buffer.Length - total), CancellationToken.None);
            }
            catch
            {
                return (false, 0);
            }

            if (result.MessageType is WebSocketMessageType.Close or WebSocketMessageType.Binary)
            {
                return (false, 0);
            }

            total += result.Count;
            if (result.EndOfMessage)
            {
                return (true, total);
            }
        }
    }

    private static void HandleFrame(ClientWebSocket ws, string text)
    {
        string type;
        string? id = null;
        var payload = "";

        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            type = root.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() ?? "" : "";
            if (root.TryGetProperty("id", out var i) && i.ValueKind == JsonValueKind.String)
            {
                id = i.GetString();
            }

            if (root.TryGetProperty("payload", out var p))
            {
                payload = p.GetRawText();
            }
        }
        catch (JsonException)
        {
            return;
        }

        switch (type)
        {
            case TypePing:
                _ = SendAsync(ws, "{\"type\":\"" + TypePong + "\"}");
                break;

            case TypeReady:
                UpdateRequestedIdentifiers(payload);
                SendFullState(ws);
                IntegrationRoleSync.RequestAll();
                break;

            case TypeResync:
                SendFullState(ws);
                break;

            case TypeGate:
                IntegrationConnections.Apply(payload);
                break;

            case TypeRoleSync:
                IntegrationRoleSync.Submit(payload);
                break;

            case TypeStartStream:
                _streaming = true;
                _ = SendAsync(ws, Frame(TypeBlips, IntegrationSnapshots.Blips));
                _ = SendAsync(ws, Frame(TypeBuckets, IntegrationSnapshots.Buckets));
                break;

            case TypeStopStream:
                _streaming = false;
                break;

            case TypeCommand:
                if (id is not null)
                {
                    _ = HandleCommandAsync(ws, id, payload);
                }

                break;
        }
    }

    private static async Task HandleCommandAsync(ClientWebSocket ws, string id, string payload)
    {
        try
        {
            var pending = IntegrationCommands.Submit(payload);
            var finished = await Task.WhenAny(pending.Reply, Task.Delay(CommandTimeoutMs));
            if (finished != pending.Reply)
            {
                pending.Abandon();

                await SendAsync(ws, AckFrame(id, 500, "{\"ok\":false,\"reason\":\"timeout\"}"));

                Observe(pending.Reply);

                return;
            }

            var reply = await pending.Reply;
            await SendAsync(ws, AckFrame(id, reply.Status, reply.Json));
        }
        catch (Exception exception)
        {
            Emit(LogLevel.Debug, $"[Integration] Socket command failed: {exception.Message}");
        }
    }

    // The integration tells vMenu what identifier types it wants for connecting players. Default: Discord only
    private static void UpdateRequestedIdentifiers(string payload)
    {
        if (payload.Length == 0)
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.ValueKind != JsonValueKind.Object
                || !doc.RootElement.TryGetProperty("identifiers", out var array)
                || array.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var types = new List<string>();
            foreach (var item in array.EnumerateArray())
            {
                // Never allow IP
                if (item.ValueKind == JsonValueKind.String
                    && item.GetString() is { Length: > 0 } type
                    && !type.Equals("ip", StringComparison.OrdinalIgnoreCase))
                {
                    types.Add(type);
                }
            }

            _requestedIdentifiers = types.Count > 0 ? [.. types] : ["discord"];
        }
        catch (JsonException)
        {
        }
    }

    private static void SendFullState(ClientWebSocket ws)
    {
        _ = SendAsync(ws, Frame(TypePlayers, IntegrationSnapshots.Map));
        _ = SendAsync(ws, Frame(TypeBlips, IntegrationSnapshots.Blips));
        _ = SendAsync(ws, Frame(TypeWorld, IntegrationSnapshots.World));
        _ = SendAsync(ws, Frame(TypeBuckets, IntegrationSnapshots.Buckets));
    }

    private static string Frame(string type, string payloadJson) =>
        "{\"type\":\"" + type + "\",\"payload\":" + payloadJson + "}";

    // id is the tool's hex correlation id, so it is safe to embed unescaped.
    private static string AckFrame(string id, int status, string resultJson) =>
        "{\"type\":\"" + TypeCommandAck + "\",\"id\":\"" + id + "\",\"payload\":{\"status\":" + status + ",\"result\":" + resultJson + "}}";

    private static async Task SendAsync(ClientWebSocket ws, string frame)
    {
        var bytes = Encoding.UTF8.GetBytes(frame);

        await SendGate.WaitAsync();
        try
        {
            if (ws.State != WebSocketState.Open)
            {
                return;
            }

            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        }
        catch (Exception exception)
        {
            Emit(LogLevel.Debug, $"[Integration] Socket send failed: {exception.Message}");
        }
        finally
        {
            SendGate.Release();
        }
    }

    private static void DrainLogs()
    {
        while (Logs.TryDequeue(out var entry))
        {
            switch (entry.Level)
            {
                case LogLevel.Warning:
                    Log.Warning(entry.Message);
                    break;
                case LogLevel.Info:
                    Log.Info(entry.Message);
                    break;
                default:
                    Log.Debug(entry.Message);
                    break;
            }
        }
    }

    // First failure of a run is loud so a genuine outage or a bad key is visible once; the repeats that
    // follow drop to Debug so a persistently refused connect never floods the console.
    private static void NoteFailure(string detail)
    {
        var level = _consecutiveFailures == 0 ? LogLevel.Warning : LogLevel.Debug;
        _consecutiveFailures++;

        Emit(level, $"[Integration] {detail}");
    }

    private static int ReconnectDelay()
    {
        var backoff = BaseReconnectMs;
        if (_consecutiveFailures > 1)
        {
            var shift = Math.Min(_consecutiveFailures - 1, 6);
            backoff = (int)Math.Min((long)BaseReconnectMs << shift, MaxReconnectMs);
        }

        return backoff + Jitter.Next(0, 5000);
    }

    private static void Emit(LogLevel level, string message) => Logs.Enqueue((level, message));
}
