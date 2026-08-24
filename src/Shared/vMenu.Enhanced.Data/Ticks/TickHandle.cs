namespace vMenu.Enhanced.Data.Ticks;

// Stopping ends the loop rather than idling it, so a feature switched off costs nothing, not even a
// per frame branch. That is why gating belongs here instead of inside handlers.
public sealed class TickHandle : IDisposable
{
    // A per frame tick would write sixty error lines a second while broken. Five is enough to prove it
    // was not a one off.
    private const int MaxFailures = 5;

    private readonly TickEngine _engine;
    private readonly Func<Task> _handler;
    private readonly TickRate _rate;
    private readonly Func<bool>? _condition;

    // Built once, not per iteration: a per frame tick would otherwise allocate this 60 times a second.
    private readonly string _scope;

    // The loop's exit condition: the state Apply committed to, not the state it wants.
    private bool _running;

    // Whether a Drive call is live, including while suspended at an await.
    private bool _driverInFlight;

    // Only consulted when there is no condition.
    private bool _manuallyStarted;

    private int _failures;
    private bool _disposed;

    internal TickHandle(
        TickEngine engine,
        string name,
        Func<Task> handler,
        TickRate rate,
        Func<bool>? condition,
        bool autoStart)
    {
        _engine = engine;
        Name = name;
        _handler = handler;
        _rate = rate;
        _condition = condition;
        _manuallyStarted = autoStart;
        _scope = $"vMenu.Enhanced.Tick.{name}";
    }

    public string Name { get; }

    public TickRate Rate => _rate;

    public bool IsRunning => _running;

    public Action? OnStarted { get; init; }

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

    // Re-arms a tick stopped by MaxFailures, so a permanently broken handler costs another five log
    // lines every time it is called.
    public void Reevaluate() => Apply();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Stop();

        _engine.Unregister(this);
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

        _engine.NotifyChanged();

        if (!shouldRun)
        {
            Notify(OnStopped);

            return;
        }

        _failures = 0;

        Notify(OnStarted);

        // A restart inside one frame leaves the previous driver suspended mid await, and a second here is
        // the overlap this type exists to prevent.
        if (!_driverInFlight)
        {
            Drive();
        }
    }

    private long WaitMilliseconds()
    {
        try
        {
            return _rate.Milliseconds;
        }
        catch (Exception exception)
        {
            _engine.Log(TickLog.Error, $"{Name} rate threw and is being treated as per frame: {exception}");

            return 0;
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
            _engine.Log(TickLog.Error, $"{Name} condition threw and is being treated as off: {exception}");

            return false;
        }
    }

    private async void Drive()
    {
        _driverInFlight = true;

        try
        {
            // So a tick body always runs from the tick pump. What starts a tick is usually a callback, and a
            // draw loop firing its first frame inside a checkbox handler surprises.
            await _engine.YieldAsync();

            while (_running)
            {
                // Opened outside the try and closed in the finally, so a handler that throws still leaves the
                // profiler balanced. An unbalanced scope corrupts every reading after it, not just this tick's.
                _engine.EnterScope(_scope);

                try
                {
                    await _handler();

                    _failures = 0;
                }
                catch (Exception exception)
                {
                    _engine.Log(TickLog.Error, $"{Name} threw: {exception}");

                    if (++_failures >= MaxFailures)
                    {
                        _engine.Log(TickLog.Error, $"{Name} stopped after {MaxFailures} consecutive failures.");

                        _running = false;

                        _engine.NotifyChanged();

                        Notify(OnStopped);

                        break;
                    }
                }
                finally
                {
                    _engine.ExitScope();
                }

                await _engine.DelayAsync(WaitMilliseconds());
            }
        }
        finally
        {
            _driverInFlight = false;
        }
    }

    // The lifecycle callbacks are the teardown path, so one throwing must not leave an entity frozen or
    // a scaleform loaded.
    private void Notify(Action? callback)
    {
        try
        {
            callback?.Invoke();
        }
        catch (Exception exception)
        {
            _engine.Log(TickLog.Error, $"{Name} lifecycle callback threw: {exception}");
        }
    }
}
