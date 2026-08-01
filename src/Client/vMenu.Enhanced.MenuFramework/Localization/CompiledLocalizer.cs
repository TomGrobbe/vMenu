namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>
/// Resolves keys against the compiled tables in <see cref="LanguageCatalog"/>.
/// </summary>
public sealed class CompiledLocalizer : ILocalizer
{
    private LanguageTable _current = LanguageCatalog.English;

    public LanguageId CurrentLanguage => _current.Id;

    public IReadOnlyList<LanguageId> AvailableLanguages => LanguageCatalog.Available;

    public event Action? LanguageChanged;

    public string Get(string key)
    {
        if (_current.TryGet(key, out var value))
        {
            return value;
        }

        // A partial translation is fine and expected; a gap in English is a bug in vMenu itself.
        if (_current.Id != LanguageId.English && LanguageCatalog.English.TryGet(key, out value))
        {
            return value;
        }

        LocalizationLog.MissingKey(key);

        return $"!!{key}!!";
    }

    public bool TrySetLanguage(LanguageId language)
    {
        if (!LanguageCatalog.TryGet(language, out var table))
        {
            return false;
        }

        if (table.Id == _current.Id)
        {
            return true;
        }

        _current = table;

        LanguageChanged?.Invoke();

        return true;
    }
}
