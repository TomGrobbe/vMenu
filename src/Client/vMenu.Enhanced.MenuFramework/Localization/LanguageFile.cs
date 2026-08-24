using System.Text.Json.Serialization;

namespace vMenu.Enhanced.MenuFramework.Localization;

// The shape of a language/<code>.json file.
internal sealed class LanguageFile
{
    public string NativeName { get; init; } = string.Empty;

    // Getter only and pre-created on purpose, to keep the ordinal comparer: a setter would have
    // System.Text.Json build its own Dictionary with the default comparer. But STJ ignores a get-only
    // collection unless told to fill the existing one, so without Populate the strings read back empty.
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public Dictionary<string, string> Strings { get; } = new(StringComparer.Ordinal);
}
