#region

using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using Autofac;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

#endregion

namespace Utopia.Core.Net;

public class ConnectHandler : IConnectionHandler
{
    private readonly WeakThreadSafeEventSource<Exception?> _event = new();

    private readonly object _lock = new();

    private readonly Pipe _pipe = new();

    private readonly ISocket _socket;

    private volatile bool _disposed;

    /// <summary>
    ///     once true,never false
    /// </summary>
    private volatile bool _eventFired;

    public ConnectHandler(ISocket socket)
    {
        Guard.IsNotNull(socket);
        this._socket = socket;

        if (!this._socket.Alive) throw new ArgumentException("the socket haven't connect yet");

        Task.Run(this.InputLoop);
    }

    public required ILogger<ConnectHandler> Logger { protected get; init; }

    public required ILifetimeScope Container { get; init; }

    public IDispatcher Dispatcher { get; } = new Dispatcher();

    public IPacketizer Packetizer { get; } = new Packetizer();

    public event Action<Exception?> ConnectionClosed
    {
        add => this._event.Register(value);
        remove => this._event.Unregister(value);
    }

    public bool Running => this.isRunning();

    public void WritePacket(Guuid packetTypeId, object obj)
    {
        var data = this.Packetizer.WritePacket(packetTypeId, obj);

        lock (this._lock)
        {
            var encoderedId = Encoding.UTF8.GetBytes(packetTypeId.ToString());

            // 转换到网络端序
            var length = BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(data.Length)
            );

            var idLength = BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(encoderedId.Length)
            );

            this._socket.Write(length);
            this._socket.Write(idLength);
            this._socket.Write(encoderedId);
            this._socket.Write(data);
        }
    }

    public void Disconnect()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Call under the lock <see cref="_lock" />
    /// </summary>
    /// <param name="exception"></param>
    private void FireEvent(Exception? exception)
    {
        lock (this._lock)
        {
            if (this._eventFired) return;
            this._eventFired = true;

            // no error
            var ex = this._event.Fire(exception, true);

            foreach (var e in ex) this.Logger.LogError(e, "get an error when firing ConnectionClosed event");
        }
    }

    private bool isRunning()
    {
        return !this._disposed;
    }

    private async Task _ReadLoop()
    {
        var writer = this._pipe.Writer;

        // true for continue
        while (this.isRunning())
        {
            var memory = writer.GetMemory(256);

            var bytesRead = await this._socket.Read(memory);

            writer.Advance(bytesRead);

            await writer.FlushAsync();
            await Task.Yield();
        }

        await writer.CompleteAsync();
    }

    private async Task _ProcessLoop()
    {
        var reader = this._pipe.Reader;
        var fourBytesBuf = new byte[4];

        // return null for socket disconnect
        async Task<int?> readInt()
        {
            ReadResult got;
            while (true)
            {
                got = await reader.ReadAtLeastAsync(4);

                if (!got.IsCompleted && !got.IsCanceled)
                    // read all
                    break;

                if (!this.isRunning()) return null;
            }

            var buf = got.Buffer.Slice(0, 4);

            buf.CopyTo(fourBytesBuf);

            var num = BitConverter.ToInt32(fourBytesBuf);
            // 从网络端序转换过来
            num = IPAddress.NetworkToHostOrder(num);

            reader.AdvanceTo(buf.End);

            return num;
        }

        while (this.isRunning())
        {
            // check we have packet to read
            var hasPacket = await readInt();

            if (!hasPacket.HasValue) break;

            var length = hasPacket.Value;
            var strLength = (await readInt()).Value;

            // read id
            var got = await reader.ReadAtLeastAsync(strLength);

            var id = Guuid.Parse(Encoding.UTF8.GetString(got.Buffer.Slice(0, strLength)));

            reader.AdvanceTo(got.Buffer.Slice(0, strLength).End);
            // read packet
            got = await reader.ReadAtLeastAsync(length);

            var packet = this.Packetizer.ConvertPacket(id, got.Buffer.Slice(0, length));

            reader.AdvanceTo(got.Buffer.GetPosition(length));
            // release packet
            new Task(async () =>
            {
                try
                {
                    var handled = await this.Dispatcher.DispatchPacket(id, packet);

                    if (!handled) this.Logger.LogWarning("Packet with id {} has no handler", id);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error when handle packet {}", id);
                }
            }).Start();
        }
    }

    private async Task InputLoop()
    {
        // wait for shutdown sign
        var one = this._ReadLoop();
        var two = this._ProcessLoop();
        Task all = Task.WhenAny(one, two);

        while (this.isRunning())
        {
            if (all.IsCompleted) break;
            await Task.Yield();
        }

        try
        {
            Task.WaitAll(one, two);
        }
        catch (Exception e)
        {
            this.Logger.LogError(e, "Socket Connection Error");

            // fire event
            this.FireEvent(e);
        }

        // fire event
        this.FireEvent(null);
        // shutdown
        this.Disconnect();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
        {
            this._socket.Shutdown();
            this._socket.Dispose();
        }

        this._disposed = true;
    }
}