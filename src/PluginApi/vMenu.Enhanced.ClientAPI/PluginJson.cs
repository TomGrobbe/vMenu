using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>JSON for the plugin protocol, kept byte compatible with vMenu's own serializer.</summary>
// The FiveM sandbox refuses Reflection.Emit, which Newtonsoft uses to build contract accessors.
// Newtonsoft has a reflection only path but picks it by asking the runtime, which claims emit is
// supported, so the private selection flag is flipped by hand. Same trick vMenu itself uses.
internal static class PluginJson
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
        DateParseHandling = DateParseHandling.None,
    };

    private static bool _prepared;

    public static string Serialize(object? value)
    {
        Prepare();

        return JsonConvert.SerializeObject(value, Settings);
    }

    public static bool TryDeserialize<T>(string json, out T? value)
    {
        Prepare();

        try
        {
            value = JsonConvert.DeserializeObject<T>(json, Settings);

            return true;
        }
        catch (JsonException)
        {
            value = default;

            return false;
        }
    }

    private static void Prepare()
    {
        if (_prepared)
        {
            return;
        }

        _prepared = true;

        try
        {
            var reflector = typeof(JsonConvert).Assembly.GetType("Newtonsoft.Json.Serialization.JsonTypeReflector");
            var flag = reflector?.GetField(
                "_dynamicCodeGeneration",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            flag?.SetValue(null, false);
        }
        catch
        {
            // A throw here would take down the whole resource, and outside the sandbox the
            // default emit path works anyway.
        }
    }
}
