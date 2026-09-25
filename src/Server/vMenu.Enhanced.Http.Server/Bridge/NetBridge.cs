using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.BrokenNatives.Server;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Http.Server.Bridge;

// Linux TLS bridge: the C# runtime on Linux cannot validate root certificates, so HTTPS and WebSocket
// traffic goes through server/net_bridge.js instead. Remove once Cfx fixes it.
// Local events reach every resource, so real event names carry a Diffie-Hellman agreed secret.
public static class NetBridge
{
    private const string Prefix = "vMenu.Enhanced:NetBridge:";

    private const string HelloEvent = Prefix + "Hello";

    private const string HelloBackEvent = Prefix + "HelloBack";

    private const string ModeConvar = "vMenu.Enhanced.NetBridge";

    private const int HelloAttempts = 5;

    private const int HelloIntervalMs = 2000;

    private const int StaleGraceMs = 30000;

    private const string Unavailable = "JS net bridge unavailable";

    // RFC 3526 group 14, the same group net_bridge.js loads as 'modp14'.
    private static readonly BigInteger Prime = BigInteger.Parse(
        "00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DD" +
        "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED" +
        "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F" +
        "83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3B" +
        "E39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA0510" +
        "15728E5A8AACAA68FFFFFFFFFFFFFFFF",
        NumberStyles.HexNumber,
        CultureInfo.InvariantCulture);

    private static readonly ConcurrentQueue<(string Name, object?[] Args)> Outbox = new();

    private static readonly ConcurrentDictionary<string, (HttpSlot Slot, int TimeoutMs, DateTimeOffset StartedAt)> PendingHttp = new();

    private static readonly ConcurrentDictionary<string, BridgeWebSocket> Sockets = new();

    private static string _resource = "";

    private static BigInteger _private;

    private static volatile string? _prefix;

    private static volatile bool _failed;

    private static int _flushScheduled;

    public static bool Active { get; private set; }

    public static void Initialize()
    {
        var mode = Native.GetConvar(ModeConvar, "auto").Trim().ToLowerInvariant();
        var version = Native.GetConvar("version", "");

        // OperatingSystem.IsLinux() is baked into the runtime's class library, which says Windows on Linux too.
        var linux = version.Contains("linux", StringComparison.OrdinalIgnoreCase)
            || Environment.OSVersion.Platform == PlatformID.Unix;

        Active = mode switch
        {
            "js" => true,
            "native" => false,
            _ => linux,
        };

        if (!Active)
        {
            Log.Debug($"[vMenu] JS net bridge off (mode '{mode}', server '{version}', os '{Environment.OSVersion}').");
            return;
        }

        _resource = Native.GetCurrentResourceName();
        Log.Info("[vMenu] Sending HTTPS and WebSocket traffic through the JS net bridge (Linux certificate workaround).");

        API.OnEvent(HelloBackEvent, new Action<string>(OnHelloBack), false);

        _private = new BigInteger(RandomNumberGenerator.GetBytes(32), isUnsigned: true, isBigEndian: true);
        _ = GreetAsync(PublicHex(BigInteger.ModPow(2, _private, Prime)));
    }

    internal static void Send(HttpRequest request, HttpSlot slot)
    {
        if (_failed)
        {
            slot.Complete(HttpReply.Unusable(Unavailable));
            return;
        }

        try
        {
            DropStale();

            var json = RequestJson(request);
            var id = Guid.NewGuid().ToString("N");
            PendingHttp[id] = (slot, request.TimeoutMs, DateTimeOffset.UtcNow);

            Post("Http", id, json);
        }
        catch (Exception exception)
        {
            slot.Complete(HttpReply.Unusable(exception.GetType().Name + ": " + exception.Message));
        }
    }

    internal static void Open(BridgeWebSocket socket, string id, string url, IReadOnlyDictionary<string, string> headers)
    {
        if (_failed)
        {
            socket.Closed(Unavailable);
            return;
        }

        Sockets[id] = socket;

        Post("WsOpen", id, url, Json(writer =>
        {
            writer.WriteStartObject();
            foreach (var header in headers)
            {
                writer.WriteString(header.Key, header.Value);
            }

            writer.WriteEndObject();
        }));
    }

    internal static void Transmit(string id, string text) => Post("WsSend", id, text);

    internal static void Close(string id)
    {
        if (Sockets.TryRemove(id, out _))
        {
            Post("WsClose", id);
        }
    }

    private static async Task GreetAsync(string hello)
    {
        for (var attempt = 0; attempt < HelloAttempts && _prefix is null; attempt++)
        {
            NativeFixer.EmitLocal(HelloEvent, hello);
            await API.Delay(HelloIntervalMs);
        }

        if (_prefix is not null)
        {
            return;
        }

        _failed = true;
        Log.Warning(
            "[vMenu] The JS net bridge never answered, so HTTPS and WebSocket calls will fail. " +
            "Make sure server/net_bridge.js is present and your server build supports node_version '22'.");

        Outbox.Clear();
        foreach (var id in PendingHttp.Keys)
        {
            if (PendingHttp.TryRemove(id, out var pending))
            {
                pending.Slot.Complete(HttpReply.Unusable(Unavailable));
            }
        }

        foreach (var id in Sockets.Keys)
        {
            if (Sockets.TryRemove(id, out var socket))
            {
                socket.Closed(Unavailable);
            }
        }
    }

