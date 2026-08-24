namespace vMenu.Enhanced.Updates.Server.Http;

// How a request ended, kept separate from what it returned. Splitting these tells "the server
// answered, whatever the status" apart from "nothing came back": a 403 rate limit is a real answer,
// not a failure to reach anything.
public enum HttpOutcome
{
    // A status code came back, whatever the status says.
    Answered,

    // The request threw before any status came back: a network, TLS or runtime failure.
    Unusable,

    // Nothing came back before the timeout.
    TimedOut,
}

public sealed class HttpReply(HttpOutcome outcome, int status, string body, string? reason, int elapsedMs)
{
    public HttpOutcome Outcome { get; } = outcome;

    public int Status { get; } = status;

    public string Body { get; } = body;

    // Why it did not answer, or null when it did.
    public string? Reason { get; } = reason;

    public int ElapsedMs { get; } = elapsedMs;

    // Answered and with something worth parsing.
    public bool IsOk => Outcome == HttpOutcome.Answered && Status is >= 200 and < 300 && Body.Length > 0;

    public static HttpReply Answered(int status, string body, int elapsedMs) =>
        new(HttpOutcome.Answered, status, body, null, elapsedMs);

    public static HttpReply Unusable(string reason) => new(HttpOutcome.Unusable, 0, string.Empty, reason, 0);

    public static HttpReply TimedOut(int afterMs) =>
        new(HttpOutcome.TimedOut, 0, string.Empty, $"no answer within {afterMs}ms", afterMs);
}
