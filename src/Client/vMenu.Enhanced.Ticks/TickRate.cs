using CitizenFX.FiveM.Client;

namespace vMenu.Enhanced.Ticks;

/// <summary>How long a tick waits between iterations.</summary>
// The wait happens after the handler returns rather than on a timer, so a slow iteration delays the
// next instead of overlapping with it. That is the whole difference from API.SetInterval.
public readonly struct TickRate
{
    private readonly long _milliseconds;

    private TickRate(long milliseconds) => _milliseconds = milliseconds;

    /// <summary>Once per frame. The default, because <c>API.Yield</c> is <c>API.Delay(0)</c>.</summary>
    public static TickRate PerFrame => default;

    public static TickRate Every(long milliseconds) => new(milliseconds);

    public long Milliseconds => _milliseconds;

    internal Task WaitAsync() => API.Delay(_milliseconds);

    public override string ToString() => _milliseconds <= 0 ? "per frame" : $"every {_milliseconds}ms";
}
