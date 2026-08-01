namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>
/// The ambient localizer, plus one event that covers every reason resolved text can change.
/// </summary>
/// <remarks>
/// Consumers subscribe to <see cref="Changed"/> rather than to
/// <see cref="ILocalizer.LanguageChanged"/> directly, so swapping the localizer itself does not
/// silently orphan their subscription.
/// </remarks>
public static class Localizer
{
    private static ILocalizer _current = new CompiledLocalizer();

    static Localizer() => _current.LanguageChanged += Raise;

    /// <summary>Raised on a language switch, and when the localizer is replaced wholesale.</summary>
    public static event Action? Changed;

    public static ILocalizer Current => _current;

    public static void Use(ILocalizer localizer)
    {
        if (ReferenceEquals(localizer, _current))
        {
            return;
        }

        _current.LanguageChanged -= Raise;
        _current = localizer;
        _current.LanguageChanged += Raise;

        Raise();
    }

    /// <summary>Convenience over <see cref="ILocalizer.TrySetLanguage"/> for the language picker.</summary>
    public static bool TrySetLanguage(LanguageId language) => _current.TrySetLanguage(language);

    private static void Raise() => Changed?.Invoke();
}
