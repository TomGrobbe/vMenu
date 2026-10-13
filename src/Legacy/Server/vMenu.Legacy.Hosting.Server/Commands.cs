using CitizenFX.Core.Native;

namespace vMenu.Legacy.Hosting;

public static class Commands
{
    public static void Register(string name, bool restricted, Action handler) =>
        API.RegisterCommand(name, new Action<int, List<object>, string>((_, _, _) => handler()), restricted);

    public static void Register(string name, bool restricted, Action<string?> handler) =>
        API.RegisterCommand(name, new Action<int, List<object>, string>((_, args, _) => handler(args.Count > 0 ? args[0]?.ToString() : null)), restricted);

    public static void Register(string name, bool restricted, Action<int, List<object>, string> handler) =>
        API.RegisterCommand(name, handler, restricted);
}
