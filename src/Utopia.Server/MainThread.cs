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

namespace Utopia.Server;

public sealed class MainThread
{
    public required ILogger<MainThread> Logger { get; init; }

    public required IEventBus EventBus { get; init; }

    public required IPluginProvider PluginProvider { get; init; }

    public required IPluginLoader<IPlugin> PluginLoader { get; init; }

    public required IResourceLocator ResourceLocator { get; init; }

    public required ILifetimeScope Container { get; init; }

    public required LauncherOption Option { get; init; }

    public required ILogicThread LogicThread { get; init; }

    public required IInternetMain InternetMain { get; init; }

    public required IInternetListener InternetListener { get; init; }

    public required ConcurrentDictionary<Guuid, IWorld> Worlds { get; init; }

    public required ConcurrentDictionary<Guuid, IWorldFactory> WorldFactories { get; init; }

    public LifeCycle CurrentLifeCycle { get; private set; }

    private readonly TaskCompletionSource _startFinishSource = new();

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

        EventBus.Register<LifeCycleEvent<LifeCycle>>(e =>
        {
            if (e is { Cycle: LifeCycle.InitializedSystem, Order: LifeCycleOrder.Current })
            {
                _InitLoggingSystem();
                Logger.LogInformation("log system initialized");
            }
        });


        ResourceLocator.CreateIfNotExist();

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
            var task = Task.WhenAll(LogicThread.Task, InternetMain.Task);

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
            LifeCycleEvent<LifeCycle>.EnterCycle(
                                                 lifeCycle,
                                                 Logger,
                                                 EventBus,
                                                 () =>
                                                 {
                                                     CurrentLifeCycle = lifeCycle;
                                                 });
        }
    }
}
