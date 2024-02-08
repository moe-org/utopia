#region

using System.Buffers;
using System.Diagnostics;
using System.IO.Hashing;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Text;

#endregion

namespace Utopia.Core.Net;

public sealed class KcpSocket : ISocket, IKcpCallback
{
    public static readonly Lazy<uint> KcpConv = new(() =>
    {
        var version = VersionUtility.UtopiaCoreVersion;

        var got = XxHash32.Hash(Encoding.UTF8.GetBytes(version.ToString()));

        return BitConverter.ToUInt32(got);
    }, true);

    private readonly PoolSegManager.Kcp _kcp;

    private readonly DateTimeOffset _originDate = new();

    private readonly ISocket _socket;

    private readonly Stopwatch _stopwatch = new();

    private readonly CancellationTokenSource _updateLoopCancellationTokenSource = new();

    private bool _disposed;

    private SpinLock _lock;

    private readonly Task _task;

    private DateTimeOffset Current => this._originDate.AddMilliseconds(this._stopwatch.ElapsedMilliseconds);

    public KcpSocket(ISocket socket, uint conv = 0)
    {
        this._socket = socket;

        this._kcp = new PoolSegManager.Kcp(KcpConv.Value + conv, this);

        this._kcp.SetMtu(Utilities.GetMtu());

        // common mode:
        // _kcp.NoDelay(0, 40, 0, 0);
        // fast mode:
        this._kcp.NoDelay(1, 10, 2, 1);

        this._stopwatch.Start();

        this._task = Task.Run(() => this.UpdateLoop(this._updateLoopCancellationTokenSource.Token));
    }

    public async void Output(IMemoryOwner<byte> buffer, int avalidLength)
    {
        using var b = buffer;
        // ensure that only this method call socket.write
        await this._socket.Write(b.Memory.Slice(0, avalidLength));
    }

    public bool Alive { get; private set; } = true;

    public EndPoint? RemoteAddress => this._socket.RemoteAddress;

    public EndPoint? LocalAddress => this._socket.LocalAddress;

    public Task<int> Read(Memory<byte> dst)
    {
        if (!this.Alive) return Task.FromResult(0);

        // receive is **not** thread-safe
        var locked = false;
        try
        {
            this._lock.Enter(ref locked);

            var get = this._kcp.Recv(dst.Span);

            if (get < 0) return Task.FromResult(0);

            return Task.FromResult(get);
        }
        finally
        {
            if (locked) this._lock.Exit();
        }
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        if (!this.Alive) return;

        // send is thread-safe
        var index = 0;
        // send all
        while (true)
        {
            var err = this._kcp.Send(data.Slice(index).Span);

            if (err == -1) throw new IOException("Kcp.Send returns -1, means error");

            if (err > 0) index += err;

            if (index >= data.Length) return;

            await Task.Yield();
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Shutdown()
    {
        this.Dispose();
    }

    /// <summary>
    ///     This should run only in one thread.
    ///     This method will read from socket and update kcp frequently.
    /// </summary>
    private async Task UpdateLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!this.Alive) return;

                var current = this.Current;

                DateTimeOffset next;

                // update is not thread-safe
                var lockTaken = false;
                try
                {
                    this._lock.Enter(ref lockTaken);

                    this._kcp.Update(current);
                    next = this._kcp.Check(current);
                }
                finally
                {
                    if (lockTaken) this._lock.Exit();
                }

                // wait
                while (next > this.Current)
                {
                    if (token.IsCancellationRequested || !this.Alive) return;

                    await Task.Yield();
                    await UpdateData();
                }

                await UpdateData();
            }
        }
        finally
        {
            Dispose(true);
        }

        // read data from socket
        async Task UpdateData()
        {
            var rent = ArrayPool<byte>.Shared.Rent(1024);

            try
            {
                // read is not thread-safe
                // but we ensure that only we are call that
                var length = await this._socket.Read(rent);

                // while input is thread-safe
                this._kcp.Input(new Span<byte>(rent, 0, length));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rent);
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
        {
            this._updateLoopCancellationTokenSource.Cancel();
            this._socket.Shutdown();
            this._socket.Dispose();
        }

        this.Alive = false;
        this._disposed = true;
    }
}
