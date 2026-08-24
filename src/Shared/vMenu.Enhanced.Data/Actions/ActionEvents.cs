namespace vMenu.Enhanced.Data.Actions;

public static class ActionEvents
{
    // Client to server: action id, request id, arguments (string, int, string[]).
    public const string Invoke = "vMenu.Enhanced:Action:Invoke";

    // Server to client: request id, ActionStatus, result data (int, int, string[]).
    public const string Result = "vMenu.Enhanced:Action:Result";
}
