namespace vMenu.Enhanced.Data.Ticks;

/// <summary>How long a tick waits between iterations.</summary>
// The wait happens after the handler returns rather than on a timer, so a slow iteration delays the
// next instead of overlapping with it. That is the whole difference from API.SetInterval.
public readonly struct TickRate
{
    private readonly long _milliseconds;

    private readonly Func<TickRate>? _varying;

    private TickRate(long milliseconds) => _milliseconds = milliseconds;

    private TickRate(Func<TickRate> varying) => _varying = varying;

    /// <summary>Once per frame on the client, once per server tick on the server.</summary>
    public static TickRate PerFrame => default;

    public static TickRate Every(long milliseconds) => new(milliseconds);

    /// <summary>Asked again after every iteration, for a loop that only sometimes needs the fast rate.</summary>
    public static TickRate Varying(Func<TickRate> rate) => new(rate);

    public long Milliseconds => _varying is null ? _milliseconds : _varying().Milliseconds;

    public override string ToString()
    {
        var milliseconds = Milliseconds;

        return milliseconds <= 0 ? "per frame" : $"every {milliseconds}ms";
    }
}
