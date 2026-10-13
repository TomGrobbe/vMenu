using System.Collections.Concurrent;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using vMenu.Enhanced.Actions.Server.Handlers;
using vMenu.Enhanced.Configuration.Server;
using vMenu.Enhanced.Logging;

using IntegrationSettings = vMenu.Enhanced.Data.Configuration.Settings.Integration;

namespace vMenu.Enhanced.Integration.Server;

public static class IntegrationCommands
{
    public readonly record struct CommandReply(int Status, string Json);

    private const string OkJson = "{\"ok\":true}";

    private const string StatusAction = "status";

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
        JToken document;
        try
        {
            document = IntegrationJson.Parse(pending.Body);
        }
        catch (JsonException)
        {
            pending.Completion.TrySetResult(new CommandReply(400, Fail("bad-request")));

            return;
        }

        if (document is not JObject root)
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

            var bucketParams = root.TryGetValue("params", StringComparison.Ordinal, out var bp) ? bp : null;
            RoutingBucketCommands.Dispatch(pending, action, bucketParams);

            return;
        }

        if (RemoteServerCommands.IsServerAction(action))
        {
            var parameters = root.TryGetValue("params", StringComparison.Ordinal, out var p) ? p : null;
            pending.Completion.TrySetResult(RemoteServerCommands.Run(action, parameters));

            return;
        }

        if (action != StatusAction && !ServerConfig.Value(IntegrationSettings.AllowActions))
        {
            pending.Completion.TrySetResult(new CommandReply(403, Fail("disabled")));

            return;
        }

        if (!TryParseCommand(root, action, out var command))
        {
            pending.Completion.TrySetResult(new CommandReply(400, Fail("bad-request")));

            return;
        }

        if (RemotePlayerCommands.IsAsync(action))
        {
            _ = RunAsync(pending, command);

            return;
        }

        pending.Completion.TrySetResult(Reply(new RemoteCommandResult(RemotePlayerCommands.Run(command))));
    }

    private static async Task RunAsync(Pending pending, RemoteCommand command)
    {
        try
        {
            pending.Completion.TrySetResult(Reply(await RemotePlayerCommands.RunAsync(command)));
        }
        catch (Exception exception)
        {
            Log.Error($"[Integration] A command failed: {exception}");

            pending.Completion.TrySetResult(new CommandReply(500, Fail("error")));
        }
    }

    private static CommandReply Reply(RemoteCommandResult result) => result.Outcome switch
    {
        RemoteCommandOutcome.Ok => new CommandReply(200, result.Json ?? OkJson),
        RemoteCommandOutcome.IdentityMismatch => new CommandReply(409, Fail("identity-mismatch")),
        RemoteCommandOutcome.NotReady => new CommandReply(409, Fail("not-ready")),
        RemoteCommandOutcome.NoVehicle => new CommandReply(409, Fail("no-vehicle")),
        RemoteCommandOutcome.GodMode => new CommandReply(409, Fail("god-mode")),
        RemoteCommandOutcome.Failed => new CommandReply(500, Fail("failed")),
        RemoteCommandOutcome.UnknownAction => new CommandReply(400, Fail("unknown-action")),
        _ => new CommandReply(400, Fail("bad-request")),
    };

    private static bool TryParseCommand(JObject root, string action, out RemoteCommand command)
    {
        command = default;

        if (!root.TryGetValue("target", StringComparison.Ordinal, out var target) || target.Type != JTokenType.Object)
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
        float? z = null;
        float? heading = null;
        string? text = null;
        string? style = null;
        string? model = null;
        string? footer = null;

        if (root.TryGetValue("params", StringComparison.Ordinal, out var parameters) && parameters.Type == JTokenType.Object)
        {
            hasPoint = IntegrationJson.TryReadFloat(parameters, "x", out x)
                & IntegrationJson.TryReadFloat(parameters, "y", out y);
            z = IntegrationJson.TryReadFloat(parameters, "z", out var pz) ? pz : null;
            heading = IntegrationJson.TryReadFloat(parameters, "heading", out var ph) ? ph : null;
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
            footer,
            z,
            heading);

        return true;
    }

    private static string Fail(string reason) => IntegrationJson.Fail(reason);
}
