namespace vMenu.Enhanced.Updates.Server.Http;

/// <summary>One GET and the few things needed to make it.</summary>
public sealed class HttpRequest(string url, string accept, string userAgent, int timeoutMs)
{
    public string Url { get; } = url;

    public string Accept { get; } = accept;

    public string UserAgent { get; } = userAgent;

    public int TimeoutMs { get; } = timeoutMs;
}
