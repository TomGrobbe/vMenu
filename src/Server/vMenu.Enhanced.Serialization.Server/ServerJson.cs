using System.Reflection;

using CitizenFX.FiveM.Server;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace vMenu.Enhanced.Serialization.Server;

/// <summary>JSON on the server, which takes one piece of setup before it works at all.</summary>
// Settings are kept in step with ClientJson: what this writes is what that reads.
// Not System.Text.Json. JsonDocument is fine, but JsonSerializer builds accessors through
// Reflection.Emit and stamps their cache with DateTime.UtcNow, which this runtime does not have, so
// the first serialize dies in NtQuerySystemInformation. Newtonsoft uses emit too, but picks it by
// asking the runtime, so Prepare can tell it not to.
// LINQ to JSON (JObject, JArray, JToken) never works and no setting fixes it. Deserialize to a type.
public static class ServerJson
{
    private const string ReflectorTypeName = "Newtonsoft.Json.Serialization.JsonTypeReflector";

    private const string FlagFieldName = "_dynamicCodeGeneration";

    private const string FactoryPropertyName = "ReflectionDelegateFactory";

    private const string LateBoundFactoryName = "LateBoundReflectionDelegateFactory";

    private const BindingFlags Hidden =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    // A plain resolver, not CamelCasePropertyNamesContractResolver, whose strategy also rewrites
    // dictionary keys and overrides explicit names.
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },

        // Keeps date-shaped strings read into object as strings. Typed DateTime members are unaffected.
        DateParseHandling = DateParseHandling.None,
    };

    /// <summary>Why the server cannot serialize, or <see langword="null"/> when it can.</summary>
    private static readonly string? Failure = Prepare();

    public static string Serialize(object? value) => JsonConvert.SerializeObject(value, Settings);

    /// <summary>For a file a server owner is expected to open and read.</summary>
    public static string SerializeIndented(object? value) =>
        JsonConvert.SerializeObject(value, Formatting.Indented, Settings);

    /// <summary>For JSON from somewhere that can send nonsense, such as a config file an owner edits.</summary>
    /// <param name="error">What was wrong with the document, or <see langword="null"/> when it read.</param>
    // A sandbox refusal still throws, because that is a broken build rather than a bad document.
    public static bool TryDeserialize<T>(string json, out T? value, out string? error)
    {
        try
        {
            value = JsonConvert.DeserializeObject<T>(json, Settings);
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

    /// <summary>Says on the startup path whether the setup took.</summary>
    // It depends on a private field of a pinned package, and an upgrade renaming that field would
    // otherwise surface as a config file quietly failing to load.
    public static void Verify()
    {
        if (Failure is null)
        {
            API.Log.Debug("[Json] Newtonsoft.Json is on the late bound path.");

            return;
        }

        API.Log.Error(
            $"[Json] Newtonsoft.Json could not be moved off Reflection.Emit ({Failure}). Every "
            + "server side serialize will now fail. Check whether the pinned Newtonsoft.Json "
            + "version changed.");
    }

    private static string? Prepare()
    {
        try
        {
            var reflector = typeof(JsonConvert).Assembly.GetType(ReflectorTypeName);

            if (reflector is null)
            {
                return ReflectorTypeName + " is missing";
            }

            var flag = reflector.GetField(FlagFieldName, Hidden);

            if (flag is null)
            {
                return ReflectorTypeName + "." + FlagFieldName + " is missing";
            }

            flag.SetValue(null, false);

            // The field is the mechanism, but the factory it selects is what has to be true.
            var factory = reflector.GetProperty(FactoryPropertyName, Hidden)?.GetValue(null);
            var selected = factory?.GetType().Name;

            return selected == LateBoundFactoryName
                ? null
                : "the delegate factory is " + (selected ?? "unreadable") + ", not " + LateBoundFactoryName;
        }
        catch (Exception exception)
        {
            // A throw here would take down every resource that touches JSON.
            return exception.Message;
        }
    }
}
