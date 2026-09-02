using System.Globalization;
using System.Text;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Server.Entities;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Logging;
using vMenu.Enhanced.Logging;

using LoggingSettings = vMenu.Enhanced.Data.Configuration.Settings.Logging;

namespace vMenu.Enhanced.Webhooks.Server;

public static class MenuAuditHandler
{
    private const string StopEvent = "onResourceStop";

    private const string DroppedEvent = "playerDropped";

    private const int MaxMenuLength = 64;

    private const int MaxItemLength = 96;

    private const int MaxValueLength = 64;

    private const int MaxDetailLength = 250;

    private static readonly HashSet<string> ReportedThemes = new(StringComparer.OrdinalIgnoreCase);

    private static readonly MenuAuditLimit Limit = new();

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        API.OnNetEvent(AuditEvents.Menu, new Action<Player, string, string, string, string>(OnMenuAction), false);
        API.OnNetEvent(AuditEvents.Action, new Action<Player, string, string, string>(OnClientAction), false);
        API.OnNetEvent(AuditEvents.Theme, new Action<Player, string, string, string>(OnTheme), false);

        API.OnEvent(StopEvent, new Action<string>(OnResourceStop), false);
        API.OnEvent(DroppedEvent, new Action<int, string?>(OnPlayerDropped), false);
    }

    // Shared with the plugin audit handler, so a plugin's rows cannot hand a player a second allowance.
    public static bool TryTakeAuditSlot(int serverId) => Limit.TryTake(serverId);

    public static string PhraseFor(string kind, string label, string value) => Phrase(kind, label, value);

    private static void OnMenuAction([FromSource] Player source, string menu, string item, string kind, string value)
    {
        if ((!WebhookLog.WantsMenuActions && !WebhookLog.WantsSecurity) || !Limit.TryTake(source.Handle))
        {
            return;
        }

        var itemKey = WebhookText.Clean(item, MaxItemLength);

        if (itemKey.Length == 0)
        {
            return;
        }

        // The player's own game filters against this same table in this same build, so a key it would
        // never send can only have come from a modified client.
        if (!AuditedMenuItems.Includes(itemKey))
        {
            WebhookLog.Security(
                WebhookActor.For(source),
                "reported a menu action that vMenu never logs.",
                ("item", itemKey));

            return;
        }

        if (!WebhookLog.WantsMenuActions)
        {
            return;
        }

        var message = Phrase(kind, AuditedMenuItems.LabelFor(itemKey), WebhookText.Clean(value, MaxValueLength));

        WebhookLog.Action(
            WebhookActor.For(source),
            message,
            ("menu", MenuName(WebhookText.Clean(menu, MaxMenuLength))));
    }

    private static void OnClientAction([FromSource] Player source, string action, string value, string detail)
    {
        if ((!WebhookLog.WantsMenuActions && !WebhookLog.WantsSecurity) || !Limit.TryTake(source.Handle))
        {
            return;
        }

        // Every call site passes an AuditActions constant, so anything else is a modified client.
        if (PhraseAction(action, WebhookText.Clean(value, MaxValueLength)) is not { } message)
        {
            WebhookLog.Security(
                WebhookActor.For(source),
                "reported an action that vMenu never logs.",
                ("action", WebhookText.Clean(action, MaxItemLength)));

            return;
        }

        if (!WebhookLog.WantsMenuActions)
        {
            return;
        }

        var extra = WebhookText.Clean(detail, MaxDetailLength);

        if (extra.Length == 0)
        {
            WebhookLog.Action(WebhookActor.For(source), message);

            return;
        }

        WebhookLog.Action(WebhookActor.For(source), message, (DetailKey(action), extra));
    }

    private static string DetailKey(string action) => action switch
    {
        AuditActions.LoadoutEquipped => "weapons",
        AuditActions.VehicleModsChanged => "mods",
        _ => "detail",
    };

    private static string? PhraseAction(string action, string value) => action switch
    {
        AuditActions.VehicleSpawned => value.Length == 0
            ? "spawned a vehicle."
            : "spawned the vehicle '" + value + "'.",
        AuditActions.TeleportWaypoint => "teleported to their waypoint.",
        AuditActions.TeleportCoords => value.Length == 0
            ? "teleported to coordinates they typed in."
            : "teleported to " + value + ".",
        AuditActions.TeleportLocation => value.Length == 0
            ? "teleported to a saved location."
            : "teleported to '" + value + "'.",
        AuditActions.LoadoutEquipped => value.Length == 0
            ? "equipped a weapon loadout."
            : "equipped the weapon loadout '" + value + "'.",
        AuditActions.VehicleModsChanged => value.Length == 0
            ? "changed the mods on their vehicle."
            : "changed the mods on their " + value + ".",
        _ => null,
    };

    private static string Phrase(string kind, string label, string value) => kind switch
    {
        MenuActionKinds.Checkbox => "turned " + label + " " + (value == "1" ? "on." : "off."),
        MenuActionKinds.List or MenuActionKinds.DynamicList or MenuActionKinds.Slider =>
            value.Length == 0 ? "changed " + label + "." : "set " + label + " to " + value + ".",
        _ => label + ".",
    };

    private static string MenuName(string auditName)
    {
        if (auditName.Length == 0)
        {
            return "unknown";
        }

        if (auditName.EndsWith("Menu", StringComparison.Ordinal) && auditName.Length > 4)
        {
            auditName = auditName[..^4];
        }

        var name = new StringBuilder(auditName.Length + 4);

        foreach (var character in auditName)
        {
            if (char.IsUpper(character) && name.Length > 0)
            {
                name.Append(' ').Append(char.ToLowerInvariant(character));

                continue;
            }

            name.Append(character);
        }

        return name.ToString();
    }

    private static void OnTheme([FromSource] Player source, string resource, string themeId, string themeName)
    {
        if (!WebhookLog.WantsMenuActions)
        {
            return;
        }

        var owner = WebhookText.Clean(resource, MaxMenuLength);
        var id = WebhookText.Clean(themeId, MaxItemLength);

        if (owner.Length == 0 || id.Length == 0 || !ReportedThemes.Add(owner + ":" + id))
        {
            return;
        }

        WebhookLog.Event(
            $"Menu theme '{WebhookText.Clean(themeName, MaxItemLength)}' registered.",
            WebhookActor.Server,
            ("resource", owner),
            ("theme", id));
    }

    private static void OnResourceStop(string stopped)
    {
        if (ReportedThemes.Count == 0)
        {
            return;
        }

        var prefix = stopped + ":";

        ReportedThemes.RemoveWhere(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static void OnPlayerDropped([FromSource] int source, string? reason = null)
    {
        if (source > 0)
        {
            Limit.Forget(source);
            SecurityThrottle.Forget(source);
        }
    }

    private sealed class MenuAuditLimit
    {
        private readonly Dictionary<int, List<int>> _byPlayer = [];

        private bool _reported;

        private int _reportedAt;

        public bool TryTake(int serverId)
        {
            var limit = ServerConfig.Value(LoggingSettings.MenuActionLimit);
            var window = ServerConfig.Value(LoggingSettings.MenuActionLimitSeconds) * 1000;

            if (limit <= 0 || window <= 0)
            {
                if (_byPlayer.Count > 0)
                {
                    _byPlayer.Clear();
                }

                return true;
            }

            var now = Native.GetGameTimer();

            Sweep(now, window);

            if (!_byPlayer.TryGetValue(serverId, out var stamps))
            {
                stamps = [];

                _byPlayer[serverId] = stamps;
            }

            if (stamps.Count < limit)
            {
                stamps.Add(now);

                return true;
            }

            ReportOnce(serverId, now, limit, window);

            return false;
        }

        public void Forget(int serverId) => _byPlayer.Remove(serverId);

        private void ReportOnce(int serverId, int now, int limit, int window)
        {
            if (_reported && now - _reportedAt < window)
            {
                return;
            }

            _reported = true;
            _reportedAt = now;

            var seconds = (window / 1000).ToString(CultureInfo.InvariantCulture);

            Log.Info(
                $"[Webhooks] Player {serverId} reported more than {limit} menu action(s) in {window / 1000}s. "
                + "The rest are being ignored.");

            WebhookLog.Warn(
                WebhookActor.For(serverId),
                "went over the menu action limit, so the rest of what they do is not being logged.",
                ("limit", limit.ToString(CultureInfo.InvariantCulture) + " actions per " + seconds + "s"));
        }

        private void Sweep(int now, int window)
        {
            List<int>? finished = null;

            foreach (var pair in _byPlayer)
            {
                var stamps = pair.Value;

                while (stamps.Count > 0 && now - stamps[0] >= window)
                {
                    stamps.RemoveAt(0);
                }

                if (stamps.Count == 0)
                {
                    finished ??= [];

                    finished.Add(pair.Key);
                }
            }

            if (finished is null)
            {
                return;
            }

            foreach (var serverId in finished)
            {
                _byPlayer.Remove(serverId);
            }
        }
    }
}
