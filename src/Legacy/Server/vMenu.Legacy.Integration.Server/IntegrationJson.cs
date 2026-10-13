using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vMenu.Enhanced.Integration.Server;

internal static class IntegrationJson
{
    public const string Ok = "{\"ok\":true}";

    private static readonly JsonLoadSettings LoadSettings = new()
    {
        CommentHandling = CommentHandling.Ignore,
        LineInfoHandling = LineInfoHandling.Ignore,
    };

    public static JToken Parse(string json)
    {
        using var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
        var token = JToken.ReadFrom(reader, LoadSettings);

        while (reader.Read())
        {
        }

        return token;
    }

    public static string? ReadString(JToken? element, string name) =>
        element is JObject obj
            && obj.TryGetValue(name, StringComparison.Ordinal, out var value)
            && value.Type == JTokenType.String
            ? (string?)value
            : null;

    public static int ReadInt(JToken? element, string name) =>
        TryReadInt(element, name, out var read) ? read : 0;

    public static bool TryReadInt(JToken? element, string name, out int result)
    {
        result = 0;

        if (element is not JObject obj
            || !obj.TryGetValue(name, StringComparison.Ordinal, out var value)
            || value.Type != JTokenType.Integer
            || ((JValue)value).Value is not long number
            || number is < int.MinValue or > int.MaxValue)
        {
            return false;
        }

        result = (int)number;

        return true;
    }

    public static bool TryReadFloat(JToken? element, string name, out float result)
    {
        result = 0f;

        if (element is not JObject obj
            || !obj.TryGetValue(name, StringComparison.Ordinal, out var value)
            || value.Type is not (JTokenType.Integer or JTokenType.Float))
        {
            return false;
        }

        var number = (float)(double)value;
        if (float.IsNaN(number) || float.IsInfinity(number))
        {
            return false;
        }

        result = number;

        return true;
    }

    public static bool TryReadBool(JToken? element, string name, out bool result)
    {
        result = false;

        if (element is not JObject obj || !obj.TryGetValue(name, StringComparison.Ordinal, out var value))
        {
            return false;
        }

        if (value.Type == JTokenType.Boolean)
        {
            result = (bool)value;

            return true;
        }

        return false;
    }

    public static string Fail(string reason) => $"{{\"ok\":false,\"reason\":\"{reason}\"}}";
}
