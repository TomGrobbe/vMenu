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

/// <summary>What a stored value is, so a key read as the wrong type is caught rather than guessed at.</summary>
public static class KvpValueType
{
    public const string Bool = "bool";

    public const string Int = "int";

    public const string Float = "float";

    public const string String = "string";

    /// <summary>Anything with a shape of its own, such as a saved vehicle.</summary>
    public const string Json = "json";
}
