// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Utopia.Core.Logging;

namespace Utopia.Core.Net;

public class UDPSocket : ISocket
{
    public ILogger<UDPSocket> Logger { protected get; init; }

    public GlobalUDPManager UdpManager { protected get; init; }

    internal readonly Pipe ReceivePipe = new();

    private bool _disposed = false;

    public UDPSocket(
        Socket socket,
        IPEndPoint local,
        IPEndPoint remote,
        ILogger<UDPSocket> logger,
        GlobalUDPManager manager)
    {
        Guard.IsNotNull(socket);
        Guard.IsNotNull(logger);
        Logger = logger;

        _udpSocket = socket;

        UdpManager = manager;

        Remote = remote;
        Local = local;

        manager.StartUDPListenFor(this);
    }

    private readonly Socket _udpSocket;

    public bool Alive => !_disposed;

    private IPEndPoint Local { get; init; }

    private IPEndPoint Remote { get; init; }

    public EndPoint? RemoteAddress => Remote;

    public EndPoint? LocalAddress => Local;

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
            UdpManager.EndUpFor(this);
            _udpSocket.Dispose();
        }

        _disposed = true;
    }
    public void Shutdown()
    {
        Dispose();
    }

    public async Task<int> Read(Memory<byte> dst)
    {
        if (!Alive)
        {
            return 0;
        }

        Debugger.Break();

        var reader = ReceivePipe.Reader;

        if(!reader.TryRead(out ReadResult result))
        {
            return 0;
        }

        var copied = (int)Math.Min(result.Buffer.Length, dst.Length);

        result.Buffer.Slice(0,copied).CopyTo(dst.Span.Slice(0, copied));

        reader.AdvanceTo(result.Buffer.Slice(0, copied).End);

        return copied;
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        if (!Alive)
        {
            return;
        }

        var index = 0;
        while (index != data.Length && Alive)
        {
            index += await _udpSocket.SendToAsync(data,(RemoteAddress as IPEndPoint)!);
        }
    }
}
