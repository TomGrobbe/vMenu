using System.Collections;

using CitizenFX.Core;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Http.Server;

public sealed class HttpCall
{
    private readonly IDictionary<string, object> _response;

    private readonly IDictionary<string, object> _headers;

    private HttpCall(
        string method,
        string path,
        string query,
        string address,
        IDictionary<string, object> headers,
        IDictionary<string, object> response)
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

    public static HttpCall From(IDictionary<string, object>? request, IDictionary<string, object>? response)
    {
        var fields = request ?? new Dictionary<string, object>();
        var raw = Field(fields, "path");
        var split = raw.IndexOf('?');

        return new HttpCall(
            Field(fields, "method").ToUpperInvariant(),
            split < 0 ? raw : raw[..split],
            split < 0 ? string.Empty : raw[(split + 1)..],
            Field(fields, "address"),
            fields.TryGetValue("headers", out var headers) && headers is IDictionary<string, object> bag ? bag : new Dictionary<string, object>(),
            response ?? new Dictionary<string, object>());
    }

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
                "[Http] The host handed back a response object this cannot answer through, so the " +
                $"caller is left hanging. It carries {string.Join(", ", _response.Keys)}.");
        }
    }

    private bool Invoke(string name, params object[] args)
    {
        if (!_response.TryGetValue(name, out var member) || member is not CallbackDelegate callback)
        {
            return false;
        }

        callback(args);

        return true;
    }

    private static string Field(IDictionary<string, object> bag, string name) =>
        bag.TryGetValue(name, out var value) ? Text(value) : string.Empty;

    private static string Text(object? value) => value switch
    {
        null => string.Empty,
        string text => text,
        IEnumerable many => string.Join(", ", many.Cast<object?>().Select(item => item?.ToString())),
        _ => value.ToString() ?? string.Empty,
    };
}
