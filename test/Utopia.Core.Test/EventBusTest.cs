// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.Test;

public class EventBusTest
{
    [Fact]
    public void EventBusFireTest()
    {
        EventBus bus = new();
        int toggle = 0;

        bus.Register<int>((s, i) =>
        {
            Assert.Same(this, s);
            Assert.Equal(1, i.Event);
            toggle++;
        });
        bus.EventFired += (s, i) =>
        {
            Assert.Same(this, s);
            Assert.Equal(1, i.Event);
            toggle++;
        };

        bus.Fire(this, 1);

        Assert.Equal(2, toggle);
    }

    [Fact]
    public void EventBusUnregisterTest()
    {
        EventBus bus = new();
        int toggle = 0;

        EventHandler<EventBusEvent<int>> lambda = (object? s, EventBusEvent<int> i) =>
        {
            Assert.Same(this, s);
            Assert.Equal(1, i.Event);
            toggle++;
        };

        bus.Register(lambda);
        bus.EventFired += (s, i) =>
        {
            lambda(s, (i as EventBusEvent<int>)!);
        };
        bus.Unregister(lambda);

        bus.Fire(this, 1);
        Assert.Equal(1, toggle);
    }
}
