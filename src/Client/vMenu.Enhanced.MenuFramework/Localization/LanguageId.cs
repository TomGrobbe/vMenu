namespace vMenu.Enhanced.MenuFramework.Localization;

/// <summary>Identifies a language by its lowercase code.</summary>
// Equality is hand written, not from a record: the generated members route through
// EqualityComparer<string>.Default, whose internal comparer the sandbox refuses to load. Same rule
// everywhere, so always hand collections an explicit comparer.
public readonly struct LanguageId(string code) : IEquatable<LanguageId>
{

    /// <summary>The fallback for every other language, and the only one required to be complete.</summary>
    public static LanguageId English { get; } = new("en");

    public string Code { get; } = code;

    public bool IsEmpty => string.IsNullOrEmpty(Code);

    public static LanguageId FromCode(string code) => new(code.Trim().ToLowerInvariant());

    public bool Equals(LanguageId other) => string.Equals(Code, other.Code, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is LanguageId other && Equals(other);

    // Plain string.GetHashCode, already ordinal, to keep the BCL machinery the sandbox must allow small.
    public override int GetHashCode() => Code?.GetHashCode() ?? 0;

    public static bool operator ==(LanguageId left, LanguageId right) => left.Equals(right);

    public static bool operator !=(LanguageId left, LanguageId right) => !left.Equals(right);

    public override string ToString() => Code ?? string.Empty;
}
