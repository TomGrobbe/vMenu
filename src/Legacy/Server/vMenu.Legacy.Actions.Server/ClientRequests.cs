using System.Globalization;

using CitizenFX.Core;

using vMenu.Enhanced.Logging;

using vMenu.Legacy.Hosting;

namespace vMenu.Enhanced.Actions.Server;

/// <summary>Questions sent to clients that are still waiting for an answer. Only the client that was asked can
/// answer, otherwise anyone could, say, confirm delivery of a message they never received.</summary>
public sealed class ClientRequests<T>
{
    private readonly Dictionary<int, Pending> _pending = [];

    private readonly string? _tag;

    private readonly string? _what;

    private int _lastId;

    /// <param name="what">Names the request in the warning logged when somebody answers one that was not theirs.
    /// Null ignores that silently.</param>
    public ClientRequests(string? tag = null, string? what = null)
    {
        _tag = tag;
        _what = what;
    }

    public (int Id, Task<T> Answer) Open(int target)
    {
        var id = ++_lastId;
        var answer = new TaskCompletionSource<T>();

        _pending[id] = new Pending(target, answer);

        return (id, answer.Task);
    }

    public void Close(int id) => _pending.Remove(id);

    /// <summary>Drops whichever of these were never answered.</summary>
    public void Close(IReadOnlyList<Task<T>> answers)
    {
        var unanswered = _pending.Where(pair => answers.Contains(pair.Value.Answer.Task)).Select(pair => pair.Key).ToList();

        foreach (var id in unanswered)
        {
            _pending.Remove(id);
        }
    }

    /// <summary>An unknown id has either timed out already or was made up. Neither is worth a log line.</summary>
    public void TryAnswer(Player source, string requestId, T value)
    {
        if (!ActionArgs.TryInt(requestId, out var id) || !_pending.TryGetValue(id, out var pending))
        {
            return;
        }

        if (pending.Target != source.ServerId())
        {
            if (_what is not null)
            {
                Log.Warning($"[{_tag}] {source.Name} answered {_what} {id}, which was not theirs. Ignored.");
            }

            return;
        }

        _pending.Remove(id);

        pending.Answer.TrySetResult(value);
    }

    /// <summary>For a player who left: everything they were asked gets <paramref name="value"/> rather than waiting
    /// out the timeout.</summary>
    public void AnswerAllFrom(int target, T value)
    {
        foreach (var pending in _pending.Values.Where(pending => pending.Target == target).ToList())
        {
            pending.Answer.TrySetResult(value);
        }
    }

    /// <summary>Must start on the tick thread. It hops back onto it before returning, because the awaits leave it
    /// and the reply has to go out from there.</summary>
    public async Task<(bool Answered, T? Value)> AskAsync(int target, Action<string> emit, int timeoutMs)
    {
        var (id, answer) = Open(target);

        try
        {
            emit(id.ToString(CultureInfo.InvariantCulture));

            var timeout = BaseScript.Delay(timeoutMs);

            return await Task.WhenAny(answer, timeout) == timeout ? (false, default) : (true, answer.Result);
        }
        finally
        {
            Close(id);

            await BaseScript.Delay(0);
        }
    }

    private sealed class Pending(int target, TaskCompletionSource<T> answer)
    {
        public int Target { get; } = target;

        public TaskCompletionSource<T> Answer { get; } = answer;
    }
}
