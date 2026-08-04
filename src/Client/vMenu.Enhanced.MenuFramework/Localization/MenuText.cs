namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>
/// A piece of display text that is resolved late rather than baked in at declaration time.
/// </summary>
// Declared instead of a string so every label can be re-resolved on a language switch. Rebuilding is
// not an option, MenuController having no way to remove a menu once added, so text must be written
// back onto the existing items.
// A plain struct, not a record: the generated equality would route through
// EqualityComparer<string>.Default, which the sandbox refuses to load.
public readonly struct MenuText
{
    private enum Kind : byte
    {
        Empty = 0,
        Literal,
        Key,
        Deferred,
    }

    private readonly Kind _kind;
    private readonly string? _text;
    private readonly Func<string>? _factory;
    private readonly (string Name, MenuText Value)[]? _arguments;

    private MenuText(Kind kind, string? text, Func<string>? factory, (string Name, MenuText Value)[]? arguments)
    {
        _kind = kind;
        _text = text;
        _factory = factory;
        _arguments = arguments;
    }

    /// <summary>Resolves to an empty string, which MenuAPI treats the same as no text at all.</summary>
    public static MenuText Empty => default;

    public bool IsEmpty => _kind is Kind.Empty;

    /// <summary>Text that must not be translated, such as a vehicle model name.</summary>
    public static MenuText Literal(string text) => new(Kind.Literal, text, null, null);

    public static MenuText Key(string key) => new(Kind.Key, key, null, null);

    /// <summary>
    /// Arguments are <see cref="MenuText"/> rather than strings on purpose: an argument that is
    /// itself a key or a game label stays late bound and re-resolves with everything else, instead
    /// of freezing whatever it happened to say when the menu was built.
    /// </summary>
    public static MenuText Key(string key, params (string Name, MenuText Value)[] arguments) =>
        new(Kind.Key, key, null, arguments);

    /// <summary>
    /// Text produced on demand. This is how GTA's own labels fit — <c>GetLabelText</c> already
    /// returns them in the game's language, so they need no vMenu translation and re-resolve for
    /// free on every relabel pass.
    /// </summary>
    public static MenuText From(Func<string> factory) => new(Kind.Deferred, null, factory, null);

    /// <summary>A bare string is a literal. Translating is the deliberate act, never the accident.</summary>
    public static implicit operator MenuText(string literal) => Literal(literal);

    public string Resolve(ILocalizer localizer) => _kind switch
    {
        Kind.Literal => _text ?? string.Empty,
        Kind.Deferred => _factory?.Invoke() ?? string.Empty,
        Kind.Key => Placeholders.Substitute(localizer.Get(_text!), _arguments, localizer),
        _ => string.Empty,
    };
}
