// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using IContainer = Autofac.IContainer;

namespace Utopia.Core;

public class LaunchEventArgs : EventArgs
{
    public required IContainer Container { get; set; }
}

public abstract class Launcher<T>
{
    public T Option { get; init; }

    protected WeakThreadSafeEventSource<LaunchEventArgs> _source = new();

    public event EventHandler<LaunchEventArgs> LaunchEvent
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

    public ContainerBuilder? Builder { get; protected set; } = new();

    public IContainer? Container { get; protected set; } = null;

    public IServiceProvider? ServiceProvider { get; protected set; }

    public Task? MainTask { get; protected set; } = null;

    public bool IsLaunched { get; protected set; } = false;

    protected abstract void _BuildDefaultContainer();

    protected abstract void _MainThread();

    public Launcher(T option)
    {
        Option = option;
    }

    public void InjectDefaultDependences()
    {
        this._BuildDefaultContainer();
    }

    /// <summary>
    /// 启动。启动之后<see cref="Builder"/>被置为null。
    /// <see cref="Container"/>和<see cref="MainTask"/>为非null。
    /// <see cref="IsLaunched"/>为true。
    /// </summary>
    public void Launch()
    {
        if (IsLaunched)
        {
            return;
        }

        IsLaunched = true;

        // set main thread task
        TaskCompletionSource source = new();
        MainTask = source.Task;

        // build container
        Container = Builder!.Build();
        ServiceProvider = new AutofacServiceProvider(Container);
        Builder = null;
        _source.Fire(this, new LaunchEventArgs { Container = Container });

        // launch
        Thread thread = new(() =>
        {
            try
            {
                _MainThread();
            }
            catch (Exception ex)
            {
                source.TrySetException(ex);
            }
            finally
            {
                source.TrySetResult();
            }
        })
        {
            Name = $"{typeof(T).Name} Thread",
            Priority = ThreadPriority.AboveNormal,
            IsBackground = false
        };
        thread.Start();
    }
}
