#region

using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

#endregion

namespace Utopia.Core.Net;

public class UDPSocket : ISocket
{
    private readonly Socket _udpSocket;

    internal readonly Pipe ReceivePipe = new();

    private bool _disposed;

    public UDPSocket(
        IPEndPoint local,
        IPEndPoint remote,
        ILogger<UDPSocket> logger,
        GlobalUDPManager manager)
    {
        Guard.IsNotNull(logger);
        this.Logger = logger;

        this._udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        this.UdpManager = manager;

        this.Remote = new IPEndPoint(remote.Address.MapToIPv4(), remote.Port);
        this.Local = new IPEndPoint(local.Address.MapToIPv4(), local.Port);

        manager.StartUDPListenFor(this);
    }

    public ILogger<UDPSocket> Logger { protected get; init; }

    public GlobalUDPManager UdpManager { protected get; init; }

    private IPEndPoint Local { get; }

    private IPEndPoint Remote { get; }

    public bool Alive => !this._disposed;

    public EndPoint? RemoteAddress => this.Remote;

    public EndPoint? LocalAddress => this.Local;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Shutdown()
    {
        this.Dispose();
    }

    public Task<int> Read(Memory<byte> dst)
    {
        if (!this.Alive) return Task.FromResult(0);

        var reader = this.ReceivePipe.Reader;

        if (!reader.TryRead(out var result)) return Task.FromResult(0);

        var copied = (int)Math.Min(result.Buffer.Length, dst.Length);

        result.Buffer.Slice(0, copied).CopyTo(dst.Span.Slice(0, copied));

        reader.AdvanceTo(result.Buffer.Slice(0, copied).End);

        return Task.FromResult(copied);
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        if (!this.Alive) return;

        var index = 0;
        while (index != data.Length && this.Alive) index += await this._udpSocket.SendToAsync(data, this.Remote);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
        {
            this.UdpManager.EndUpFor(this);
            this._udpSocket.Dispose();
        }

        this._disposed = true;
    }
}
