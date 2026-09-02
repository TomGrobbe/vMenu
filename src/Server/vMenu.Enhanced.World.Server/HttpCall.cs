using CitizenFX.FiveM.Shared.FuncRef;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.World.Server;

// The request and response the host hands the handler. Both arrive as raw MessagePack, because the
// runtime cannot turn a bag holding function references into a plain object, so every field is read
// out by hand here and may come back empty.
internal sealed class HttpCall
{
    private readonly IReadOnlyDictionary<string, MessagePackBuffer> _response;

    private readonly IReadOnlyDictionary<string, MessagePackBuffer> _headers;

    private HttpCall(
        string method,
        string path,
        string query,
        string address,
        IReadOnlyDictionary<string, MessagePackBuffer> headers,
        IReadOnlyDictionary<string, MessagePackBuffer> response)
    {
        Method = method;
        Path = path;
        Query = query;
        Address = address;
        _headers = headers;
        _response = response;
    }

    public string Method { get; }

    public string Path { get; }

    public string Query { get; }

    public string Address { get; }

    public static HttpCall From(MessagePackBuffer? request, MessagePackBuffer? response)
    {
        var fields = Bag(request);
        var raw = Field(fields, "path");
        var split = raw.IndexOf('?');

        return new HttpCall(
            Field(fields, "method").ToUpperInvariant(),
            split < 0 ? raw : raw[..split],
            split < 0 ? string.Empty : raw[(split + 1)..],
            Field(fields, "address"),
            fields.TryGetValue("headers", out var headers) ? Bag(headers) : new Dictionary<string, MessagePackBuffer>(),
            Bag(response));
    }

    // Header names are not case sensitive, so this walks the bag rather than looking the name up.
    public string Header(string name)
    {
        foreach (var pair in _headers)
        {
            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return Text(pair.Value);
            }
        }

        return string.Empty;
    }

    public string QueryValue(string name)
    {
        foreach (var pair in Query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var split = pair.IndexOf('=');
            var key = split < 0 ? pair : pair[..split];

            if (string.Equals(Uri.UnescapeDataString(key), name, StringComparison.OrdinalIgnoreCase))
            {
                return split < 0 ? string.Empty : Uri.UnescapeDataString(pair[(split + 1)..]);
            }
        }

        return string.Empty;
    }

    public void Reply(int status, string contentType, string body)
    {
        var headers = new Dictionary<string, object>
        {
            ["Content-Type"] = contentType,
            ["Cache-Control"] = "no-store",
        };

        if (!Invoke("writeHead", status, headers) || !Invoke("send", body))
        {
            Log.Error(
                "[WorldApi] The host handed back a response object this cannot answer through, so the " +
                $"caller is left hanging. It carries {string.Join(", ", _response.Keys)}.");
        }
    }

    private bool Invoke(string name, params object?[] args)
    {
        if (!_response.TryGetValue(name, out var member))
        {
            return false;
        }

        var reference = Read<FunctionReference>(member);

        if (reference is null)
        {
            return false;
        }

        reference.CallVoid(args);

        return true;
    }

    private static IReadOnlyDictionary<string, MessagePackBuffer> Bag(MessagePackBuffer? value) =>
        (value is null ? null : Read<Dictionary<string, MessagePackBuffer>>(value)) ??
        new Dictionary<string, MessagePackBuffer>();

    private static string Field(IReadOnlyDictionary<string, MessagePackBuffer> bag, string name) =>
        bag.TryGetValue(name, out var value) ? Text(value) : string.Empty;

    // A header the caller sent more than once arrives as a list, so both shapes are read here.
    private static string Text(MessagePackBuffer value) =>
        Read<string>(value) ?? (Read<string[]>(value) is { } many ? string.Join(", ", many) : string.Empty);

    private static T? Read<T>(MessagePackBuffer value)
        where T : class
    {
        try
        {
            return value.DeserializeTo<T>(true);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
