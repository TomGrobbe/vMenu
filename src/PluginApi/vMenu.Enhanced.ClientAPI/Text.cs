using vMenu.Enhanced.PluginContracts;

namespace vMenu.Enhanced.ClientAPI;

/// <summary>A piece of display text, either a literal or a key into your plugin's translation
/// tables. A bare string converts to a literal, so translating is always the deliberate act. Keys
/// are resolved by vMenu against the tables you registered, following its selected language with
/// your English table as the fallback.</summary>
// A struct rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the FiveM sandbox refuses to load.
public readonly struct Text
{
    private readonly string? _value;

    private readonly bool _isKey;

    private readonly (string Name, Text Value)[]? _arguments;

    private Text(string? value, bool isKey, (string Name, Text Value)[]? arguments)
    {
        _value = value;
        _isKey = isKey;
        _arguments = arguments;
    }

    public static Text Empty => default;

    public bool IsEmpty => _value is null;

    /// <summary>Text shown exactly as written, never translated.</summary>
    public static Text Literal(string text) => new(text, false, null);

    public static Text Key(string key) => new(key, true, null);

    /// <summary>A key with named placeholder values. In the translated string, <c>{name}</c> is replaced
    /// with the matching value, which can itself be a literal or another key.</summary>
    public static Text Key(string key, params (string Name, Text Value)[] arguments) =>
        new(key, true, arguments);

    public static implicit operator Text(string literal) => Literal(literal);

    internal TextRef? ToRef()
    {
        if (_value is null)
        {
            return null;
        }

        if (!_isKey)
        {
            return TextRef.Literal(_value);
        }

        var reference = TextRef.ForKey(_value);

        if (_arguments is { Length: > 0 } arguments)
        {
            reference.Args = new Dictionary<string, TextRef>(StringComparer.Ordinal);

            foreach (var (name, value) in arguments)
            {
                if (value.ToRef() is { } argument)
                {
                    reference.Args[name] = argument;
                }
            }
        }

        return reference;
    }
}
