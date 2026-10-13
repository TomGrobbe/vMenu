using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace vMenu.Enhanced.ServerAPI;

// JSON for the plugin protocol, kept byte compatible with vMenu's own serializer. System.Text.Json,
// matching vMenu's ServerJson: camelCase names, comment and trailing-comma tolerance. Works under
// the FiveM runtime as of API 0.0.4, no Reflection.Emit involved.
internal static class PluginJson
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,

            // Kept in step with vMenu's own serializer: named floating point literals rather than a throw.
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };

        ApplyReadableEncoder(options);

        return options;
    }

    // The sandbox refuses both the encoder property and the Encoder setter, and this runs from a static
    // initializer where a throw would kill the type. A refusal is taken as an answer.
    private static void ApplyReadableEncoder(JsonSerializerOptions options)
    {
        try
        {
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            return;
        }
        catch (Exception)
        {
        }

        try
        {
            options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        }
        catch (Exception)
        {
        }
    }

    public static string Serialize(object? value) => JsonSerializer.Serialize(value, Options);

    public static bool TryDeserialize<T>(string json, out T? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<T>(json, Options);

            return true;
        }
        catch (JsonException)
        {
            value = default;

            return false;
        }
    }
}
