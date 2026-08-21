using System.Text;
using System.Text.Json;

namespace vMenu.Enhanced.Serialization;

/// <summary>
/// Carries forward fields one build wrote that another does not know about, so overwriting a save
/// written by a newer vMenu keeps what this build cannot see rather than dropping it.
/// </summary>
// All JsonDocument, JsonElement and Utf8JsonWriter, which work under the sandbox as of API 0.0.4. No
// mutable DOM (JsonNode), which is the piece that never worked, the same way Newtonsoft's JObject did not.
public static class JsonMerge
{
    /// <summary>
    /// Deep merges two JSON documents. <paramref name="preferred"/> wins everywhere the two overlap,
    /// and any object key only <paramref name="fallback"/> has is kept, at every level. Arrays and
    /// scalars are taken whole from <paramref name="preferred"/>.
    /// </summary>
    public static string Merge(string preferred, string fallback)
    {
        using var preferredDoc = JsonDocument.Parse(preferred);
        using var fallbackDoc = JsonDocument.Parse(fallback);
        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteMerged(writer, preferredDoc.RootElement, fallbackDoc.RootElement);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Whether <paramref name="candidate"/> holds every key and array element
    /// <paramref name="required"/> does, at every level. Scalar values may differ, since a build
    /// editing a field it understands is not data loss; only missing structure is.
    /// </summary>
    public static bool IsSupersetOf(string candidate, string required)
    {
        using var candidateDoc = JsonDocument.Parse(candidate);
        using var requiredDoc = JsonDocument.Parse(required);

        return Covers(candidateDoc.RootElement, requiredDoc.RootElement);
    }

    private static void WriteMerged(Utf8JsonWriter writer, JsonElement preferred, JsonElement fallback)
    {
        // Only objects merge. For anything else the newer-vs-older question does not arise field by
        // field, so the build doing the writing keeps its own value.
        if (preferred.ValueKind != JsonValueKind.Object || fallback.ValueKind != JsonValueKind.Object)
        {
            preferred.WriteTo(writer);

            return;
        }

        writer.WriteStartObject();

        foreach (var property in preferred.EnumerateObject())
        {
            writer.WritePropertyName(property.Name);

            if (fallback.TryGetProperty(property.Name, out var other))
            {
                WriteMerged(writer, property.Value, other);
            }
            else
            {
                property.Value.WriteTo(writer);
            }
        }

        // The fields this build never knew about: kept exactly as the newer build left them.
        foreach (var property in fallback.EnumerateObject())
        {
            if (!preferred.TryGetProperty(property.Name, out _))
            {
                writer.WritePropertyName(property.Name);
                property.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }

    private static bool Covers(JsonElement candidate, JsonElement required)
    {
        switch (required.ValueKind)
        {
            case JsonValueKind.Object:
                if (candidate.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                foreach (var property in required.EnumerateObject())
                {
                    if (!candidate.TryGetProperty(property.Name, out var value) || !Covers(value, property.Value))
                    {
                        return false;
                    }
                }

                return true;

            case JsonValueKind.Array:
                if (candidate.ValueKind != JsonValueKind.Array
                    || candidate.GetArrayLength() != required.GetArrayLength())
                {
                    return false;
                }

                var candidateItems = candidate.EnumerateArray();
                var requiredItems = required.EnumerateArray();

                while (candidateItems.MoveNext() && requiredItems.MoveNext())
                {
                    if (!Covers(candidateItems.Current, requiredItems.Current))
                    {
                        return false;
                    }
                }

                return true;

            default:
                return true;
        }
    }
}
