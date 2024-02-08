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
    IEnumerable<IUnloadedPlugin> GetPluginAtDirectory(string pathToPlugin);

    /// <summary>
    /// 从文件夹获取插件。
    /// 这个文件夹中可以包含多个插件，也可以在任意深度的子文件中包含插件。
    /// </summary>
    /// <param name="directory">文件夹</param>
    /// <returns>获取到的插件</returns>
    IEnumerable<IUnloadedPlugin> GetAllPluginsFrom(string directory);
}
