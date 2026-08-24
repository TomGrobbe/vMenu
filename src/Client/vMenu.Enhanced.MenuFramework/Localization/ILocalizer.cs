namespace vMenu.Enhanced.MenuFramework.Localization;

// Resolves translation keys to display strings. A seam so the storage can change without touching a
// call site: tables are compiled C# today, and a file backed implementation would drop in here.
public interface ILocalizer
{
    LanguageId CurrentLanguage { get; }

    IReadOnlyList<LanguageId> AvailableLanguages { get; }

    // Raised after a successful TrySetLanguage; menus re-label in place.
    event Action? LanguageChanged;

    // Never throws and never returns null. A key missing even from English renders as a visible marker,
    // because throwing here would land inside menu construction or a draw loop and take the whole menu
    // down over a typo.
    string Get(string key);

    // False when the language is not registered; the current one is kept.
    bool TrySetLanguage(LanguageId language);
}
