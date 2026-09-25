using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.Serialization;

using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Permissions.Server;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;
using vMenu.Enhanced.Ticks.Server;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

// The integration says which ACE principals an identifier should have. vMenu adds and removes only the
// ones it added itself, and remembers them in KVP because principals outlive a resource restart.
public static class IntegrationRoleSync
{
    private const string JoiningEvent = "playerJoining";

    private const string DroppedEvent = "playerDropped";

    private const string StopEvent = "onResourceStop";

    private const string KeyPrefix = "vMenu.Enhanced:RoleSync:";

    private const int PlayersPerFrame = 200;

    private const int MaxValueLength = 64;

    private const int MaxTypeLength = 32;

    private const int MaxPrincipalLength = 64;

    private static readonly ConcurrentQueue<string> Incoming = new();

    private static readonly Dictionary<string, List<string>> Tracked = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<int, HashSet<string>> TrackedByPlayer = [];

    private static volatile bool _requestAll;

    private static bool _registered;

    public static void Initialize()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;

        ClearLeftovers();

        API.OnEvent(JoiningEvent, new Action<int>(OnJoining), false);
        API.OnEvent(DroppedEvent, new Action<int>(OnDropped), false);
        API.OnEvent(StopEvent, new Action<string>(OnResourceStop), false);

        ServerTickRegistry.Register("integration:rolesync", Drain, TickRate.Every(100), condition: () => IntegrationAuth.IsConfigured);

