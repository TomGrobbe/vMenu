using System.Diagnostics.CodeAnalysis;

namespace vMenu.Enhanced.Localization;

/// <summary>
/// One language's strings.
/// </summary>
/// <remarks>
/// Only <see cref="LanguageId.English"/> is expected to be complete; every other table may be
/// partial and falls back to English key by key, so a half-finished translation is still usable.
/// </remarks>
public sealed class LanguageTable
{
    private readonly Dictionary<string, string> _strings;

    public LanguageTable(LanguageId id, string nativeName, IReadOnlyDictionary<string, string> strings)
    {
        Id = id;
        NativeName = nativeName;
        _strings = new Dictionary<string, string>(strings, StringComparer.Ordinal);
    }

    public LanguageId Id { get; }

    /// <summary>The language's name in itself ("Nederlands", not "Dutch"), for the language picker.</summary>
    public string NativeName { get; }

    public IReadOnlyCollection<string> Keys => _strings.Keys;

    public bool TryGet(string key, [MaybeNullWhen(false)] out string value) =>
        _strings.TryGetValue(key, out value);

    public bool ContainsKey(string key) => _strings.ContainsKey(key);
}
