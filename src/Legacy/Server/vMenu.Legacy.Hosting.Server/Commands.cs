using System.Threading;

using CitizenFX.Core.Native;

namespace vMenu.Legacy.Hosting;

public static class Commands
{
    // AsyncLocal so lines logged after an await inside the command still reach whoever typed it.
    private static readonly AsyncLocal<int> CallerSlot = new();

    // The player whose command is running, or 0 for the console and for anything not started by a command.
    public static int Caller => CallerSlot.Value;

    public static void Register(string name, bool restricted, Action handler) =>
        Register(name, restricted, _ => handler());

    public static void Register(string name, bool restricted, Action<string?> handler) =>
        API.RegisterCommand(name, new Action<int, List<object>, string>((source, args, _) => Run(source, handler, args.Count > 0 ? args[0]?.ToString() : null)), restricted);

    private static void Run(int source, Action<string?> handler, string? argument)
    {
        CallerSlot.Value = source;

        try
        {
            handler(argument);
        }
        finally
        {
            CallerSlot.Value = 0;
        }
    }
}
