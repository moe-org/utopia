// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Helpers;

namespace Utopia.Core;

/// <summary>
/// 线程安全的,使用弱引用的事件源.
/// </summary>
/// <typeparam name="T"></typeparam>
public class WeakThreadSafeEventSource<T> : IEventSource<T>
{
    private readonly List<WeakReference<Action<T>>> _handlers = new();

    private SpinLock _spinLock = new();

    public void ClearAllHandlers()
    {
        bool taken = false;
        try
        {
            _spinLock.Enter(ref taken);

            _handlers.Clear();
        }
        finally
        {
            if (taken)
            {
                _spinLock.Exit();
            }
        }
    }

    public IEnumerable<Exception> Fire(T @event, bool ignoreError)
    {
        bool taken = false;
        try
        {
            _spinLock.Enter(ref taken);

            List<Exception> exceptions = new(4);
            for(var index=0; index< _handlers.Count; index++)
            {
                try
                {
                    var handler = _handlers[index];

                    if(handler.TryGetTarget(out var target))
                    {
                        target.Invoke(@event);
                        continue;
                    }
                    _handlers.RemoveAt(index);
                    index--;
                }
                catch(Exception ex)
                {
                    if (!ignoreError)
                    {
                        throw;
                    }
                    else
                    {
                        exceptions.Add(ex);
                    }
                }
            }

            return exceptions;
        }
        finally
        {
            if (taken)
            {
                _spinLock.Exit();
            }
        }
    }

    public void Register(Action<T> handler)
    {
        bool taken = false;
        try
        {
            _spinLock.Enter(ref taken);

            _handlers.Add(new(handler));
        }
        finally
        {
            if (taken)
            {
                _spinLock.Exit();
            }
        }
    }

    public void Unregister(Action<T> handler)
    {
        bool taken = false;
        try
        {
            _spinLock.Enter(ref taken);

            bool removed = false;

            _handlers.RemoveAll((e) =>
            {
                if (e.TryGetTarget(out var obj))
                {
                    if(!removed && obj == handler)
                    {
                        // only remove once
                        removed = true;
                        return true;
                    }

                    return false;
                }
                else
                {
                    // remove null
                    return true;
                }
            });
        }
        finally
        {
            if (taken)
            {
                _spinLock.Exit();
            }
        }
    }

}
