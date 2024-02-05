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

namespace Utopia.Server;

public sealed class MainThread
{
    private readonly CancellationTokenSource _logicThreadStartWaitTokenSource = new();

    private readonly CancellationTokenSource _internetThreadStartWaitTokenSource = new();

    public required ILogger<MainThread> Logger { get; init; }

    public required IEventBus EventBus { get; init; }

    public required IPluginLoader<IPlugin> PluginLoader { get; init; }

    public required IResourceLocator FileSystem { get; init; }

    public required ILifetimeScope Container { get; init; }

    public required LauncherOption Option { get; init; }

    public required ILogicThread LogicThread { get; init; }

    public required IInternetMain InternetMain { get; init; }

    public required IInternetListener InternetListener { get;init; }

    public required ConcurrentDictionary<long, IWorld> Worlds { get; init; }

    public required ConcurrentDictionary<Guuid, IWorldFactory> WorldFactories { get; init; }

    public LifeCycle CurrentLifeCycle { get; private set; }

    private void _StartLogicThread()
    {
        var logicT = new Thread(() =>
        {
            KeyValuePair<long, IWorld>[] worlds = Worlds.ToArray();
            foreach (KeyValuePair<long, IWorld> world in worlds)
            {
                LogicThread.Updatables.Add(world.Value);
            }
            // 注册关闭事件
            EventBus.Register<LifeCycleEvent<LifeCycle>>((cycle) =>
            {
                if (cycle is { Cycle: LifeCycle.Stop, Order: LifeCycleOrder.After })
                {
                    LogicThread.StopTokenSource.Cancel();
                }
            });
            try
            {
                ((StandardLogicThread)LogicThread).Run(_logicThreadStartWaitTokenSource);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Logic Thread Crashed");
            }
            finally {
                LogicThread.StopTokenSource.Cancel();
            }
        })
        {
            Name = "Server Logic Thread"
        };
        logicT.Start();
    }


    /// <summary>
    /// 启动网络线程
    /// </summary>
    private void _StartNetworkThread()
    {
        InternetListener.Listen(Option.Port);
        Logger.LogInformation("listen to {port}", Option.Port);

        InternetMain netThread = (InternetMain)InternetMain;
        var thread = new Thread( () =>
        {
            // 注册关闭事件
            EventBus.Register<LifeCycleEvent<LifeCycle>>((cycle) =>
            {
                if (cycle is { Cycle: LifeCycle.Stop, Order: LifeCycleOrder.After })
                {
                    netThread.StopTokenSource.Cancel();
                }
            });
            try
            {
                netThread.Run(_internetThreadStartWaitTokenSource).Wait();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Internet Thread Crashed");
            }
            finally
            {
                netThread.StopTokenSource.Cancel();
            }
        })
        {
            Name = "Server Networking Thread"
        };
        thread.Start();
    }

    public void _LoadSave()
    {
        KeyValuePair<Guuid, IWorldFactory>[] array = WorldFactories.ToArray();

        // TODO: Load saves from file
        if (array.Length == 1)
        {
            Worlds.TryAdd(0, array[0].Value.GenerateNewWorld());
        }
    }

    /// <summary>
    /// 使用参数启动服务器
    /// </summary>
    /// <param name="startTokenSource">when the server started,cancel the token</param>
    public void Launch(CancellationTokenSource startTokenSource)
    {
        ArgumentNullException.ThrowIfNull(startTokenSource);

        Thread.CurrentThread.Name = "Server Main Thread";

        FileSystem.CreateIfNotExist();

        try
        {
            ChangeLifecycle(LifeCycle.InitializedSystem, () => { });

            // 加载插件
            /*
            ChangeLifecycle(LifeCycle.LoadPlugin, () =>
            {
                PluginLoader.AddPlugin(
                    PluginHelper.BuildPluginFromType(
                        typeof(Plugin.Plugin),
                        FileSystem.RootDirectory,
                        null,
                        string.IsNullOrWhiteSpace(Assembly.GetExecutingAssembly().Location)
                            ? null
                            : Assembly.GetExecutingAssembly().Location,
                        new()));

                foreach(var plugin in
                    PluginHelper.LoadAllPackedPluginsFromDirectory(
                        FileSystem.PackedPluginsDirectory))
                {
                    PluginLoader.AddPlugin(plugin);
                }
                PluginLoader.ActiveAllPlugins();
            });
            */

            // 创建世界
            ChangeLifecycle(LifeCycle.LoadSavings, _LoadSave);

            // 设置逻辑线程
            TimeUtilities.SetAnNoticeWhenCancel(
                Logger, "Logic Thread", _logicThreadStartWaitTokenSource.Token);
            ChangeLifecycle(LifeCycle.StartLogicThread, _StartLogicThread);

            // 设置网络线程
            TimeUtilities.SetAnNoticeWhenCancel(
                Logger, "Internet Thread", _internetThreadStartWaitTokenSource.Token);
            ChangeLifecycle(LifeCycle.StartNetThread, _StartNetworkThread);

            var wait = new SpinWait();

            var netToken = InternetMain.StopTokenSource;
            var logicToken = LogicThread.StopTokenSource;

            using var stopTokenSource = CancellationTokenSource.CreateLinkedTokenSource(netToken.Token, logicToken.Token);

            // wait for starting
            while (!(_internetThreadStartWaitTokenSource.IsCancellationRequested
                && _logicThreadStartWaitTokenSource.IsCancellationRequested))
            {
                wait.SpinOnce();

                // 出师未捷身先死
                if (stopTokenSource.IsCancellationRequested)
                {
                    return;
                }
            }
            startTokenSource.CancelAfter(100/* wait for fun :-) */);

            // stop when any of threads stop
            while (!stopTokenSource.IsCancellationRequested)
            {
                wait.SpinOnce();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "the server crash");
            ChangeLifecycle(LifeCycle.Crash, () => { });
        }
        finally
        {
            Logger.LogInformation("stop");
            ChangeLifecycle(LifeCycle.Stop, () => { });
        }

        return;

        void ChangeLifecycle(LifeCycle lifeCycle, Action action)
        {
            LifeCycleEvent<LifeCycle>.EnterCycle(
                                                 lifeCycle,
                                                 action,
                                                 this.Logger,
                                                 this.EventBus, () =>
                                                 {
                                                     this.CurrentLifeCycle = lifeCycle;
                                                 });
        }
    }
}
