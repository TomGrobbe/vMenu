using CitizenFX.FiveM.Shared.FuncRef;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.World.Server;

// The request and response the host hands the handler. The native takes a plain function reference,
// so both arrive as loosely typed bags and every read here may come back empty.
internal sealed class HttpCall
{
    private readonly object? _response;

    private HttpCall(string method, string path, string query, string address, object? headers, object? response)
    {
        Method = method;
        Path = path;
        Query = query;
        Address = address;
        Headers = headers;
        _response = response;
    }

    public string Method { get; }

    public string Path { get; }

    public string Query { get; }

    public string Address { get; }

    private object? Headers { get; }

    public static HttpCall From(object? request, object? response)
    {
        var raw = Text(Member(request, "path"));
        var split = raw.IndexOf('?');

        return new HttpCall(
            Text(Member(request, "method")).ToUpperInvariant(),
            split < 0 ? raw : raw[..split],
            split < 0 ? string.Empty : raw[(split + 1)..],
            Text(Member(request, "address")),
            Member(request, "headers"),
            response);
    }

    // Header names are not case sensitive, so this walks the bag rather than looking the name up.
    public string Header(string name)
    {
        foreach (var (key, value) in Pairs(Headers))
        {
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                return Text(value);
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

        if (!Invoke(_response, "writeHead", status, headers) || !Invoke(_response, "send", body))
        {
            Log.Error(
                "[WorldApi] The host handed back a response object this cannot answer through, so the " +
                $"caller is left hanging. It arrived as {Describe(_response)}. See " +
                "https://github.com/citizenfx/rfc/discussions/257 for why a function reference may not " +
                "survive the trip.");
        }
    }

    private static bool Invoke(object? target, string name, params object?[] args)
    {
        switch (Member(target, name))
        {
            case FunctionReference reference:
                reference.CallVoid(args);

                return true;

            case Delegate method:
                method.DynamicInvoke(args);

                return true;

            default:
                return false;
        }
    }

    private static object? Member(object? target, string name)
    {
        foreach (var (key, value) in Pairs(target))
        {
            if (key == name)
            {
                return value;
            }
        }

        return null;
    }

    private static IEnumerable<(string Key, object? Value)> Pairs(object? target)
    {
        switch (target)
        {
            case IDictionary<string, object?> typed:
                foreach (var pair in typed)
                {
                    yield return (pair.Key, pair.Value);
                }

                break;

            case System.Collections.IDictionary loose:
                foreach (System.Collections.DictionaryEntry pair in loose)
                {
                    yield return (pair.Key as string ?? pair.Key.ToString() ?? string.Empty, pair.Value);
                }

                break;
        }
    }

    private static string Text(object? value) => value as string ?? value?.ToString() ?? string.Empty;

    private static string Describe(object? value) => value?.GetType().FullName ?? "null";
}
