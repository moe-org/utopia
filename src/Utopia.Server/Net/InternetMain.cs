// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using System.Net.Sockets;
using Autofac;
using CommunityToolkit.Diagnostics;
using Utopia.Core;
using Utopia.Core.Exceptions;
using Utopia.Core.Net;

namespace Utopia.Server.Net;

public sealed class SocketCreatedEvent(ISocket socket,IConnectHandler connectHandler)
{
    public ISocket Socket { get; } = socket;

    private IConnectHandler _handler = connectHandler;

    public IConnectHandler ConnectHandler
    {
        get => this._handler;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this._handler = value;
        }
    }
}

/// <summary>
/// 网络线程。负责维护连接池.
/// </summary>
public interface IInternetMain
{
    /// <summary>
    /// 构建<see cref="IConnectHandler.Container"/>的容器.
    /// The <see cref="ISocket"/> and <see cref="IConnectHandler"/> was registered to the builder.
    /// </summary>
    public event Action<ContainerBuilder> ClientContainerCreateEvent;

    /// <summary>
    /// 在套接字连接上的时候触发此事件.
    /// </summary>
    public event Action<SocketCreatedEvent> SocketCreatedEvent;

    /// <summary>
    /// 连接池。
    /// </summary>
    public IEnumerable<IConnectHandler> ConnectionPool { get; }

    /// <summary>
    /// 网络线程停机/崩溃 source。
    /// </summary>
    CancellationTokenSource StopTokenSource { get; }
}

/// <summary>
/// 网络线程
/// </summary>
internal sealed class InternetMain : IInternetMain
{
    public required IInternetListener InternetListener { get; init; }

    public required ILifetimeScope Container { get; init; }

    /// <summary>
    /// TODO: check client alive frequaently and shutdown all the clients when shutdown
    /// </summary>
    private readonly List<IConnectHandler> _clients = new();

    public CancellationTokenSource StopTokenSource { get; } = new();
    
    private readonly object _lock = new();

    private readonly WeakThreadSafeEventSource<ContainerBuilder> _clientSource = new();

    private readonly WeakThreadSafeEventSource<SocketCreatedEvent> _socketSource = new();
    
    public event Action<ContainerBuilder> ClientContainerCreateEvent
    {
        add => this._clientSource.Register(value);
        remove => this._clientSource.Unregister(value);
    }

    public event Action<SocketCreatedEvent> SocketCreatedEvent
    {
        add => this._socketSource.Register(value);
        remove => this._socketSource.Unregister(value);
    }

    public IEnumerable<IConnectHandler> ConnectionPool
    {
        get
        {
            lock (this._lock)
            {
                return this._clients.ToArray();
            }
        }
    }

    private void Unregister(IConnectHandler connectHandler)
    {
        lock (this._lock)
        {
            this._clients.Remove(connectHandler);
        }
    }

    private void RemoveDeadConnection()
    {
        lock (this._lock)
        {
            this._clients.RemoveAll((client) => client.Running);
        }
    }

    public async Task Run(CancellationTokenSource startTokenSource)
    {
        try
        {
            while (!StopTokenSource.IsCancellationRequested)
            {
                // wait for accept
                startTokenSource.CancelAfter(100 /* wait for fun :-) */);

                Task<ISocket> accept = InternetListener.Accept();
                await accept.WaitAsync(StopTokenSource.Token);

                if (StopTokenSource.IsCancellationRequested)
                {
                    accept.Dispose();
                    return;
                }

                var socket = accept.Result;

                // construct container
                var container = Container.BeginLifetimeScope((builder) =>
                {
                    builder
                        .RegisterInstance(socket)
                        .SingleInstance();

                    builder
                        .RegisterType<ConnectHandler>()
                        .SingleInstance()
                        .As<IConnectHandler>();

                    this._clientSource.Fire(builder, false);
                });
                // create connection
                try
                {
                    var e = new SocketCreatedEvent(
                                                   socket,
                                                   container.Resolve<IConnectHandler>());
                    this._socketSource.Fire(e,false);

                    // add to the pool
                    lock (this._lock)
                    {
                        this._clients.Add(e.ConnectHandler);

                        // unregister if not alive
                        e.ConnectHandler.ConnectionClosed += (_) => Unregister(e.ConnectHandler);
                    }
                }
                catch
                {
                    container.Dispose();
                    throw;
                }
                
                // clear dead connections
                RemoveDeadConnection();
            }
        }
        finally
        {
            this.StopTokenSource.Cancel();
        }
    }
}
