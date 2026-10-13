using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using vMenu.Legacy.Hosting;

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

        Script.EventHandlers[ServerStateEvent] += new Action<string>(OnServerState);
        Script.EventHandlers[StopEvent] += new Action<string>(OnResourceStop);

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
            if (IntegrationJson.Parse(_payload) is not JObject root
                || !root.TryGetValue("buckets", StringComparison.Ordinal, out var buckets)
                || buckets is not JArray array)
            {
                return ids;
            }

            foreach (var bucket in array)
            {
                if (bucket is not JObject entry)
                {
                    break;
                }

                if (!entry.TryGetValue("id", StringComparison.Ordinal, out var id))
                {
                    continue;
                }

                if (id.Type is not (JTokenType.Integer or JTokenType.Float))
                {
                    break;
                }

                if (IntegrationJson.TryReadInt(entry, "id", out var value))
                {
                    ids.Add(value);
                }
            }
        }
        catch (JsonException)
        {
        }

        return ids;
    }

    private static bool IsValid(string json)
    {
        try
        {
            return IntegrationJson.Parse(json) is JObject root
                && root.TryGetValue("buckets", StringComparison.Ordinal, out var buckets)
                && buckets.Type == JTokenType.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
