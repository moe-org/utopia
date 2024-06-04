// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.Test;

public class EventBusTest
{
    private class TestEvent(int count) : EventArgs
    {
        public int Count { get; } = count;
    }

    [Fact]
    public void EventBusFireTest()
    {
        EventBus bus = new();
        int toggle = 0;

        bus.Register<TestEvent>((s, i) =>
        {
            Assert.Null(s);
            Assert.Equal(1, i.Count);
            toggle++;
        });
        bus.EventFired += (s, i) =>
        {
            Assert.Null(s);
            Assert.IsType<TestEvent>(i);
            Assert.Equal(1, ((TestEvent)i).Count);
            toggle++;
        };

        bus.Fire(null, new TestEvent(1));

        Assert.Equal(2, toggle);
    }

    [Fact]
    public void EventBusUnregisterTest()
    {
        EventBus bus = new();
        int toggle = 0;

        EventHandler<TestEvent> lambda = (object? s, TestEvent i) =>
        {
            Assert.Same(this, s);
            Assert.Equal(1, i.Count);
            toggle++;
        };

        bus.Register(lambda);
        bus.EventFired += (s, i) =>
        {
            lambda(s, (i as TestEvent)!);
        };
        bus.Unregister(lambda);

        bus.Fire(this, new TestEvent(1));
        Assert.Equal(1, toggle);
    }
}
