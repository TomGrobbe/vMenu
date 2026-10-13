using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Serialization;

public static class ClientJson
{
    private static readonly JsonSerializerSettings Options = CreateSettings(indented: false, escapeHtml: true);

    private static readonly JsonSerializerSettings IndentedOptions = CreateSettings(indented: true, escapeHtml: false);

    private static readonly string? Failure = SelfTest();

    public static string Serialize(object? value) => JsonConvert.SerializeObject(value, Options);

    public static string SerializeIndented(object? value) => JsonConvert.SerializeObject(value, IndentedOptions);

    public static T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Options);

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

    public static void Verify()
    {
        if (Failure is null)
        {
            Log.Debug("[Json] Newtonsoft.Json is working.");

            return;
        }

        Log.Error($"[Json] Newtonsoft.Json failed its start-up self test ({Failure}). Every client side serialize will now fail.");
    }

    public static JsonSerializerSettings CreateSettings(bool indented, bool escapeHtml) => new()
    {
        ContractResolver = new PropertiesOnlyResolver(),
        Formatting = indented ? Formatting.Indented : Formatting.None,
        StringEscapeHandling = escapeHtml ? StringEscapeHandling.EscapeHtml : StringEscapeHandling.Default,
        FloatFormatHandling = FloatFormatHandling.String,
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None,
        DateParseHandling = DateParseHandling.None,
    };

    private static string? SelfTest()
    {
        try
        {
            var json = JsonConvert.SerializeObject(new SelfTestModel { Value = 1 }, Options);
            var read = JsonConvert.DeserializeObject<SelfTestModel>(json, Options);

            return read?.Value == 1 ? null : "the round trip did not return the value";
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
    }

    private sealed class SelfTestModel
    {
        public int Value { get; set; }
    }

    private sealed class PropertiesOnlyResolver : DefaultContractResolver
    {
        public PropertiesOnlyResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType) =>
            base.GetSerializableMembers(objectType).Where(member => member is PropertyInfo).ToList();
    }
}
