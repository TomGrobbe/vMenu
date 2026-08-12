using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

using vMenu.Enhanced.Data.Diagnostics;
using vMenu.Enhanced.Data.Ticks;

namespace vMenu.Enhanced.Ticks;

/// <summary>Every per frame loop in the client, so each one can be named, gated and stopped.</summary>
public static class TickRegistry
{
    private const string DumpCommand = "vmenu_ticks";

    private static readonly TickEngine Engine = new(
        ms => API.Delay(ms),
        () => API.Yield(),
        Write,
        Native.ProfilerEnterScope,
        Native.ProfilerExitScope);

    public static IReadOnlyList<TickHandle> Handles => Engine.Handles;

    public static event Action? Changed
    {
        add => Engine.Changed += value;
        remove => Engine.Changed -= value;
    }

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, DebugCommands.Gate(Dump));

        TickOverlay.Initialize();
    }

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
        API.Log.Info($"[Tick] {Engine.Handles.Count} registered:");

        foreach (var line in Engine.Describe())
        {
            API.Log.Info("[Tick]   " + line);
        }
    }

    private static void Write(TickLog level, string message)
    {
        switch (level)
        {
            case TickLog.Error:
                API.Log.Error($"[Tick] {message}");
                break;
            case TickLog.Warn:
                API.Log.Warn($"[Tick] {message}");
                break;
            case TickLog.Info:
                API.Log.Info($"[Tick] {message}");
                break;
            default:
                API.Log.Debug($"[Tick] {message}");
                break;
        }
    }
}
