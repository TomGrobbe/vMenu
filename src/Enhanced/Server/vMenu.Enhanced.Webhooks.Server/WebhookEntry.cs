using System.Globalization;
using System.Text;

using vMenu.Enhanced.Data.Logging;

namespace vMenu.Enhanced.Webhooks.Server;

public sealed class WebhookEntry(
    LogCategory category,
    DateTimeOffset at,
    string message,
    WebhookActor actor,
    WebhookActor? target,
    IReadOnlyList<(string Key, string Value)> data,
    bool withIdentifiers = false,
    bool warning = false)
{
    private const int MaxMessageLength = 400;

    private const int MaxValueLength = 250;

    private const string Bullet = "\u2022 ";

    private const string WarningMark = "\u26a0\ufe0f ";

    private const string Separator = " \u00b7 ";

    private const string Subtext = "\n-# ";

    public LogCategory Category { get; } = category;

    public DateTimeOffset At { get; } = at;

    public string Message { get; } = message;

    public WebhookActor Actor { get; } = actor;

    public WebhookActor? Target { get; } = target;

    public IReadOnlyList<(string Key, string Value)> Data { get; } = data;

    public bool WithIdentifiers { get; } = withIdentifiers;

    public bool IsWarning { get; } = warning;

    public string Line()
    {
        var line = new StringBuilder();
        var stamp = At.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        line.Append(Bullet);

        if (IsWarning)
        {
            line.Append(WarningMark);
        }

        line
            .Append("<t:").Append(stamp).Append(":d> ")
            .Append("<t:").Append(stamp).Append(":T> ");

        if (!Actor.IsServer)
        {
            line.Append(Actor.Line()).Append(' ');
        }

        line.Append(WebhookText.Clean(Message, MaxMessageLength));

        if (Target is { } target)
        {
            line.Append(' ').Append(target.Line());
        }

        Finish(line);

        var details = Details();

        return details.Length == 0 ? line.ToString() : line.Append(Subtext).Append(details).ToString();
    }

    private static void Finish(StringBuilder line)
    {
        if (line.Length == 0 || line[^1] is '.' or '!' or '?')
        {
            return;
        }

        line.Append('.');
    }

    private string Details()
    {
        var parts = new List<string>(Data.Count + 1);

        foreach (var (key, value) in Data)
        {
            parts.Add(WebhookText.Clean(key, MaxValueLength) + ": " + WebhookText.Clean(value, MaxValueLength));
        }

        if (WithIdentifiers && Actor.Identifiers() is { Length: > 0 } identifiers)
        {
            parts.Add(identifiers);
        }

        return string.Join(Separator, parts);
    }
}
