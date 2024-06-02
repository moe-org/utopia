// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Microsoft.Extensions.Logging;

namespace Utopia.Core;

/// <summary>
/// e.g.:
/// Before Lifecycle(Fire events) -> Enter Lifecycle(Execute the code about the lifecycle) -> After Lifecycle(Fire events)
/// </summary>
public enum LifeCycleOrder
{
    /// <summary>
    /// Before enter next lifecycle. The <see cref="LifeCycleEventArgs{CycleT}.Cycle"/> hasn't changed.
    /// </summary>
    Before,
    Current,
    /// <summary>
    /// After enter next lifecycle.
    /// </summary>
    After,
}

public class LifeCycleEventArgs<TCycle> : EventArgs
{
    public LifeCycleOrder Order { get; }

    public TCycle Cycle { get; }

    public LifeCycleEventArgs(LifeCycleOrder order, TCycle cycle)
    {
        Cycle = cycle;
        Order = order;
    }

    /// <summary>
    /// About how we will fire the event,see <see cref="LifeCycleOrder"/>
    /// </summary>
    public static void EnterCycle(TCycle cycle, ILogger logger, Action<LifeCycleEventArgs<TCycle>> fireEventAction, Action switchAction)
    {
        ArgumentNullException.ThrowIfNull(cycle);
        logger.LogInformation("enter pre-{lifecycle} lifecycle", cycle);
        fireEventAction.Invoke(new LifeCycleEventArgs<TCycle>(LifeCycleOrder.Before, cycle));
        logger.LogInformation("enter {lifecycle} lifecycle", cycle);
        switchAction.Invoke();
        fireEventAction.Invoke(new LifeCycleEventArgs<TCycle>(LifeCycleOrder.Current, cycle));
        logger.LogInformation("enter post-{lifecycle} lifecycle", cycle);
        fireEventAction.Invoke(new LifeCycleEventArgs<TCycle>(LifeCycleOrder.After, cycle));
    }

    /// <summary>
    /// About how we will fire the event,see <see cref="LifeCycleOrder"/>
    /// </summary>
    public static void EnterCycle(TCycle cycle, ILogger logger, object source, WeakThreadSafeEventSource<LifeCycleEventArgs<TCycle>> bus, Action switchAction)
    {
        EnterCycle(
            cycle,
            logger,
            (e) =>
            {
                bus.Fire(source, e);
            },
            switchAction);
    }

    /// <summary>
    /// About how we will fire the event,see <see cref="LifeCycleOrder"/>
    /// </summary>
    public static void EnterCycle(TCycle cycle, ILogger logger, object source, IEventBus bus, Action switchAction)
    {
        EnterCycle(
            cycle,
            logger,
            (e) =>
            {
                bus.Fire(source, e);
            },
            switchAction);
    }
}
