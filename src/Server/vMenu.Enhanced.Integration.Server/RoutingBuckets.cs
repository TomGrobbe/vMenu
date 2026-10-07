using System.Text.Json;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.BrokenNatives.Server;

namespace vMenu.Enhanced.Integration.Server;

public static class RoutingBuckets
{
    private const string ServerStateEvent = "vMenu.RoutingBucketsPlugin:ServerState";

    private const string RequestStateEvent = "vMenu.RoutingBucketsPlugin:RequestServerState";

    private const string StopEvent = "onResourceStop";

    private const string PluginResource = "vMenu.RoutingBucketsPlugin";

    private const string Empty = "{\"buckets\":[]}";

    private static volatile string _payload = Empty;

    private static bool _started;

    public static string Payload => _payload;

    public static void Initialize()
    {
        if (_started)
        {
            return;
        }

        _started = true;

        API.OnEvent(ServerStateEvent, new Action<string>(OnServerState), false);
        API.OnEvent(StopEvent, new Action<string>(OnResourceStop), false);

        NativeFixer.EmitLocal(RequestStateEvent);
    }

    private static void OnServerState(string json)
    {
        if (!string.IsNullOrEmpty(json) && IsValid(json))
        {
            _payload = json;
        }
    }

    private static void OnResourceStop(string stopped)
    {
        if (string.Equals(stopped, PluginResource, StringComparison.OrdinalIgnoreCase))
        {
            _payload = Empty;
        }
    }

    public static IReadOnlyCollection<int> WorldIds()
    {
        var ids = new List<int>();

        try
        {
            using var doc = JsonDocument.Parse(_payload);

            foreach (var bucket in doc.RootElement.GetProperty("buckets").EnumerateArray())
            {
                if (bucket.TryGetProperty("id", out var id) && id.TryGetInt32(out var value))
                {
                    ids.Add(value);
                }
            }
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
        }

        return ids;
    }

    private static bool IsValid(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("buckets", out var buckets)
                && buckets.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
