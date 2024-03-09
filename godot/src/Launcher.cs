// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Godot;
using Microsoft.Extensions.Logging;
using Utopia.Core;
using IContainer = Autofac.IContainer;

namespace Utopia.Godot;

/// <summary>
/// 启动器
/// </summary>
public class Launcher : IDisposable
{
    /// <summary>
    /// 启动参数
    /// </summary>
    public class LaunchOptions
    {

    }

    private WeakThreadSafeEventSource<IContainer> _source = new();

    public event Action<IContainer> GameLaunch
    {
        add
        {
            _source.Register(value);
        }
        remove
        {
            _source.Unregister(value);
        }
    }

    public ContainerBuilder Builder { get; set; } = new();

    public LaunchOptions Options { get; }

    public Launcher(LaunchOptions options, Node root)
    {
        ArgumentNullException.ThrowIfNull(options);
        Options = options;
        _RegisterDefault(root);
    }

    private void _RegisterDefault(Node root)
    {
        Builder
            .RegisterType<MainThread>()
            .SingleInstance()
            .AsSelf();
        Builder
            .RegisterInstance(root)
            .SingleInstance()
            .AsSelf();
    }

    protected bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Launcher()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {

        }

        _disposed = true;
    }

    public bool Launched { get; private set; }

    public IContainer? Container { get; private set; } = null;

    public void Launch()
    {
        if (Launched)
        {
            throw new InvalidOperationException("you can not launch the game more than once");
        }
        if (_disposed)
        {
            throw new InvalidOperationException("the launcher was disposed");
        }

        Launched = true;

        // build container
        Container = Builder.Build();

        _source.Fire(Container);

        // add to node to update
        var main = Container.Resolve<MainThread>();
        var node = Container.Resolve<Node>();

        node.AddChild(main);
    }
}
