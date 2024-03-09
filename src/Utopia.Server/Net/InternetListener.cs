// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Buffers;
using System.CodeDom;
using System.Net;
using System.Net.Sockets;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using Utopia.Core;
using Utopia.Core.Net;

namespace Utopia.Server.Net;

/// <summary>
/// using TCP
/// </summary>
internal class InternetListener : IInternetListener
{
    public required ILogger<InternetListener> Logger { private get; init; }

    private readonly WeakThreadSafeEventSource<SocketAcceptEvent> _source = new();

    private bool _disposed = false;
    private readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private int? _port = null;
    private readonly object _lock = new();

    public InternetListener(IEventBus eventBus, Launcher.LauncherOption option, ILogger<InternetListener> logger)
    {
        eventBus.Register<LifeCycleEvent<LifeCycle>>(
        status =>
        {
            if (status is not { Cycle: LifeCycle.StartNetThread, Order: LifeCycleOrder.Before })
            {
                return;
            }

            logger.LogInformation("listen port {port}", option.Port);
            Listen(option.Port);
        });
    }

    public event Action<SocketAcceptEvent> AcceptEvent
    {
        add => this._source.Register(value);
        remove => this._source.Unregister(value);
    }

    public int Port
    {
        get
        {
            lock (_lock)
            {
                return _port ?? throw new InvalidOperationException("the net server is not working now!");
            }
        }
    }

    public bool Working
    {
        get
        {
            lock (_lock)
            {
                return this._port != null;
            }
        }
    }

    public async Task<ISocket> Accept()
    {
        lock (_lock)
        {
            if (this._port == null)
            {
                throw new InvalidOperationException("the server is not working now!");
            }
        }

        var socket = await this._socket.AcceptAsync();

        var e = new TcpAcceptEvent(socket, new StandardSocket(socket));
        this._source.Fire(e);

        return e.Socket;
    }

    public void Listen(int port)
    {
        if (this._disposed)
        {
            throw new InvalidOperationException("can not listen after disposing");
        }

        lock (_lock)
        {
            if (this._port != null)
            {
                throw new InvalidOperationException("the server has started!");
            }

            _port = port;

            this._socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }
    }

    public void Shutdown()
    {
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            lock (_lock)
            {
                _port = null;
                _socket.Close();
                _socket.Dispose();
            }
        }

        _disposed = true;
    }
}
