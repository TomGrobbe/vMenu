using System.Text.Json.Serialization;

using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Webhooks.Server;

public static class DiscordPayload
{
    public const int MaxDescription = 3900;

    public static string Build(LogCategory category, string description, string footer) =>
        ServerJson.Serialize(new DiscordMessage
        {
            Username = "vMenu",
            Embeds =
            [
                new DiscordEmbed
                {
                    Title = LogCategories.TitleOf(category),
                    Description = description,
                    Color = LogCategories.ColourOf(category),
                    Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                    Footer = new DiscordFooter { Text = footer },
                },
            ],
        });

    public static float? RetryAfter(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return null;
        }

        return ServerJson.TryDeserialize<DiscordRateLimit>(body, out var limit, out _)
            ? limit?.RetryAfter
            : null;
    }
}

public sealed class DiscordMessage
{
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("allowed_mentions")]
    public DiscordAllowedMentions AllowedMentions { get; set; } = new();

    public List<DiscordEmbed> Embeds { get; set; } = [];
}

public sealed class DiscordAllowedMentions
{
    public List<string> Parse { get; set; } = [];
}

public sealed class DiscordEmbed
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Color { get; set; }

    public string Timestamp { get; set; } = string.Empty;

    public DiscordFooter? Footer { get; set; }
}

public sealed class DiscordFooter
{
    public string Text { get; set; } = string.Empty;
}

public sealed class DiscordRateLimit
{
    [JsonPropertyName("retry_after")]
    public float RetryAfter { get; set; }
}
