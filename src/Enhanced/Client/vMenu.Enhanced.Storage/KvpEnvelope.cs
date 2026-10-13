using System.Text.Json.Serialization;

namespace vMenu.Enhanced.Storage;

// What every KVP this resource writes contains. Generic rather than an object member, so the payload
// stays a typed value. KvpStore also writes this shape with T as a JsonElement when it has to carry
// forward fields a newer build added, see TryWritePreservingNewer.
internal sealed class KvpEnvelope<T>
{
    public string Key { get; init; } = string.Empty;

    public T? Value { get; init; }

    public string Type { get; init; } = string.Empty;

    // Lets an older vMenu recognise data written by a newer one, so it can carry that version's unknown
    // fields through a save rather than dropping them, and refuse the write if it cannot.
    public int Version { get; init; }

    // Set only when an older build last wrote this, to the version that build understood. Version alone
    // cannot say that: a merged save keeps the newer version so the newer build still finds its fields,
    // which means the number on its own claims a freshness the payload does not have. Omitted when
    // unset, so an ordinary envelope is byte for byte what it always was.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MergedBy { get; init; }
}

// Enough of an envelope to identify a key without deserializing its payload. System.Text.Json
// ignores undeclared properties, so reading into this never touches value.
internal sealed class KvpHeader
{
    public string Key { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public int Version { get; init; }
}

// What a stored value is, so a key read as the wrong type is caught rather than guessed at.
public static class KvpValueType
{
    public const string Bool = "bool";

    public const string Int = "int";

    public const string Float = "float";

    public const string String = "string";

    // Anything with a shape of its own, such as a saved vehicle.
    public const string Json = "json";
}
