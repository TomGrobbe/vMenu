using System.Globalization;
using System.Text;

using CitizenFX.FiveM.Server;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Configuration;
using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Http.Server;
using vMenu.Enhanced.Logging;

namespace vMenu.Enhanced.Webhooks.Server;

internal sealed class DiscordChannel(
    LogCategory category,
    StringSetting urlSetting,
    StringSetting? fallbackSetting = null)
{
    private const int TimeoutMs = 10000;

    private const int MaxAttempts = 5;

    private const int MaxRateLimitRetries = 10;

    private const int BaseBackoffMs = 2000;

    private const int MaxBackoffMs = 60000;

    private const int QuietMs = 300000;

    private const string DroppedPrefix = "\n-# ";

    private readonly WebhookQueue _queue = new();

    private List<WebhookEntry>? _inFlight;

    private int _inFlightDropped;

    private int _nextAttemptAt;

    private int _attempts;

    private int _rateLimitRetries;

    private bool _disabled;

    private bool _reported;

    private int _reportedAt;

    public bool IsConfigured => Url().Length > 0;

    public void Add(WebhookEntry entry)
    {
        if (_disabled || !IsConfigured)
        {
            return;
        }

        _queue.Add(entry);
    }

    public void Reset()
    {
        _disabled = false;
        _attempts = 0;
        _rateLimitRetries = 0;
        _nextAttemptAt = 0;
        _reported = false;

        if (IsConfigured)
        {
            return;
        }

        _queue.Clear();
        _inFlight = null;
        _inFlightDropped = 0;
    }

    public async Task FlushAsync()
    {
        if (_disabled)
        {
            return;
        }

        var url = Url();

        if (url.Length == 0 || Native.GetGameTimer() < _nextAttemptAt)
        {
            return;
        }

        var batch = _inFlight ??= TakeBatch();

        if (batch.Count == 0)
        {
            _inFlight = null;

            return;
        }

        var reply = await HttpSend.SendAsync(
            new HttpRequest(url, "application/json", WebhookIdentity.UserAgent(), TimeoutMs)
            {
                Method = "POST",
                Body = DiscordPayload.Build(category, Describe(batch), WebhookIdentity.Footer()),
            });

        Handle(reply, batch.Count);
    }

    private List<WebhookEntry> TakeBatch()
    {
        var length = 0;
        var taken = 0;

        while (taken < _queue.Count)
        {
            var line = _queue.At(taken).Line().Length + 1;

            if (taken > 0 && length + line > DiscordPayload.MaxDescription)
            {
                break;
            }

            length += line;
            taken++;
        }

        _inFlightDropped = _queue.Dropped;

        return taken == 0 ? [] : _queue.Take(taken);
    }

    private string Describe(List<WebhookEntry> batch)
    {
        var description = new StringBuilder();

        foreach (var entry in batch)
        {
            if (description.Length > 0)
            {
                description.Append('\n');
            }

            description.Append(entry.Line());
        }

        if (description.Length > DiscordPayload.MaxDescription)
        {
            return WebhookText.Truncate(description.ToString(), DiscordPayload.MaxDescription);
        }

        if (_inFlightDropped > 0)
        {
            description
                .Append(DroppedPrefix)
                .Append("... and ")
                .Append(_inFlightDropped.ToString(CultureInfo.InvariantCulture))
                .Append(" more line(s) dropped, the queue was full.");
        }

        return description.ToString();
    }

    private void Handle(HttpReply reply, int sent)
    {
        if (reply.IsAccepted)
        {
            _queue.Forgive(_inFlightDropped);

            _inFlight = null;
            _inFlightDropped = 0;
            _attempts = 0;
            _rateLimitRetries = 0;
            _nextAttemptAt = 0;
            _reported = false;

            return;
        }

        if (reply.Status == 429)
        {
            RateLimited(reply);

            return;
        }

        if (reply.Status is 401 or 403 or 404)
        {
            Disable(reply.Status);

            return;
        }

        if (reply.Outcome == HttpOutcome.Answered && reply.Status is >= 400 and < 500)
        {
            Report(
                $"[Webhooks] {Name()} was refused with status {reply.Status}. "
                + $"Dropping {sent} line(s). Body: {WebhookText.Truncate(reply.Body, 300)}");

            Drop();

            return;
        }

        _attempts++;

        if (_attempts >= MaxAttempts)
        {
            Report(
                $"[Webhooks] {Name()} failed {_attempts} time(s) in a row "
                + $"({Why(reply)}). Dropping {sent} line(s) and carrying on.");

            Drop();

            return;
        }

        var backoff = Math.Min(MaxBackoffMs, BaseBackoffMs << (_attempts - 1));

        _nextAttemptAt = Native.GetGameTimer() + backoff;

        Report($"[Webhooks] {Name()} did not go through ({Why(reply)}). Retrying in {backoff / 1000}s.");
    }

    private void RateLimited(HttpReply reply)
    {
        _rateLimitRetries++;

        if (_rateLimitRetries > MaxRateLimitRetries)
        {
            Report(
                $"[Webhooks] {Name()} has been rate limited {_rateLimitRetries} time(s) in a row. "
                + "Dropping this batch. Raise vMenu.Enhanced.Logging.FlushSeconds if this keeps happening.");

            Drop();

            return;
        }

        var seconds = reply.RetryAfterSeconds ?? DiscordPayload.RetryAfter(reply.Body) ?? 1f;
        var wait = Math.Clamp((int)(seconds * 1000f), 250, MaxBackoffMs);

        _nextAttemptAt = Native.GetGameTimer() + wait;

        Log.Debug($"[Webhooks] {Name()} is rate limited, waiting {wait}ms.");
    }

    private void Disable(int status)
    {
        _disabled = true;

        Drop();

        _queue.Clear();

        Log.Error(
            $"[Webhooks] Discord answered {status} for {Name()}, which means that URL is wrong "
            + "or the webhook was deleted. Nothing more will be sent to it until you correct the setting.");
    }

    private void Drop()
    {
        _queue.Forgive(_inFlightDropped);

        _inFlight = null;
        _inFlightDropped = 0;
        _attempts = 0;
        _rateLimitRetries = 0;
        _nextAttemptAt = 0;
    }

    private static string Why(HttpReply reply) => reply.Outcome switch
    {
        HttpOutcome.Answered => "status " + reply.Status.ToString(CultureInfo.InvariantCulture),
        HttpOutcome.TimedOut => "timed out",
        _ => reply.Reason ?? "no answer",
    };

    private void Report(string message)
    {
        var now = Native.GetGameTimer();

        if (_reported && now - _reportedAt < QuietMs)
        {
            Log.Debug(message);

            return;
        }

        _reported = true;
        _reportedAt = now;

        Log.Warning(message);
    }

    private string Url()
    {
        var url = ServerConfig.Value(urlSetting).Trim();

        return url.Length > 0 || fallbackSetting is null ? url : ServerConfig.Value(fallbackSetting).Trim();
    }

    // Names whichever setting actually supplied the URL, so a failing fallback does not blame the
    // convar the owner left empty.
    private string Name() =>
        fallbackSetting is null || ServerConfig.Value(urlSetting).Trim().Length > 0
            ? urlSetting.Name
            : fallbackSetting.Name;
}
