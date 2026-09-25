using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace vMenu.Enhanced.Http.Server.Bridge;

// Linux TLS bridge: a WebSocket whose connection lives in net_bridge.js. Text frames only.
public sealed class BridgeWebSocket(IReadOnlyDictionary<string, string> headers) : WebSocket
{
    private readonly string _id = Guid.NewGuid().ToString("N");

    private readonly ConcurrentQueue<byte[]> _inbox = new();

    private readonly SemaphoreSlim _arrivals = new(0);

    private readonly TaskCompletionSource _opened = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private byte[] _current = [];

    private int _offset;

    private volatile WebSocketState _state = WebSocketState.None;

    private int _ended;

    public override WebSocketState State => _state;

    public override WebSocketCloseStatus? CloseStatus => null;

    public override string? CloseStatusDescription => null;

    public override string? SubProtocol => null;

    public Task ConnectAsync(Uri uri)
    {
        _state = WebSocketState.Connecting;
        NetBridge.Open(this, _id, uri.ToString(), headers);

        return _opened.Task;
    }

    public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        if (_offset >= _current.Length)
        {
            await _arrivals.WaitAsync();
            if (!_inbox.TryDequeue(out var next))
            {
                _arrivals.Release();

                return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);
            }

            _current = next;
            _offset = 0;
        }

        var count = Math.Min(buffer.Count, _current.Length - _offset);
        Array.Copy(_current, _offset, buffer.Array!, buffer.Offset, count);
        _offset += count;

        return new WebSocketReceiveResult(count, WebSocketMessageType.Text, _offset >= _current.Length);
    }

    public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        if (_state == WebSocketState.Open)
        {
            NetBridge.Transmit(_id, Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));
        }

        return Task.CompletedTask;
    }

    public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        Shutdown(WebSocketState.Closed);

        return Task.CompletedTask;
    }

    public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        Shutdown(WebSocketState.Closed);

        return Task.CompletedTask;
    }

    public override void Abort() => Shutdown(WebSocketState.Aborted);

    public override void Dispose() => Shutdown(WebSocketState.Closed);

    internal void Opened()
    {
        if (_state == WebSocketState.Connecting)
        {
            _state = WebSocketState.Open;
            _opened.TrySetResult();
        }
    }

    internal void Received(string text)
    {
        _inbox.Enqueue(Encoding.UTF8.GetBytes(text));
        _arrivals.Release();
    }

    internal void Closed(string reason)
    {
        var wasConnecting = _state == WebSocketState.Connecting;
        if (_state is not WebSocketState.Aborted)
        {
            _state = WebSocketState.Closed;
        }

        // Only fault while someone awaits the connect; an unobserved fault crashes this runtime.
        if (wasConnecting)
        {
            _opened.TrySetException(new WebSocketException(reason));
        }

        End();
    }

    private void Shutdown(WebSocketState state)
    {
        var previous = _state;
        if (previous is WebSocketState.Closed or WebSocketState.Aborted)
        {
            return;
        }

        _state = state;
        if (previous == WebSocketState.None)
        {
            return;
        }

        NetBridge.Close(_id);

        if (previous == WebSocketState.Connecting)
        {
            _opened.TrySetException(new WebSocketException("aborted"));
        }

        End();
    }

    // One extra release with an empty queue tells the reader the socket is gone.
    private void End()
    {
        if (Interlocked.Exchange(ref _ended, 1) == 0)
        {
            _arrivals.Release();
        }
    }
}
