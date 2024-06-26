#region

using Autofac;

#endregion

namespace Utopia.Core.Plugin;

public class PluginActivateEvent(ContainerBuilder builer) : EventArgs
{
    public ContainerBuilder Builder => builer;
}

/// <summary>
///     这个类用于加载插件。负责对类型<see cref="IUnloadedPlugin" />进行处理,并且还负责对插件的依赖进行处理.
/// </summary>
public interface IPluginLoader<PluginT> where PluginT : IPluginBase
{
    /// <summary>
    ///     已经加载过的插件.
    /// </summary>
    IEnumerable<PluginT> LoadedPlugins { get; }

    /// <summary>
    ///     激活插件的时候构造触发这个事件.
    /// </summary>
    event EventHandler<PluginActivateEvent> ActivatingPlugin;

    /// <summary>
    ///     激活所有<see cref="UnloadedPlugins" />插件.
    ///     所有激活过的插件实例将会被添加到<see cref="LoadedPlugins"/>中.
    /// </summary>
    void Activate(IEnumerable<IUnloadedPlugin> plugins);
}
