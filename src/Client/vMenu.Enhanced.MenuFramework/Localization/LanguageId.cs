namespace vMenu.Enhanced.Localization;

/// <summary>
/// Identifies a language by its lowercase code.
/// </summary>
/// <remarks>
/// Equality is written out by hand rather than left to a <see langword="record"/>: the generated
/// members route through <c>EqualityComparer&lt;string&gt;.Default</c>, and the FiveM sandbox refuses
/// to load the internal comparer behind it. The same rule applies anywhere else in this assembly —
/// always hand collections an explicit comparer.
/// </remarks>
public readonly struct LanguageId : IEquatable<LanguageId>
{
    public LanguageId(string code) => Code = code;

    /// <summary>The fallback for every other language, and the only one required to be complete.</summary>
    public static LanguageId English { get; } = new("en");

    public string Code { get; }

    public bool IsEmpty => string.IsNullOrEmpty(Code);

    public static LanguageId FromCode(string code) => new(code.Trim().ToLowerInvariant());

    public bool Equals(LanguageId other) => string.Equals(Code, other.Code, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is LanguageId other && Equals(other);

    /// <summary>Plain <see cref="string.GetHashCode()"/>, which is already ordinal, to keep the
    /// amount of BCL machinery the sandbox has to allow as small as possible.</summary>
    public override int GetHashCode() => Code?.GetHashCode() ?? 0;

    public static bool operator ==(LanguageId left, LanguageId right) => left.Equals(right);

    public static bool operator !=(LanguageId left, LanguageId right) => !left.Equals(right);

    public override string ToString() => Code ?? string.Empty;
}
