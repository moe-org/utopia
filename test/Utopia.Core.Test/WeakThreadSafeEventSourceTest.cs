// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Test;

public class WeakThreadSafeEventSourceTest
{

    [Fact]
    public void RegisterTest()
    {
        WeakThreadSafeEventSource<int> source = new WeakThreadSafeEventSource<int>();

        int get = 0;

        var action = (int i) =>
        {
            get += i;
        };

        source.Register(action);
        source.Register(action);

        source.Fire(2, false);

        Assert.Equal(2 + 2, get);
    }

    [Fact]
    public void UnregisterTest()
    {
        WeakThreadSafeEventSource<int> source = new WeakThreadSafeEventSource<int>();

        int get = 0;

        var action = (int i) =>
        {
            get += i;
        };

        source.Register(action);
        source.Register(action);
        source.Unregister(action);

        source.Fire(2,false);

        Assert.Equal(2, get);
    }

    [Fact]
    public void UnregisterAllTest()
    {
        WeakThreadSafeEventSource<int> source = new WeakThreadSafeEventSource<int>();

        int get = 0;

        var action = (int i) =>
        {
            get += i;
        };

        source.Register(action);
        source.Register(action);
        source.ClearAllHandlers();

        source.Fire(2, false);

        Assert.Equal(0, get);
    }

    [Fact]
    public void ThrowTest()
    {
        WeakThreadSafeEventSource<int> source = new WeakThreadSafeEventSource<int>();

        var action = (int i) =>
        {
            throw new InvalidOperationException();
        };

        source.Register(action);

        Assert.Throws<InvalidOperationException>(() =>
        {
            source.Fire(0, false);
        });
    }

    [Fact]
    public void NotThrowTest()
    {
        WeakThreadSafeEventSource<int> source = new WeakThreadSafeEventSource<int>();

        var action = (int i) =>
        {
            throw new InvalidOperationException();
        };

        source.Register(action);
        source.Register(action);

        var ex = source.Fire(0,true).ToArray();

        Assert.Equal(2, ex.Length);
        Assert.IsType<InvalidOperationException>(ex[0]);
        Assert.IsType<InvalidOperationException>(ex[1]);
    }
}
