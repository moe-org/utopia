// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using Autofac;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using Microsoft.Extensions.Logging;

namespace Utopia.Core.Net;

public class ConnectHandler : IConnectHandler
{
    public required ILogger<ConnectHandler> Logger { protected get; init; }

    public required ILifetimeScope Container { get; init; }

    public IDispatcher Dispatcher { get; } = new Dispatcher();

    public IPacketizer Packetizer { get; } = new Packetizer();

    private readonly WeakThreadSafeEventSource<Exception?> _event = new();

    public event Action<Exception?> ConnectionClosed
    {
        add
        {
            _event.Register(value);
        }
        remove
        {
            _event.Unregister(value);
        }
    }

    /// <summary>
    /// once true,never false
    /// </summary>
    private volatile bool _eventFired = false;

    private volatile bool _disposed = false;

    private readonly object _lock = new();

    private readonly ISocket _socket;

    private readonly Pipe _pipe = new();

    public bool Running => isRunning();

    /// <summary>
    /// Call under the lock <see cref="_lock"/>
    /// </summary>
    /// <param name="exception"></param>
    private void FireEvent(Exception? exception)
    {
        lock (_lock)
        {
            if (_eventFired) return;
            _eventFired = true;

            // no error
            var ex = _event.Fire(exception, true);

            foreach (var e in ex)
            {
                Logger.LogError(e, "get an error when firing ConnectionClosed event");
            }
        }
    }

    private bool isRunning()
    {
        return !_disposed;
    }

    public ConnectHandler(ISocket socket)
    {
        Guard.IsNotNull(socket);
        _socket = socket;

        if (!_socket.Alive)
        {
            throw new ArgumentException("the socket haven't connect yet");
        }

        Task.Run(InputLoop);
    }

    private async Task _ReadLoop()
    {
        PipeWriter writer = _pipe.Writer;

        // true for continue
        while (isRunning())
        {
            Memory<byte> memory = writer.GetMemory(256);

            int bytesRead = await _socket.Read(memory);

            writer.Advance(bytesRead);

            await writer.FlushAsync();
            await Task.Yield();
        }
        await writer.CompleteAsync();
    }

    private async Task _ProcessLoop()
    {
        PipeReader reader = _pipe.Reader;
        byte[] fourBytesBuf = new byte[4];

        // return null for socket disconnect
        async Task<int?> readInt()
        {
            ReadResult got;
            while (true)
            {
                got = await reader.ReadAtLeastAsync(4);

                if (!got.IsCompleted && !got.IsCanceled)
                {
                    // read all
                    break;
                }

                if (!isRunning())
                {
                    return null;
                }
            }

            ReadOnlySequence<byte> buf = got.Buffer.Slice(0, 4);

            buf.CopyTo(fourBytesBuf);

            int num = BitConverter.ToInt32(fourBytesBuf);
            // 从网络端序转换过来
            num = IPAddress.NetworkToHostOrder(num);

            reader.AdvanceTo(buf.End);

            return num;
        }

        while (isRunning())
        {
            // check we have packet to read
            var hasPacket = await readInt();

            if (!hasPacket.HasValue)
            {
                break;
            }

            int length = hasPacket.Value;
            int strLength = (await readInt()).Value;

            // read id
            ReadResult got = await reader.ReadAtLeastAsync(strLength);

            var id = Guuid.Parse(Encoding.UTF8.GetString(got.Buffer.Slice(0, strLength)));

            reader.AdvanceTo(got.Buffer.Slice(0, strLength).End);
            // read packet
            got = await reader.ReadAtLeastAsync(length);

            object packet = Packetizer.ConvertPacket(id, got.Buffer.Slice(0, length));

            reader.AdvanceTo(got.Buffer.GetPosition(length));
            // release packet
            new Task(async () =>
            {
                try
                {
                    var handled = await Dispatcher.DispatchPacket(id, packet);

                    if (!handled)
                    {
                        Logger.LogWarning("Packet with id {} has no handler", id);
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex,"Error when handle packet {}", id);
                }
            }).Start();
        }
    }

    private async Task InputLoop()
    {
        // wait for shutdown sign
        var one = _ReadLoop();
        var two = _ProcessLoop();
        Task all = Task.WhenAny(one, two);

        while (isRunning())
        {
            if (all.IsCompleted)
            {
                break;
            }
            await Task.Yield();
        }

        try
        {
            Task.WaitAll(one, two);
        }
        catch(Exception e)
        {
            Logger.LogError(e, "Socket Connection Error");

            // fire event
            FireEvent(e);
        }

        // fire event
        FireEvent(null);
        // shutdown
        Disconnect();
    }

    public void WritePacket(Guuid packetTypeId, object obj)
    {
        var data = Packetizer.WritePacket(packetTypeId, obj);

        lock (_lock)
        {
            byte[] encoderedId = Encoding.UTF8.GetBytes(packetTypeId.ToString());

            // 转换到网络端序
            byte[] length = BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder(data.Length)
                );

            byte[] idLength = BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder(encoderedId.Length)
                );

            _socket.Write(length);
            _socket.Write(idLength);
            _socket.Write(encoderedId);
            _socket.Write(data);
        }
    }

    public void Disconnect()
    {
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _socket.Shutdown();
            _socket.Dispose();
        }

        _disposed = true;
    }
}

