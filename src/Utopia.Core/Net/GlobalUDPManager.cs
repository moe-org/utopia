#region

using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     这个类负责管理应用层面的UDP链接.
/// </summary>
public class GlobalUDPManager
{
    private readonly ConcurrentDictionary<string, bool> _maintainerStarted = [];

    private readonly List<UDPSocket> _sockets = [];

    private SpinLock _spinLock;

    public GlobalUDPManager()
    {
        Task.Run(this.MapMaintainer);
    }

    public required ILogger<GlobalUDPManager> Logger { private get; init; }

    private UDPSocket[] GetSockets()
    {
        var lockTaken = false;
        UDPSocket[] sockets;
        try
        {
            this._spinLock.Enter(ref lockTaken);
            sockets = this._sockets.ToArray();
        }
        finally
        {
            if (lockTaken) this._spinLock.Exit();
        }

        return sockets;
    }

    private void RemoveSocket(UDPSocket socket)
    {
        var lockTaken = false;
        try
        {
            this._spinLock.Enter(ref lockTaken);
            this._sockets.Remove(socket);
        }
        finally
        {
            if (lockTaken) this._spinLock.Exit();
        }
    }

    private void AddSocket(UDPSocket socket)
    {
        var lockTaken = false;
        try
        {
            this._spinLock.Enter(ref lockTaken);
            this._sockets.Add(socket);
        }
        finally
        {
            if (lockTaken) this._spinLock.Exit();
        }
    }

    /// <summary>
    ///     负责对not Alive的sockets进行清空.
    /// </summary>
    private async Task MapMaintainer()
    {
        while (true)
            try
            {
                foreach (var socket in this.GetSockets())
                    if (!socket.Alive)
                    {
                        this.RemoveSocket(socket);
                        break;
                    }

                await Task.Yield();
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "The UDP maintainer get an error.");
            }
    }

    /// <summary>
    ///     这个维护者负责全局转发收到的UDP包.
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
                try
                {
                    using var buffer = MemoryPool<byte>.Shared.Rent(2048);
                    var result = await udp.ReceiveFromAsync(buffer.Memory, allAddress);

                    var remote = ToStandardPoint(result.RemoteEndPoint);

                    foreach (var socket in this.GetSockets())
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
                    this.Logger.LogError(ex, "The UDP maintainer get an error when loop.");
                }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "The UDP maintainer get an error when initialize.");
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
        return string.Format("{0}-{1}", endPoint.Address.ToString(), endPoint.Port).ToLowerInvariant().Trim();
    }

    public void StartUDPListenFor(UDPSocket socket)
    {
        Guard.IsNotNull(socket);

        this.AddSocket(socket);

        var localAddress = ToStandardPoint(socket.LocalAddress);

        _ = this._maintainerStarted.GetOrAdd(ToStandardString(localAddress), local =>
        {
            Task.Run(() => this.UdpMaintainer(localAddress));
            return true;
        });
    }

    public void EndUpFor(UDPSocket socket)
    {
        this.RemoveSocket(socket);
    }
}