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

        bus.Register<int>(i =>
        {
            Assert.Equal(1, i);
            toggle++;
        });
        bus.EventFired += (i) =>
        {
            Assert.Equal(1, i);
            toggle++;
        };

        bus.Fire(1);

        Assert.Equal(2, toggle);
    }

    [Fact]
    public void EventBusUnregisterTest()
    {
        EventBus bus = new();
        int toggle = 0;

        var lambda = (int i) =>
        {
            Assert.Equal(1, i);
            toggle++;
        };

        bus.Register(lambda);
        bus.EventFired += (i) =>
        {
            Assert.Equal(1, i);
            toggle++;
        };
        bus.Unregister(lambda);

        bus.Fire(1);
        Assert.Equal(1, toggle);
    }

    [Fact]
    public void ClearTest()
    {

    }

}
