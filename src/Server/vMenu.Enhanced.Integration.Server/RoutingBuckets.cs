using System.Text.Json;

using CitizenFX.FiveM.Server;

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

        API.EmitLocal(RequestStateEvent);
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
