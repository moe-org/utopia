namespace Utopia.Core;

/// <summary>
///     线程安全的,使用弱引用的事件源.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct WeakThreadSafeEventSource<T>() where T : EventArgs
{
    private readonly List<WeakReference<EventHandler<T>>> _handlers = [];

    private SpinLock _spinLock = new();

    public void ClearAllHandlers()
    {
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            this._handlers.Clear();
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }

    public IEnumerable<Exception> Fire(object? source, T @event, bool ignoreError = false)
    {
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            List<Exception> exceptions = new(4);
            for (var index = 0; index < this._handlers.Count; index++)
                try
                {
                    var handler = this._handlers[index];

                    if (handler.TryGetTarget(out var target))
                    {
                        target.Invoke(source, @event);
                        continue;
                    }

                    this._handlers.RemoveAt(index);
                    index--;
                }
                catch (Exception ex)
                {
                    if (!ignoreError)
                        throw;
                    exceptions.Add(ex);
                }

            return exceptions;
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }

    public void Register(EventHandler<T> handler)
    {
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            this._handlers.Add(new WeakReference<EventHandler<T>>(handler));
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }

    public void Unregister(EventHandler<T> handler)
    {
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            var removed = false;

            this._handlers.RemoveAll(e =>
            {
                if (e.TryGetTarget(out var obj))
                {
                    if (!removed && obj == handler)
                    {
                        // only remove once
                        removed = true;
                        return true;
                    }

                    return false;
                }

                // remove null anyhow
                return true;
            });
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }
}

/// <summary>
/// <see cref="WeakThreadSafeEventSource{T}"/>的无参数版本
/// </summary>
public struct WeakThreadSafeEventSource()
{
    private readonly List<WeakReference<EventHandler>> _handlers = [];

    private SpinLock _spinLock = new();

    public void ClearAllHandlers()
    {
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            this._handlers.Clear();
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }

    public IEnumerable<Exception> Fire(object source, bool ignoreError = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            List<Exception> exceptions = new(4);
            for (var index = 0; index < this._handlers.Count; index++)
                try
                {
                    var handler = this._handlers[index];

                    if (handler.TryGetTarget(out var target))
                    {
                        target.Invoke(source, EventArgs.Empty);
                        continue;
                    }

                    this._handlers.RemoveAt(index);
                    index--;
                }
                catch (Exception ex)
                {
                    if (!ignoreError)
                        throw;
                    exceptions.Add(ex);
                }

            return exceptions;
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }

    public void Register(EventHandler handler)
    {
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            this._handlers.Add(new WeakReference<EventHandler>(handler));
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }

    public void Unregister(EventHandler handler)
    {
        var taken = false;
        try
        {
            this._spinLock.Enter(ref taken);

            var removed = false;

            this._handlers.RemoveAll(e =>
            {
                if (e.TryGetTarget(out var obj))
                {
                    if (!removed && obj == handler)
                    {
                        // only remove once
                        removed = true;
                        return true;
                    }

                    return false;
                }

                // remove null
                return true;
            });
        }
        finally
        {
            if (taken) this._spinLock.Exit();
        }
    }
}
