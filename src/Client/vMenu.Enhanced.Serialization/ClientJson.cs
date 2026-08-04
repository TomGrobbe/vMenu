using System.Reflection;

using CitizenFX.FiveM.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace vMenu.Enhanced.Serialization;

/// <summary>JSON on the client, which takes one piece of setup before it works at all.</summary>
// The sandbox refuses Reflection.Emit, which Newtonsoft uses to build contract accessors, so an
// unprepared call dies with a SecurityException. Newtonsoft has a reflection only path but picks it
// by asking the runtime, which says emit is supported. Prepare answers for it.
// LINQ to JSON (JObject, JArray, JToken) never works and no setting fixes it, because the sandbox
// refuses Newtonsoft the Collection<JToken>.InsertItem call. Always deserialize into a type.
public static class ClientJson
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

    /// <summary>Why the client cannot serialize, or <see langword="null"/> when it can.</summary>
    private static readonly string? Failure = Prepare();

    public static bool IsUsable => Failure is null;

    public static string Serialize(object? value) => JsonConvert.SerializeObject(value, Settings);

    public static string SerializeIndented(object? value) =>
        JsonConvert.SerializeObject(value, Formatting.Indented, Settings);

    public static T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings);

    /// <summary>For JSON from somewhere that can send nonsense, such as a saved file or the page.</summary>
    // A sandbox refusal still throws, because that is a broken build rather than a bad document.
    public static bool TryDeserialize<T>(string json, out T? value)
    {
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

    /// <summary>Says on the startup path whether the setup took.</summary>
    // It depends on a private field of a pinned package, and an upgrade renaming that field would
    // otherwise surface as a menu quietly failing.
    public static void Verify()
    {
        if (Failure is null)
        {
            API.Log.Debug("[Json] Newtonsoft.Json is on the late bound path.");

            return;
        }

        API.Log.Error(
            $"[Json] Newtonsoft.Json could not be moved off Reflection.Emit ({Failure}). Every "
            + "client side serialize will now fail. Check whether the pinned Newtonsoft.Json "
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
