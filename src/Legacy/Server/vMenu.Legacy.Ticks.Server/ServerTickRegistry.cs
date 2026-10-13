using CitizenFX.Core;
using CitizenFX.Core.Native;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Ticks;
using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Ticks.Server;

// Every loop on the server, so each one can be named, gated and stopped.
public static class ServerTickRegistry
{
    private const string DumpCommand = "vmenu_ticks";

    private static readonly TickEngine Engine = new(
        ms => BaseScript.Delay((int)ms),
        () => BaseScript.Delay(0),
        Write,
        API.ProfilerEnterScope,
        API.ProfilerExitScope,
        () => Script.IsMainThread,
        Script.NextTick);

    public static IReadOnlyList<TickHandle> Handles => Engine.Handles;

    public static event Action? Changed
    {
        add => Engine.Changed += value;
        remove => Engine.Changed -= value;
    }

    public static void Initialize() =>
        Commands.Register(DumpCommand, true, DebugCommands.Gate(Dump));

    public static TickHandle Register(
        string name,
        Func<Task> handler,
        TickRate rate = default,
        Func<bool>? condition = null,
        Action? onStarted = null,
        Action? onStopped = null,
        bool autoStart = true) =>
        Engine.Register(name, handler, rate, condition, onStarted, onStopped, autoStart);

    public static TickHandle Register(
        string name,
        Action handler,
        TickRate rate = default,
        Func<bool>? condition = null,
        Action? onStarted = null,
        Action? onStopped = null,
        bool autoStart = true) =>
        Engine.Register(name, handler, rate, condition, onStarted, onStopped, autoStart);

    public static void Reevaluate() => Engine.Reevaluate();

    public static void Dump()
    {
        Log.Info($"[Tick] {Engine.Handles.Count} registered:");

        foreach (var line in Engine.Describe())
        {
            Log.Info("[Tick]   " + line);
        }
    }

    private static void Write(TickLog level, string message)
    {
        switch (level)
        {
            case TickLog.Error:
                Log.Error($"[Tick] {message}");
                break;
            case TickLog.Warn:
                Log.Warning($"[Tick] {message}");
                break;
            case TickLog.Info:
                Log.Info($"[Tick] {message}");
                break;
            default:
                Log.Debug($"[Tick] {message}");
                break;
        }
    }
}
