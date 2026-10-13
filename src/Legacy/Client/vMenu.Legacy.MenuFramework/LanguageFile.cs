using Newtonsoft.Json;

namespace vMenu.Enhanced.MenuFramework;

// The shape of a language/<code>.json file.
internal sealed class LanguageFile
{
    public string NativeName { get; init; } = string.Empty;

    /// <summary>Getter only and pre-created on purpose, to keep the ordinal comparer: a setter would have
    /// Newtonsoft build its own Dictionary with the default comparer. Reuse makes it fill this one.</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
    public Dictionary<string, string> Strings { get; } = new(StringComparer.Ordinal);
}
