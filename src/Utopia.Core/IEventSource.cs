// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core;
public interface IEventSource<T>
{
    /// <summary>
    /// Register a handler. Allow duplication.
    /// </summary>
    /// <param name="handler"></param>
    void Register(Action<T> handler);

    /// <summary>
    /// Remove a handler equals to the argument.
    /// </summary>
    /// <param name="handler"></param>
    void Unregister(Action<T> handler);

    /// <summary>
    /// Fire a event.
    /// </summary>
    void Fire(T @event)
    {
        Fire(@event, false);
    }

    /// <summary>
    /// Fire a event.
    /// </summary>
    /// <param name="event">The event will be throw.</param>
    /// <param name="ignoreError">if true,ignore exception.Otherwise throw it.</param>
    /// <returns>If ignore errors,will return will exception the handler thrown.</returns>
    IEnumerable<Exception> Fire(T @event, bool ignoreError);

    /// <summary>
    /// Clear all handlers.
    /// </summary>
    void ClearAllHandlers();
}
