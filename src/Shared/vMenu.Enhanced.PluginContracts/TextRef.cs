namespace vMenu.Enhanced.PluginContracts;

/// <summary>A piece of text in a payload, either a literal or a key into the plugin's translation
/// tables with optional placeholder arguments. When both are set the key wins.</summary>
// A class with settable properties on purpose: payload types cross the FiveM sandbox, where
// generated record equality fails to load, and the JSON serializer needs setters.
public class TextRef
{
    public string? Text { get; set; }

    public string? Key { get; set; }

    /// <summary>Placeholder values substituted into the resolved text by name.</summary>
    public Dictionary<string, TextRef>? Args { get; set; }

    public static TextRef Literal(string text) => new() { Text = text };

    public static TextRef ForKey(string key) => new() { Key = key };
}
