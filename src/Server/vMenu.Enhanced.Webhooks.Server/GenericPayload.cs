using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Webhooks.Server;

public static class GenericPayload
{
    public static string Build(IReadOnlyList<WebhookEntry> batch)
    {
        var events = new List<GenericEvent>(batch.Count);

        foreach (var entry in batch)
        {
            events.Add(Describe(entry));
        }

        return ServerJson.Serialize(new GenericBatch
        {
            Resource = WebhookIdentity.Resource,
            Version = WebhookIdentity.Version,
            SentAt = DateTimeOffset.UtcNow.ToString("o"),
            Events = events,
        });
    }

    private static GenericEvent Describe(WebhookEntry entry)
    {
        var data = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, value) in entry.Data)
        {
            data[key] = value;
        }

        return new GenericEvent
        {
            Category = LogCategories.NameOf(entry.Category),
            Message = entry.Message,
            Timestamp = entry.At.ToString("o"),
            Actor = Describe(entry.Actor),
            Targets = entry.Target is { } target ? [Describe(target)] : [],
            Data = data,
        };
    }

    private static GenericParty Describe(WebhookActor actor) => new()
    {
        Name = actor.Name,
        ServerId = actor.ServerId,
        Identifiers = new GenericIdentifiers
        {
            Discord = actor.Discord,
            Steam = actor.Steam,
            License = actor.License,
            License2 = actor.License2,
        },
    };
}

public sealed class GenericBatch
{
    public string Resource { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string SentAt { get; set; } = string.Empty;

    public List<GenericEvent> Events { get; set; } = [];
}

public sealed class GenericEvent
{
    public string Category { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Timestamp { get; set; } = string.Empty;

    public GenericParty? Actor { get; set; }

    public List<GenericParty> Targets { get; set; } = [];

    public Dictionary<string, string> Data { get; set; } = new(StringComparer.Ordinal);
}

public sealed class GenericParty
{
    public string Name { get; set; } = string.Empty;

    public int ServerId { get; set; }

    public GenericIdentifiers Identifiers { get; set; } = new();
}

public sealed class GenericIdentifiers
{
    public string? Discord { get; set; }

    public string? Steam { get; set; }

    public string? License { get; set; }

    public string? License2 { get; set; }
}
