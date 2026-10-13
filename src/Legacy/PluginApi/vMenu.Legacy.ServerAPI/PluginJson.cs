using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace vMenu.Enhanced.ServerAPI;

internal static class PluginJson
{
    private static readonly JsonSerializerSettings Options = new()
    {
        ContractResolver = new Resolver(),
        FloatFormatHandling = FloatFormatHandling.String,
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None,
        DateParseHandling = DateParseHandling.None,
    };

    public static string Serialize(object? value) => JsonConvert.SerializeObject(value, Options);

    public static bool TryDeserialize<T>(string json, out T? value)
    {
        try
        {
            value = JsonConvert.DeserializeObject<T>(json, Options);

            return true;
        }
        catch (JsonException)
        {
            value = default;

            return false;
        }
    }

    private sealed class Resolver : DefaultContractResolver
    {
        public Resolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType) =>
            base.GetSerializableMembers(objectType).Where(member => member is PropertyInfo).ToList();
    }
}
