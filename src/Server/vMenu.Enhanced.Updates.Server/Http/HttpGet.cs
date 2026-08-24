namespace vMenu.Enhanced.Updates.Server.Http;

// One GET over HttpClient, started on the thread pool and waited on the tick thread so everything
// the caller does after it can still call natives.
public static class HttpGet
{
    // One client for the resource's lifetime. Built on first use rather than at type load, so a
    // constructor that ever throws in this runtime turns into an unusable reply instead of a class that
    // will not load.
    private static HttpClient? _client;

    public static async Task<HttpReply> GetAsync(HttpRequest request)
    {
        var slot = new HttpSlot();

        // Fire and forget: the request runs on the thread pool and drops its answer into the slot, while the
        // caller waits for it on the tick thread through HttpWait. That is deliberate, because the caller
        // calls natives the moment it has the reply, and only the tick thread may.
        _ = SendAsync(request, slot);

        return await HttpWait.ForAsync(slot, request.TimeoutMs);
    }

    private static async Task SendAsync(HttpRequest request, HttpSlot slot)
    {
        // DateTimeOffset and not GetGameTimer: everything past the first await runs off the tick thread,
        // where a native call is not allowed, and the clock is fine to read from anywhere.
        var startedAt = DateTimeOffset.UtcNow;

        try
        {
            var client = _client ??= Build();

            using var message = new HttpRequestMessage(HttpMethod.Get, request.Url);
            message.Headers.TryAddWithoutValidation("User-Agent", request.UserAgent);
            message.Headers.TryAddWithoutValidation("Accept", request.Accept);

            using var cancel = new CancellationTokenSource(request.TimeoutMs);
            using var response = await client.SendAsync(message, cancel.Token);

            var body = await response.Content.ReadAsStringAsync();

            slot.Complete(HttpReply.Answered((int)response.StatusCode, body, Elapsed(startedAt)));
        }
        catch (TaskCanceledException)
        {
            slot.Complete(HttpReply.TimedOut(request.TimeoutMs));
        }
        catch (Exception exception)
        {
            slot.Complete(HttpReply.Unusable(exception.GetType().Name + ": " + exception.Message));
        }
    }

    // Proxy off on purpose. The default handler resolves the Windows system proxy, which loads
    // Microsoft.Win32.Registry, and that assembly is not shipped next to the resource so the load throws
    // "could not load file or assembly". vMenu talks straight to github.com and nuget.org anyway.
    private static HttpClient Build() => new(new SocketsHttpHandler { UseProxy = false });

    private static int Elapsed(DateTimeOffset from) => (int)(DateTimeOffset.UtcNow - from).TotalMilliseconds;
}
