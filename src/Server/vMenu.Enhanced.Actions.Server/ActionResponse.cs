using vMenu.Enhanced.Data.Actions;

namespace vMenu.Enhanced.Actions.Server;

/// <summary>
/// What a handler hands back to the dispatcher.
/// </summary>
public sealed class ActionResponse(ActionStatus status, string[] data)
{
    public ActionStatus Status { get; } = status;

    public string[] Data { get; } = data;

    public static ActionResponse Ok(params string[] data) => new(ActionStatus.Ok, data);

    public static ActionResponse Failed() => new(ActionStatus.Failed, []);

    public static ActionResponse Refused() => new(ActionStatus.Refused, []);

    public static ActionResponse InvalidRequest() => new(ActionStatus.InvalidRequest, []);

    public static ActionResponse NotFound() => new(ActionStatus.NotFound, []);

    public static ActionResponse TooFar() => new(ActionStatus.TooFar, []);
}
