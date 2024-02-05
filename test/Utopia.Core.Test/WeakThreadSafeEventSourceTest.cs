namespace Utopia.Core.Test;

public class WeakThreadSafeEventSourceTest
{
    [Fact]
    public void RegisterTest()
    {
        var source = new WeakThreadSafeEventSource<int>();

        var get = 0;

        var action = (int i) => { get += i; };

        source.Register(action);
        source.Register(action);

        source.Fire(2, false);

        Assert.Equal(2 + 2, get);
    }

    [Fact]
    public void UnregisterTest()
    {
        var source = new WeakThreadSafeEventSource<int>();

        var get = 0;

        var action = (int i) => { get += i; };

        source.Register(action);
        source.Register(action);
        source.Unregister(action);

        source.Fire(2, false);

        Assert.Equal(2, get);
    }

    [Fact]
    public void UnregisterAllTest()
    {
        var source = new WeakThreadSafeEventSource<int>();

        var get = 0;

        var action = (int i) => { get += i; };

        source.Register(action);
        source.Register(action);
        source.ClearAllHandlers();

        source.Fire(2, false);

        Assert.Equal(0, get);
    }

    [Fact]
    public void ThrowTest()
    {
        var source = new WeakThreadSafeEventSource<int>();

        var action = (int i) => { throw new InvalidOperationException(); };

        source.Register(action);

        Assert.Throws<InvalidOperationException>(() => { source.Fire(0, false); });
    }

    [Fact]
    public void NotThrowTest()
    {
        var source = new WeakThreadSafeEventSource<int>();

        var action = (int i) => { throw new InvalidOperationException(); };

        source.Register(action);
        source.Register(action);

        var ex = source.Fire(0, true).ToArray();

        Assert.Equal(2, ex.Length);
        Assert.IsType<InvalidOperationException>(ex[0]);
        Assert.IsType<InvalidOperationException>(ex[1]);
    }
}