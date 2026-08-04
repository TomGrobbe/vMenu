using CitizenFX.FiveM.Client;
using CitizenFX.FiveM.Shared;

namespace vMenu.Enhanced.Ticks;

/// <summary>Every per frame loop in the client, so each one can be named, gated and stopped.</summary>
// Enhanced has no tick registration that awaits its handler. ScheduleRepeated takes an Action, so an
// async Task handler re-arms the timer at its first await and the next invocation starts while the
// previous is still suspended. That leaves await API.Yield(), which TickHandle drives.
// Conditions are a bare Func<bool> so this project stays free of the configuration and permission
// modules.
public static class TickRegistry
{
    private const string DumpCommand = "vmenu_ticks";

    private static readonly List<TickHandle> Registered = [];

    public static IReadOnlyList<TickHandle> Handles => Registered;

    /// <summary>Raised when a tick starts, stops, joins or leaves the registry.</summary>
    // A single Reevaluate pass raises it once per tick it flips, so a subscriber doing real work
    // should coalesce.
    public static event Action? Changed;

    public static void Initialize()
    {
        SharedAPI.Commands.RegisterCommand(DumpCommand, false, new Action(Dump));

        TickOverlay.Initialize();
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

        NotifyChanged();

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

    /// <summary>Re-runs every condition.</summary>
    public static void Reevaluate()
    {
        // Indexed, because a condition is caller code and one that registers or disposes a tick
        // would invalidate the enumerator mid pass.
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

    internal static void Unregister(TickHandle handle)
    {
        if (Registered.Remove(handle))
        {
            NotifyChanged();
        }
    }

    /// <summary>A throwing subscriber must not abort the state change that raised the event.</summary>
    internal static void NotifyChanged()
    {
        try
        {
            Changed?.Invoke();
        }
        catch (Exception exception)
        {
            API.Log.Error($"[Tick] a change subscriber threw: {exception}");
        }
    }
}
