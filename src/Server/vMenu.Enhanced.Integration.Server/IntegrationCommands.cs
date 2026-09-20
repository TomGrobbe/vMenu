using System.Collections.Concurrent;
using System.Text.Json;

using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Logging;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

public static class IntegrationCommands
{
    public readonly record struct CommandReply(int Status, string Json);

    private const string OkJson = "{\"ok\":true}";

    public sealed class Pending(string body)
    {
        private volatile bool _abandoned;

        public string Body { get; } = body;

        public TaskCompletionSource<CommandReply> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<CommandReply> Reply => Completion.Task;

        public bool Abandoned => _abandoned;

        public void Abandon() => _abandoned = true;
    }

    private static readonly ConcurrentQueue<Pending> Queue = new();

    public static Pending Submit(string body)
    {
        var pending = new Pending(body);

        Queue.Enqueue(pending);

        return pending;
    }

    // Runs on the tick thread, so everything below may call natives.
    public static void Drain()
    {
        while (Queue.TryDequeue(out var pending))
        {
            if (pending.Abandoned)
            {
                continue;
            }

            try
            {
                Execute(pending);
            }
            catch (Exception exception)
            {
                Log.Error($"[Integration] A command failed: {exception}");

                pending.Completion.TrySetResult(new CommandReply(500, Fail("error")));
            }
        }
    }

    private static void Execute(Pending pending)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(pending.Body);
        }
        catch (JsonException)
        {
            pending.Completion.TrySetResult(new CommandReply(400, Fail("bad-request")));

            return;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                pending.Completion.TrySetResult(new CommandReply(400, Fail("bad-request")));

                return;
            }

            var action = IntegrationJson.ReadString(root, "action")?.Trim().ToLowerInvariant() ?? string.Empty;
            if (action.Length == 0)
            {
                pending.Completion.TrySetResult(new CommandReply(400, Fail("bad-request")));

                return;
            }

            if (RoutingBucketCommands.IsBucketAction(action))
            {
                if (!ServerConfig.Value(IntegrationSettings.AllowActions))
                {
                    pending.Completion.TrySetResult(new CommandReply(403, Fail("disabled")));

                    return;
                }

                var bucketParams = root.TryGetProperty("params", out var bp) ? bp : default;
                RoutingBucketCommands.Dispatch(pending, action, bucketParams);

                return;
            }

            if (RemoteServerCommands.IsServerAction(action))
            {
                // Runs before the document is disposed, so the params element stays valid.
                var parameters = root.TryGetProperty("params", out var p) ? p : default;
                pending.Completion.TrySetResult(RemoteServerCommands.Run(action, parameters));

                return;
            }

            if (!ServerConfig.Value(IntegrationSettings.AllowActions))
            {
                pending.Completion.TrySetResult(new CommandReply(403, Fail("disabled")));

                return;
            }

            if (!TryParseCommand(root, action, out var command))
            {
                pending.Completion.TrySetResult(new CommandReply(400, Fail("bad-request")));

                return;
            }

            pending.Completion.TrySetResult(RemotePlayerCommands.Run(command) switch
            {
                RemoteCommandOutcome.Ok => new CommandReply(200, OkJson),
                RemoteCommandOutcome.IdentityMismatch => new CommandReply(409, Fail("identity-mismatch")),
                RemoteCommandOutcome.NotReady => new CommandReply(409, Fail("not-ready")),
                RemoteCommandOutcome.UnknownAction => new CommandReply(400, Fail("unknown-action")),
                _ => new CommandReply(400, Fail("bad-request")),
            });
        }
    }

    private static bool TryParseCommand(JsonElement root, string action, out RemoteCommand command)
    {
        command = default;

        if (!root.TryGetProperty("target", out var target) || target.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var name = IntegrationJson.ReadString(target, "name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var discord = IntegrationJson.ReadString(target, "discord");

        float x = 0f;
        float y = 0f;
        var hasPoint = false;
        string? text = null;
        string? style = null;
        string? model = null;
        string? footer = null;

        if (root.TryGetProperty("params", out var parameters) && parameters.ValueKind == JsonValueKind.Object)
        {
            hasPoint = IntegrationJson.TryReadFloat(parameters, "x", out x)
                & IntegrationJson.TryReadFloat(parameters, "y", out y);
            text = IntegrationJson.ReadString(parameters, "text");
            style = IntegrationJson.ReadString(parameters, "style");
            model = IntegrationJson.ReadString(parameters, "model");
            footer = IntegrationJson.ReadString(parameters, "footer");
        }

        command = new RemoteCommand(
            action,
            IntegrationJson.ReadInt(target, "serverId"),
            name,
            string.IsNullOrWhiteSpace(discord) ? null : discord,
            IntegrationJson.ReadString(root, "operator") ?? string.Empty,
            x,
            y,
            text,
            style,
            model,
            hasPoint,
            footer);

        return true;
    }

    private static string Fail(string reason) => IntegrationJson.Fail(reason);
}
