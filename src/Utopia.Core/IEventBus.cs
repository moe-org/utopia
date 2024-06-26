// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;

namespace Utopia.Core;

/// <summary>
/// 事件总线.线程安全.
/// </summary>
public interface IEventBus
{
    public void Register<T>(EventHandler<T> handler) where T : EventArgs;

    public void Unregister<T>(EventHandler<T> handler) where T : EventArgs;

    public void Fire<T>(object source, T @event) where T : EventArgs;

    /// <summary>
    /// 事件总线抛出事件事件。
    /// 将会在事件总线抛出事件前调用。
    /// </summary>
    event EventHandler<EventArgs> EventFired;
}
