namespace vMenu.Enhanced.Data.Ticks;

public enum TickLog
{
    Debug,
    Info,
    Warn,
    Error,
}

/// <summary>Every named loop on one side, so each one can be named, gated and stopped.</summary>
// Neither runtime has a tick registration that awaits its handler. ScheduleRepeated takes an Action,
// so an async Task handler re-arms the timer at its first await and the next invocation starts while
// the previous is still suspended. That leaves driving the loop by hand, which TickHandle does.
// Both sides own an instance and supply their own waiting and logging. Conditions are a bare
// Func<bool> so this stays free of the configuration and permission modules.
public sealed class TickEngine(Func<long, Task> delay, Func<Task> yield, Action<TickLog, string> write)
{
    private readonly List<TickHandle> _registered = [];

    public IReadOnlyList<TickHandle> Handles => _registered;

    /// <summary>Raised when a tick starts, stops, joins or leaves the engine.</summary>
    // A single Reevaluate pass raises it once per tick it flips, so a subscriber doing real work
    // should coalesce.
    public event Action? Changed;

    /// <param name="condition">
    /// Re-run by <see cref="Reevaluate"/>. When null the tick answers to <see cref="TickHandle.Start"/>
    /// and <see cref="TickHandle.Stop"/> instead.
    /// </param>
    /// <param name="autoStart">Ignored when <paramref name="condition"/> is set.</param>
    public TickHandle Register(
        string name,
        Func<Task> handler,
        TickRate rate = default,
        Func<bool>? condition = null,
        Action? onStarted = null,
        Action? onStopped = null,
        bool autoStart = true)
    {
        var handle = new TickHandle(this, name, handler, rate, condition, autoStart)
        {
            OnStarted = onStarted,
            OnStopped = onStopped,
        };

        _registered.Add(handle);

        handle.Apply();

        NotifyChanged();

        return handle;
    }

    /// <summary>Wrapped once here rather than once per iteration.</summary>
    public TickHandle Register(
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
    public void Reevaluate()
    {
        // Indexed, because a condition is caller code and one that registers or disposes a tick
        // would invalidate the enumerator mid pass.
        for (var i = 0; i < _registered.Count; i++)
        {
            _registered[i].Apply();
        }
    }

    public IEnumerable<string> Describe()
    {
        foreach (var handle in _registered)
        {
            yield return $"{handle.Name} ({handle.Rate}): {(handle.IsRunning ? "running" : "stopped")}";
        }
    }

    internal Task DelayAsync(long milliseconds) => delay(milliseconds);

    internal Task YieldAsync() => yield();

    internal void Log(TickLog level, string message) => write(level, message);

    internal void Unregister(TickHandle handle)
    {
        if (_registered.Remove(handle))
        {
            NotifyChanged();
        }
    }

    /// <summary>A throwing subscriber must not abort the state change that raised the event.</summary>
    internal void NotifyChanged()
    {
        try
        {
            Changed?.Invoke();
        }
        catch (Exception exception)
        {
            Log(TickLog.Error, $"a change subscriber threw: {exception}");
        }
    }
}
