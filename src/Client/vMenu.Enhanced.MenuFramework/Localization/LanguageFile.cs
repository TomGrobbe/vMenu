namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>The shape of a <c>language/&lt;code&gt;.json</c> file.</summary>
internal sealed class LanguageFile
{
    public string NativeName { get; init; } = string.Empty;

    // Getter only and pre-created on purpose. Given a setter, Newtonsoft constructs its own
    // Dictionary with EqualityComparer<string>.Default, whose internal comparer the sandbox refuses.
    // With none it populates this instance and the ordinal comparer survives.
    public Dictionary<string, string> Strings { get; } = new(StringComparer.Ordinal);
}
