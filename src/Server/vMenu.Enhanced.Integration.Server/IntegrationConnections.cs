using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

using CitizenFX.FiveM.Server;
using CitizenFX.FiveM.Shared.FuncRef;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.Players.Server;
using vMenu.Enhanced.Serialization.Server;

namespace vMenu.Enhanced.Integration.Server;

public static class IntegrationConnections
{
    public sealed class PendingConnection(
        string id,
        int handle,
        string name,
        FunctionReference update,
        FunctionReference done,
        FunctionReference? presentCard,
        string defaultReason)
    {
        public string Id { get; } = id;

        public int Handle { get; } = handle;

        public string Name { get; } = name;

        public FunctionReference Update { get; } = update;

        public FunctionReference Done { get; } = done;

        public FunctionReference? PresentCard { get; } = presentCard;

        public volatile bool CanSkip;

        public bool CardShown;

        public Action<object, object>? SkipCallback;

        public volatile int Revision;

        public volatile string Card = "Checking with the server...";

        public volatile bool Terminal;

        public volatile bool Admit;

        public volatile string Reason = defaultReason;

        public volatile bool SocketDrop;

        public volatile int DelaySeconds = -1;

        public long DelaySetAtTicks;

        public long LastCardTicks;

        public long LastActivityTicks;

        public long LastAliveCheckTicks;

        public int MissedAliveChecks;

        public int FailedCalls;

        public long RetryAtTicks;

        public int AppliedRevision;

        public string? ShownPlayers;

        public bool Finished;
    }

    private static readonly long HeartbeatTimeoutTicks = TimeSpan.FromSeconds(30).Ticks;

    // A slow connection can miss a check or a call now and then, so a player only loses their place after this
    // many in a row, about a second apart.
    private const int MaxMisses = 3;

    private static readonly ConcurrentDictionary<string, PendingConnection> Pending = new();

    private static readonly ConcurrentDictionary<int, string> ByHandle = new();

    public static PendingConnection Register(
        string id,
        int handle,
        string name,
        FunctionReference update,
        FunctionReference done,
        FunctionReference? presentCard,
        string defaultReason)
    {
        var pending = new PendingConnection(id, handle, name, update, done, presentCard, defaultReason)
        {
            LastActivityTicks = DateTime.UtcNow.Ticks,
        };

        Pending[id] = pending;
        ByHandle[handle] = id;

        return pending;
    }

    // A decision made without asking the integration, finished by the next Drain.
    public static void RegisterDecided(
        int handle, string name, FunctionReference update, FunctionReference done, FunctionReference? presentCard, string? refusal)
    {
        var pending = Register(Guid.NewGuid().ToString("N"), handle, name, update, done, presentCard, refusal ?? string.Empty);
        pending.Admit = refusal is null;
        pending.Terminal = true;
    }

    public static void Apply(string payload)
    {
        string? id;
        string state;
        string? card;
        string? reason;
        int? delaySeconds;
        bool canSkip;

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            id = IntegrationJson.ReadString(root, "id");
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            state = IntegrationJson.ReadString(root, "state")?.Trim().ToLowerInvariant() ?? string.Empty;
            card = BuildCard(root);
            reason = IntegrationJson.ReadString(root, "reason");
            delaySeconds = IntegrationJson.TryReadInt(root, "delaySeconds", out var d) ? d : null;
            canSkip = IntegrationJson.TryReadBool(root, "canSkip", out var cs) && cs;
        }
        catch (JsonException)
        {
            return;
        }

        if (!Pending.TryGetValue(id, out var pending))
        {
            return;
        }

        pending.LastActivityTicks = DateTime.UtcNow.Ticks;

        switch (state)
        {
            case "admit":
                pending.Admit = true;
                pending.Terminal = true;
                break;

            case "reject":
                pending.Admit = false;
                pending.Reason = string.IsNullOrWhiteSpace(reason) ? pending.Reason : reason!;
                pending.Terminal = true;
                break;

            default:
                if (!string.IsNullOrEmpty(card))
                {
                    pending.Card = card!;
                }

                if (delaySeconds is > 0)
                {
                    pending.DelaySeconds = delaySeconds.Value;
                    pending.DelaySetAtTicks = DateTime.UtcNow.Ticks;
                }
                else
                {
                    pending.DelaySeconds = -1;
                }

                pending.CanSkip = canSkip;

                break;
        }

