using vMenu.Enhanced.Data.Actions;

namespace vMenu.Enhanced.Actions;

/// <summary>
/// What the server answered, or what happened instead of an answer.
/// </summary>
public sealed class ActionResult(ActionStatus status, string[] data)
{
    public ActionStatus Status { get; } = status;

    public string[] Data { get; } = data;

    public bool IsOk => Status == ActionStatus.Ok;

    internal static ActionResult From(ActionStatus status) => new(status, []);
}
