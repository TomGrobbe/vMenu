using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vMenu.Enhanced.Serialization;

public static class JsonMerge
{
    public static string Merge(string preferred, string fallback) =>
        Merged(Parse(preferred), Parse(fallback)).ToString(Formatting.None);

    public static bool IsSupersetOf(string candidate, string required) => Covers(Parse(candidate), Parse(required));

    public static JToken Parse(string json) =>
        JToken.Parse(json, new JsonLoadSettings { CommentHandling = CommentHandling.Ignore, LineInfoHandling = LineInfoHandling.Ignore });

    private static JToken Merged(JToken preferred, JToken fallback)
    {
        if (preferred is not JObject preferredObject || fallback is not JObject fallbackObject)
        {
            return preferred.DeepClone();
        }

        var merged = new JObject();

        foreach (var property in preferredObject.Properties())
        {
            merged[property.Name] = fallbackObject.TryGetValue(property.Name, StringComparison.Ordinal, out var other)
                ? Merged(property.Value, other)
                : property.Value.DeepClone();
        }

        foreach (var property in fallbackObject.Properties())
        {
            if (!preferredObject.ContainsKey(property.Name))
            {
                merged[property.Name] = property.Value.DeepClone();
            }
        }

        return merged;
    }

    private static bool Covers(JToken candidate, JToken required)
    {
        switch (required)
        {
            case JObject requiredObject:
                return candidate is JObject candidateObject
                    && requiredObject.Properties().All(property =>
                        candidateObject.TryGetValue(property.Name, StringComparison.Ordinal, out var value) && Covers(value, property.Value));

            case JArray requiredArray:
                return candidate is JArray candidateArray
                    && candidateArray.Count == requiredArray.Count
                    && candidateArray.Zip(requiredArray, Covers).All(covered => covered);

            default:
                return true;
        }
    }
}