        pending.Revision++;
    }

    private static string? BuildCard(JsonElement root)
    {
        var message = IntegrationJson.ReadString(root, "message");
        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        if (!IntegrationJson.TryReadInt(root, "position", out var position))
        {
            return null;
        }

        // Players only care where they stand overall, not which queue they are in.
        var ahead = IntegrationJson.TryReadInt(root, "ahead", out var a) && a > 0 ? a : 0;

        return $"You are number {position + ahead} in line.";
    }

    public static string? Cancel(int handle)
    {
        if (ByHandle.TryRemove(handle, out var id) && Pending.TryRemove(id, out var pending))
        {
            Dispose(pending);

            return id;
        }

        return null;
    }

    public static void ReleaseOne(PendingConnection pending, string reason)
    {
        pending.Admit = false;
        pending.Reason = reason;
        pending.SocketDrop = true;
        pending.Terminal = true;
        pending.Revision++;
    }

    public static void ReleaseAll(string reason)
    {
        foreach (var pending in Pending.Values)
        {
            pending.Admit = false;
            pending.Reason = reason;
            pending.SocketDrop = true;
            pending.Terminal = true;
            pending.Revision++;
        }
    }

    public static void Drain()
    {
        if (Pending.IsEmpty)
        {
            return;
        }

        var nowTicks = DateTime.UtcNow.Ticks;
        var players = PlayersLine();

        foreach (var pending in Pending.Values)
        {
            if (pending.Finished)
            {
                continue;
            }

            // A mandatory spike-protection delay is driven locally, so the manager stays quiet on purpose while
            // it ticks down. Don't treat that silence as an unresponsive manager, or a delay longer than the
            // heartbeat window would kick the player mid-countdown.
            var delayActive = pending.DelaySeconds >= 0
                && nowTicks - pending.DelaySetAtTicks < (long)pending.DelaySeconds * TimeSpan.TicksPerSecond;

            if (!pending.Terminal && !delayActive && nowTicks - pending.LastActivityTicks > HeartbeatTimeoutTicks)
            {
                pending.Admit = false;
                pending.Reason = "The server did not respond in time. Please try to join again.";
                pending.Terminal = true;

                // The integration may still place them later, so tell it they are gone or they hold a place forever.
                IntegrationSocket.TrySend("gate-cancel", ServerJson.Serialize(new { id = pending.Id }));
            }

            // playerDropped does not reliably fire for someone still on the loading screen, so a player who pressed
            // Cancel would otherwise hold their place in the integration's queue forever.
            if (!pending.Terminal && nowTicks - pending.LastAliveCheckTicks >= TimeSpan.TicksPerSecond)
            {
                pending.LastAliveCheckTicks = nowTicks;

                if (!string.IsNullOrEmpty(Native.GetPlayerEndpoint(pending.Handle.ToString(CultureInfo.InvariantCulture))))
                {
                    pending.MissedAliveChecks = 0;
                }
                else if (++pending.MissedAliveChecks >= MaxMisses)
                {
                    Abandon(pending);
                    continue;
                }
            }

            if (nowTicks < pending.RetryAtTicks)
            {
                continue;
            }

            try
            {
                if (pending.Terminal)
                {
                    pending.Finished = true;

                    if (pending.Admit)
                    {
                        pending.Done.CallVoid([]);
                    }
                    else
                    {
                        if (!pending.SocketDrop)
                        {
                            Log.Info($"[Integration] {pending.Name} was refused entry: {pending.Reason}");
                        }

                        pending.Done.CallVoid([pending.Reason]);
                    }

                    Remove(pending);
                    continue;
                }

                if (pending.CanSkip && pending.PresentCard is not null)
                {
                    var cardRevision = pending.Revision;
                    var cardFrameChanged = cardRevision != pending.AppliedRevision;
                    var cardCountdownDue = pending.DelaySeconds >= 0
                        && nowTicks - pending.LastCardTicks >= TimeSpan.TicksPerSecond;
                    var cardPlayersChanged = players != pending.ShownPlayers;

                    if (pending.CardShown && !cardFrameChanged && !cardCountdownDue && !cardPlayersChanged)
                    {
                        continue;
                    }

                    pending.CardShown = true;
                    pending.AppliedRevision = cardRevision;
                    pending.LastCardTicks = nowTicks;
                    pending.ShownPlayers = players;
                    PresentSkipCard(pending, nowTicks, players);
                    continue;
                }

                var revision = pending.Revision;
                var revisionChanged = revision != pending.AppliedRevision;
                var countdownDue = pending.DelaySeconds >= 0
                    && nowTicks - pending.LastCardTicks >= TimeSpan.TicksPerSecond;
                var playersChanged = players != pending.ShownPlayers;

                if (!revisionChanged && !countdownDue && !playersChanged)
                {
                    continue;
                }

                pending.AppliedRevision = revision;
                pending.LastCardTicks = nowTicks;
                pending.ShownPlayers = players;
                pending.Update.CallVoid([BuildDisplay(pending, nowTicks, players)]);
                pending.FailedCalls = 0;
            }
            catch (Exception exception)
            {
                Log.Debug($"[Integration] Driving a deferral failed ({pending.FailedCalls + 1}/{MaxMisses}): {exception.Message}");

                if (++pending.FailedCalls >= MaxMisses)
                {
                    Abandon(pending);
                    continue;
                }

                // Try the same call again in a second: a final decision is still pending, and the screen redraws.
                pending.Finished = false;
                pending.CardShown = false;
                pending.AppliedRevision = -1;
                pending.ShownPlayers = null;
                pending.RetryAtTicks = nowTicks + TimeSpan.TicksPerSecond;
            }
        }
    }

    // The player is gone without a decision, so free their place with the integration too.
    private static void Abandon(PendingConnection pending)
    {
        pending.Finished = true;
        Remove(pending);

        IntegrationSocket.TrySend("gate-cancel", ServerJson.Serialize(new { id = pending.Id }));
        Log.Debug($"[Integration] {pending.Name} left the loading screen, so their place was given up.");
    }

    private static string PlayersLine()
    {
        var count = ConnectedPlayers.All().Count;
        var max = IntegrationStatus.MaxClients();

        return max > 0 ? $"Players online: {count}/{max}" : $"Players online: {count}";
    }

    // Failures reach the caller, so a card that did not arrive is retried like any other update.
    private static void PresentSkipCard(PendingConnection pending, long nowTicks, string players)
    {
        if (pending.SkipCallback is null)
        {
            var id = pending.Id;
            pending.SkipCallback = new Action<object, object>((_, _) => OnSkip(id));
        }

        pending.PresentCard!.CallVoid([BuildSkipCard(pending, nowTicks, players), pending.SkipCallback]);
        pending.FailedCalls = 0;
    }

    private static void OnSkip(string id) =>
        IntegrationSocket.TrySend("skip-queue", "{\"id\":\"" + id + "\"}");

    private static string BuildSkipCard(PendingConnection pending, long nowTicks, string players)
    {
        // A card TextBlock does not reliably break on a single newline, so each line gets its own block.
        var body = string.Join(",", pending.Card.Split('\n').Select(line => TextBlock(line)));

        if (RemainingDelay(pending, nowTicks) is { } remaining)
        {
            body += "," + TextBlock(DelayLine(remaining));
        }

        body += "," + TextBlock(players);

        body += "," + TextBlock(
            "You have been given queue and join spike protection bypass permissions.", heading: true, separator: true);
        body += "," + TextBlock("Press Skip Queue to join now, or wait to keep your place in line.");

        return "{\"type\":\"AdaptiveCard\",\"version\":\"1.0\","
            + "\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"body\":[" + body
            + "],\"actions\":[{\"type\":\"Action.Submit\",\"title\":\"Skip Queue\",\"data\":{\"action\":\"skip\"}}]}";
    }

    private static string TextBlock(string text, bool heading = false, bool separator = false) =>
        "{\"type\":\"TextBlock\",\"wrap\":true,\"text\":\"" + JsonEscape(text) + "\""
        + (heading ? ",\"weight\":\"Bolder\",\"size\":\"Medium\"" : "")
        + (separator ? ",\"separator\":true,\"spacing\":\"Medium\"" : "")
        + "}";

    private static string JsonEscape(string text) => text
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\t", "\\t");

    private static string BuildDisplay(PendingConnection pending, long nowTicks, string players) =>
        RemainingDelay(pending, nowTicks) is { } remaining
            ? pending.Card + "\n" + DelayLine(remaining) + "\n" + players
            : pending.Card + "\n" + players;

    private static int? RemainingDelay(PendingConnection pending, long nowTicks)
    {
        var delay = pending.DelaySeconds;
        if (delay < 0)
        {
            return null;
        }

        var elapsed = (int)((nowTicks - pending.DelaySetAtTicks) / TimeSpan.TicksPerSecond);
        var remaining = delay - elapsed;
        if (remaining > 0)
        {
            return remaining;
        }

        pending.DelaySeconds = -1;

        return null;
    }

    // Called an estimate on purpose: a server filling up or staff changes can still move it.
    private static string DelayLine(int seconds) =>
        "Estimated wait due to join spike protection: " + FormatDuration(seconds);

    private static string FormatDuration(int seconds)
    {
        var minutes = seconds / 60;
        var secs = seconds % 60;

        return minutes > 0
            ? $"{minutes} minute{(minutes == 1 ? "" : "s")} {secs} second{(secs == 1 ? "" : "s")}"
            : $"{secs} second{(secs == 1 ? "" : "s")}";
    }

    private static void Remove(PendingConnection pending)
    {
        Pending.TryRemove(pending.Id, out _);

        if (ByHandle.TryGetValue(pending.Handle, out var current) && current == pending.Id)
        {
            ByHandle.TryRemove(pending.Handle, out _);
        }

        Dispose(pending);
    }

    private static void Dispose(PendingConnection pending)
    {
        try
        {
            pending.Update.Dispose();
            pending.Done.Dispose();
            pending.PresentCard?.Dispose();
        }
        catch (Exception exception)
        {
            Log.Debug($"[Integration] Disposing a deferral reference failed: {exception.Message}");
        }
    }
}
