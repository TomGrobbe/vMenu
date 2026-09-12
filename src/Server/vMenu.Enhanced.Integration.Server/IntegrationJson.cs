using System.Text.Json;

namespace vMenu.Enhanced.Integration.Server;

internal static class IntegrationJson
{
    public const string Ok = "{\"ok\":true}";

    public static string? ReadString(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public static int ReadInt(JsonElement element, string name) =>
        TryReadInt(element, name, out var read) ? read : 0;

    public static bool TryReadInt(JsonElement element, string name, out int result)
    {
        result = 0;

        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out result);
    }

    public static bool TryReadFloat(JsonElement element, string name, out float result)
    {
        result = 0f;

        return element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetSingle(out result);
    }

    public static bool TryReadBool(JsonElement element, string name, out bool result)
    {
        result = false;

        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value))
        {
            return false;
        }

        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            result = value.GetBoolean();

            return true;
        }

        return false;
    }

    public static string Fail(string reason) => $"{{\"ok\":false,\"reason\":\"{reason}\"}}";
}
