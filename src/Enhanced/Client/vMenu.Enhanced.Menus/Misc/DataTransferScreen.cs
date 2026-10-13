using CitizenFX.FiveM.Client;

using vMenu.Enhanced.Logging;
using vMenu.Enhanced.MenuFramework;
using vMenu.Enhanced.Serialization;

namespace vMenu.Enhanced.Menus.Misc;

// The second focus taking screen after UserInput, and a copy of its handshake for the same reasons.
// A third one is the point at which all of that belongs in the framework rather than in a third copy.
public static class DataTransferScreen
{
    private const string ReadyCallback = "vMenuTransferReady";

    private const string CloseCallback = "vMenuTransferClose";

    private const string ImportCallback = "vMenuTransferImport";

    private const string FailedCallback = "vMenuTransferFailed";

    private const string CloseMessage = """{"type":"transfer_close"}""";

    private const int ReadyTimeoutMs = 3000;

    // The page's first callback of a session takes seconds; every one after is immediate.
    private const int FirstReadyTimeoutMs = 15000;

    // How long the rest of a pasted code has to turn up once the first piece has.
    private const int AssemblyTimeoutMs = 10000;

    // Well under the 64K where a buffer limit would sit. Nothing in vMenu has ever posted more than a
    // typed line back from a page, so the size that works is not known, only the size that is safe.
    private const int ChunkLength = 32 * 1024;

    private static bool _callbacksRegistered;
    private static bool _handshaken;
    private static bool _open;
    private static int _token;
    private static int _arrived;
    private static bool _assembling;
    private static string?[] _incoming = [];
    private static TaskCompletionSource<bool>? _ready;
    private static TaskCompletionSource<string?>? _finished;

    public static Task ShowAsync(TransferPrompt prompt, string code) => RunAsync(prompt, code);

    // What the player pasted, decoded, or null if they gave up.
    public static Task<string?> AskAsync(TransferPrompt prompt) => RunAsync(prompt, null);

    private static async Task<string?> RunAsync(TransferPrompt prompt, string? payload)
    {
        if (_open)
        {
            return null;
        }

        EnsureCallbacks();

        _open = true;

        var token = ++_token;
        var chunks = payload is null ? [] : Split(payload);
        var ready = new TaskCompletionSource<bool>();
        var finished = new TaskCompletionSource<string?>();

        _ready = ready;
        _finished = finished;
        _incoming = [];
        _arrived = 0;
        _assembling = false;

        MenuButtonLock.Take();

        try
        {
            Native.SendNuiMessage(BuildOpenMessage(prompt, payload is not null, token, chunks.Count));
            Native.SetNuiFocus(true, true);

            var timeout = _handshaken ? ReadyTimeoutMs : FirstReadyTimeoutMs;

            if (await Task.WhenAny(ready.Task, API.Delay(timeout)) != ready.Task)
            {
                Log.Error($"[Transfer] The screen did not answer within {timeout}ms. Is ui/index.html part of the resource?");

                return null;
            }

            // After the handshake, so a piece is never in flight while it is still unknown whether there is a
            // page to receive it.
            for (var index = 0; index < chunks.Count; index++)
            {
                Native.SendNuiMessage(BuildChunkMessage(token, index, chunks.Count, chunks[index]));
            }

            return await finished.Task;
        }
        finally
        {
            _open = false;
            _ready = null;
            _finished = null;
            _incoming = [];

            Native.SetNuiFocus(false, false);
            Native.SendNuiMessage(CloseMessage);

            MenuButtonLock.Release();
        }
    }

    private static void EnsureCallbacks()
    {
        if (_callbacksRegistered)
        {
            return;
        }

        _callbacksRegistered = true;

        NuiCallbacks.Register(ReadyCallback, body =>
        {
            if (!Mine(body, out _))
            {
                return;
            }

            _handshaken = true;
            _ready?.TrySetResult(true);
        });

        NuiCallbacks.Register(CloseCallback, body =>
        {
            if (Mine(body, out _))
            {
                _finished?.TrySetResult(null);
            }
        });

        NuiCallbacks.Register(ImportCallback, body =>
        {
            if (Mine(body, out var post))
            {
                Receive(post!);
            }
        });

        // The page says so itself and stays open for another attempt, so this only has to reach F8.
        NuiCallbacks.Register(FailedCallback, body =>
        {
            if (Mine(body, out var post))
            {
                Log.Warning($"[Transfer] The screen could not read what was pasted in: {post!.Reason}.");
            }
        });
    }

