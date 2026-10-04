using System.Net.Sockets;

namespace vMenu.Enhanced.Http.Server;

// FiveM doesn't have CancelIoEx implemented, sandboxing bullshit so this is a workaround to not crash the server on aborted connections
public static class SafeSockets
{
    // Proxy off: resolving the Windows proxy loads Microsoft.Win32.Registry, which is not shipped and throws.
    public static SocketsHttpHandler Handler() => new() { UseProxy = false, ConnectCallback = ConnectAsync };

    private static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken _)
    {
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

        try
        {
            await socket.ConnectAsync(context.DnsEndPoint);
        }
        catch
        {
            socket.Dispose();
            throw;
        }

        return new TokenlessStream(new NetworkStream(socket, ownsSocket: true));
    }

    private sealed class TokenlessStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;

        public override bool CanWrite => inner.CanWrite;

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override int Read(Span<byte> buffer) => inner.Read(buffer);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken _) =>
            inner.ReadAsync(buffer, offset, count, CancellationToken.None);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken _ = default) =>
            inner.ReadAsync(buffer, CancellationToken.None);

        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

        public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken _) =>
            inner.WriteAsync(buffer, offset, count, CancellationToken.None);

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken _ = default) =>
            inner.WriteAsync(buffer, CancellationToken.None);

        public override void Flush() => inner.Flush();

        public override Task FlushAsync(CancellationToken _) => inner.FlushAsync(CancellationToken.None);

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
