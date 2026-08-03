namespace vMenu.Enhanced.Data.Actions;

/// <summary>
/// Network events carrying every client requested server action.
/// </summary>
public static class ActionEvents
{
    /// <summary>Client to server: action id, request id, arguments (string, int, string[]).</summary>
    public const string Invoke = "vMenu.Enhanced:Action:Invoke";

    /// <summary>
    /// Server to client: request id, <see cref="ActionStatus"/>, result data (int, int, string[]).
    /// </summary>
    public const string Result = "vMenu.Enhanced:Action:Result";
}
