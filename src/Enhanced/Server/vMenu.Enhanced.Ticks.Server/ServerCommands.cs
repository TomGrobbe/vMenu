using CitizenFX.FiveM.Shared;
using CitizenFX.FiveM.Shared.Serialization;

namespace vMenu.Enhanced.Ticks.Server;

public static class ServerCommands
{
    // AsyncLocal so lines logged after an await inside the command still reach whoever typed it.
    private static readonly AsyncLocal<int> CallerSlot = new();

    // The player whose command is running, or 0 for the console and for anything not started by a command.
    public static int Caller => CallerSlot.Value;

    public static void Register(string name, Action handler) =>
        Register(name, _ => handler());

    public static void Register(string name, Action<string?> handler) =>
        SharedAPI.Commands.RegisterCommand(name, true, new Action<int, string?>(new Invocation(handler).Run));

    private sealed class Invocation
    {
        private readonly Action<string?> _handler;

        public Invocation(Action<string?> handler) => _handler = handler;

        // The runtime binds every unmarked parameter to a typed argument by position, so the caller has to
        // come from [FromSource] and the argument has to be optional.
        public void Run([FromSource] int source, string? argument = null)
        {
            CallerSlot.Value = source;

            try
            {
                _handler(argument);
            }
            finally
            {
                CallerSlot.Value = 0;
            }
        }
    }
}
