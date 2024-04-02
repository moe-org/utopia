// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Utopia.Core.Net;

namespace Utopia.Godot.src.Net;

public enum ConnectionType
{
    Tcp,
    UdpKcp
}

/// <summary>
/// 连接器
/// </summary>
public class Connector : IDisposable
{
    private Socket Socket { get; set; } = null!;

    private bool _disposed = false;

    public void Connect(string address, int port, ConnectionType type)
    {
        if (type == ConnectionType.Tcp)
        {
            var tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            tcp.Connect(address, port);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    ~Connector()
    {
        Dispose(false);
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
            Socket.SafeHandle.DangerousRelease();
            Socket.Dispose();
        }
    }

    /// <summary>
    /// This was parepared for the Kestrel server
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ulong GetHandle()
    {
        bool success = false;

        Socket.SafeHandle.DangerousAddRef(ref success);

        if (!success)
        {
            throw new InvalidOperationException("failed to add reference to the safe handle");
        }

        return BitConverter.ToUInt64(BitConverter.GetBytes(Socket.SafeHandle.DangerousGetHandle().ToInt64()));
    }
}
