namespace vMenu.Enhanced.ClientAPI;

/// <summary>Your plugin's translation tables: language code to key to text. An English table under
/// <c>"en"</c> is required as soon as any of your texts use keys, and it is the fallback whenever the
/// selected language has no entry. vMenu follows its currently selected language live.</summary>
public sealed class PluginTranslations
{
    private readonly Dictionary<string, Dictionary<string, string>> _tables = new(StringComparer.OrdinalIgnoreCase);

    private readonly VMenuPlugin _plugin;

    internal PluginTranslations(VMenuPlugin plugin) => _plugin = plugin;

    internal Dictionary<string, Dictionary<string, string>> Tables => _tables;

    /// <summary>Adds or extends one language's table. Later entries win over earlier ones.</summary>
    public void Add(string languageCode, IReadOnlyDictionary<string, string> entries)
    {
        var code = languageCode.Trim().ToLowerInvariant();

        if (!_tables.TryGetValue(code, out var table))
        {
            table = new Dictionary<string, string>(StringComparer.Ordinal);
            _tables[code] = table;
        }

        var merged = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var pair in entries)
        {
            table[pair.Key] = pair.Value;
            merged[pair.Key] = pair.Value;
        }

        _plugin.MergeTranslations(code, merged);
    }
}