    private static void Receive(TransferPost post)
    {
        if (post.Count <= 0 || post.Index < 0 || post.Index >= post.Count)
        {
            return;
        }

        if (_incoming.Length != post.Count)
        {
            if (_arrived > 0)
            {
                return;
            }

            _incoming = new string?[post.Count];
        }

        if (_incoming[post.Index] is not null)
        {
            return;
        }

        _incoming[post.Index] = post.Text;
        _arrived++;

        if (!_assembling)
        {
            _assembling = true;

            _ = WatchAssemblyAsync(_token);
        }

        // Counted rather than ordered: the browser allows six connections per host, so the pieces do not
        // necessarily arrive in the order the page sent them.
        if (_arrived < _incoming.Length)
        {
            return;
        }

        var parts = new string[_incoming.Length];

        for (var index = 0; index < parts.Length; index++)
        {
            parts[index] = _incoming[index] ?? string.Empty;
        }

        _finished?.TrySetResult(string.Concat(parts));
    }

    private static async Task WatchAssemblyAsync(int token)
    {
        await API.Delay(AssemblyTimeoutMs);

        if (!_open || token != _token || _finished is not { Task.IsCompleted: false } finished)
        {
            return;
        }

        Log.Error($"[Transfer] Only {_arrived} of {_incoming.Length} pieces of that code arrived.");

        finished.TrySetResult(null);
    }

    private static bool Mine(string body, out TransferPost? post)
    {
        post = Parse(body);

        return _open && post is not null && post.Token == _token;
    }

    // The page posts an object, which arrives as the request text. Should the runtime ever unwrap it the
    // way UserInput's page needs, this still reads it.
    private static TransferPost? Parse(string body)
    {
        if (body.Length == 0)
        {
            return null;
        }

        if (ClientJson.TryDeserialize<TransferPost>(body, out var post) && post is not null)
        {
            return post;
        }

        return ClientJson.TryDeserialize<string>(body, out var inner)
            && !string.IsNullOrEmpty(inner)
            && ClientJson.TryDeserialize<TransferPost>(inner, out var nested)
                ? nested
                : null;
    }

    private static List<string> Split(string text)
    {
        var parts = new List<string>();
        var at = 0;

        while (at < text.Length)
        {
            var end = Math.Min(at + ChunkLength, text.Length);

            // Never between a surrogate pair: half of one is not valid UTF-8 once it leaves here.
            if (end < text.Length && char.IsHighSurrogate(text[end - 1]))
            {
                end--;
            }

            parts.Add(text[at..end]);
            at = end;
        }

        return parts;
    }

    private static string BuildOpenMessage(TransferPrompt prompt, bool exporting, int token, int chunks) =>
        ClientJson.Serialize(new OpenMessage
        {
            Mode = exporting ? "export" : "import",
            Token = token,
            Chunks = chunks,
            Title = prompt.Title,
            Summary = prompt.Summary,
            Warning = prompt.Warning,
            Hint = prompt.Hint,
            Placeholder = prompt.Placeholder,
            Copy = prompt.Copy,
            Copied = prompt.Copied,
            CopyFailed = prompt.CopyFailed,
            Confirm = prompt.Confirm,
            Close = prompt.Close,
            Working = prompt.Working,
            EmptyCode = prompt.EmptyCode,
            NotACode = prompt.NotACode,
            BadCode = prompt.BadCode,
        });

    private static string BuildChunkMessage(int token, int index, int count, string text) =>
        ClientJson.Serialize(new ChunkMessage
        {
            Token = token,
            Index = index,
            Count = count,
            Text = text,
        });

    private sealed class OpenMessage
    {
        public string Type { get; } = "transfer_open";

        public required string Mode { get; init; }

        public required int Token { get; init; }

        public required int Chunks { get; init; }

        public required string Title { get; init; }

        public required string Summary { get; init; }

        public required string Warning { get; init; }

        public required string Hint { get; init; }

        public required string Placeholder { get; init; }

        public required string Copy { get; init; }

        public required string Copied { get; init; }

        public required string CopyFailed { get; init; }

        public required string Confirm { get; init; }

        public required string Close { get; init; }

        public required string Working { get; init; }

        public required string EmptyCode { get; init; }

        public required string NotACode { get; init; }

        public required string BadCode { get; init; }
    }

    private sealed class ChunkMessage
    {
        public string Type { get; } = "transfer_chunk";

        public required int Token { get; init; }

        public required int Index { get; init; }

        public required int Count { get; init; }

        public required string Text { get; init; }
    }

    private sealed class TransferPost
    {
        public int Token { get; init; }

        public int Index { get; init; }

        public int Count { get; init; }

        public string Text { get; init; } = string.Empty;

        public string Reason { get; init; } = string.Empty;
    }
}

// A class rather than a record: generated equality routes through
// EqualityComparer<string>.Default, which the sandbox refuses to load.
public sealed class TransferPrompt
{
    public required string Title { get; init; }

    public required string Summary { get; init; }

    public required string Warning { get; init; }

    public required string Hint { get; init; }

    public required string Placeholder { get; init; }

    public required string Copy { get; init; }

    public required string Copied { get; init; }

    public required string CopyFailed { get; init; }

    public required string Confirm { get; init; }

    public required string Close { get; init; }

    public required string Working { get; init; }

    public required string EmptyCode { get; init; }

    public required string NotACode { get; init; }

    public required string BadCode { get; init; }
}
