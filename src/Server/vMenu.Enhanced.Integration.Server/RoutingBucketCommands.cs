using System.Collections.Concurrent;
using System.Text.Json;

using CitizenFX.FiveM.Server;

namespace vMenu.Enhanced.Integration.Server;

public static class RoutingBucketCommands
{
    private const string ServerCommandEvent = "vMenu.RoutingBucketsPlugin:ServerCommand";

    private const string ServerCommandResultEvent = "vMenu.RoutingBucketsPlugin:ServerCommandResult";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    private const int Ok = 0;
    private const int Denied = 1;
    private const int UnknownBucket = 2;
    private const int NameTaken = 3;
    private const int BadName = 4;
    private const int TooManyBuckets = 5;
    private const int CannotModifyDefault = 6;
    private const int BucketNotEmpty = 7;
    private const int UnknownPlayer = 8;
    private const int SaveFailed = 9;

    private static readonly HashSet<string> Actions =
    [
        "world-create", "world-rename", "world-settings", "world-delete",
        "world-move-one", "world-move-all", "world-move-ids", "world-empty", "world-transfer",
    ];

    private static readonly ConcurrentDictionary<string, IntegrationCommands.Pending> Pendings = new();

    private static bool _started;

    public static bool IsBucketAction(string action) => Actions.Contains(action);

    public static void Initialize()
    {
        if (_started)
        {
            return;
        }

        _started = true;

        API.OnEvent(ServerCommandResultEvent, new Action<string, int, string>(OnResult), false);
    }

    public static void Dispatch(IntegrationCommands.Pending pending, string action, JsonElement parameters)
    {
        var id = Guid.NewGuid().ToString("n");
        var paramsJson = parameters.ValueKind == JsonValueKind.Object ? parameters.GetRawText() : "{}";

        Pendings[id] = pending;

        _ = TimeoutAsync(id);

        API.EmitLocal(ServerCommandEvent, id, action, paramsJson);
    }

    private static async Task TimeoutAsync(string id)
    {
        await Task.Delay(Timeout);

        if (Pendings.TryRemove(id, out var pending))
        {
            pending.Completion.TrySetResult(new IntegrationCommands.CommandReply(503, IntegrationJson.Fail("plugin-unavailable")));
        }
    }

    private static void OnResult(string id, int outcome, string detail)
    {
        if (Pendings.TryRemove(id, out var pending))
        {
            pending.Completion.TrySetResult(Map(outcome));
        }
    }

    private static IntegrationCommands.CommandReply Map(int outcome) => outcome switch
    {
        Ok or SaveFailed => new IntegrationCommands.CommandReply(200, IntegrationJson.Ok),
        Denied => new IntegrationCommands.CommandReply(403, IntegrationJson.Fail("denied")),
        UnknownBucket => new IntegrationCommands.CommandReply(404, IntegrationJson.Fail("unknown-world")),
        NameTaken => new IntegrationCommands.CommandReply(409, IntegrationJson.Fail("name-taken")),
        BadName => new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("bad-name")),
        TooManyBuckets => new IntegrationCommands.CommandReply(409, IntegrationJson.Fail("too-many")),
        CannotModifyDefault => new IntegrationCommands.CommandReply(409, IntegrationJson.Fail("cannot-modify-main")),
        BucketNotEmpty => new IntegrationCommands.CommandReply(409, IntegrationJson.Fail("not-empty")),
        UnknownPlayer => new IntegrationCommands.CommandReply(404, IntegrationJson.Fail("unknown-player")),
        _ => new IntegrationCommands.CommandReply(400, IntegrationJson.Fail("failed")),
    };
}
