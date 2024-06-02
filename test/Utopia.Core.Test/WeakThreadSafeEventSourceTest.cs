namespace Utopia.Core.Test;

public class WeakThreadSafeEventSourceTest
{
    private class TestEventArgs : EventArgs
    {
        public int Event { get; set; }
    }

    [Fact]
    public void RegisterTest()
    {
        var source = new WeakThreadSafeEventSource<TestEventArgs>();

        var get = 0;
        object? firer = null;

        EventHandler<TestEventArgs> action = (object? o, TestEventArgs i) => { get += i.Event; firer = o; };

        source.Register(action);
        source.Register(action);

        source.Fire(EventArgs.Empty, new() { Event = 2 }, false);

        Assert.Equal(2 + 2, get);
        Assert.Equal(EventArgs.Empty, firer);
    }

    [Fact]
    public void UnregisterTest()
    {
        var source = new WeakThreadSafeEventSource<TestEventArgs>();

        var get = 0;

        EventHandler<TestEventArgs> action = (object? o, TestEventArgs i) => { get += i.Event; };

        source.Register(action);
        source.Register(action);
        source.Unregister(action);

        source.Fire(EventArgs.Empty, new TestEventArgs() { Event = 2 }, false);

        Assert.Equal(2, get);
    }

    [Fact]
    public void UnregisterAllTest()
    {
        var source = new WeakThreadSafeEventSource<TestEventArgs>();

        var get = 0;

        EventHandler<TestEventArgs> action = (object? o, TestEventArgs i) => { get += i.Event; };

        source.Register(action);
        source.Register(action);
        source.ClearAllHandlers();

        source.Fire(EventArgs.Empty, new TestEventArgs() { Event = 2 }, false);

        Assert.Equal(0, get);
    }

    [Fact]
    public void ThrowTest()
    {
        var source = new WeakThreadSafeEventSource<TestEventArgs>();

        EventHandler<TestEventArgs> action = (object? o, TestEventArgs i) => { throw new InvalidOperationException(); };

        source.Register(action);

        Assert.Throws<InvalidOperationException>(() => { source.Fire(EventArgs.Empty, new TestEventArgs(), false); });
        // test default argument
        Assert.Throws<InvalidOperationException>(() => { source.Fire(EventArgs.Empty, new TestEventArgs()); });
    }

    [Fact]
    public void NotThrowTest()
    {
        var source = new WeakThreadSafeEventSource<TestEventArgs>();

        EventHandler<TestEventArgs> action = (object? o, TestEventArgs i) => { throw new InvalidOperationException(); };

        source.Register(action);
        source.Register(action);

        var ex = source.Fire(EventArgs.Empty, new TestEventArgs(), true).ToArray();

        Assert.Equal(2, ex.Length);
        Assert.IsType<InvalidOperationException>(ex[0]);
        Assert.IsType<InvalidOperationException>(ex[1]);
    }
}
