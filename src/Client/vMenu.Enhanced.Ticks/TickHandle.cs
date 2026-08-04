using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Ticks;

/// <summary>One registered tick, and the only way to start or stop it.</summary>
// Stopping ends the loop rather than idling it, so a feature switched off costs nothing, not even a
// per frame branch. That is why gating belongs here instead of inside handlers.
public sealed class TickHandle : IDisposable
{
    // A per frame tick would write sixty error lines a second while broken. Five is enough to prove
    // it was not a one off.
    private const int MaxFailures = 5;

    private readonly Func<Task> _handler;
    private readonly TickRate _rate;
    private readonly Func<bool>? _condition;

    /// <summary>The loop's exit condition: the state <see cref="Apply"/> committed to, not the state it wants.</summary>
    private bool _running;

    /// <summary>Whether a <see cref="Drive"/> call is live, including while suspended at an await.</summary>
    private bool _driverInFlight;

    /// <summary>Only consulted when there is no condition.</summary>
    private bool _manuallyStarted;

    private int _failures;
    private bool _disposed;

    internal TickHandle(string name, Func<Task> handler, TickRate rate, Func<bool>? condition, bool autoStart)
    {
        Name = name;
        _handler = handler;
        _rate = rate;
        _condition = condition;
        _manuallyStarted = autoStart;
    }

    public string Name { get; }

    public TickRate Rate => _rate;

    public bool IsRunning => _running;

    /// <summary>Runs when the tick starts, for setup that must not happen per iteration.</summary>
    public Action? OnStarted { get; init; }

    /// <summary>Runs when the tick stops, for the teardown that pairs with <see cref="OnStarted"/>.</summary>
    public Action? OnStopped { get; init; }

    public void Start()
    {
        _manuallyStarted = true;

        Apply();
    }

    public void Stop()
    {
        _manuallyStarted = false;

        Apply();
    }

    /// <summary>Re-runs the condition.</summary>
    // This re-arms a tick stopped by MaxFailures, so a permanently broken handler costs another five
    // log lines every time it is called.
    public void Reevaluate() => Apply();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Stop();

        TickRegistry.Unregister(this);
    }

    internal void Apply()
    {
        var shouldRun = !_disposed && EvaluateCondition();
        var alreadyInDesiredState = shouldRun == _running;

        if (alreadyInDesiredState)
        {
            return;
        }

        _running = shouldRun;

        TickRegistry.NotifyChanged();

        if (!shouldRun)
        {
            Notify(OnStopped);

            return;
        }

        _failures = 0;

        Notify(OnStarted);

        // A restart inside one frame leaves the previous driver suspended mid await, and a second
        // here is the overlap this type exists to prevent.
        if (!_driverInFlight)
        {
            Drive();
        }
    }

    // Fails closed: a throwing condition must not leave a tick stuck on, and a registry wide
    // re-evaluation must not abort partway through.
    private bool EvaluateCondition()
    {
        try
        {
            return _condition is null ? _manuallyStarted : _condition();
        }
        catch (Exception exception)
        {
            API.Log.Error($"[Tick] {Name} condition threw and is being treated as off: {exception}");

            return false;
        }
    }

    private async void Drive()
    {
        _driverInFlight = true;

        try
        {
            // So a tick body always runs from the tick pump. What starts a tick is usually a
            // callback, and a draw loop firing its first frame inside a checkbox handler surprises.
            await API.Yield();

            while (_running)
            {
                try
                {
                    await _handler();

                    _failures = 0;
                }
                catch (Exception exception)
                {
                    API.Log.Error($"[Tick] {Name} threw: {exception}");

                    if (++_failures >= MaxFailures)
                    {
                        API.Log.Error($"[Tick] {Name} stopped after {MaxFailures} consecutive failures.");

                        _running = false;

                        TickRegistry.NotifyChanged();

                        Notify(OnStopped);

                        break;
                    }
                }

                await _rate.WaitAsync();
            }
        }
        finally
        {
            _driverInFlight = false;
        }
    }

    // The lifecycle callbacks are the teardown path, so one throwing must not leave an entity frozen
    // or a scaleform loaded.
    private void Notify(Action? callback)
    {
        try
        {
            callback?.Invoke();
        }
        catch (Exception exception)
        {
            API.Log.Error($"[Tick] {Name} lifecycle callback threw: {exception}");
        }
    }
}
