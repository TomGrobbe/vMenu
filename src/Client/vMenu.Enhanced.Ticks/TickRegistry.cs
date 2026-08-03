using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Ticks;

/// <summary>
/// Every per frame loop in the client, so each one can be named, gated and stopped.
/// </summary>
/// <remarks>
/// Enhanced has no tick registration that awaits its handler: <c>TickManager.ScheduleRepeated</c>
/// takes an <see cref="Action"/>, so an <c>async Task</c> handler re-arms the timer at its first
/// await and the next invocation starts while the previous one is still suspended. The only
/// primitive left is <c>await API.Yield()</c>, which is what <see cref="TickHandle"/> drives.
/// <para>
/// Conditions are a bare <see cref="Func{TResult}"/> rather than a gate type so this project can
/// stay free of the configuration and permission modules. <c>Main</c> subscribes
/// <see cref="Reevaluate"/> to their change events, and <c>MenuGate.Evaluate</c> is directly
/// usable as a condition.
/// </para>
/// </remarks>
public static class TickRegistry
{
    private const string DumpCommand = "vmenu_ticks";

    private static readonly List<TickHandle> Registered = [];

    public static IReadOnlyList<TickHandle> Handles => Registered;

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, new Action(Dump));
    }

    /// <param name="condition">
    /// Re-run by <see cref="Reevaluate"/>. When null the tick answers to
    /// <see cref="TickHandle.Start"/> and <see cref="TickHandle.Stop"/> instead.
    /// </param>
    /// <param name="autoStart">Ignored when <paramref name="condition"/> is set.</param>
    public static TickHandle Register(
        string name,
        Func<Task> handler,
        TickRate rate = default,
        Func<bool>? condition = null,
        Action? onStarted = null,
        Action? onStopped = null,
        bool autoStart = true)
    {
        var handle = new TickHandle(name, handler, rate, condition, autoStart)
        {
            OnStarted = onStarted,
            OnStopped = onStopped,
        };

        Registered.Add(handle);

        handle.Apply();

        return handle;
    }

    /// <summary>Wrapped once here rather than once per iteration.</summary>
    public static TickHandle Register(
        string name,
        Action handler,
        TickRate rate = default,
        Func<bool>? condition = null,
        Action? onStarted = null,
        Action? onStopped = null,
        bool autoStart = true)
    {
        return Register(
            name,
            () =>
            {
                handler();

                return Task.CompletedTask;
            },
            rate,
            condition,
            onStarted,
            onStopped,
            autoStart);
    }

    /// <summary>
    /// Re-runs every condition. Subscribed to the configuration and permission change events, and
    /// called directly by features whose own state decides whether their tick has anything to do.
    /// </summary>
    public static void Reevaluate()
    {
        // Indexed rather than foreach: a condition is caller code, and one that registers or
        // disposes a tick would otherwise invalidate the enumerator mid pass.
        for (var i = 0; i < Registered.Count; i++)
        {
            Registered[i].Apply();
        }
    }

    public static void Dump()
    {
        API.Log.Info($"[Tick] {Registered.Count} registered:");

        foreach (var handle in Registered)
        {
            API.Log.Info($"[Tick]   {handle.Name} ({handle.Rate}): {(handle.IsRunning ? "running" : "stopped")}");
        }
    }

    internal static void Unregister(TickHandle handle) => Registered.Remove(handle);
}
