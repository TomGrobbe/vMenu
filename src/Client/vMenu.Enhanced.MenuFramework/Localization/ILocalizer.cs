namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>
/// Resolves translation keys to display strings.
/// </summary>
/// <remarks>
/// The seam exists so the storage can change without touching a single call site. Today every table
/// is compiled C# because the CitizenFX Enhanced sandbox cannot parse JSON client side; when that is
/// fixed a file backed implementation drops in here.
/// </remarks>
public interface ILocalizer
{
    LanguageId CurrentLanguage { get; }

    IReadOnlyList<LanguageId> AvailableLanguages { get; }

    /// <summary>Raised after a successful <see cref="TrySetLanguage"/>; menus re-label in place.</summary>
    event Action? LanguageChanged;

    /// <summary>
    /// Never throws and never returns null: a key missing even from English is reported and rendered
    /// as a visible marker, because throwing here would land inside menu construction or a draw loop
    /// and take the whole menu down over a typo.
    /// </summary>
    string Get(string key);

    /// <returns><see langword="false"/> when the language is not registered; the current one is kept.</returns>
    bool TrySetLanguage(LanguageId language);
}
