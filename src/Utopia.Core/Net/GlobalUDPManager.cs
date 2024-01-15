// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Utopia.Core.Net;

/// <summary>
/// 这个类负责管理应用层面的UDP链接.
/// </summary>
public class GlobalUDPManager
{
    public required ILogger<GlobalUDPManager> Logger { private get; init; }

    public GlobalUDPManager()
    {
        Task.Run(MapMaintainer);
    }

    private SpinLock _spinLock = new();

    private readonly List<UDPSocket> _sockets = [];

    private readonly ConcurrentDictionary<string, bool> _maintainerStarted = [];

    private UDPSocket[] GetSockets()
    {
        bool lockTaken = false;
        UDPSocket[] sockets;
        try
        {
            _spinLock.Enter(ref lockTaken);
            sockets = _sockets.ToArray();
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit();
            }
        }
        return sockets;
    }

    private void RemoveSocket(UDPSocket socket)
    {
        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);
            _sockets.Remove(socket);
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit();
            }
        }
    }

    private void AddSocket(UDPSocket socket)
    {
        bool lockTaken = false;
        try
        {
            _spinLock.Enter(ref lockTaken);
            _sockets.Add(socket);
        }
        finally
        {
            if (lockTaken)
            {
                _spinLock.Exit();
            }
        }
    }

    private async Task MapMaintainer()
    {
        while (true)
        {
            try
            {
                foreach (var socket in GetSockets())
                {
                    if (!socket.Alive)
                    {
                        RemoveSocket(socket);
                        break;
                    }
                }

                await Task.Yield();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "The UDP maintainer get an error.");
            }
        }
    }

    /// <summary>
    /// 这个维护者负责全局转发收到的UDP包.
    /// </summary>
    private async Task UdpMaintainer(
        EndPoint bindTo)
    {
        try
        {
            var allAddress = new IPEndPoint(IPAddress.Any, 0);
            Socket udp = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udp.Bind(bindTo);
            while (true)
            {
                try
                {
                    using var buffer = MemoryPool<byte>.Shared.Rent(256);
                    var result = await udp.ReceiveFromAsync(buffer.Memory, allAddress);

                    // Debugger.Break();

                    var remote = ToStandardPoint(result.RemoteEndPoint);

                    foreach (var socket in GetSockets())
                    {
                        var writer = socket.ReceivePipe.Writer;
                        var memory = writer.GetMemory(result.ReceivedBytes);

                        buffer.Memory.Slice(0, result.ReceivedBytes).CopyTo(memory);

                        writer.Advance(result.ReceivedBytes);
                        await writer.FlushAsync();
                    }

                    await Task.Yield();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "The UDP maintainer get an error when loop.");
                }
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "The UDP maintainer get an error when initialize.");
            throw;
        }
    }

    private static IPEndPoint ToStandardPoint(EndPoint? endPoint)
    {
        return new IPEndPoint(
            (endPoint as IPEndPoint)!.Address.MapToIPv4(),
            (endPoint as IPEndPoint)!.Port);
    }

    private static string ToStandardString(IPEndPoint endPoint)
    {
        return string.Format("{0}-{1}",endPoint.Address.ToString(),endPoint.Port).ToLower().Trim();
    }

    public void StartUDPListenFor(UDPSocket socket)
    {
        Guard.IsNotNull(socket);

        AddSocket(socket);

        var localAddress = ToStandardPoint(socket.LocalAddress);

        _ = _maintainerStarted.GetOrAdd(ToStandardString(localAddress), (local) =>
        {
            Task.Run(() => UdpMaintainer(localAddress));
            return true;
        });
    }

    public void EndUpFor(UDPSocket socket)
    {
        RemoveSocket(socket);
    }
}
