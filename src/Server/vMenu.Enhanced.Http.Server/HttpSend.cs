using System.Globalization;

using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Http.Server;

// One request over HttpClient, started on the thread pool and waited on the tick thread so everything
// the caller does after it can still call natives.
public static class HttpSend
{
    // One client for the resource's lifetime. Built on first use rather than at type load, so a
    // constructor that ever throws in this runtime turns into an unusable reply instead of a class that
    // will not load.
    private static HttpClient? _client;

    private static HttpClient? _unverifiedClient;

    public static async Task<HttpReply> SendAsync(HttpRequest request)
    {
        var slot = new HttpSlot();

        // Fire and forget: the request runs on the thread pool and drops its answer into the slot, while the
        // caller waits for it on the tick thread through HttpWait. That is deliberate, because the caller
        // calls natives the moment it has the reply, and only the tick thread may.
        _ = RunAsync(request, slot);

        return await HttpWait.ForAsync(slot, request.TimeoutMs);
    }

    private static async Task RunAsync(HttpRequest request, HttpSlot slot)
    {
        // DateTimeOffset and not GetGameTimer: everything past the first await runs off the tick thread,
        // where a native call is not allowed, and the clock is fine to read from anywhere.
        var startedAt = DateTimeOffset.UtcNow;

        try
        {
            var client = Client(request.AllowInvalidCertificates);

            using var message = new HttpRequestMessage(new HttpMethod(request.Method), request.Url);
            message.Headers.TryAddWithoutValidation("User-Agent", request.UserAgent);
            message.Headers.TryAddWithoutValidation("Accept", request.Accept);

            if (request.Body is { } body)
            {
                message.Content = new StringContent(body, System.Text.Encoding.UTF8, request.ContentType);
            }

            using var cancel = new CancellationTokenSource(request.TimeoutMs);
            using var response = await client.SendAsync(message, cancel.Token);

            var text = await response.Content.ReadAsStringAsync();

            slot.Complete(HttpReply.Answered((int)response.StatusCode, text, Elapsed(startedAt), RetryAfter(response)));
        }
        catch (TaskCanceledException)
        {
            slot.Complete(HttpReply.TimedOut(request.TimeoutMs));
        }
        catch (Exception exception)
        {
            Log.Debug(
                $"""
                ^1[Error] ^0An exception occurred during ^2HTTP {request.Method}^0.

                ^3Message:^0
                {exception.Message}

                ^3Inner exception message:^0
                {exception.InnerException?.Message}

                ^3Stacktrace:^0
                {exception.StackTrace}

                ^3Inner exception stacktrace:^0
                {exception.InnerException?.StackTrace}
                """
            );

            slot.Complete(HttpReply.Unusable(exception.GetType().Name + ": " + exception.Message));
        }
    }

    private static float? RetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta)
        {
            return (float)delta.TotalSeconds;
        }

        if (!response.Headers.TryGetValues("Retry-After", out var values))
        {
            return null;
        }

        foreach (var value in values)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            {
                return seconds;
            }
        }

        return null;
    }

    private static HttpClient Client(bool allowInvalidCertificates) => allowInvalidCertificates
        ? _unverifiedClient ??= new HttpClient(Handler(trustAnything: true))
        : _client ??= new HttpClient(Handler(trustAnything: false));

    // Proxy off on purpose. The default handler resolves the Windows system proxy, which loads
    // Microsoft.Win32.Registry, and that assembly is not shipped next to the resource so the load throws
    // "could not load file or assembly". vMenu talks straight to github.com and nuget.org anyway.
    private static SocketsHttpHandler Handler(bool trustAnything)
    {
        var handler = new SocketsHttpHandler { UseProxy = false };

        if (trustAnything)
        {
            handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        }

        return handler;
    }

    private static int Elapsed(DateTimeOffset from) => (int)(DateTimeOffset.UtcNow - from).TotalMilliseconds;
}
