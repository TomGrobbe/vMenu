using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

namespace vMenu.Enhanced.Webhooks.Server;

public sealed class WebhookActor
{
    private const string IpPrefix = "ip";

    private const string DiscordType = "discord";

    private const int MaxNameLength = 64;

    private readonly List<(string Type, string Value)> _identifiers = [];

    private WebhookActor(string name, int serverId)
    {
        Name = name;
        ServerId = serverId;
    }

    public string Name { get; }

    public int ServerId { get; }

    public string? Discord => Value(DiscordType);

    public IReadOnlyList<(string Type, string Value)> IdentifierList => _identifiers;

    public static WebhookActor Server { get; } = new("the server", 0);

    public bool IsServer => ServerId == 0;

    public static WebhookActor For(Player player) => For(player.Handle, player.Name);

    public static WebhookActor For(int serverId) => For(serverId, null);

    public static WebhookActor For(int serverId, string? name)
    {
        if (serverId <= 0)
        {
            return Server;
        }

        var handle = serverId.ToString(CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(name))
        {
            name = Native.GetPlayerName(handle);
        }

        var actor = new WebhookActor(
            string.IsNullOrWhiteSpace(name) ? "#" + handle : name,
            serverId);

        if (!Native.DoesPlayerExist(handle))
        {
            return actor;
        }

        var count = Native.GetNumPlayerIdentifiers(handle);

        for (var index = 0; index < count; index++)
        {
            actor.Take(Native.GetPlayerIdentifier(handle, index));
        }

        return actor;
    }

    public string Line()
    {
        if (IsServer)
        {
            return Name;
        }

        var line = "**" + WebhookText.Clean(Name, MaxNameLength) + "** ("
            + ServerId.ToString(CultureInfo.InvariantCulture) + ")";

        return Mention() is { } mention ? line + " " + mention : line;
    }

    // Empty allowed_mentions on every payload keeps this from pinging, so it just shows the name.
    private string? Mention() => Discord is { } discord ? "<@" + discord + ">" : null;

    public string Identifiers()
    {
        var parts = new List<string>(_identifiers.Count);

        foreach (var (type, value) in _identifiers)
        {
            parts.Add(type + ":" + value);
        }

        return string.Join(" · ", parts);
    }

    private string? Value(string type)
    {
        foreach (var (candidate, value) in _identifiers)
        {
            if (string.Equals(candidate, type, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

    private void Take(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return;
        }

        var colon = identifier.IndexOf(':');

        if (colon <= 0 || colon >= identifier.Length - 1)
        {
            return;
        }

        var type = identifier[..colon];

        if (string.Equals(type, IpPrefix, StringComparison.OrdinalIgnoreCase) || Value(type) is not null)
        {
            return;
        }

        _identifiers.Add((type, identifier[(colon + 1)..]));
    }
}
