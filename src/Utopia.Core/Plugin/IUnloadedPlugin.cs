namespace Utopia.Core.Plugin;

/// <summary>
///     代表一个未加载的,但是知道其依赖关系以及加载方式的插件.
/// </summary>
public interface IUnloadedPlugin
{
    /// <summary>
    ///     这个插件的依赖信息
    /// </summary>
    IPluginDependencyInformation Info { get; }

    /// <summary>
    ///     返回需要实例化的类型.
    /// </summary>
    IEnumerable<Type> Load();
}
