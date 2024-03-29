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
using IContainer = Autofac.IContainer;

namespace Utopia.Core;
public abstract class Launcher<T>(T option)
{
    public T Option { get; init; } = option;

    protected WeakThreadSafeEventSource<IContainer> _source = new();

    public event Action<IContainer> LaunchEvent
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

    public Task? MainTask { get; protected set; } = null;

    public bool IsLaunched { get; protected set; } = false;

    protected abstract void _BuildDefaultContainer();

    protected abstract void _MainThread();

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
        Builder = null;
        _source.Fire(Container);

        // launch
        Thread thread = new Thread(() =>
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
