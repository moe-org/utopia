// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;

namespace Utopia.Core;

public class EventBusEvent(object @event) : EventArgs
{
    public virtual object Event => @event;
}

public class EventBusEvent<T>(T @event) : EventBusEvent(@event)
{
    public new T Event => @event;
}

/// <summary>
/// 事件总线.线程安全.
/// </summary>
public interface IEventBus
{
    public void Register<T>(EventHandler<EventBusEvent<T>> handler);

    public void Unregister<T>(EventHandler<EventBusEvent<T>> handler);

    public void Fire<T>(object source, T @event);

    /// <summary>
    /// 事件总线抛出事件事件。
    /// 将会在事件总线抛出事件前调用。
    /// </summary>
    event EventHandler<EventBusEvent> EventFired;
}
