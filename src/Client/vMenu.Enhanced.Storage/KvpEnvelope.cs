namespace vMenu.Enhanced.Storage;

/// <summary>What every KVP this resource writes contains.</summary>
// Generic rather than an object member, which would make Newtonsoft build a JObject. The sandbox
// refuses those.
internal sealed class KvpEnvelope<T>
{
    public string Key { get; init; } = string.Empty;

    public T? Value { get; init; }

    public string Type { get; init; } = string.Empty;

    /// <summary>Lets an older vMenu recognise data it cannot fully read and refuse to overwrite it.</summary>
    public int Version { get; init; }
}

/// <summary>Enough of an envelope to identify a key without deserializing its payload.</summary>
// Newtonsoft skips undeclared properties, so reading into this never touches value.
internal sealed class KvpHeader
{
    public string Key { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public int Version { get; init; }
}

internal static class KvpValueType
{
    internal const string Bool = "bool";

    internal const string Int = "int";

    internal const string Float = "float";

    internal const string String = "string";

    internal const string Json = "json";
}
