namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>Resolves translation keys to display strings.</summary>
// A seam so the storage can change without touching a call site. Tables are compiled C# today, and a
// file backed implementation would drop in here.
public interface ILocalizer
{
    LanguageId CurrentLanguage { get; }

    IReadOnlyList<LanguageId> AvailableLanguages { get; }

    /// <summary>Raised after a successful <see cref="TrySetLanguage"/>; menus re-label in place.</summary>
    event Action? LanguageChanged;

    /// <summary>Never throws and never returns null.</summary>
    // A key missing even from English renders as a visible marker, because throwing here would land
    // inside menu construction or a draw loop and take the whole menu down over a typo.
    string Get(string key);

    /// <returns><see langword="false"/> when the language is not registered; the current one is kept.</returns>
    bool TrySetLanguage(LanguageId language);
}
