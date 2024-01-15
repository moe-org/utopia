// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;

namespace Utopia.Core.Net;

/// <summary>
/// 对于<see cref="Socket"/>的封装(KCP).
/// </summary>
/// <param name="socket"></param>
public class StandardSocket(Socket socket) : ISocket
{
    private bool _disposed = false;

    private readonly Socket _socket = socket;

    public bool Alive { get; private set; } = true;

    public async Task SocketMaintainer()
    {
        while (!_disposed)
        {
            if (!Alive)
            {
                return;
            }

            // try ping
            var result = await Utilities.TryPing((_socket.RemoteEndPoint as IPEndPoint)?.Address
                ?? throw new NotImplementedException("not implement for no IpEndPoint"));

            if(result == null)
            {
                Alive = false;
                return;
            }

            // check tcp
            bool part1 = _socket.Poll(2000, SelectMode.SelectRead);
            bool part2 = (_socket.Available == 0);

            if (part1 && part2)
                Alive = false;

            // ping per five seconds check once
            await Task.Delay(1000 * 5);
        }
    }

    public EndPoint? RemoteAddress => _socket.RemoteEndPoint;

    public EndPoint? LocalAddress => _socket.LocalEndPoint;

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
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Dispose();
        }

        Alive = false;
        _disposed = true;
    }

    public async Task<int> Read(Memory<byte> dst)
    {
        if (!Alive)
        {
            return 0;
        }

        return await _socket.ReceiveAsync(dst);
    }

    public void Shutdown()
    {
        Dispose();
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        if (!Alive)
        {
            return;
        }

        await _socket.SendAsync(data);
    }
}
