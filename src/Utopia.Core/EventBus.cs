// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;

namespace Utopia.Core;

public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, (object, object)> _handlers = new();

    private readonly WeakThreadSafeEventSource<EventArgs> _source = new();

    public event EventHandler<EventArgs> EventFired
    {
        add => this._source.Register(value);
        remove => this._source.Unregister(value);
    }

    private (List<WeakReference<EventHandler<T>>> handlers, object @lock) _Get<T>() =>
        ((List<WeakReference<EventHandler<T>>>, object))_handlers.GetOrAdd
            (typeof(T), (t) => { return new(new List<WeakReference<EventHandler<T>>>(), new object()); });

    public void Clear<T>() where T : EventArgs
    {
        var get = _Get<T>();
        lock (get.@lock)
        {
            get.handlers.Clear();
        }
    }

    public void Fire<T>(object? source, T @event) where T : EventArgs
    {
        ArgumentNullException.ThrowIfNull(@event);
        this._source.Fire(source, @event, false);

        var get = _Get<T>();
        List<WeakReference<EventHandler<T>>> shouldRemove = new(get.handlers.Count);

        lock (get.@lock)
        {
            // fire event
            foreach (var handle in get.handlers)
            {
                if (handle.TryGetTarget(out var a))
                {
                    a.Invoke(source, @event);
                }
                else
                {
                    shouldRemove.Add(handle);
                }
            }

            // remove invalid
            foreach (var remove in shouldRemove)
            {
                get.handlers.Remove(remove);
            }
        }
    }

    public void Register<T>(EventHandler<T> handler) where T : EventArgs
    {
        var get = _Get<T>();
        lock (get.@lock)
        {
            get.handlers.Add(new WeakReference<EventHandler<T>>(handler));
        }
    }

    public void Unregister<T>(EventHandler<T> handler) where T : EventArgs
    {
        var get = _Get<T>();
        lock (get.@lock)
        {
            bool removed = false;
            get.handlers.RemoveAll((refer) =>
            {
                if (refer.TryGetTarget(out var obj))
                {
                    if (obj == handler && !removed)
                    {
                        removed = true;
                        return true;
                    }

                    return false;
                }

                return true;
            });
        }
    }
}
