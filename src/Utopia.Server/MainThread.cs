// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Utopia.Core;
using Utopia.Server.Logic;
using Utopia.Server.Map;
using Utopia.Server.Net;
using static Utopia.Server.Launcher;
using Utopia.Core.Plugin;
using Autofac;
using Utopia.Core.IO;
using Utopia.Core.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Utopia.Core.Net;
using System.Net.Sockets;
using System.Net;
using Utopia.Core.Net.Packet;
using MemoryPack;

namespace Utopia.Server;

public sealed class MainThread
{
    public required ILogger<MainThread> Logger { get; init; }

    public required IEventBus EventBus { get; init; }

    public required IPluginProvider PluginProvider { get; init; }

    public required IPluginLoader<IPlugin> PluginLoader { get; init; }

    public required IResourceLocator ResourceLocator { get; init; }

    public required ILifetimeScope Container { get; init; }

    public required Options Option { get; init; }

    public required ILogicThread LogicThread { get; init; }

    public required ConcurrentDictionary<Guuid, IWorld> Worlds { get; init; }

    public required ConcurrentDictionary<Guuid, IWorldFactory> WorldFactories { get; init; }

    public LifeCycle CurrentLifeCycle { get; private set; }

    private readonly TaskCompletionSource _startFinishSource = new();

    public TaskCompletionSource GracefulStop { get; init; } = new();

    /// <summary>
    /// 启动任务。当此任务完成的时候代表服务器已经完成启动。但是此任务不指示服务器状态（包括停止，异常等）。
    /// </summary>
    public Task StartTask => _startFinishSource.Task;

    private void _LoadSave()
    {
        KeyValuePair<Guuid, IWorldFactory>[] array = WorldFactories.ToArray();

        // TODO: Load saves from file
        if (array.Length == 1)
        {
            Worlds.TryAdd(Guuid.Unique(), array[0].Value.GenerateNewWorld());
        }
    }

    private void _InitLoggingSystem()
    {
        LogManager.Init(Option.LogOption ?? LogManager.LogOption.CreateBatch());
    }

    /// <summary>
    /// 使用参数启动服务器
    /// </summary>
    public void Launch()
    {
        Thread.CurrentThread.Name = "Server Main Thread";
        CancellationTokenSource kestrelSource = new();

        EventBus.Register<LifeCycleEventArgs<LifeCycle>>((_, e) =>
        {
            if (e is { Cycle: LifeCycle.InitializedSystem, Order: LifeCycleOrder.Current })
            {
                _InitLoggingSystem();
                Logger.LogInformation("log system initialized");
                ResourceLocator.CreateIfNotExist();
                Logger.LogInformation("create directory for resources");
            }
        });
        EventBus.Register<LifeCycleEventArgs<LifeCycle>>((_, e) =>
        {
            if (e is { Cycle: LifeCycle.StartNetThread, Order: LifeCycleOrder.Current })
            {
                Logger.LogInformation("start kestrel server");
                Container.Resolve<KestrelServer>().StartAsync(new EmptyApplication(), kestrelSource.Token).ContinueWith((t) =>
                {
                    Logger.LogInformation("kestrel started");
                });
            }
        });
        EventBus.Register<LifeCycleEventArgs<LifeCycle>>((_, e) =>
        {
            if (e is { Cycle: LifeCycle.Stop, Order: LifeCycleOrder.Current })
            {
                Logger.LogInformation("stop kestrel server");
                kestrelSource.Cancel();
                Container.Resolve<KestrelServer>().StopAsync(kestrelSource.Token).Wait();
                Logger.LogInformation("kestrel server stoped");
            }
            else if (e is { Cycle: LifeCycle.Crash, Order: LifeCycleOrder.Current })
            {
                Logger.LogInformation("stop kestrel server");
                kestrelSource.Cancel();
                var cancel = new CancellationTokenSource();
                cancel.Cancel();
                Container.Resolve<KestrelServer>().StopAsync(cancel.Token).Wait();
                Logger.LogInformation("kestrel server stoped");
            }
        });

        try
        {
            // 初始化系统
            ChangeLifecycle(LifeCycle.InitializedSystem);

            // 加载插件
            ChangeLifecycle(LifeCycle.LoadPlugin);

            // 创建世界
            ChangeLifecycle(LifeCycle.LoadSavings);

            // 设置逻辑线程
            ChangeLifecycle(LifeCycle.StartLogicThread);

            // 设置网络线程
            ChangeLifecycle(LifeCycle.StartNetThread);

            // finish
            _startFinishSource.SetResult();

            // stop when any of threads stop
            var task = Task.WhenAny(LogicThread.Task, GracefulStop.Task);

            task.Wait();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "the server crash");
            ChangeLifecycle(LifeCycle.Crash);
            throw;
        }
        finally
        {
            Logger.LogInformation("stop");
            ChangeLifecycle(LifeCycle.Stop);
        }

        return;

        void ChangeLifecycle(LifeCycle lifeCycle)
        {
            LifeCycleEventArgs<LifeCycle>.EnterCycle(
                                                 lifeCycle,
                                                 Logger,
                                                 this,
                                                 EventBus,
                                                 () =>
                                                 {
                                                     CurrentLifeCycle = lifeCycle;
                                                 });
        }
    }
}
