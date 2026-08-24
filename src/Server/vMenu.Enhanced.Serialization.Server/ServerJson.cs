using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Serialization.Server;

// JSON on the server. System.Text.Json, which works as of API 0.0.4, with settings kept in step with
// ClientJson: what this writes is what that reads. JsonSerializer used to build accessors with
// Reflection.Emit and stamp their cache with DateTime.UtcNow, which the server runtime lacked, so the
// first serialize died. 0.0.4 fixed the clock, and the runtime reports dynamic code unsupported, so
// STJ stays on its plain reflection path.
public static class ServerJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // Newtonsoft matched property names loosely; keep that so anything an older build wrote still reads.
        PropertyNameCaseInsensitive = true,

        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,

        // Newtonsoft wrote a NaN or an infinity as a bare token; this throws on one instead, which would
        // take a whole save down rather than write one bad number.
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,

        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly JsonSerializerOptions IndentedOptions = new(Options)
    {
        WriteIndented = true,
    };

    // Why the server cannot serialize, or null when it can.
    private static readonly string? Failure = SelfTest();

    public static string Serialize(object? value) => JsonSerializer.Serialize(value, Options);

    // For a file a server owner is expected to open and read.
    public static string SerializeIndented(object? value) => JsonSerializer.Serialize(value, IndentedOptions);

    // For JSON from somewhere that can send nonsense, such as a config file an owner edits. error says
    // what was wrong with the document, or null when it read.
    public static bool TryDeserialize<T>(string json, out T? value, out string? error)
    {
        try
        {
            value = JsonSerializer.Deserialize<T>(json, Options);
            error = null;

            return true;
        }
        catch (JsonException exception)
        {
            value = default;
            error = exception.Message;

            return false;
        }
    }

    public static void Verify()
    {
        if (Failure is null)
        {
            Log.Debug("[Json] System.Text.Json is working, escaping with UnsafeRelaxedJsonEscaping.");

            return;
        }

        Log.Error(
            $"[Json] System.Text.Json failed its start-up self test ({Failure}). Every server side "
            + "serialize will now fail.");
    }

    // A round trip on the startup path, so a runtime that ever refuses STJ says so loudly here rather
    // than failing quietly at the first real call.
    private static string? SelfTest()
    {
        try
        {
            var json = JsonSerializer.Serialize(new SelfTestModel { Value = 1 }, Options);
            var read = JsonSerializer.Deserialize<SelfTestModel>(json, Options);

            return read?.Value == 1 ? null : "the round trip did not return the value";
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
    }

    private sealed class SelfTestModel
    {
        public int Value { get; set; }
    }
}
