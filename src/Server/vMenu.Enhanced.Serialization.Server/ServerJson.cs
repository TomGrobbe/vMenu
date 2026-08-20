using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Serialization.Server;

/// <summary>JSON on the server. System.Text.Json, which works as of API 0.0.4.</summary>
// Settings are kept in step with ClientJson: what this writes is what that reads. The old reason to
// avoid JsonSerializer is gone. It used to build accessors with Reflection.Emit and stamp their cache
// with DateTime.UtcNow, which the server runtime lacked, so the first serialize died. 0.0.4 fixed the
// clock, and the runtime reports dynamic code unsupported, so STJ stays on its plain reflection path.
public static class ServerJson
{
    // Declared before Options: ApplyReadableEncoder writes it from that field's initializer.
    private static string _encoderName = "the default encoder";

    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static readonly JsonSerializerOptions IndentedOptions = new(Options)
    {
        WriteIndented = true,
    };

    /// <summary>Why the server cannot serialize, or <see langword="null"/> when it can.</summary>
    private static readonly string? Failure = SelfTest();

    public static string Serialize(object? value) => JsonSerializer.Serialize(value, Options);

    /// <summary>For a file a server owner is expected to open and read.</summary>
    public static string SerializeIndented(object? value) => Unescape(JsonSerializer.Serialize(value, IndentedOptions));

    /// <summary>For JSON from somewhere that can send nonsense, such as a config file an owner edits.</summary>
    /// <param name="error">What was wrong with the document, or <see langword="null"/> when it read.</param>
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

    /// <summary>Says on the startup path whether JSON works at all.</summary>
    public static void Verify()
    {
        if (Failure is null)
        {
            Log.Debug($"[Json] System.Text.Json is working, escaping with {_encoderName}.");

            return;
        }

        Log.Error(
            $"[Json] System.Text.Json failed its start-up self test ({Failure}). Every server side "
            + "serialize will now fail.");
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

            // Newtonsoft matched property names loosely; keep that so anything an older build wrote still reads.
            PropertyNameCaseInsensitive = true,

            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,

            // Newtonsoft wrote a NaN or an infinity as a bare token; this throws on one instead, which
            // would take a whole save down rather than write one bad number.
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
            _encoderName = "UnsafeRelaxedJsonEscaping";

            return;
        }
        catch (Exception)
        {
        }

        try
        {
            options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            _encoderName = "Create(UnicodeRanges.All)";
        }
        catch (Exception)
        {
        }
    }

    // Stands in for the encoder the sandbox will not let us set, so a file an owner edits is not full
    // of \uXXXX. Indented output only: compact output feeds NUI and stored values.
    // Delete once citizenfx/rfc#440 lands.
    private static string Unescape(string json)
    {
        if (json.IndexOf("\\u", StringComparison.Ordinal) < 0)
        {
            return json;
        }

        var builder = new StringBuilder(json.Length);
        var index = 0;

        while (index < json.Length)
        {
            var current = json[index];

            // Copied whole, so a literal \\ cannot turn the u behind it into an escape.
            if (current == '\\' && index + 1 < json.Length && json[index + 1] != 'u')
            {
                builder.Append(current);
                builder.Append(json[index + 1]);
                index += 2;

                continue;
            }

            if (current != '\\'
                || index + 5 >= json.Length
                || !TryReadHex(json, index + 2, out var value)
                || !CanUnescape(value))
            {
                builder.Append(current);
                index++;

                continue;
            }

            builder.Append((char)value);
            index += 6;
        }

        return builder.ToString();
    }

    // Unescaping these would break the document. Surrogates stay escaped, as under the relaxed encoder.
    private static bool CanUnescape(int value) =>
        value >= 0x20 && value != 0x22 && value != 0x5C && value != 0x7F && (value < 0xD800 || value > 0xDFFF);

    // Not int.Parse: its ReadOnlySpan<char> overload wins resolution and the sandbox refuses it.
    private static bool TryReadHex(string json, int start, out int value)
    {
        value = 0;

        for (var offset = 0; offset < 4; offset++)
        {
            var digit = json[start + offset];
            int part;

            if (digit >= '0' && digit <= '9')
            {
                part = digit - '0';
            }
            else if (digit >= 'a' && digit <= 'f')
            {
                part = digit - 'a' + 10;
            }
            else if (digit >= 'A' && digit <= 'F')
            {
                part = digit - 'A' + 10;
            }
            else
            {
                return false;
            }

            value = (value * 16) + part;
        }

        return true;
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
