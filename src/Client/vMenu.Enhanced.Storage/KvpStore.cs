using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Storage;

/// <summary>
/// The only thing in vMenu that touches FiveM's key/value store.
/// </summary>
/// <remarks>
/// Everything is written as a <see cref="KvpEnvelope{T}"/> string. The typed natives
/// (<c>SetResourceKvpInt</c>, <c>SetResourceKvpFloat</c>) are never called: legacy used them and both
/// its float and int paths made an unset value indistinguishable from a zero one.
/// </remarks>
public static class KvpStore
{
    /// <summary>What a payload with no schema history of its own is written as.</summary>
    public const int InitialVersion = 1;

    /// <remarks>
    /// Reads are hot: a permission resync walks every entry of every menu, and each one re-reads
    /// whatever backs it.
    /// </remarks>
    private static readonly Dictionary<string, Cached> Cache = new(StringComparer.Ordinal);

    /// <summary>Keys already complained about, so one bad value does not log on every refresh.</summary>
    private static readonly HashSet<string> Reported = new(StringComparer.Ordinal);

    /// <param name="storedVersion">
    /// The version actually on disk, which may exceed <paramref name="knownVersion"/> when a newer
    /// vMenu wrote it. The value is still returned — a partial read beats none — but a caller seeing
    /// a higher number must expect <see cref="TryWrite"/> to refuse.
    /// </param>
    /// <returns><see langword="false"/> when the key is absent, unreadable, or holds another type.</returns>
    public static bool TryRead<T>(string key, string expectedType, int knownVersion, out T? value, out int storedVersion)
    {
        // A cached value of another type means two callers disagree about what this key holds. Fall
        // through and re-read rather than reporting a miss, so the store stays self-healing.
        if (Cache.TryGetValue(key, out var cached) && cached.Value is T hit)
        {
            storedVersion = cached.Version;
            value = hit;

            return true;
        }

        storedVersion = knownVersion;
        value = default;

        var raw = Native.GetResourceKvpString(key);

        if (string.IsNullOrEmpty(raw))
        {
            return false;
        }

        if (!ClientJson.TryDeserialize<KvpEnvelope<T>>(raw, out var envelope) || envelope is null)
        {
            Complain(key, "is not readable as a vMenu envelope, so it is being ignored");
            return false;
        }

        if (!string.Equals(envelope.Type, expectedType, StringComparison.Ordinal))
        {
            Complain(key, $"holds a '{envelope.Type}' but was read as a '{expectedType}', so it is being ignored");
            return false;
        }

        // Not fatal, but it means something copied a value between keys without rewriting it, and a
        // dump would read misleadingly.
        if (!string.Equals(envelope.Key, key, StringComparison.Ordinal))
        {
            Complain(key, $"names itself '{envelope.Key}', which is not the key it is stored under");
        }

        storedVersion = envelope.Version;
        value = envelope.Value;

        Cache[key] = new Cached(value, envelope.Version);

        return true;
    }

    /// <summary>
    /// Writes a value, unless doing so would destroy data this build does not understand.
    /// </summary>
    /// <param name="version">
    /// What this build knows the payload's shape to be. A stored version above this means a newer
    /// vMenu wrote fields that are not on <typeparamref name="T"/>, which serializing over it would
    /// silently drop.
    /// </param>
    /// <returns><see langword="false"/> when the write was refused. Nothing was changed.</returns>
    public static bool TryWrite<T>(string key, string type, int version, T value)
    {
        if (StoredVersion(key) is { } stored && stored > version)
        {
            API.Log.Warn(
                $"[Storage] '{key}' was saved by a newer version of vMenu (version {stored}, this "
                + $"build understands {version}). Refusing to overwrite it, because doing so would "
                + "discard whatever that version added.");

            return false;
        }

        var envelope = new KvpEnvelope<T>
        {
            Key = key,
            Value = value,
            Type = type,
            Version = version,
        };

        Native.SetResourceKvp(key, ClientJson.Serialize(envelope));

        Cache[key] = new Cached(value, version);
        Reported.Remove(key);

        return true;
    }

    public static void Delete(string key)
    {
        Native.DeleteResourceKvp(key);

        Cache.Remove(key);
        Reported.Remove(key);
    }

    /// <summary>Every key starting with <paramref name="prefix"/>.</summary>
    /// <remarks>
    /// Materialised rather than lazy so a caller can delete while iterating, and the handle is
    /// released in a <see langword="finally"/> — legacy's <c>GetSavedPeds</c> leaked one per call.
    /// </remarks>
    public static List<string> Keys(string prefix)
    {
        var keys = new List<string>();
        var handle = Native.StartFindKvp(prefix);

        if (handle == -1)
        {
            return keys;
        }

        try
        {
            while (Native.FindKvp(handle) is { Length: > 0 } key)
            {
                keys.Add(key);
            }
        }
        finally
        {
            Native.EndFindKvp(handle);
        }

        return keys;
    }

    /// <summary>One line per key, raw, for a dump command.</summary>
    public static IEnumerable<string> Describe(string prefix)
    {
        foreach (var key in Keys(prefix))
        {
            var raw = Native.GetResourceKvpString(key);

            yield return string.IsNullOrEmpty(raw) ? $"{key} = <empty>" : $"{key} = {raw}";
        }
    }

    /// <summary>Drops every cached value, so the next read goes back to the store.</summary>
    public static void InvalidateCache()
    {
        Cache.Clear();
        Reported.Clear();
    }

    /// <summary>
    /// Reads only the header, so a payload whose shape this build does not know is never guessed at.
    /// </summary>
    private static int? StoredVersion(string key)
    {
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached.Version;
        }

        var raw = Native.GetResourceKvpString(key);

        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        return ClientJson.TryDeserialize<KvpHeader>(raw, out var header) && header is not null
            ? header.Version
            : null;
    }

    private static void Complain(string key, string problem)
    {
        if (Reported.Add(key))
        {
            API.Log.Warn($"[Storage] '{key}' {problem}.");
        }
    }

    /// <remarks>
    /// Not a <see langword="record"/>: the generated equality reaches for
    /// <c>EqualityComparer&lt;T&gt;.Default</c>, which the client sandbox refuses.
    /// </remarks>
    private readonly struct Cached(object? value, int version)
    {
        internal object? Value { get; } = value;

        internal int Version { get; } = version;
    }
}
