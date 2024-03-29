// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using System.Net.Sockets;
using Autofac;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Utopia.Core;
using Utopia.Core.Exceptions;
using Utopia.Core.Net;

namespace Utopia.Server.Net;

public sealed class SocketCreatedEvent(ISocket socket, IConnectionHandler connectHandler)
{
    public ISocket Socket { get; } = socket;

    private IConnectionHandler _handler = connectHandler;

    public IConnectionHandler ConnectHandler
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
    /// 构建<see cref="IConnectionHandler.Container"/>的容器.
    /// The <see cref="ISocket"/> and <see cref="IConnectionHandler"/> was registered to the builder.
    /// </summary>
    public event Action<ContainerBuilder> ClientContainerCreateEvent;

    /// <summary>
    /// 在套接字连接上的时候触发此事件.
    /// </summary>
    public event Action<SocketCreatedEvent> SocketCreatedEvent;

    /// <summary>
    /// 连接池。
    /// </summary>
    public IEnumerable<IConnectionHandler> ConnectionPool { get; }

    public void Stop();

    /// <summary>
    /// 网络线程Task.代表网络现场的状态。
    /// </summary>
    Task Task { get; }
}

/// <summary>
/// 网络线程
/// </summary>
internal sealed class InternetMain : IInternetMain
{
    public required IInternetListener InternetListener { get; init; }

    public InternetMain(IEventBus eventBus, ILogger<InternetMain> logger, Launcher.LauncherOption option)
    {
        eventBus.Register<LifeCycleEvent<LifeCycle>>((cycle) =>
        {
            if (cycle is not { Cycle: LifeCycle.StartNetThread, Order: LifeCycleOrder.Current })
            {
                return;
            }

            var thread = new Thread(() =>
            {
                // 注册关闭事件
                eventBus.Register<LifeCycleEvent<LifeCycle>>((stopCycle) =>
                {
                    if (stopCycle is { Cycle: LifeCycle.Stop, Order: LifeCycleOrder.Current })
                    {
                        Stop();
                    }
                });
                try
                {
                    Run().Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Internet Thread Crashed");
                }
                finally
                {
                    Stop();
                }
            })
            {
                Name = "Server Networking Thread"
            };
            thread.Start();
        });
    }

    public required ILifetimeScope Container { get; init; }

    private readonly List<IConnectionHandler> _clients = new();

    private readonly TaskCompletionSource _source = new();

    public void Stop()
    {
        _source.TrySetResult();
    }

    public Task Task => _source.Task;

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

    public IEnumerable<IConnectionHandler> ConnectionPool
    {
        get
        {
            lock (this._lock)
            {
                return this._clients.ToArray();
            }
        }
    }

    private void Unregister(IConnectionHandler connectHandler)
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

    public async Task Run()
    {
        try
        {
            while (!Task.IsCompleted)
            {
                Task<ISocket> accept = InternetListener.Accept();
                await accept;

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
                        .As<IConnectionHandler>();

                    this._clientSource.Fire(builder, false);
                });
                // create connection
                try
                {
                    var e = new SocketCreatedEvent(
                        socket,
                        container.Resolve<IConnectionHandler>());
                    this._socketSource.Fire(e, false);

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
        catch (Exception e)
        {
            _source.TrySetException(e);
        }
        finally
        {
            _source.TrySetResult();
        }
    }
}
