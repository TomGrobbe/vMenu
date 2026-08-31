namespace vMenu.Enhanced.Http.Server;

public sealed class HttpRequest(string url, string accept, string userAgent, int timeoutMs)
{
    public string Url { get; } = url;

    public string Accept { get; } = accept;

    public string UserAgent { get; } = userAgent;

    public int TimeoutMs { get; } = timeoutMs;

    public string Method { get; init; } = "GET";

    public string? Body { get; init; }

    public string ContentType { get; init; } = "application/json";

    public bool AllowInvalidCertificates { get; init; }
}
