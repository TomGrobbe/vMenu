using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Http.Server;
using vMenu.Enhanced.Logging;

using LoggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Logging;

namespace vMenu.Enhanced.Webhooks.Server;

internal sealed class GenericChannel
{
    private const int TimeoutMs = 5000;

    private const int MaxPerBatch = 200;

    private const int QuietMs = 300000;

    private readonly WebhookQueue _queue = new();

    private bool _reported;

    private int _reportedAt;

    public bool IsConfigured => Url().Length > 0;

    public void Add(WebhookEntry entry)
    {
        if (!IsConfigured)
        {
            return;
        }

        _queue.Add(entry);
    }

    public void Reset()
    {
        _reported = false;

        if (!IsConfigured)
        {
            _queue.Clear();
        }
    }

    public async Task FlushAsync()
    {
        var url = Url();

        if (url.Length == 0 || _queue.IsEmpty)
        {
            return;
        }

        var batch = _queue.Take(Math.Min(MaxPerBatch, _queue.Count));

        _queue.Forgive(_queue.Dropped);

        var reply = await HttpSend.SendAsync(
            new HttpRequest(url, "application/json", WebhookIdentity.UserAgent(), TimeoutMs)
            {
                Method = "POST",
                Body = GenericPayload.Build(batch),
                AllowInvalidCertificates = true,
            });

        if (reply.IsAccepted)
        {
            _reported = false;

            return;
        }

        Report(batch.Count, reply);
    }

    private void Report(int sent, HttpReply reply)
    {
        var now = Native.GetGameTimer();

        var message =
            $"[Webhooks] {LoggingSettings.GenericWebhook.Name} did not accept {sent} line(s) "
            + $"({reply.Reason ?? "status " + reply.Status}). They are gone; this webhook never retries.";

        if (_reported && now - _reportedAt < QuietMs)
        {
            Log.Debug(message);

            return;
        }

        _reported = true;
        _reportedAt = now;

        Log.Warning(message);
    }

    private static string Url() => ServerConfig.Value(LoggingSettings.GenericWebhook).Trim();
}
