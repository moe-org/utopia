namespace Utopia.Core.Plugin;

/// <summary>
///     Warning: <see cref="PluginLifeCycle.Created" /> won't work.
///     That lifecycle won't fire any event and it should has no handler.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LifecycleHandlerAttribute : Attribute
{
    public readonly PluginLifeCycle Lifecycle;

    public LifecycleHandlerAttribute(PluginLifeCycle value)
    {
        this.Lifecycle = value;
    }
}