        if (ServerConfig.Value(IntegrationSettings.AllowActions) && !PrincipalCommandsAllowed())
        {
            var resource = Native.GetCurrentResourceName();

            Log.Warning(
                $"[Integration] Role sync cannot change permission groups until you add these two lines to your permissions.cfg: " +
                $"'add_ace resource.{resource} command.add_principal allow' and 'add_ace resource.{resource} command.remove_principal allow'.");
        }
    }

    public static bool PrincipalCommandsAllowed()
    {
        var principal = "resource." + Native.GetCurrentResourceName();

        return Native.IsPrincipalAceAllowed(principal, "command.add_principal")
            && Native.IsPrincipalAceAllowed(principal, "command.remove_principal");
    }

    // Socket thread: only queue, the tick does the game calls.
    public static void Submit(string payload) => Incoming.Enqueue(payload);

    public static void RequestAll() => _requestAll = true;

    private static bool Enabled => ServerConfig.Value(IntegrationSettings.AllowActions);

    private static void Drain()
    {
        if (_requestAll)
        {
            _requestAll = false;

            if (Enabled && IntegrationSocket.IsConnected)
            {
                SendRequest(ConnectedPlayers.All().Select(static p => p.ServerId));
            }
        }

        while (Incoming.TryDequeue(out var payload))
        {
            if (!Enabled)
            {
                continue;
            }

            try
            {
                Apply(payload);
            }
            catch (Exception exception)
            {
                Log.Error($"[Integration] Applying a role sync failed: {exception}");
            }
        }
    }

    private static void OnJoining([FromSource] int source)
    {
        if (source > 0 && Enabled && IntegrationSocket.IsConnected)
        {
            SendRequest([source]);
        }
    }

    private static void SendRequest(IEnumerable<int> serverIds)
    {
        var types = IntegrationSocket.RequestedIdentifiers;
        List<RequestPlayer> batch = [];

        foreach (var serverId in serverIds)
        {
            var identifiers = PlayerIdentifiers.Collect(serverId.ToString(CultureInfo.InvariantCulture), types);
            identifiers.Remove("ip");

            if (identifiers.Count == 0)
            {
                continue;
            }

            batch.Add(new RequestPlayer { ServerId = serverId, Identifiers = identifiers });

            if (batch.Count >= PlayersPerFrame)
            {
                IntegrationSocket.TrySend(IntegrationSocket.TypeRoleSyncRequest, ServerJson.Serialize(new { players = batch }));
                batch = [];
            }
        }

        if (batch.Count > 0)
        {
            IntegrationSocket.TrySend(IntegrationSocket.TypeRoleSyncRequest, ServerJson.Serialize(new { players = batch }));
        }
    }

    private static void Apply(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("players", out var players)
            || players.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var droppedPrincipals = false;

        foreach (var entry in players.EnumerateArray())
        {
            if (!IntegrationJson.TryReadInt(entry, "serverId", out var serverId)
                || serverId <= 0
                || IntegrationJson.ReadString(entry, "identifier") is not { } identifier
                || !IsValidIdentifier(identifier)
                || !entry.TryGetProperty("principals", out var principals)
                || principals.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var handle = serverId.ToString(CultureInfo.InvariantCulture);
            if (!Native.DoesPlayerExist(handle) || PlayerIdentifiers.Find(handle, identifier) is not { } own)
            {
                continue;
            }

            var desired = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var item in principals.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String || item.GetString() is not { } principal || !IsValidPrincipal(principal))
                {
                    droppedPrincipals = true;
                    continue;
                }

                if (seen.Add(principal))
                {
                    desired.Add(principal);
                }
            }

            if (Sync(own, desired))
            {
                PermissionsSync.RefreshOne(serverId);
            }

            if (desired.Count > 0)
            {
                Remember(serverId, own);
            }
        }

        if (droppedPrincipals)
        {
            Log.Warning("[Integration] Role sync sent principal names that are not allowed, so those were skipped.");
        }
    }

    private static bool Sync(string identifier, List<string> desired)
    {
        var current = Tracked.TryGetValue(identifier, out var existing) ? existing : [];
        var changed = false;

        foreach (var principal in desired)
        {
            if (!current.Contains(principal, StringComparer.Ordinal))
            {
                Native.ExecuteCommand($"add_principal identifier.{identifier} {principal}");
                changed = true;
            }
        }

        foreach (var principal in current)
        {
            if (!desired.Contains(principal, StringComparer.Ordinal))
            {
                Native.ExecuteCommand($"remove_principal identifier.{identifier} {principal}");
                changed = true;
            }
        }

        if (!changed)
        {
            return false;
        }

        var key = KeyPrefix + identifier;

        if (desired.Count == 0)
        {
            Tracked.Remove(identifier);
            Native.DeleteResourceKvp(key);
        }
        else
        {
            Tracked[identifier] = desired;
            Native.SetResourceKvp(key, string.Join('\n', desired));
        }

        Log.Debug($"[Integration] Role sync set {identifier} to [{string.Join(", ", desired)}].");

        return true;
    }

    private static void Remember(int serverId, string identifier)
    {
        if (!TrackedByPlayer.TryGetValue(serverId, out var identifiers))
        {
            identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            TrackedByPlayer[serverId] = identifiers;
        }

        identifiers.Add(identifier);
    }

    private static void OnDropped([FromSource] int source)
    {
        if (!TrackedByPlayer.Remove(source, out var identifiers))
        {
            return;
        }

        var others = ConnectedPlayers.All().Where(p => p.ServerId != source).ToList();

        foreach (var identifier in identifiers)
        {
            if (!Tracked.ContainsKey(identifier))
            {
                continue;
            }

            // Somebody else with the same identifier keeps the groups, and takes over the cleanup.
            var holder = others.FirstOrDefault(p =>
                PlayerIdentifiers.Find(p.ServerId.ToString(CultureInfo.InvariantCulture), identifier) is not null);

            if (holder is not null)
            {
                Remember(holder.ServerId, identifier);
                continue;
            }

            Sync(identifier, []);
        }
    }

    // The KVP goes too, because permissions.cfg has already run by the next start, and clearing leftovers then
    // would strip a hand-given group that matches one role sync had added. Only a crash leaves KVP behind.
    private static void OnResourceStop(string stopped)
    {
        if (!string.Equals(stopped, Native.GetCurrentResourceName(), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (var (identifier, principals) in Tracked)
        {
            foreach (var principal in principals)
            {
                Native.ExecuteCommand($"remove_principal identifier.{identifier} {principal}");
            }

            Native.DeleteResourceKvp(KeyPrefix + identifier);
        }

        Tracked.Clear();
    }

    private static void ClearLeftovers()
    {
        List<string> keys = [];
        var handle = Native.StartFindKvp(KeyPrefix);

        if (handle != -1)
        {
            try
            {
                while (Native.FindKvp(handle) is { Length: > 0 } key)
                {
                    if (key.StartsWith(KeyPrefix, StringComparison.Ordinal))
                    {
                        keys.Add(key);
                    }
                }
            }
            finally
            {
                Native.EndFindKvp(handle);
            }
        }

        foreach (var key in keys)
        {
            var identifier = key[KeyPrefix.Length..];
            var stored = Native.GetResourceKvpString(key) ?? string.Empty;

            // Re-validated because these values go straight into a console command.
            if (IsSafeIdentifier(identifier))
            {
                foreach (var principal in stored.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (IsValidPrincipal(principal))
                    {
                        Native.ExecuteCommand($"remove_principal identifier.{identifier} {principal}");
                    }
                }
            }

            Native.DeleteResourceKvp(key);
        }

        if (keys.Count > 0)
        {
            Log.Debug($"[Integration] Role sync cleared {keys.Count} identifier(s) left over from the last run.");
        }
    }

    private static bool IsValidIdentifier(string identifier)
    {
        if (!IsSafeIdentifier(identifier))
        {
            return false;
        }

        var type = identifier[..identifier.IndexOf(':')];

        return IntegrationSocket.RequestedIdentifiers.Any(t => t.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSafeIdentifier(string identifier)
    {
        var colon = identifier.IndexOf(':');
        if (colon <= 0)
        {
            return false;
        }

        var type = identifier[..colon];
        var value = identifier[(colon + 1)..];

        return type.Length <= MaxTypeLength
            && !type.Equals("ip", StringComparison.OrdinalIgnoreCase)
            && value.Length is > 0 and <= MaxValueLength
            && type.All(char.IsAsciiLetterOrDigit)
            && value.All(char.IsAsciiLetterOrDigit);
    }

    private static bool IsValidPrincipal(string principal) =>
        principal.Length is > 0 and <= MaxPrincipalLength
        && principal.All(static c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-' or ':');

    private sealed class RequestPlayer
    {
        public int ServerId { get; init; }

        public Dictionary<string, string> Identifiers { get; init; } = [];
    }
}
