using System.Globalization;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;

namespace vMenu.Enhanced.Webhooks.Server;

public sealed class WebhookActor
{
    private const string DiscordPrefix = "discord:";

    private const string SteamPrefix = "steam:";

    private const string LicensePrefix = "license:";

    private const string License2Prefix = "license2:";

    private const int MaxNameLength = 64;

    private WebhookActor(string name, int serverId)
    {
        Name = name;
        ServerId = serverId;
    }

    public string Name { get; }

    public int ServerId { get; }

    public string? Discord { get; private set; }

    public string? Steam { get; private set; }

    public string? License { get; private set; }

    public string? License2 { get; private set; }

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

        return "**" + WebhookText.Clean(Name, MaxNameLength) + "** ("
            + ServerId.ToString(CultureInfo.InvariantCulture) + ")";
    }

    public string Identifiers()
    {
        var parts = new List<string>(4);

        if (Discord is { } discord)
        {
            parts.Add(DiscordPrefix + discord);
        }

        if (Steam is { } steam)
        {
            parts.Add(SteamPrefix + steam);
        }

        if (License is { } license)
        {
            parts.Add(LicensePrefix + license);
        }

        if (License2 is { } license2)
        {
            parts.Add(License2Prefix + license2);
        }

        return string.Join(" \u00b7 ", parts);
    }

    private void Take(string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return;
        }

        if (identifier.StartsWith(DiscordPrefix, StringComparison.OrdinalIgnoreCase))
        {
            Discord ??= identifier[DiscordPrefix.Length..];
        }
        else if (identifier.StartsWith(SteamPrefix, StringComparison.OrdinalIgnoreCase))
        {
            Steam ??= identifier[SteamPrefix.Length..];
        }
        else if (identifier.StartsWith(License2Prefix, StringComparison.OrdinalIgnoreCase))
        {
            License2 ??= identifier[License2Prefix.Length..];
        }
        else if (identifier.StartsWith(LicensePrefix, StringComparison.OrdinalIgnoreCase))
        {
            License ??= identifier[LicensePrefix.Length..];
        }
    }
}
