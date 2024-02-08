// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Utopia.Core;
using Utopia.Server.Map;

namespace Utopia.Server.Logic;

/// <summary>
/// 逻辑线程，按照tick进行更新操作。
/// </summary>
public interface ILogicThread
{
    ConcurrentBag<IUpdatable> Updatables { get; }

    /// <summary>
    /// 逻辑线程更新器
    /// </summary>
    IUpdater Updater { get; }

    /// <summary>
    /// 已经进行了多少次Tick
    /// </summary>
    long Ticks { get; }

    /// <summary>
    /// 用于指示状态。
    /// </summary>
    Task Task{ get; }

    void Stop();
}

public class StandardLogicThread : ILogicThread
{
    public StandardLogicThread(IEventBus eventBus,ILogger<StandardLogicThread> logger)
    {
        eventBus.Register<LifeCycleEvent<LifeCycle>>((cycle) =>
        {
            if (cycle is not { Cycle: LifeCycle.StartLogicThread, Order: LifeCycleOrder.Current })
            {
                return;
            }

            var logicT = new Thread(() =>
            {
                // 注册关闭事件
                eventBus.Register<LifeCycleEvent<LifeCycle>>((stopCycle) =>
                {
                    if (stopCycle is { Cycle: LifeCycle.Stop, Order: LifeCycleOrder.After })
                    {
                        Stop();
                    }
                });
                try
                {
                    Run();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Logic Thread Crashed");
                }
                finally {
                    Stop();
                }
            })
            {
                Name = "Server Logic Thread"
            };
            logicT.Start();
        });
    }

    private readonly ITicker _ticker = new Ticker();

    public IUpdater Updater { get; } = new SimplyUpdater();

    public long Ticks => _ticker.MillisecondFromLastTick;

    public ConcurrentBag<IUpdatable> Updatables { get; init; } = [];

    private readonly TaskCompletionSource _source = new();

    public Task Task => _source.Task;

    public void Stop()
    {
        _source.TrySetResult();
    }

    public void Run()
    {
        _ticker.Start();

        try
        {
            while (true)
            {
                while (!Task.IsCompleted)
                {
                    foreach (IUpdatable task in Updatables.ToArray())
                    {
                        task.Update(Updater);
                    }
                }

                _ticker.WaitToNextTick();
                _ticker.Tick();
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
