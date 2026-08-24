namespace vMenu.Enhanced.MenuFramework.Localization;

// The ambient localizer, plus one event covering every reason resolved text can change. Consumers
// subscribe to Changed rather than ILocalizer.LanguageChanged, so swapping the localizer does not
// orphan their subscription.
public static class Localizer
{
    private static ILocalizer _current = new CompiledLocalizer();

    static Localizer() => _current.LanguageChanged += Raise;

    // Raised on a language switch, and when the localizer is replaced wholesale.
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

    public static bool TrySetLanguage(LanguageId language) => _current.TrySetLanguage(language);

    private static void Raise() => Changed?.Invoke();
}
