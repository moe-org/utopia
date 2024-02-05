namespace Utopia.Core.Plugin;

/// <summary>
///     这个接口用于提供插件加载信息,供<see cref="IPluginLoader{PluginT}" />使用.
/// </summary>
public interface IPluginProvider
{
    /// <summary>
    ///     从文件夹获取应该加载的插件。
    ///     这个文件夹应该只包含单个插件。
    /// </summary>
    IEnumerable<IUnloadedPlugin> GetPluginsFrom(string pathToPlugin);
}