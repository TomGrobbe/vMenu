using System.Globalization;

using vMenu.Enhanced.Http.Server.Bridge;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Http.Server;

public static class HttpSend
{
    private static HttpClient? _client;

    private static HttpClient? _unverifiedClient;

    public static async Task<HttpReply> SendAsync(HttpRequest request)
    {
        var slot = new HttpSlot();

        // Linux TLS bridge
        if (NetBridge.Active)
        {
            NetBridge.Send(request, slot);
        }
        else
        {
            _ = RunAsync(request, slot);
        }

        return await HttpWait.ForAsync(slot, request.TimeoutMs);
    }

    private static async Task RunAsync(HttpRequest request, HttpSlot slot)
    {
        var startedAt = DateTimeOffset.UtcNow;

        try
        {
            var client = Client(request.AllowInvalidCertificates);

            using var message = new HttpRequestMessage(new HttpMethod(request.Method), request.Url);
            message.Headers.TryAddWithoutValidation("User-Agent", request.UserAgent);
            message.Headers.TryAddWithoutValidation("Accept", request.Accept);

            if (request.Headers is { } extra)
            {
                foreach (var header in extra)
                {
                    message.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (request.Body is { } body)
            {
                message.Content = new StringContent(body, System.Text.Encoding.UTF8, request.ContentType);
            }

            // Another FiveM bug/crash. So no cancel token: cancelling a socket op calls CancelIoEx, unimplemented in FiveM and crashes the server. HttpWait is the only timeout.
            using var response = await client.SendAsync(message);

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
        ? _unverifiedClient ??= Build(trustAnything: true)
        : _client ??= Build(trustAnything: false);

    // Infinite timeout: HttpClient's timeout cancels the socket via CancelIoEx, unimplemented in FiveM and crashes the server. HttpWait handles timeouts instead.
    private static HttpClient Build(bool trustAnything) =>
        new(Handler(trustAnything)) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };

    // Proxy off: resolving the Windows proxy loads Microsoft.Win32.Registry, which is not shipped with
    // the resource and throws on load. vMenu talks straight to github.com and nuget.org anyway.
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
