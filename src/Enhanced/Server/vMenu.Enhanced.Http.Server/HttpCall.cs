using CitizenFX.FiveM.Server.Http;

namespace vMenu.Enhanced.Http.Server;

public sealed class HttpCall
{
    private readonly IReadOnlyDictionary<string, string> _headers;

    private readonly HttpResponse _response;

    private HttpCall(string method, string path, string query, string address, IReadOnlyDictionary<string, string> headers, HttpResponse response)
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

    public static HttpCall From(CitizenFX.FiveM.Server.Http.HttpRequest request, HttpResponse response)
    {
        var raw = request.Path;
        var split = raw.IndexOf('?');

        return new HttpCall(
            request.Method.ToUpperInvariant(),
            split < 0 ? raw : raw[..split],
            split < 0 ? string.Empty : raw[(split + 1)..],
            request.Address,
            request.Headers,
            response);
    }

    public string Header(string name) => _headers.TryGetValue(name, out var value) ? value : string.Empty;

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
        if (_response.Sent)
        {
            return;
        }

        _response.WriteHead(status, new Dictionary<string, string>
        {
            ["Content-Type"] = contentType,
            ["Cache-Control"] = "no-store",
        });

        _response.Send(body);
    }
}
