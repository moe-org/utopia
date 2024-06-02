#region

using System.Net;
using System.Net.Sockets;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     对于<see cref="Socket" />的封装(TCP).
/// </summary>
/// <param name="socket"></param>
public class StandardSocket(Socket socket) : ISocket
{
    private readonly Socket _socket = socket;
    private bool _disposed;

    public bool Alive { get; private set; } = true;

    public EndPoint? RemoteAddress => this._socket.RemoteEndPoint;

    public EndPoint? LocalAddress => this._socket.LocalEndPoint;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<int> Read(Memory<byte> dst)
    {
        if (!this.Alive) return 0;

        return await this._socket.ReceiveAsync(dst);
    }

    public void Shutdown()
    {
        this.Dispose();
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        if (!this.Alive) return;

        await this._socket.SendAsync(data).ConfigureAwait(false);
    }

    public async Task SocketMaintainer()
    {
        while (!this._disposed)
        {
            if (!this.Alive) return;

            // try ping
            var result = await Utilities.TryPing((this._socket.RemoteEndPoint as IPEndPoint)?.Address
                                                 ?? throw new NotSupportedException(
                                                     "not implement for no IpEndPoint")).ConfigureAwait(false);

            if (result == null)
            {
                this.Alive = false;
                return;
            }

            // check tcp
            var part1 = this._socket.Poll(2000, SelectMode.SelectRead);
            var part2 = this._socket.Available == 0;

            if (part1 && part2) this.Alive = false;

            // ping per five seconds check once
            await Task.Delay(1000 * 5).ConfigureAwait(false);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
        {
            this._socket.Shutdown(SocketShutdown.Both);
            this._socket.Dispose();
        }

        this.Alive = false;
        this._disposed = true;
    }
}
