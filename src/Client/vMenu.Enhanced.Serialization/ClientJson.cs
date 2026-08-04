using System.Reflection;

using CitizenFX.FiveM.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace vMenu.Enhanced.Serialization;

/// <summary>
/// JSON on the client, which takes one piece of setup before it works at all.
/// </summary>
/// <remarks>
/// The sandbox refuses <c>System.Reflection.Emit</c>, which Newtonsoft reaches for to build contract
/// accessors, so an unprepared <see cref="JsonConvert"/> call dies with a
/// <see cref="System.Security.SecurityException"/>. Newtonsoft carries a reflection-only path for
/// platforms that cannot emit, but decides by asking the runtime, which answers that emit is
/// supported — true of the runtime, not of the sandbox on top of it. <see cref="Prepare"/> answers
/// for it.
/// <para>
/// LINQ-to-JSON — <c>JObject</c>, <c>JArray</c>, <c>JToken</c> — does not work and no setting fixes
/// it: <c>JPropertyKeyedCollection</c> overrides <c>Collection&lt;JToken&gt;.InsertItem</c>, and the
/// sandbox refuses Newtonsoft that call across the assembly boundary. Deserialize into a type.
/// </para>
/// </remarks>
public static class ClientJson
{
    private const string ReflectorTypeName = "Newtonsoft.Json.Serialization.JsonTypeReflector";

    private const string FlagFieldName = "_dynamicCodeGeneration";

    private const string FactoryPropertyName = "ReflectionDelegateFactory";

    private const string LateBoundFactoryName = "LateBoundReflectionDelegateFactory";

    private const BindingFlags Hidden =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    /// <summary>
    /// A plain resolver rather than <c>CamelCasePropertyNamesContractResolver</c>, whose strategy
    /// also rewrites dictionary keys and overrides explicit names.
    /// </summary>
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

    /// <summary>
    /// For JSON from somewhere that can send nonsense — a saved file, the NUI page. A sandbox
    /// refusal still throws, because that is a broken build rather than a bad document.
    /// </summary>
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

    /// <summary>
    /// Says on the startup path whether the setup took, because it depends on a private field of a
    /// pinned package and an upgrade that renames it would otherwise surface as a menu quietly
    /// failing.
    /// </summary>
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
