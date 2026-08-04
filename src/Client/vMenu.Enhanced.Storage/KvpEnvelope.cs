namespace vMenu.Enhanced.Storage;

/// <summary>
/// What every KVP this resource writes actually contains.
/// </summary>
/// <remarks>
/// The key and the type are repeated inside the value on purpose: a KVP dump is otherwise a list of
/// opaque strings under opaque names, and the typed natives give no way to ask what a key holds
/// without already knowing.
/// <para>
/// Generic rather than carrying an <see cref="object"/>, which would make Newtonsoft build a
/// <c>JObject</c> for any nested payload — refused by the sandbox, see <c>ClientJson</c>.
/// </para>
/// </remarks>
internal sealed class KvpEnvelope<T>
{
    public string Key { get; init; } = string.Empty;

    public T? Value { get; init; }

    /// <summary>One of <see cref="KvpValueType"/>. Verified on read, never branched on.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Bumped by whoever adds a field to a stored shape, so an older vMenu can recognise data it does
    /// not fully understand and decline to overwrite it.
    /// </summary>
    public int Version { get; init; }
}

/// <summary>
/// Enough of an envelope to identify a key without deserializing its payload.
/// </summary>
/// <remarks>
/// Newtonsoft skips properties a type does not declare, so reading into this never touches
/// <c>value</c> — which is what makes it safe for a key whose payload shape is unknown.
/// </remarks>
internal sealed class KvpHeader
{
    public string Key { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public int Version { get; init; }
}

/// <summary>The vocabulary of the <c>type</c> field. Deliberately not .NET type names.</summary>
internal static class KvpValueType
{
    internal const string Bool = "bool";

    internal const string Int = "int";

    internal const string Float = "float";

    internal const string String = "string";

    /// <summary>Anything with a shape of its own: a saved vehicle, a ped, an outfit.</summary>
    internal const string Json = "json";
}
