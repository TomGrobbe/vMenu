using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Serialization.Server;

public static class ServerJson
{
    private static readonly JsonSerializerSettings Options = CreateSettings(indented: false);

    private static readonly JsonSerializerSettings IndentedOptions = CreateSettings(indented: true);

    private static readonly string? Failure = SelfTest();

    public static string Serialize(object? value) => JsonConvert.SerializeObject(value, Options);

    public static string SerializeIndented(object? value) => JsonConvert.SerializeObject(value, IndentedOptions);

    public static bool TryDeserialize<T>(string json, out T? value, out string? error)
    {
        try
        {
            value = JsonConvert.DeserializeObject<T>(json, Options);
            error = null;

            return true;
        }
        catch (JsonException exception)
        {
            value = default;
            error = exception.Message;

            return false;
        }
    }

    public static JToken Parse(string json)
    {
        using var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };

        return JToken.ReadFrom(reader, new JsonLoadSettings { CommentHandling = CommentHandling.Ignore, LineInfoHandling = LineInfoHandling.Ignore });
    }

    public static void Verify()
    {
        if (Failure is null)
        {
            Log.Debug("[Json] Newtonsoft.Json is working.");

            return;
        }

        Log.Error($"[Json] Newtonsoft.Json failed its start-up self test ({Failure}). Every server side serialize will now fail.");
    }

    private static JsonSerializerSettings CreateSettings(bool indented) => new()
    {
        ContractResolver = new PropertiesOnlyResolver(),
        Formatting = indented ? Formatting.Indented : Formatting.None,
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