    private static void OnHelloBack(string theirs)
    {
        if (!FromSelf() || _prefix is not null || _failed)
        {
            return;
        }

        try
        {
            var peer = BigInteger.Parse("0" + theirs, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            if (peer <= 1 || peer >= Prime - 1)
            {
                return;
            }

            var secret = PublicHex(BigInteger.ModPow(peer, _private, Prime)).TrimStart('0');
            var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.ASCII.GetBytes(secret)))[..32];
            var prefix = Prefix + hash + ":";

            API.OnEvent(prefix + "HttpDone", new Action<string, string>(OnHttpDone), false);
            API.OnEvent(prefix + "WsOpened", new Action<string>(OnWsOpened), false);
            API.OnEvent(prefix + "WsMessage", new Action<string, string>(OnWsMessage), false);
            API.OnEvent(prefix + "WsClosed", new Action<string, string>(OnWsClosed), false);

            _prefix = prefix;
            Log.Debug("[vMenu] JS net bridge connected.");

            Flush();
        }
        catch (FormatException)
        {
        }
    }

    private static void OnHttpDone(string id, string json)
    {
        if (FromSelf() && PendingHttp.TryRemove(id, out var pending))
        {
            pending.Slot.Complete(ReadReply(json, pending.TimeoutMs, pending.StartedAt));
        }
    }

    private static void OnWsOpened(string id)
    {
        if (FromSelf() && Sockets.TryGetValue(id, out var socket))
        {
            socket.Opened();
        }
    }

    private static void OnWsMessage(string id, string text)
    {
        if (FromSelf() && Sockets.TryGetValue(id, out var socket))
        {
            socket.Received(text);
        }
    }

    private static void OnWsClosed(string id, string reason)
    {
        if (FromSelf() && Sockets.TryRemove(id, out var socket))
        {
            socket.Closed(reason);
        }
    }

    // HttpWait gave up on these long ago, and a reply that never came would otherwise stay here forever.
    private static void DropStale()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (id, pending) in PendingHttp)
        {
            if ((now - pending.StartedAt).TotalMilliseconds > pending.TimeoutMs + StaleGraceMs)
            {
                PendingHttp.TryRemove(id, out _);
            }
        }
    }

    private static bool FromSelf() => Native.GetInvokingResource() == _resource;

    private static void Post(string name, params object?[] args)
    {
        Outbox.Enqueue((name, args));
        Flush();
    }

    private static void Flush()
    {
        if (_prefix is null || Interlocked.Exchange(ref _flushScheduled, 1) == 1)
        {
            return;
        }

        _ = FlushAsync();
    }

    // Posts come from thread pool continuations, and natives only work on the main thread.
    private static async Task FlushAsync()
    {
        await API.JumpToMainThread();

        Volatile.Write(ref _flushScheduled, 0);

        var prefix = _prefix!;
        while (Outbox.TryDequeue(out var item))
        {
            try
            {
                NativeFixer.EmitLocal(prefix + item.Name, item.Args);
            }
            catch (Exception exception)
            {
                Log.Debug($"[vMenu] JS net bridge emit failed: {exception.Message}");
            }
        }
    }

    private static string RequestJson(HttpRequest request) => Json(writer =>
    {
        writer.WriteStartObject();
        writer.WriteString("method", request.Method);
        writer.WriteString("url", request.Url);
        writer.WriteNumber("timeoutMs", request.TimeoutMs);

        writer.WriteStartObject("headers");
        writer.WriteString("User-Agent", request.UserAgent);
        writer.WriteString("Accept", request.Accept);
        if (request.Body is not null)
        {
            writer.WriteString("Content-Type", request.ContentType + "; charset=utf-8");
        }

        if (request.Headers is { } extra)
        {
            foreach (var header in extra)
            {
                writer.WriteString(header.Key, header.Value);
            }
        }

        writer.WriteEndObject();

        if (request.Body is { } body)
        {
            writer.WriteString("body", body);
        }

        writer.WriteEndObject();
    });

    private static HttpReply ReadReply(string json, int timeoutMs, DateTimeOffset startedAt)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("status", out var status) && status.TryGetInt32(out var code))
            {
                var body = root.TryGetProperty("body", out var b) && b.ValueKind == JsonValueKind.String ? b.GetString() ?? "" : "";
                var elapsed = (int)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds;

                return HttpReply.Answered(code, body, elapsed, RetryAfter(root));
            }

            if (root.TryGetProperty("timedOut", out var timedOut) && timedOut.ValueKind == JsonValueKind.True)
            {
                return HttpReply.TimedOut(timeoutMs);
            }

            return HttpReply.Unusable(root.TryGetProperty("error", out var error) ? error.ToString() : "unknown bridge error");
        }
        catch (JsonException)
        {
            return HttpReply.Unusable("unreadable bridge reply");
        }
    }

    private static float? RetryAfter(JsonElement root) =>
        root.TryGetProperty("retryAfter", out var value)
        && value.ValueKind == JsonValueKind.String
        && float.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            ? seconds
            : null;

    private static string PublicHex(BigInteger value) =>
        Convert.ToHexStringLower(value.ToByteArray(isUnsigned: true, isBigEndian: true));

    private static string Json(Action<Utf8JsonWriter> write)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            write(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
